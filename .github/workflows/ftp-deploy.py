import os
import sys
import ftplib
import time

def deploy():
    server = os.environ.get('FTP_SERVER', '').strip()
    user = os.environ.get('FTP_USERNAME', '').strip()
    password = os.environ.get('FTP_PASSWORD', '').strip()
    local_dir = os.environ.get('LOCAL_DIR', '').strip()
    remote_dir = os.environ.get('REMOTE_DIR', '/').strip()

    if not server or not user or not password or not local_dir:
        print("Error: Missing required FTP parameters (FTP_SERVER, FTP_USERNAME, FTP_PASSWORD, LOCAL_DIR)")
        sys.exit(1)

    print(f"Connecting to FTP server: {server} as {user}...")
    ftp = ftplib.FTP()
    ftp.connect(server, 21, timeout=60)
    ftp.login(user, password)
    ftp.set_pasv(True)
    print("Connected successfully.")

    target_base_dir = remote_dir if remote_dir else "/"
    print(f"Target deployment directory: '{target_base_dir}'")

    def ensure_dir(path):
        dirs = [d for d in path.split('/') if d]
        ftp.cwd("/")
        for d in dirs:
            try:
                ftp.cwd(d)
            except Exception:
                try:
                    ftp.mkd(d)
                    ftp.cwd(d)
                except Exception:
                    pass

    ensure_dir(target_base_dir)

    # 1. Upload app_offline.htm to shut down IIS worker process and release locked .dll files
    offline_file = "app_offline.htm"
    print(f"Uploading app_offline.htm to {target_base_dir} to release IIS file locks...")
    with open(offline_file, "w", encoding="utf-8") as f:
        f.write("<!DOCTYPE html><html><body><h2>Deploying update...</h2></body></html>")

    try:
        ensure_dir(target_base_dir)
        with open(offline_file, "rb") as f:
            ftp.storbinary(f"STOR {offline_file}", f)
        print("app_offline.htm uploaded. Waiting 3 seconds for IIS worker process to release file locks...")
        time.sleep(3)
    except Exception as ex:
        print(f"Warning uploading app_offline.htm: {ex}")

    # 2. Upload all published files recursively
    print(f"Uploading files from local '{local_dir}' directly to '{target_base_dir}'...")
    uploaded_count = 0

    for root, dirs, files in os.walk(local_dir):
        rel_path = os.path.relpath(root, local_dir).replace('\\', '/')
        if rel_path == '.':
            current_target = target_base_dir
        else:
            current_target = f"{target_base_dir}/{rel_path}".replace('//', '/')
        
        ensure_dir(current_target)

        for file in files:
            if file == "app_offline.htm":
                continue
            local_file = os.path.join(root, file)
            with open(local_file, "rb") as f:
                ftp.storbinary(f"STOR {file}", f)
            uploaded_count += 1
            display_path = os.path.join(rel_path, file) if rel_path != '.' else file
            print(f"  ✓ Uploaded: {display_path}")

    # 3. Remove app_offline.htm so IIS immediately boots with the new version
    ensure_dir(target_base_dir)
    try:
        ftp.delete(offline_file)
        print("✓ Removed app_offline.htm - IIS is now actively running the new code!")
    except Exception as ex:
        print(f"Notice when deleting app_offline.htm: {ex}")

    if os.path.exists(offline_file):
        os.remove(offline_file)

    ftp.quit()
    print(f"\n=======================================================")
    print(f"🎉 Deployment completed successfully! Total files uploaded: {uploaded_count} directly to {target_base_dir}")
    print(f"=======================================================")

if __name__ == "__main__":
    deploy()

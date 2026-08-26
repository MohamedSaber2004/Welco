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
        print(f"Error: Missing required FTP parameters (Server: '{server}', User: '{user}', Password Set: {bool(password)}, LocalDir: '{local_dir}')")
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

    offline_file = "app_offline.htm"
    web_config_file = None
    uploaded_count = 0

    try:
        # 1. Upload app_offline.htm to shut down IIS worker process and release locked .dll files
        print(f"Uploading app_offline.htm to {target_base_dir} to release IIS file locks...")
        with open(offline_file, "w", encoding="utf-8") as f:
            f.write("<!DOCTYPE html><html><body><h2>Deploying update...</h2></body></html>")

        ensure_dir(target_base_dir)
        with open(offline_file, "rb") as f:
            ftp.storbinary(f"STOR {offline_file}", f)
        
        print("app_offline.htm uploaded. Waiting 6 seconds for IIS worker process to gracefully shut down...")
        time.sleep(6)

        # 2. Upload all published files recursively (delaying web.config to the end)
        print(f"Uploading files from local '{local_dir}' directly to '{target_base_dir}'...")

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
                # Defer web.config to the very end to trigger clean IIS recycle
                if file.lower() == "web.config" and rel_path == '.':
                    web_config_file = os.path.join(root, file)
                    continue

                local_file = os.path.join(root, file)
                
                max_retries = 3
                for attempt in range(max_retries):
                    try:
                        with open(local_file, "rb") as f:
                            ftp.storbinary(f"STOR {file}", f)
                        uploaded_count += 1
                        display_path = os.path.join(rel_path, file) if rel_path != '.' else file
                        print(f"  ✓ Uploaded: {display_path}")
                        break
                    except Exception as ex:
                        if attempt < max_retries - 1:
                            print(f"  ⚠ Retry uploading {file} (attempt {attempt + 1})...")
                            time.sleep(2)
                        else:
                            print(f"  ✗ Failed to upload {file}: {ex}")
                            raise

        # Upload web.config now at the end
        if web_config_file and os.path.exists(web_config_file):
            ensure_dir(target_base_dir)
            with open(web_config_file, "rb") as f:
                ftp.storbinary("STOR web.config", f)
            uploaded_count += 1
            print("  ✓ Uploaded: web.config (triggers IIS AppPool recycle)")

    finally:
        # 3. Always remove app_offline.htm so IIS immediately boots with the new version
        try:
            ensure_dir(target_base_dir)
            ftp.delete(offline_file)
            print("✓ Removed app_offline.htm - IIS is now actively running the new code!")
        except Exception as ex:
            print(f"Notice when deleting app_offline.htm: {ex}")

        if os.path.exists(offline_file):
            os.remove(offline_file)

        try:
            ftp.quit()
        except Exception:
            pass

    print(f"\n=======================================================")
    print(f"🎉 Deployment completed successfully! Total files uploaded: {uploaded_count} directly to {target_base_dir}")
    print(f"=======================================================")

if __name__ == "__main__":
    deploy()

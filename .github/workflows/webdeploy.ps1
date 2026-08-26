param(
    [Parameter(Mandatory=$true)]
    [string]$ProjectPath,
    
    [Parameter(Mandatory=$true)]
    [string]$PublishDir,
    
    [Parameter(Mandatory=$true)]
    [string]$PublishProfileContent
)

$ErrorActionPreference = "Stop"

Write-Host "======================================================="
Write-Host "🚀 Building and publishing project: $ProjectPath"
Write-Host "======================================================="
dotnet publish $ProjectPath -c Release -o $PublishDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed!"
    exit $LASTEXITCODE
}

Write-Host "Parsing Publish Profile XML..."
[xml]$xml = $PublishProfileContent
$profile = $xml.publishData.publishProfile | Where-Object { $_.publishMethod -eq "MSDeploy" }

if (-not $profile) {
    $profile = $xml.publishData.publishProfile | Where-Object { $_.publishMethod -eq "FTP" }
}

if (-not $profile) {
    $profile = $xml.publishData.publishProfile[0]
}

if (-not $profile) {
    Write-Error "Invalid Publish Profile XML format! Please ensure the entire XML content from the .publishsettings file is pasted into GitHub Secrets."
    exit 1
}

$publishUrl = $profile.publishUrl
$siteName = $profile.msdeploySite
$userName = $profile.userName
$password = $profile.userPWD
$publishMethod = $profile.publishMethod

Write-Host "Detected Publish Method: $publishMethod"
Write-Host "Target Site: $siteName"
Write-Host "Publish URL: $publishUrl"

if ($publishMethod -eq "MSDeploy" -or $publishUrl -match "msdeploy|:8172") {
    # Locate msdeploy.exe
    $msdeployPaths = @(
        "C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe",
        "C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe"
    )
    $msdeploy = $msdeployPaths | Where-Object { Test-Path $_ } | Select-Object -First 1

    if (-not $msdeploy) {
        Write-Error "msdeploy.exe not found on runner!"
        exit 1
    }

    # Format computerName
    if (-not ($publishUrl -match "^https?://")) {
        if ($publishUrl -notmatch "msdeploy\.axd") {
            $computerName = "https://${publishUrl}:8172/msdeploy.axd"
        } else {
            $computerName = "https://${publishUrl}"
        }
    } else {
        $computerName = $publishUrl
    }

    $fullPublishPath = (Resolve-Path $PublishDir).Path
    Write-Host "Deploying via Web Deploy to $computerName..."

    $destArg = "-dest:contentPath=$siteName,computerName=$computerName,userName=$userName,password=$password,authType=Basic,includeAcls=False"
    $sourceArg = "-source:contentPath=$fullPublishPath"

    $msdeployArgs = @(
        "-verb:sync",
        $sourceArg,
        $destArg,
        "-enableRule:AppOffline",
        "-allowUntrusted",
        "-verbose"
    )

    & $msdeploy $msdeployArgs

    if ($LASTEXITCODE -ne 0) {
        Write-Error "MSDeploy failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }
} else {
    Write-Host "Falling back to FTP deployment with profile credentials..."
    $env:FTP_SERVER = $profile.publishUrl -replace "^ftp://", "" -replace "/.*$", ""
    $env:FTP_USERNAME = $profile.userName
    $env:FTP_PASSWORD = $profile.userPWD
    $env:LOCAL_DIR = $PublishDir
    $env:REMOTE_DIR = "/"

    python .github/workflows/ftp-deploy.py
}

Write-Host "======================================================="
Write-Host "🎉 Web Deploy finished successfully!"
Write-Host "======================================================="

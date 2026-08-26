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

# Resolve password: prioritize explicit secret over XML (since .publishsettings often omits the password)
$password = $env:PROFILE_PASSWORD
if (-not $password) {
    $password = $profile.userPWD
}
if (-not $password) {
    $password = $env:AUTH_FTP_PASSWORD
}
if (-not $password) {
    $password = $env:GATEWAY_FTP_PASSWORD
}

$publishMethod = $profile.publishMethod

Write-Host "Detected Publish Method: $publishMethod"
Write-Host "Target Site: $siteName"
Write-Host "Publish URL: $publishUrl"
Write-Host "User Name: $userName"

$msdeploySucceeded = $false

if ($publishMethod -eq "MSDeploy" -or $publishUrl -match "msdeploy|:8172") {
    $msdeployPaths = @(
        "C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe",
        "C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe"
    )
    $msdeploy = $msdeployPaths | Where-Object { Test-Path $_ } | Select-Object -First 1

    if ($msdeploy) {
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
        Write-Host "Attempting Web Deploy to $computerName..."

        $destArg = "-dest:contentPath=$siteName,computerName=$computerName,userName=$userName,password=$password,authType=Basic,includeAcls=False"
        $sourceArg = "-source:contentPath=$fullPublishPath"

        $msdeployArgs = @(
            "-verb:sync",
            $sourceArg,
            $destArg,
            "-enableRule:AppOffline",
            "-allowUntrusted"
        )

        try {
            & $msdeploy $msdeployArgs
            if ($LASTEXITCODE -eq 0) {
                $msdeploySucceeded = $true
                Write-Host "🎉 Web Deploy finished successfully!"
            } else {
                Write-Warning "Web Deploy returned exit code $LASTEXITCODE. Falling back to robust FTP deployment..."
            }
        } catch {
            Write-Warning "Web Deploy failed: $_. Falling back to robust FTP deployment..."
        }
    }
}

if (-not $msdeploySucceeded) {
    Write-Host "======================================================="
    Write-Host "🔄 Executing IIS-Safe FTP Deployment..."
    Write-Host "======================================================="

    # Extract FTP server from publishUrl or publishProfile
    $ftpServer = $profile.publishUrl -replace "^https?://", "" -replace "^ftp://", "" -replace ":[0-9]+.*$", "" -replace "/.*$", ""
    
    $env:FTP_SERVER = $ftpServer
    $env:FTP_USERNAME = $userName
    $env:FTP_PASSWORD = $password
    $env:LOCAL_DIR = $PublishDir
    $env:REMOTE_DIR = "/"

    # Run Python deployer
    $scriptPath = Join-Path $PSScriptRoot "ftp-deploy.py"
    python $scriptPath
}

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

$publishUrl = [string]$profile.publishUrl
$siteName = [string]$profile.msdeploySite
$userName = [string]$profile.userName
$publishMethod = [string]$profile.publishMethod

# 1. Prioritize userPWD from the XML publish profile
$password = [string]$profile.userPWD
if ([string]::IsNullOrWhiteSpace($password)) {
    $password = [string]$env:PROFILE_PASSWORD
}
if ([string]::IsNullOrWhiteSpace($password)) {
    $password = [string]$env:AUTH_FTP_PASSWORD
}
if ([string]::IsNullOrWhiteSpace($password)) {
    $password = [string]$env:GATEWAY_FTP_PASSWORD
}

$authType = if ($profile.authType) { [string]$profile.authType } else { "Basic" }

Write-Host "Detected Publish Method: $publishMethod"
Write-Host "Target Site: $siteName"
Write-Host "Publish URL: $publishUrl"
Write-Host "User Name: $userName"
Write-Host "Auth Type: $authType"
Write-Host "Password Loaded: $(if ([string]::IsNullOrWhiteSpace($password)) { 'NO (Empty)' } else { 'YES (' + $password.Length + ' chars)' })"

$msdeploySucceeded = $false

if ($publishMethod -eq "MSDeploy" -or $publishUrl -match "msdeploy|:8172") {
    $msdeployPaths = @(
        "C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe",
        "C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe"
    )
    $msdeploy = $msdeployPaths | Where-Object { Test-Path $_ } | Select-Object -First 1

    if ($msdeploy) {
        $rawHost = $publishUrl.Trim().TrimEnd('/')
        if ($rawHost.StartsWith("http://", [System.StringComparison]::OrdinalIgnoreCase)) {
            $rawHost = $rawHost.Substring(7)
        } elseif ($rawHost.StartsWith("https://", [System.StringComparison]::OrdinalIgnoreCase)) {
            $rawHost = $rawHost.Substring(8)
        }

        if (-not $rawHost.Contains(":8172") -and -not $rawHost.Contains("msdeploy.axd")) {
            $computerName = "https://" + $rawHost + ":8172/msdeploy.axd"
        } elseif (-not $rawHost.Contains("msdeploy.axd")) {
            $computerName = "https://" + $rawHost + "/msdeploy.axd"
        } else {
            $computerName = "https://" + $rawHost
        }

        $fullPublishPath = (Resolve-Path $PublishDir).Path
        Write-Host "Attempting Web Deploy to $computerName..."

        $destArg = "-dest:contentPath=" + $siteName + ",computerName=" + $computerName + ",userName=" + $userName + ",password=" + $password + ",authType=" + $authType + ",includeAcls=False"
        $sourceArg = "-source:contentPath=" + $fullPublishPath

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
                Write-Host "======================================================="
                Write-Host "🎉 Web Deploy finished successfully!"
                Write-Host "======================================================="
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

    $ftpServer = $publishUrl -replace "^https?://", "" -replace "^ftp://", "" -replace ":[0-9]+.*$", "" -replace "/.*$", ""
    
    $env:FTP_SERVER = $ftpServer
    $env:FTP_USERNAME = $userName
    $env:FTP_PASSWORD = $password
    $env:LOCAL_DIR = $PublishDir
    $env:REMOTE_DIR = "/"

    # Run Python deployer
    $scriptPath = Join-Path $PSScriptRoot "ftp-deploy.py"
    python $scriptPath
}

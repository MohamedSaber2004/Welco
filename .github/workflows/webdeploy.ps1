param(
    [Parameter(Mandatory=$true)]
    [string]$ProjectPath,
    
    [Parameter(Mandatory=$true)]
    [string]$PublishDir,
    
    [Parameter(Mandatory=$true)]
    [string]$PublishProfileContent
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($PublishProfileContent)) {
    Write-Error "Error: Publish Profile XML is empty! Please verify your GitHub Secret (AUTH_PUBLISH_PROFILE / GATEWAY_PUBLISH_PROFILE)."
    exit 1
}

Write-Host "======================================================="
Write-Host "🚀 Deploying with Publish Profile (.publishsettings)"
Write-Host "Project: $ProjectPath"
Write-Host "======================================================="

# 1. Parse XML to inspect profile properties
[xml]$xml = $PublishProfileContent
$profile = $xml.publishData.publishProfile | Where-Object { $_.publishMethod -eq "MSDeploy" }

if (-not $profile) {
    $profile = $xml.publishData.publishProfile[0]
}

if (-not $profile) {
    Write-Error "Invalid Publish Profile XML format! Please ensure the entire XML content from the .publishsettings file is pasted into GitHub Secrets."
    exit 1
}

$siteName = [string]$profile.msdeploySite
$publishUrl = [string]$profile.publishUrl
$userName = [string]$profile.userName
$password = [string]$profile.userPWD

Write-Host "Target Site: $siteName"
Write-Host "Publish URL: $publishUrl"
Write-Host "User Name: $userName"
Write-Host "Password in XML: $(if ([string]::IsNullOrWhiteSpace($password)) { 'NO (Empty)' } else { 'YES (' + $password.Length + ' chars)' })"

# 2. Save .publishsettings to temporary file
$tempProfilePath = Join-Path $env:RUNNER_TEMP "MonsterASP.publishsettings"
Set-Content -Path $tempProfilePath -Value $PublishProfileContent -Encoding UTF8
Write-Host "Saved Publish Profile to: $tempProfilePath"

# 3. Format computerName for MSDeploy
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

# 4. Execute dotnet publish with MSDeploy
Write-Host "======================================================="
Write-Host "📦 Building and Deploying via MSDeploy to $computerName..."
Write-Host "======================================================="

$msdeployParams = @(
    "publish",
    $ProjectPath,
    "-c", "Release",
    "-o", $PublishDir,
    "/p:DeployOnBuild=true",
    "/p:PublishProfile=$tempProfilePath",
    "/p:AllowUntrustedCertificate=True",
    "/p:EnableMSDeployAppOffline=true"
)

if (-not [string]::IsNullOrWhiteSpace($password)) {
    $msdeployParams += "/p:Password=$password"
}

dotnet @msdeployParams

if ($LASTEXITCODE -ne 0) {
    # If MSBuild targets need direct msdeploy execution
    Write-Host "Attempting direct msdeploy.exe sync..."

    $msdeployPaths = @(
        "C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe",
        "C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe"
    )
    $msdeploy = $msdeployPaths | Where-Object { Test-Path $_ } | Select-Object -First 1

    if ($msdeploy) {
        $fullPublishPath = (Resolve-Path $PublishDir).Path
        $destArg = "-dest:contentPath=" + $siteName + ",computerName=" + $computerName + ",userName=" + $userName + ",password=" + $password + ",authType=Basic,includeAcls=False"
        $sourceArg = "-source:contentPath=" + $fullPublishPath

        $directArgs = @(
            "-verb:sync",
            $sourceArg,
            $destArg,
            "-enableRule:AppOffline",
            "-allowUntrusted",
            "-verbose"
        )

        & $msdeploy $directArgs

        if ($LASTEXITCODE -ne 0) {
            Write-Error "Deployment failed with exit code $LASTEXITCODE. Please check credentials in MonsterASP."
            exit $LASTEXITCODE
        }
    } else {
        Write-Error "Deployment failed."
        exit $LASTEXITCODE
    }
}

# Clean up temp file
if (Test-Path $tempProfilePath) {
    Remove-Item $tempProfilePath -Force -ErrorAction SilentlyContinue
}

Write-Host "======================================================="
Write-Host "🎉 Deployment completed successfully via Publish Profile!"
Write-Host "======================================================="

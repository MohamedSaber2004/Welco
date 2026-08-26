param(
    [Parameter(Mandatory=$true)]
    [string]$ProjectPath,
    
    [Parameter(Mandatory=$true)]
    [string]$PublishDir,
    
    [Parameter(Mandatory=$true)]
    [string]$PublishProfileContent
)

$ErrorActionPreference = "Stop"
$PSNativeCommandArgumentPassing = "Legacy"

if ([string]::IsNullOrWhiteSpace($PublishProfileContent)) {
    Write-Error "Error: Publish Profile XML is empty! Please verify your GitHub Secret (AUTH_PUBLISH_PROFILE / GATEWAY_PUBLISH_PROFILE)."
    exit 1
}

Write-Host "======================================================="
Write-Host "Building and publishing project: $ProjectPath"
Write-Host "======================================================="

# 1. Parse XML
[xml]$xml = $PublishProfileContent
$profile = $xml.publishData.publishProfile | Where-Object { $_.publishMethod -eq "MSDeploy" }

if (-not $profile) {
    $profile = $xml.publishData.publishProfile[0]
}

if (-not $profile) {
    Write-Error "Invalid Publish Profile XML format!"
    exit 1
}

$siteName = [string]$profile.msdeploySite
$publishUrl = [string]$profile.publishUrl
$userName = [string]$profile.userName
$password = [string]$profile.userPWD

Write-Host "Target Site: $siteName"
Write-Host "Publish URL: $publishUrl"
Write-Host "User Name: $userName"
Write-Host "Password in XML: $(if ([string]::IsNullOrWhiteSpace($password)) { 'NO' } else { 'YES (' + $password.Length + ' chars)' })"

# 2. Build and publish locally first
dotnet publish $ProjectPath -c Release -o $PublishDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet publish failed!"
    exit $LASTEXITCODE
}

# 3. Format computerName
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

# 4. Locate msdeploy.exe
$msdeployPaths = @(
    "C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe",
    "C:\Program Files (x86)\IIS\Microsoft Web Deploy V3\msdeploy.exe"
)
$msdeploy = $msdeployPaths | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $msdeploy) {
    Write-Error "msdeploy.exe not found on system!"
    exit 1
}

$fullPublishPath = (Resolve-Path $PublishDir).Path
Write-Host "Deploying via MSDeploy to $computerName..."

$sourceArg = "-source:contentPath=$fullPublishPath"
$destArg = "-dest:contentPath=$siteName,computerName=$computerName,userName=$userName,password=$password,authType=Basic,includeAcls=False"

$msdeployArgs = @(
    "-verb:sync",
    $sourceArg,
    $destArg,
    "-enableRule:AppOffline",
    "-allowUntrusted"
)

& $msdeploy $msdeployArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "MSDeploy failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "======================================================="
Write-Host "🎉 Deployment completed successfully via Publish Profile!"
Write-Host "======================================================="

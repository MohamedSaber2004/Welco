<#Requires -Version 5.1>
<#
.SYNOPSIS
  Generates a REAL integration client_id + client_secret for Welco<->SNUL service auth.

.DESCRIPTION
  Uses .NET CSPRNG (RandomNumberGenerator.GetString, unbiased) to create a
  48-char URL-safe secret. Prints the pair ONCE with the exact environment
  variable lines for both hosts. The secret is never written to disk by this
  script — paste it into the host environment and close this window.

  Symmetric values required: the SAME client_id + client_secret must be set
  on Welco Auth (WelcoServiceSettings__Clients__<id>__Secret) and on the SNUL
  caller (WelcoIntegration__ClientSecret or WelcoIntegration__Systems__<sys>__ClientSecret).

.EXAMPLE
  .\New-IntegrationClient.ps1 -ClientId snul
#>
[CmdletBinding()]
param(
    [Parameter()]
    [ValidatePattern('^[a-z0-9][a-z0-9-]{2,31}$')]
    [string]$ClientId = "snul",

    [Parameter()]
    [ValidateRange(32, 128)]
    [int]$SecretLength = 48
)

$alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_".ToCharArray()
# 64-symbol alphabet divides the 256 byte range evenly, so mod-64 is unbiased.
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
try {
    $raw = New-Object byte[] $SecretLength
    $rng.GetBytes($raw)
    $chars = New-Object char[] $SecretLength
    for ($i = 0; $i -lt $SecretLength; $i++) { $chars[$i] = $alphabet[$raw[$i] % $alphabet.Length] }
    $secret = New-Object string (,$chars)
}
finally { $rng.Dispose() }

Write-Output "=== NEW INTEGRATION CREDENTIALS (shown once - store in host env now) ==="
Write-Output "client_id:     $ClientId"
Write-Output "client_secret: $secret"
Write-Output ""
Write-Output "--- Welco Auth host env (Test/Prod) ---"
Write-Output "WelcoServiceSettings__Clients__${ClientId}__Secret=$secret"
Write-Output ""
Write-Output "--- SNUL caller host env, flat (Test/Prod) ---"
Write-Output "WelcoIntegration__ClientId=$ClientId"
Write-Output "WelcoIntegration__ClientSecret=$secret"
Write-Output ""
Write-Output "--- SNUL caller host env, per-system alternative ---"
Write-Output "WelcoIntegration__Systems__${ClientId}__ClientId=$ClientId"
Write-Output "WelcoIntegration__Systems__${ClientId}__ClientSecret=$secret"
Write-Output ""
Write-Output "WARNING: never commit this secret. Restart both services after setting."

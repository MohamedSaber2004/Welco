$json = Get-Content "E:\Welco Site\Welco\Welco.API\Ocelot\ocelot.merged.Test.json" -Raw | ConvertFrom-Json
Write-Host "Routes in merged config: $($json.Routes.Count)"

$baseRoutes = @("/api/v1/products", "/api/v1/categories", "/api/v1/currencies", "/api/v1/exchange-rates/cart-total")
foreach ($route in $baseRoutes) {
    $found = $json.Routes | Where-Object { $_.UpstreamPathTemplate -eq $route }
    if ($found) { Write-Host "  $route : PRESENT" } else { Write-Host "  $route : MISSING" }
}

$productRoutes = $json.Routes | Where-Object { $_.DownstreamHostAndPorts[0].Host -eq "welco-product.runasp.net" }
Write-Host ""
Write-Host "Product host routes: $($productRoutes.Count)"
Write-Host "Expected: 24"

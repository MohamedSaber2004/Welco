#!/usr/bin/env python3
"""
Verify Ocelot merged configuration - Python equivalent of check_verify.ps1
For use on systems where PowerShell is not available (e.g., MonsterASP free plan)
"""

import json
import sys
from pathlib import Path

def main():
    # Path to the merged Ocelot configuration
    # Support both running from repo root and from script directory
    repo_root = Path(__file__).parent
    publish_path = repo_root / "publish" / "gateway" / "Ocelot" / "ocelot.merged.Test.json"
    if publish_path.exists():
        config_path = publish_path
        print(f"Verifying published config at: {config_path}")
    else:
        config_path = repo_root / "Welco.API" / "Ocelot" / "ocelot.merged.Test.json"
        if not config_path.exists():
            alt_path = Path.cwd() / "Welco.API" / "Ocelot" / "ocelot.merged.Test.json"
            if alt_path.exists():
                config_path = alt_path
            else:
                print(f"Error: Configuration file not found at {config_path}")
                sys.exit(1)
        print(f"Verifying source config at: {config_path}")
    
    try:
        with open(config_path, 'r', encoding='utf-8-sig') as f:
            data = json.load(f)
    except Exception as e:
        print(f"Error reading JSON file: {e}")
        sys.exit(1)
    
    routes = data.get('Routes', [])
    
    # 1. Count total routes
    total_routes = len(routes)
    print(f"Routes in merged config: {total_routes}")
    
    # 2. Check base routes
    base_routes = ["/api/v1/products", "/api/v1/categories", "/api/v1/currencies", "/api/v1/exchange-rates/cart-total"]
    for route in base_routes:
        found = any(r.get('UpstreamPathTemplate') == route for r in routes)
        status = "PRESENT" if found else "MISSING"
        print(f"  {route} : {status}")
    
    # 3. Count product host routes
    product_routes = [
        r for r in routes 
        if r.get('DownstreamHostAndPorts') 
        and len(r['DownstreamHostAndPorts']) > 0 
        and r['DownstreamHostAndPorts'][0].get('Host') == 'welco-product.runasp.net'
    ]
    product_count = len(product_routes)
    
    print("")
    print(f"Product host routes: {product_count}")
    print("Expected: 24")
    
    # Exit with error code if counts don't match expectations
    if product_count != 24:
        sys.exit(1)
    
if __name__ == "__main__":
    main()
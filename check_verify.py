#!/usr/bin/env python3
"""Verify Ocelot merged configuration for API Gateway deploy.
Validates that merged Ocelot configs are well-formed and contain routes."""

import json
import sys
from pathlib import Path

def verify_merged_config(merged_path, env_label):
    if not merged_path.exists():
        print(f"ERROR: Merged config not found at {merged_path}")
        return False
    try:
        with open(merged_path, 'r', encoding='utf-8-sig') as f:
            data = json.load(f)
    except json.JSONDecodeError as e:
        print(f"ERROR: Invalid JSON in {merged_path}: {e}")
        return False
    except Exception as e:
        print(f"ERROR: Could not read {merged_path}: {e}")
        return False

    routes = data.get('Routes', [])
    global_config = data.get('GlobalConfiguration', {})
    print(f"✓ {env_label}: {len(routes)} routes, GlobalConfiguration keys: {list(global_config.keys())}")
    return True

def main():
    repo_root = Path(__file__).parent if '__file__' in dir() else Path.cwd()
    publish_dir = repo_root / "publish" / "gateway" / "Ocelot"
    source_dir = repo_root / "Welco.API" / "Ocelot"

    environments = ["Development", "Production", "Test"]
    all_ok = True

    for env in environments:
        merged = publish_dir / f"ocelot.merged.{env}.json"
        if merged.exists():
            if not verify_merged_config(merged, env):
                all_ok = False
        else:
            source = source_dir / f"ocelot.merged.{env}.json"
            if source.exists():
                if not verify_merged_config(source, f"{env} (source)"):
                    all_ok = False
            else:
                print(f"WARNING: No merged config found for {env}")

    if not all_ok:
        print("FAILED: One or more Ocelot configs are invalid")
        sys.exit(1)

    print("OK: All Ocelot configurations verified")
    sys.exit(0)

if __name__ == "__main__":
    main()
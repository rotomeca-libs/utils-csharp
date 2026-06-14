#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "$0")/.." && pwd)"

cat "$root/readme/README.github.header.md" "$root/readme/README.base.md" > "$root/README.md"
cat "$root/readme/README.nuget.header.md"  "$root/readme/README.base.md" > "$root/README.nuget.md"

echo "README.md et README.nuget.md générés."
$root   = Split-Path $PSScriptRoot -Parent

$base   = Get-Content "$root/readme/README.base.md"         -Raw -Encoding UTF8
$github = Get-Content "$root/readme/README.github.header.md" -Raw -Encoding UTF8
$nuget  = Get-Content "$root/readme/README.nuget.header.md"  -Raw -Encoding UTF8

Set-Content "$root/README.md"       -Value ($github + "`n" + $base) -Encoding UTF8 -NoNewline
Set-Content "$root/README.nuget.md" -Value ($nuget  + "`n" + $base) -Encoding UTF8 -NoNewline

Write-Host "README.md et README.nuget.md générés."

#Requires -Version 5.1
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'umc22.psm1') -Force
$p = Get-Umc22Probe
Write-Umc22Probe $p
if (-not $p.Present) { exit 2 }

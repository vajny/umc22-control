#Requires -Version 5.1
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
dotnet run --project (Join-Path $PSScriptRoot 'src\Umc22Control\Umc22Control.csproj') -c Release

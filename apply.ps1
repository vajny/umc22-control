#Requires -Version 5.1
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'umc22.psm1') -Force

Write-Host 'UMC22 capture apply: 48 kHz / 16-bit + disable enhancements on USB Audio CODEC mic'
Write-Host 'Does not touch Sonar, FlexASIO, or Ploytec.'
Write-Host ''

$result = Invoke-Umc22CaptureApply
if (-not $result.ok) {
    Write-Host ("FAIL: {0}" -f $result.error)
    exit 1
}
if (-not $result.devices -or @($result.devices).Count -eq 0) {
    Write-Host 'FAIL: no capture endpoint matching USB Audio CODEC'
    exit 2
}

$failed = $false
foreach ($d in @($result.devices)) {
    Write-Host ("  {0}" -f $d.name)
    if ($d.disableSysFxError) {
        Write-Host ("      disable APO: FAIL {0}" -f $d.disableSysFxError)
        $failed = $true
    } else {
        Write-Host '      disable APO: ok'
    }
    if ($d.setFormatError) {
        Write-Host ("      set 48k/16: FAIL {0}  (try elevated PowerShell)" -f $d.setFormatError)
        $failed = $true
    } else {
        Write-Host '      set 48k/16: ok'
    }
    Write-Host ("      exclusive 48k/16 stereo={0} mono={1}" -f $d.exclusive48k16Stereo, $d.exclusive48k16Mono)
}

Write-Host ''
Write-Host 'Re-read after apply:'
Write-Umc22Probe (Get-Umc22Probe)

if ($failed) { exit 3 }
exit 0

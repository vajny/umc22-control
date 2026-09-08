#Requires -Version 5.1
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'umc22.psm1') -Force

$r = Test-Umc22Alive
Write-Host ("USB VID_08BB&PID_2902 present : {0}" -f $r.UsbPresent)
Write-Host ("usbaudio running              : {0}" -f $r.UsbaudioRunning)
Write-Host ("MI_00 bound to usbaudio       : {0}" -f $r.AudioBound)
Write-Host ("capture USB Audio CODEC       : {0}" -f $r.CapturePresent)
if ($r.Passed) {
    Write-Host 'PASS'
    exit 0
}
Write-Host 'FAIL'
exit 1

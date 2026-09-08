$script:Umc22Root = Split-Path -Parent $PSCommandPath
if (-not $script:Umc22Root) { $script:Umc22Root = (Get-Location).Path }

$script:VidPid = 'VID_08BB&PID_2902'
$script:NameContains = 'USB Audio CODEC'
$script:DesktopDriver = Join-Path $env:USERPROFILE 'Desktop\BEHRINGER_2902_X64_2.8.40\BEHRINGER_2902_X64_2.8.40'
$script:FlexAsioToml = Join-Path $env:USERPROFILE 'FlexASIO.toml'

function Get-Umc22WasapiType {
    if (-not ('Umc22.WasapiCapture' -as [type])) {
        Add-Type -Path (Join-Path $script:Umc22Root 'src\WasapiCapture.cs') -ErrorAction Stop
    }
}

function Get-Umc22PnpNodes {
    Get-PnpDevice | Where-Object { $_.InstanceId -match $script:VidPid }
}

function Get-Umc22NodeProps {
    param([Parameter(Mandatory)][string]$InstanceId)
    $keys = @(
        'DEVPKEY_Device_DriverInfPath',
        'DEVPKEY_Device_DriverVersion',
        'DEVPKEY_Device_DriverProvider',
        'DEVPKEY_Device_DriverDesc',
        'DEVPKEY_Device_Service',
        'DEVPKEY_Device_Manufacturer',
        'DEVPKEY_Device_DriverDate',
        'DEVPKEY_Device_MatchingDeviceId'
    )
    $out = [ordered]@{ InstanceId = $InstanceId }
    foreach ($k in $keys) {
        $p = Get-PnpDeviceProperty -InstanceId $InstanceId -KeyName $k -ErrorAction SilentlyContinue
        $name = $k.Replace('DEVPKEY_Device_', '')
        $out[$name] = if ($p) { [string]$p.Data } else { $null }
    }
    $dev = Get-PnpDevice -InstanceId $InstanceId -ErrorAction SilentlyContinue
    $out.Status = if ($dev) { [string]$dev.Status } else { $null }
    $out.Class = if ($dev) { [string]$dev.Class } else { $null }
    $out.FriendlyName = if ($dev) { [string]$dev.FriendlyName } else { $null }
    [pscustomobject]$out
}

function Get-Umc22ServiceState {
    param([string[]]$Names)
    foreach ($n in $Names) {
        $s = Get-CimInstance Win32_SystemDriver -Filter "Name='$n'" -ErrorAction SilentlyContinue
        if (-not $s) {
            [pscustomobject]@{ Name = $n; Present = $false; State = $null; Path = $null }
        } else {
            [pscustomobject]@{
                Name    = $n
                Present = $true
                State   = [string]$s.State
                Path    = [string]$s.PathName
                Start   = [string]$s.StartMode
            }
        }
    }
}

function Get-FileSha256OrMissing {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) { return $null }
    (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash
}

function Get-Umc22DriverStoreSys {
    param([string]$InfPrefix)
    $dir = Get-ChildItem 'C:\Windows\System32\DriverStore\FileRepository' -Directory -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -like "$InfPrefix.inf_*" } |
        Select-Object -First 1
    if (-not $dir) { return $null }
    Get-ChildItem $dir.FullName -Filter '*.sys' | Select-Object -First 1
}

function Get-Umc22DriverCompare {
    $pairs = @(
        @{ Label = 'busb2902.sys'; Desktop = Join-Path $script:DesktopDriver 'busb2902.sys'; StoreInf = 'busb2902' }
        @{ Label = 'busbwdm.sys';  Desktop = Join-Path $script:DesktopDriver 'busbwdm.sys';  StoreInf = 'busbwdm' }
    )
    foreach ($p in $pairs) {
        $storeFile = Get-Umc22DriverStoreSys $p.StoreInf
        $deskHash = Get-FileSha256OrMissing $p.Desktop
        $storeHash = if ($storeFile) { Get-FileSha256OrMissing $storeFile.FullName } else { $null }
        [pscustomobject]@{
            File          = $p.Label
            DesktopPath   = $p.Desktop
            DesktopHash   = $deskHash
            StorePath     = if ($storeFile) { $storeFile.FullName } else { $null }
            StoreHash     = $storeHash
            HashesMatch   = ($deskHash -and $storeHash -and $deskHash -eq $storeHash)
        }
    }
}

function Get-Umc22FlexAsioLeftover {
    if (-not (Test-Path -LiteralPath $script:FlexAsioToml)) {
        return [pscustomobject]@{ Present = $false; Path = $script:FlexAsioToml; Note = 'no FlexASIO.toml' }
    }
    $raw = Get-Content -LiteralPath $script:FlexAsioToml -Raw -ErrorAction SilentlyContinue
    [pscustomobject]@{
        Present = $true
        Path    = $script:FlexAsioToml
        Note    = 'leftover; not on the mic capture path; not modified'
        Snippet = ($raw -replace '\s+', ' ').Trim()
    }
}

function Get-Umc22CaptureWasapi {
    Get-Umc22WasapiType
    $json = [Umc22.WasapiCapture]::Probe($script:NameContains)
    $json | ConvertFrom-Json
}

function Invoke-Umc22CaptureApply {
    Get-Umc22WasapiType
    $json = [Umc22.WasapiCapture]::Apply($script:NameContains)
    $json | ConvertFrom-Json
}

function Get-Umc22Probe {
    $nodes = @(Get-Umc22PnpNodes | ForEach-Object { Get-Umc22NodeProps $_.InstanceId })
    $audio = $nodes | Where-Object { $_.InstanceId -match 'MI_00' } | Select-Object -First 1
    [pscustomobject]@{
        VidPid         = $script:VidPid
        Present        = ($nodes.Count -gt 0)
        UsbNodes       = $nodes
        Services       = @(Get-Umc22ServiceState @('usbaudio', 'BEHRINGER_2902', 'BUSB_AUDIO_WDM'))
        AudioService   = if ($audio) { $audio.Service } else { $null }
        DriverCompare  = @(Get-Umc22DriverCompare)
        Capture        = Get-Umc22CaptureWasapi
        FlexAsio       = Get-Umc22FlexAsioLeftover
    }
}

function Write-Umc22Probe {
    param($Probe)
    Write-Host 'UMC22 probe'
    Write-Host ("VID/PID: {0}  present: {1}" -f $Probe.VidPid, $Probe.Present)
    Write-Host ''
    Write-Host 'USB nodes'
    foreach ($n in $Probe.UsbNodes) {
        Write-Host ("  [{0}] {1}  {2}" -f $n.Status, $n.Class, $n.FriendlyName)
        Write-Host ("      {0}" -f $n.InstanceId)
        Write-Host ("      inf={0}  svc={1}  provider={2}  ver={3}" -f $n.DriverInfPath, $n.Service, $n.DriverProvider, $n.DriverVersion)
        Write-Host ("      desc={0}  match={1}" -f $n.DriverDesc, $n.MatchingDeviceId)
    }
    Write-Host ''
    Write-Host 'Kernel services'
    foreach ($s in $Probe.Services) {
        if ($s.Present) {
            Write-Host ("  {0}: {1} (start {2}) {3}" -f $s.Name, $s.State, $s.Start, $s.Path)
        } else {
            Write-Host ("  {0}: not installed" -f $s.Name)
        }
    }
    Write-Host ''
    Write-Host 'Desktop package vs Driver Store (Ploytec leftover; not rebound)'
    foreach ($c in $Probe.DriverCompare) {
        $flag = if ($c.HashesMatch) { 'MATCH' } elseif (-not $c.DesktopHash) { 'no desktop file' } elseif (-not $c.StoreHash) { 'not in store' } else { 'DIFFER' }
        Write-Host ("  {0}: {1}" -f $c.File, $flag)
        if ($c.DesktopHash) { Write-Host ("      desktop {0}" -f $c.DesktopHash) }
        if ($c.StoreHash) { Write-Host ("      store   {0}" -f $c.StoreHash) }
    }
    Write-Host ''
    Write-Host 'Capture WASAPI (Mikrofon USB Audio CODEC)'
    if (-not $Probe.Capture.devices -or $Probe.Capture.devices.Count -eq 0) {
        Write-Host '  no matching capture endpoint'
    } else {
        foreach ($d in @($Probe.Capture.devices)) {
            Write-Host ("  {0}" -f $d.name)
            Write-Host ("      id {0}" -f $d.id)
            if ($d.mix) {
                Write-Host ("      mix    {0} Hz / {1}-bit / {2} ch" -f $d.mix.rate, $d.mix.bits, $d.mix.channels)
            }
            if ($d.stored) {
                Write-Host ("      stored {0} Hz / {1}-bit / {2} ch" -f $d.stored.rate, $d.stored.bits, $d.stored.channels)
            }
            Write-Host ("      period default {0} ms  min {1} ms" -f $d.defaultPeriodMs, $d.minPeriodMs)
            Write-Host ("      exclusive 48k/16 stereo={0} mono={1}  shared 48k/16 stereo={2}" -f $d.exclusive48k16Stereo, $d.exclusive48k16Mono, $d.shared48k16Stereo)
            $fx = if ($null -eq $d.sysFxDisabled) { 'unknown' } elseif ($d.sysFxDisabled) { 'disabled (good)' } else { 'ENABLED (Windows APO in path)' }
            Write-Host ("      enhancements/APO: {0}" -f $fx)
            if ($d.wasapiError) { Write-Host ("      wasapi error {0}" -f $d.wasapiError) }
        }
    }
    Write-Host ''
    Write-Host 'FlexASIO leftover (not modified)'
    Write-Host ("  present={0}  {1}" -f $Probe.FlexAsio.Present, $Probe.FlexAsio.Note)
}

function Test-Umc22Alive {
    $p = Get-Umc22Probe
    $usb = $p.Present
    $svc = $p.Services | Where-Object { $_.Name -eq 'usbaudio' } | Select-Object -First 1
    $usbaudio = $svc -and $svc.Present -and $svc.State -eq 'Running'
    $cap = $p.Capture.devices -and (@($p.Capture.devices).Count -gt 0)
    $audioBound = $p.AudioService -eq 'usbaudio'
    [pscustomobject]@{
        Probe           = $p
        UsbPresent      = $usb
        UsbaudioRunning = [bool]$usbaudio
        AudioBound      = [bool]$audioBound
        CapturePresent  = [bool]$cap
        Passed          = [bool]($usb -and $usbaudio -and $cap)
    }
}

Export-ModuleMember -Function @(
    'Get-Umc22Probe',
    'Write-Umc22Probe',
    'Invoke-Umc22CaptureApply',
    'Test-Umc22Alive',
    'Get-Umc22PnpNodes'
)

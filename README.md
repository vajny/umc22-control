# umc22-control

GUI pro Behringer U-Phoria UMC22 na Windows 11. Karta je TI PCM2902 (`VID_08BB&PID_2902`) a Windows ji bere jako **USB Audio CODEC** přes `usbaudio.sys`.

Ovládá Windows cestu mikrofonu **před Sonarem**: hlasitost, mute, sample rate, vypnutí vylepšení. Gain / PAD / 48V jsou knoby na krabici — USB to neumí.

## Spuštění

```powershell
.\run.ps1
```

nebo `dotnet run --project src\Umc22Control\Umc22Control.csproj -c Release`

## Co appka umí

- hlasitost a mute (Windows mixer na capture endpointu)
- vstupní metr
- 44.1 / 48 kHz, 16-bit, 1 nebo 2 kanály
- vypnout Windows audio enhancements

## Co tohle není

- kernel driver / Ploytec 2.8.40
- analogový gain, PAD, phantom
- Sonar routing


# umc22-control

Userspace nástroje pro Behringer U-Phoria UMC22 na Windows 11. Karta je TI PCM2902 (`VID_08BB&PID_2902`) a Windows ji bere jako **USB Audio CODEC** přes inbox `usbaudio.sys`.

Primární použití: mikrofon, cesta **před** Sonarem.

## Co tohle není

- Žádný kernel driver a žádný Ploytec 2.8.40 (leftover, nenavazovat).
- Žádné ovládání analogového gainu / PAD / 48V — to jsou knoby na krabici.
- Žádný Sonar routing.

## Použití

```powershell
# co Windows teď bere (driver, WASAPI capture, APO)
.\probe.ps1

# 48 kHz / 16-bit na mic endpointu + vypnout enhancements
.\apply.ps1

# self-check: karta + usbaudio + capture endpoint
.\check.ps1
```

`apply.ps1` mění Windows audio policy u capture endpointu. Když to Windows odmítne, spusť PowerShell jako správce.

## Strop hardwaru

PCM2902 ADC: 16 bit, max 48 kHz. Softwarově jde nastavit formát a vypnout APO. Analogový hiss preampu tím nespravíš.

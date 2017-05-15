Data Logger
==============

Hvordan er formatet?
-------------------

Dataene I datalogger er skævet I følgende format.
`[tid brugt], [data type], [data værdi]`

I nogle data typer som f.eks. Accelerometer og Gyroskop vil der kunne findes 3 værdier for hver type, i så fald vil formatet se ligeledes ud.
`[tid brugt], [data type], [data værdi X], [data værdi Y], [data værdi Z]`

Data type er en hex kode som angiver hvilke data vi trækker ud, disse hex koder er de samme som bilens databus forstår, så denne dump er mere eller mindre en direkte dump af busdata.

|Data Type	|Data Beskrivelse	|
|--------------|---					|
|0x104|Motor stress|
|0x105|Motor køler temperatur|
|0x10a|Tank tryk|
|0x10b|Køler indtag tryk|
|0x10c|Motor RPM|
|0x10d|Hastighed|
|0x10e|Timing forskydning|
|0x10f|Indtag luft temperatur|
|0x111|Speeder position (i procent)|
|0x11f|Tid som motoren har kørt|
|0x12f|Benzin niveau|
|0x15c|Motor olie temperatur|
|0x11|UTC Dato (DDMMYY)|
|0x10|UTC Tid (HHMMSSmm)|
|0xA|Latitude|
|0xB|Longitude|
|0xC|GPS Højde|
|0x20|Accelerometer data (x/y/z)|
|0x21|Gyroskop data (x/y/z)|
|0x30|Km i bilens trip måler|

Så et eksempel af log data vil se ud som efterfølgende.
```markdown
174,10C,1095	
14,10D,0
21,111,17
3,104,17
39,105,58
2,20,44,6,-80
0,21,-1,0,1
161,10C,1089
13,10D,0
20,111,17
2,104,17
42,10F,37
4,20,39,3,-80
0,21,-1,1,-2
```

https://stackedit.io
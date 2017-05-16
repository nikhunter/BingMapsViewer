Project Voyage
==============


Hvad er det?
-------------------

Vi har købt en OBD-II læser/logger, den består af en Arduino MEGA, et Telematics 3.5" TFT LCD touch display, et OBD-II adapter stik designet til Arduino boards og en GPS(valgfrit).
 
 ***(Fremover skriver vi OBD istedet for OBD-II)***
 
Måden det fungerer på er at boardet får strøm fra et 12v stik som sidder i OBD stikket. 

 - Trin 1: Boardet starter med tjekke om den kan læse fra OBD.
 - Trin 2: Boardet tjekker om der er en GPS tilgængelig.
 - Trin 3: Boardet finder alle de data typer som er tilgængelige. 
*(Listen af data typer som den leder efter er specificeret i sketchen)*

Hvis trin 1 og 3 lykkes skifter den til et nyt display hvor den løbende opdaterer informationer som den læser, såsom hastighed, RPM, motor stress, osv.

Hvad er projektet?
-------------------

Vores projekt gik ud på at bygge vores egen sketch til denne enhed men da den ene af vores Arduino ikke længere gad læse fra OBD stikket turde vi ikke ødelægge den anden.

Vores plan var at lave en sketch som ville tegne en analog RPM skive som man ser i instrument panelet på en bil, derefter ville vi skrive vores egen logger så vi kunne styre formatet som det skulle læses i. Da den ene Arduino ikke længere gad læse droppede vi dette og begyndte at decode den CSV log format som den medfulgte sketch allerede lavede.

Hvordan er formatet?
-------------------

Dataene I datalogger er skævet I følgende format.

`[tid brugt], [data type], [data værdi]`

I nogle data typer som f.eks. Accelerometer og Gyroskop vil der kunne findes 3 værdier for hver type, i så fald vil formatet se ligeledes ud.

`[tid brugt], [data type], [data værdi X], [data værdi Y], [data værdi Z]`

Data type er en hex kode som angiver hvilke data vi trækker ud, disse hex koder er de samme som bilens databus forstår, så denne dump er mere eller mindre en direkte dump af busdata.

Standard data typer

|Data Type|Data Beskrivelse|
|:---:|:---|
|0x104|Motor stress|
|0x105|Motor køler temperatur|
|0x10A|Tank tryk|
|0x10B|Køler indtag tryk|
|0x10C|Motor RPM|
|0x10D|Hastighed|
|0x10E|Timing forskydning|
|0x10F|Indtag luft temperatur|
|0x111|Speeder position (i procent)|
|0x11F|Tid som motoren har kørt|
|0x12F|Benzin niveau|
|0x15C|Motor olie temperatur|

Brugerdefineret data typer

|Data Type|Data Beskrivelse|
|:---:|:---|
|0x11|UTC Dato (DDMMYY)|
|0x10|UTC Tid (HHMMSSmm)|
|0xA|Latitude|
|0xB|Longitude|
|0xC|GPS Højde|
|0x20|Accelerometer data (x/y/z)|
|0x21|Gyroskop data (x/y/z)|
|0x30|Km i bilens trip måler|

Så et eksempel af log data vil se ud som efterfølgende.

```csv
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

Som vi kan se i eksemplet er linje 1’s data type `0x10C` dvs. at 1095 er motoren RPM og det tog 174 ms at få værdien.

På linje 6 kan vi også se at data typen er `0x20` dvs. accelerometer, så den har 3 værdier, X,Y og Z.

Hvordan kommer data ind i programmet?
------------------------------------

Som vi kan se i vores eksempel fra før, starter filen med data typen `0x10C`.

Loggeren vil **altid** starte med at logge RPM så vi kan gå ud fra at den har logget alle tilgængelige data når vi ser en linje med koden `0x10C`

Når programmet får en fil vil den starte med at tjekke om filen er valid ved at tjekke om den starter med værdien for RPM

Efter dette vil den starte en while der vil hente data fra de efterfølgende linjer, indtil RPM bliver registreret igen

Til slut vil `importData()` tilføje et Datapoint til PushPinCollection.

PushPinCollection er et ObservableCollection som er blevet binded til vores kort, det har den side effekt at når vi tilføjer et Datapoint vil der automatisk blive tilføjet et pin til kortet

Datapoint er en klasse der indeholder følgende

```CS
public class Datapoint {
    public Location Location { get; set; }
    public string Date { get; set; }
    public string Time { get; set; }
    public int Speed { get; set; }
    public int Rpm { get; set; }

    public Datapoint(Location location, string date, string time, int speed, int rpm) {
        Location = location;
        Date = date;
        Time = time;
        Speed = speed;
        Rpm = rpm;
    }
}
```

Bilag:
------

I brug:

![img 1](http://cdn.wejlgaard.com/3.jpg)

Diagram:

![img 2](http://cdn.wejlgaard.com/diagram.png)

Inputs:

![img 3](http://cdn.wejlgaard.com/sockets.jpg)



Krav:
-----
Bing SDK: https://msdn.microsoft.com/da-dk/library/hh750210.aspx

# 📌 Pinventory

Kleine Lagerverwaltung für Elektronik-Bauteile – Blazor Web-App (.NET 10) mit [MudBlazor](https://mudblazor.com) und SQLite.

## Funktionen

- **Schnellerfassung** zum Einlagern am Stück – komplett per Tastatur (siehe unten)
- **Bauformen** als Auswahlliste – ca. 125 Standard-Gehäuse (0201–2512, SOT, SOIC, QFN, DIP, TO, Axial …) sind vorab angelegt und unter *Bauformen* erweiterbar
- **Kategorie-Eigenschaften** wie Vorwärtsspannung bei Dioden oder R_DS(on) bei MOSFETs – erscheinen automatisch, sobald die Kategorie gewählt ist (siehe unten)
- **Bauteile** mit Name, Wert, Bauform, Hersteller, Teilenummer, Menge, Mindestbestand, Datenblatt-Link und Notizen
- **Kategorien** und **Lagerorte** beliebig verschachtelt (z.B. *Regal A › Sortimentskasten 2 › Fach 14*)
- **Schnellsuche** über alle Felder – mehrere Wörter werden kombiniert (`10k 0805`)
- Filter nach Kategorie / Lagerort (inkl. Unterebenen) und „nur nachbestellen“
- Menge direkt in der Liste per **−/+** anpassen
- „Speichern & nächstes“ und „Duplizieren“ zum schnellen Erfassen ganzer Sortimente
- Übersicht mit Kennzahlen, Nachbestell-Liste und zuletzt geänderten Bauteilen
- Helles und dunkles Design (startet passend zum Betriebssystem)

## Schnellerfassung

Unter **Schnellerfassung** (oder „Einlagern“ oben rechts) lassen sich viele Bauteile ohne Maus erfassen:

1. **Kategorie** und **Lagerort** oben einmal wählen – tippen filtert (`halb trans` → *Halbleiter › Transistoren*), `Tab` übernimmt.
2. Zeile ausfüllen: `Tab` springt von Feld zu Feld, `Enter` lagert ein – aus jedem Feld heraus.
3. Danach ist die Zeile leer und der Cursor wieder vorne. `Esc` leert die Zeile ohne zu speichern.

**Felder behalten (📌):** Angepinnte Felder bleiben nach dem Einlagern stehen, der Cursor springt ins erste freie Feld.
Beispiel Widerstandsserie: *Name*, *Bauform* und *Menge* anpinnen → nur noch `10k` `Enter`, `4k7` `Enter`, `1k` `Enter` …

**Bauform und Auswahl-Eigenschaften** per Tastatur: tippen (`sot`, `to-220`, `n`) und mit `Tab` übernehmen.

**Keine Dubletten:** Gibt es ein Bauteil mit gleichem Namen, Wert und Bauform am selben Lagerort schon, wird nur die Menge erhöht.
Jeder Eintrag der Sitzung lässt sich unten mit ↶ rückgängig machen.

Tipp: Das ⚡-Symbol bei einem Lagerort oder der Button in der gefilterten Bauteilliste öffnet die Schnellerfassung mit passender Vorbelegung.

## Kategorie-Eigenschaften

Unter **Kategorien** → Symbol *Regler* (🎚) legt man fest, welche Eigenschaften eine Kategorie hat:

- **Zahl** mit Einheit (z.B. *U_F* in V) – Eingabe mit Komma oder Punkt, in der Bauteilliste numerisch sortierbar
- **Auswahl** aus festen Möglichkeiten (z.B. *N-Kanal; P-Kanal*) – tippen filtert, `Tab` übernimmt
- **Text** für alles andere

Eigenschaften werden an Unterkategorien vererbt. Für gängige Kategorien (Widerstände, Kondensatoren, Dioden,
Transistoren, MOSFETs, LEDs, Spannungsregler …) sind Startwerte angelegt.
Filtert man die Bauteilliste nach einer Kategorie, werden ihre Eigenschaften als Spalten angezeigt.

## Starten

```powershell
dotnet run --project src/Pinventory --launch-profile http
```

Dann im Browser <http://localhost:5285> öffnen.

**Vom Handy / anderen Geräten im Netz** erreichbar machen:

```powershell
dotnet run --project src/Pinventory --urls http://0.0.0.0:5285
```

(Port 5285 muss dafür in der Windows-Firewall freigegeben sein.)

## Daten

Die Datenbank liegt in `src/Pinventory/pinventory.db`. Zum Sichern einfach diese Datei kopieren.
Sie wird beim ersten Start automatisch angelegt und mit Startkategorien befüllt.

## Datenmodell ändern

Nach Änderungen an `Data/Models.cs` eine Migration erzeugen – sie wird beim nächsten Start automatisch angewendet:

```powershell
dotnet tool restore
dotnet ef migrations add <Name> --project src/Pinventory --output-dir Data/Migrations
```

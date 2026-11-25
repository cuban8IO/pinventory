# 📌 Pinventory

Kleine Lagerverwaltung für Elektronik-Bauteile – Blazor Web-App (.NET 10) mit SQLite.

## Funktionen

- **Bauteile** mit Name, Wert, Bauform, Hersteller, Teilenummer, Menge, Mindestbestand, Datenblatt-Link und Notizen
- **Kategorien** und **Lagerorte** beliebig verschachtelt (z.B. *Regal A › Sortimentskasten 2 › Fach 14*)
- **Schnellsuche** über alle Felder – mehrere Wörter werden kombiniert (`10k 0805`)
- Filter nach Kategorie / Lagerort (inkl. Unterebenen) und „nur nachbestellen“
- Menge direkt in der Liste per **−/+** anpassen
- „Speichern & nächstes“ und „Duplizieren“ zum schnellen Erfassen ganzer Sortimente
- Übersicht mit Kennzahlen, Nachbestell-Liste und zuletzt geänderten Bauteilen

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

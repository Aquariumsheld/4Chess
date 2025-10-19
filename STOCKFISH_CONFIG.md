# Stockfish AI Konfiguration

Diese Datei erklärt, wie du die Stockfish AI-Einstellungen anpassen kannst.

## Konfigurationsdatei

**Datei:** `4Chess/ChessAi/StockfishAi.cs`
**Methode:** `ConfigureDifficulty()` (Zeile ~43)

## Verfügbare Parameter

Jeder Schwierigkeitsgrad hat 5 konfigurierbare Parameter:

### 1. `_depth` - Suchtiefe
- **Was ist das?** Wie viele Züge im Voraus Stockfish berechnet
- **Werte:** 1-100 (realistisch: 5-30)
- **Auswirkung:**
  - Höher = Stärkere Züge, aber langsamer
  - Niedriger = Schneller, aber schwächer
- **Beispiel:** `_depth = 15;`

### 2. `_skillLevel` - ELO-Stärke
- **Was ist das?** Stockfish's Spielstärke (künstliche Abschwächung)
- **Werte:** 0-20
  - 0 = ~800 ELO (Anfänger)
  - 10 = ~1500 ELO (Fortgeschritten)
  - 20 = ~3500 ELO (Weltklasse)
- **Auswirkung:** Bei niedrigeren Werten macht Stockfish absichtlich Fehler
- **Beispiel:** `_skillLevel = 10;`

### 3. `_multiPV` - Anzahl der Varianten
- **Was ist das?** Wie viele beste Züge gleichzeitig berechnet werden
- **Werte:** 1-500 (realistisch: 1-10)
- **Auswirkung:**
  - 1 = Nur bester Zug (schnell)
  - 3-5 = Mehrere Alternativen für Ultrathink
  - 10+ = Sehr langsam, viele Varianten
- **Beispiel:** `_multiPV = 3;`

### 4. `_moveTimeMs` - Zeitlimit pro Zug
- **Was ist das?** Maximale Bedenkzeit in Millisekunden
- **Werte:**
  - 0 = Unbegrenzt (nur Depth zählt)
  - 1000-60000 = 1-60 Sekunden
- **Auswirkung:** Stockfish stoppt nach dieser Zeit, auch wenn Depth nicht erreicht
- **Beispiel:** `_moveTimeMs = 5000;` (5 Sekunden)

### 5. `ShowThinking` - Ultrathink-Visualisierung
- **Was ist das?** Zeigt die Denkprozesse von Stockfish an
- **Werte:** `true` oder `false`
- **Auswirkung:** Bei `true` werden die besten Züge rechts neben dem Brett angezeigt
- **Beispiel:** `ShowThinking = true;`

## Standard-Einstellungen

```csharp
case ChessAiDifficulty.Low:
    _depth = 5;              // Sehr flache Suche
    _skillLevel = 5;         // Schwache Züge
    _multiPV = 1;            // Nur bester Zug
    _moveTimeMs = 1000;      // 1 Sekunde
    ShowThinking = false;    // Kein Ultrathink
    break;

case ChessAiDifficulty.Medium:
    _depth = 10;             // Mittlere Suche
    _skillLevel = 10;        // Durchschnittliche Züge
    _multiPV = 1;            // Nur bester Zug
    _moveTimeMs = 3000;      // 3 Sekunden
    ShowThinking = false;    // Kein Ultrathink
    break;

case ChessAiDifficulty.High:
    _depth = 15;             // Tiefe Suche
    _skillLevel = 15;        // Starke Züge
    _multiPV = 1;            // Nur bester Zug
    _moveTimeMs = 5000;      // 5 Sekunden
    ShowThinking = false;    // Kein Ultrathink
    break;

case ChessAiDifficulty.Ultra:
    _depth = 20;             // Sehr tiefe Suche
    _skillLevel = 20;        // Maximale Stärke
    _multiPV = 3;            // 3 Varianten
    _moveTimeMs = 10000;     // 10 Sekunden
    ShowThinking = true;     // Ultrathink aktiv
    break;

case ChessAiDifficulty.UltraPlus:
    _depth = 25;             // Extrem tiefe Suche
    _skillLevel = 20;        // Maximale Stärke
    _multiPV = 5;            // 5 Varianten
    _moveTimeMs = 15000;     // 15 Sekunden
    ShowThinking = true;     // Ultrathink aktiv
    break;

case ChessAiDifficulty.Overthinker:
    _depth = 60;             // Wahnsinnig tiefe Suche
    _skillLevel = 20;        // Maximale Stärke
    _multiPV = 10;           // 10 Varianten
    _moveTimeMs = 30000;     // 30 Sekunden
    ShowThinking = true;     // Ultrathink aktiv
    break;
```

## Beispiel: Eigenen Schwierigkeitsgrad erstellen

### 1. Füge einen neuen Enum-Wert hinzu

**Datei:** `ChessAiHelper.cs`
```csharp
public enum ChessAiDifficulty
{
    Low,
    Medium,
    High,
    Ultra,
    UltraPlus,
    Overthinker,
    Blitz  // NEU!
}
```

### 2. Füge die Konfiguration hinzu

**Datei:** `StockfishAi.cs` → `ConfigureDifficulty()`
```csharp
case ChessAiDifficulty.Blitz:
    _depth = 8;              // Schnelle Suche
    _skillLevel = 15;        // Trotzdem stark
    _multiPV = 1;            // Nur bester Zug
    _moveTimeMs = 500;       // Nur 0.5 Sekunden!
    ShowThinking = false;    // Kein Ultrathink
    break;
```

### 3. Füge einen UI-Button hinzu

**Datei:** `_4ChessGame.cs` → `GameInit()` (bei den Difficulty-Buttons)
```csharp
UIComponents.Add("DifficultyBlitzBtn", new BIERButton(" Blitz ", WINDOW_WIDTH / 2 + 300, WINDOW_HEIGHT / 2 - 50, 150, 80, WHITE, SKYBLUE, null, 2, true)
{
    ClickEvent = () =>
    {
        ChessAi = new ChessAi.ChessAi(1, 6); // 6 = Blitz Index
        AiMode = true;
        HideDifficultyButtons();
    }
});
```

## Performance-Tipps

### Schnellere Züge
- Reduziere `_depth` (z.B. 5-10)
- Reduziere `_moveTimeMs` (z.B. 1000-3000)
- Setze `_multiPV = 1`

### Stärkere Züge
- Erhöhe `_depth` (z.B. 20-30)
- Erhöhe `_skillLevel = 20`
- Erhöhe `_moveTimeMs` (z.B. 10000+)

### Balance zwischen Speed & Stärke
- Nutze `_moveTimeMs` als Limit, aber hohe `_depth`
- Beispiel: `_depth = 30, _moveTimeMs = 5000`
  - Stockfish sucht so tief wie möglich in 5 Sekunden
  - Stoppt nach 5 Sekunden, auch wenn Depth 30 nicht erreicht wurde

## Wie funktioniert das Zeitlimit?

Der UCI-Befehl kombiniert beide Parameter:
```
go depth 20 movetime 5000
```

Das bedeutet:
- **Suche bis Depth 20** ODER
- **Stoppe nach 5000ms (5 Sekunden)**
- **Was auch immer zuerst erreicht wird**

In schwierigen Positionen erreicht Stockfish vielleicht nur Depth 12 in 5 Sekunden.
In einfachen Positionen erreicht es Depth 20 in 2 Sekunden und stoppt dann.

## Technische Details: UCI-Optionen

Die Einstellungen werden **VOR JEDEM ZUG** an Stockfish gesendet (in `GetBestMoveAsync()`):

### Niedrige Schwierigkeitsgrade (Low, Medium, High)
```csharp
setoption name Skill Level value 15
setoption name MultiPV value 1
isready
// Warte auf "readyok"
position fen [FEN-String]
go depth 15 movetime 15000  // BEIDE Parameter: Depth UND Zeit
```

### Hohe Schwierigkeitsgrade (Ultra, UltraPlus, Overthinker)
```csharp
setoption name Skill Level value 20
setoption name MultiPV value 10
isready
// Warte auf "readyok"
position fen [FEN-String]
go movetime 30000  // NUR Zeit, KEIN Depth-Limit!
```

**Warum nur Zeitlimit bei hohen Schwierigkeitsgraden?**
- Bei niedrigen Depths (z.B. 20) erreicht Stockfish das Limit oft in 1-2 Sekunden und stoppt dann
- Ohne Depth-Limit nutzt Stockfish die **volle Zeit** (z.B. 30 Sekunden) und erreicht automatisch Depth 30-50+
- Das macht die KI deutlich stärker!

Dies stellt sicher, dass:
1. Die Schwierigkeitseinstellungen korrekt angewendet werden
2. MultiPV (für Ultrathink) immer aktuell ist
3. Jeder Zug mit den richtigen Parametern berechnet wird
4. Hohe Schwierigkeitsgrade die volle Bedenkzeit nutzen

Die Log-Datei zeigt vor jedem Zug:
```
INFO: Stockfish calculating - Difficulty: Overthinker, Depth: 60, Skill: 20, MultiPV: 10, TimeLimit: 30000ms
```

Und nach jedem Zug:
```
INFO: Stockfish finished - Elapsed: 15.23s, Max Depth: 35/60, Target Time: 30000ms
```
Hier sieht man, dass Stockfish Depth 35 in 15 Sekunden erreicht hat (ohne Depth-Limit wäre es weiter gegangen bis 30s).

## Troubleshooting

**Problem:** AI ist zu langsam
- **Lösung:** Reduziere `_moveTimeMs` und/oder `_depth`

**Problem:** AI ist zu schwach
- **Lösung:** Erhöhe `_skillLevel` auf 20 und `_depth` auf mindestens 15

**Problem:** Ultrathink zeigt nichts an
- **Lösung:** Stelle sicher dass `_multiPV > 1` und `ShowThinking = true`

**Problem:** AI denkt unendlich lange
- **Lösung:** Setze `_moveTimeMs` auf einen Wert > 0 (z.B. 10000)

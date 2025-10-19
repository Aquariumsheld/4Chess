# Stockfish AI Integration

Diese Dokumentation beschreibt die vollständige Integration von Stockfish 17.1 in das 4Chess-Spiel.

## Installation

### Stockfish herunterladen

1. Lade Stockfish 17.1 herunter von: https://stockfishchess.org/download/
2. Wähle die Version für dein Betriebssystem:
   - **Windows**: `stockfish-windows-x86-64-avx2.zip`
   - **Linux**: `stockfish-ubuntu-x86-64-avx2.tar`
   - **macOS**: `stockfish-macos-m1-apple-silicon.tar`

### Stockfish platzieren

Die Engine wird automatisch an folgenden Orten gesucht (in dieser Reihenfolge):

1. Im aktuellen Verzeichnis: `stockfish.exe` oder `stockfish_17.1.exe`
2. Im Verzeichnis der ausführbaren Datei: `<GameDir>/stockfish.exe`
3. **Im `res/engine` Unterverzeichnis: `<GameDir>/res/engine/stockfish.exe`** ⭐ **EMPFOHLEN**
4. Im `res` Unterverzeichnis: `<GameDir>/res/stockfish.exe`
5. In den Systemvariablen (PATH)
6. Im Standard-Installationsverzeichnis: `C:\Program Files\Stockfish\stockfish.exe`

**Empfohlen**: Platziere `stockfish.exe` im `4Chess/Res/engine/` Ordner. Dieser wird beim Build automatisch ins Output-Verzeichnis kopiert.

## Features

### 1. UCI-Kommunikation

Die Integration verwendet das Universal Chess Interface (UCI) Protokoll:
- Vollständige Unterstützung für alle UCI-Befehle
- FEN-Notation für Board-State-Übertragung
- UCI-Notation für Züge (z.B. "e2e4", "e7e8q" für Bauernumwandlung)

### 2. Schwierigkeitsgrade

Die AI bietet 6 verschiedene Schwierigkeitsgrade:

| Stufe | Tiefe | Skill Level | Multi-PV | Beschreibung |
|-------|-------|-------------|----------|--------------|
| **Low** | 5 | 5 | 1 | Anfänger - Macht absichtlich Fehler |
| **Medium** | 10 | 10 | 1 | Fortgeschritten - Gute Balance |
| **High** | 15 | 15 | 1 | Schwer - Starke Züge |
| **Ultra** | 20 | 20 | 3 | Sehr schwer - Mit Ultrathink |
| **UltraPlus** | 25 | 20 | 5 | Extrem schwer - Mehr Ultrathink |
| **Overthinker** | 30 | 20 | 10 | Maximum - Vollständiges Ultrathink |

#### Parameter-Erklärung:

- **Tiefe**: Wie viele Züge im Voraus Stockfish berechnet (höher = stärker, aber langsamer)
- **Skill Level**: Stockfish ELO-Anpassung (0-20, wobei 20 = maximale Stärke)
- **Multi-PV**: Anzahl der besten Züge, die gleichzeitig berechnet werden (für Ultrathink-Visualisierung)

### 3. Ultrathink-Modus

Ab Schwierigkeitsgrad **Ultra** wird der Ultrathink-Modus aktiviert:

#### Was wird angezeigt:

```
Stockfish Thinking:
#1 [20] +0.85: e2e4 e7e5 g1f3 b8c6 f1b5
#2 [20] +0.42: d2d4 d7d5 c2c4 e7e6 b1c3
#3 [20] +0.21: g1f3 d7d5 d2d4 g8f6 c2c4
```

- **#1, #2, #3**: Ranking der besten Züge
- **[20]**: Suchtiefe (wie viele Züge im Voraus berechnet)
- **+0.85**: Bewertung in Bauern-Einheiten (positiv = Vorteil für Weiß, negativ = Vorteil für Schwarz)
- **e2e4...**: Die beste Zugfolge (Principal Variation)

#### Farb-Kodierung:

- **Grün (#1)**: Der beste Zug
- **Gelb (#2)**: Zweitbester Zug
- **Orange (#3)**: Drittbester Zug
- **Rot (#4+)**: Weitere Alternativen

#### Matt-Anzeige:

Wenn Stockfish ein Matt sieht, wird statt der Bewertung angezeigt:
```
#1 [20] Matt in 3: d1h5 g7g6 h5e5
```

### 4. Spezielle Züge

Die AI unterstützt alle Schachregeln:

#### Bauernumwandlung
- Automatische Umwandlung in Dame, Turm, Läufer oder Springer
- UCI-Notation: `e7e8q` (Bauer wird zur Dame)

#### Rochade
- Kingside (kurze Rochade): `e1g1` oder `e8g8`
- Queenside (lange Rochade): `e1c1` oder `e8c8`
- Automatische Überprüfung der Rochade-Rechte

#### En Passant
- Wird automatisch erkannt und ausgeführt
- UCI-Notation: Normaler diagonaler Bauernzug

### 5. Spiel-Integration

#### Im Menü:

1. Klicke auf "Play against AI"
2. Klicke auf "Play against Stockfish"
3. Wähle einen Schwierigkeitsgrad:
   - **Easy**: Für Anfänger
   - **Medium**: Für Fortgeschrittene
   - **Hard**: Für erfahrene Spieler
   - **Ultra (Ultrathink)**: Für Profis + Denkprozess-Visualisierung

#### Während des Spiels:

- Du spielst als **Weiß** (unten)
- Stockfish spielt als **Schwarz** (oben)
- Nach deinem Zug denkt Stockfish automatisch
- Bei Ultrathink siehst du die Denkprozesse rechts neben dem Brett

## Architektur

### Datei-Struktur

```
4Chess/ChessAi/
├── ChessAi.cs           # AI-Manager
├── ChessAiHelper.cs     # FEN-Konvertierung & UCI-Utilities
├── StockfishAi.cs       # Stockfish-Integration & UCI-Kommunikation
└── CustomAi.cs          # Eigene AI (noch nicht implementiert)
```

### Klassen-Übersicht

#### `StockfishAi`
- Startet und verwaltet den Stockfish-Prozess
- UCI-Kommunikation (Befehle senden/empfangen)
- Schwierigkeitsgrad-Konfiguration
- Multi-PV Parsing für Ultrathink
- Automatische Zug-Ausführung

#### `ChessAiHelper`
- FEN-Notation Generierung aus Board-State
- UCI-Move Parsing und Konvertierung
- Rochade-Rechte Erkennung
- En Passant Feld-Berechnung

#### `StockfischThinkingLine`
- Speichert eine Denkzeile (PV-Line)
- Formatierung der Bewertung
- Anzeige der Zugfolge

### UCI-Kommunikationsfluss

```
1. Initialisierung:
   >> uci
   << uciok
   >> setoption name Skill Level value 20
   >> setoption name MultiPV value 3
   >> isready
   << readyok

2. Zug-Berechnung:
   >> position fen rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
   >> go depth 20
   << info depth 1 score cp 50 pv e2e4
   << info depth 2 score cp 45 pv e2e4 e7e5
   ...
   << bestmove e2e4

3. Beenden:
   >> quit
```

## Debugging

### Konsolen-Output

Alle UCI-Nachrichten werden in der Konsole ausgegeben:
- `>>` = An Stockfish gesendete Befehle
- `<<` = Von Stockfish empfangene Antworten

### Häufige Probleme

**Problem**: "Stockfish executable not found!"
- **Lösung**: Platziere `stockfish.exe` im Spielverzeichnis oder füge es zu PATH hinzu

**Problem**: AI macht keinen Zug
- **Lösung**: Überprüfe die Konsole auf Fehler, stelle sicher dass Stockfish läuft

**Problem**: Build-Fehler
- **Lösung**: Stelle sicher, dass alle Dependencies installiert sind: `dotnet restore`

## Performance-Tipps

- **Low/Medium**: Sofortige Antwort (< 1 Sekunde)
- **High**: Schnelle Antwort (1-3 Sekunden)
- **Ultra**: Moderate Wartezeit (3-10 Sekunden)
- **UltraPlus/Overthinker**: Längere Wartezeit (10-30+ Sekunden)

Für schnellere Züge: Reduziere die Tiefe in `ConfigureDifficulty()` (StockfishAi.cs:46)

## Erweiterungen

### Eigene Schwierigkeitsgrade hinzufügen

In `ChessAiHelper.cs`:
```csharp
public enum ChessAiDifficulty
{
    Low,
    Medium,
    High,
    Ultra,
    UltraPlus,
    Overthinker,
    Custom  // Neu hinzufügen
}
```

In `StockfishAi.cs` → `ConfigureDifficulty()`:
```csharp
case ChessAiDifficulty.Custom:
    _depth = 35;
    _skillLevel = 20;
    _multiPV = 15;
    ShowThinking = true;
    break;
```

### Zeit-basierte Suche

Anstatt Tiefe, kann Stockfish auch Zeit-basiert suchen:
```csharp
// Statt: SendCommand($"go depth {_depth}");
SendCommand($"go movetime 5000"); // 5 Sekunden Bedenkzeit
```

## Weitere Ressourcen

- **Stockfish Dokumentation**: https://github.com/official-stockfish/Stockfish
- **UCI Protokoll**: https://www.chessprogramming.org/UCI
- **FEN Notation**: https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

4Chess is a chess game written in C# targeting .NET 8.0 with AOT compilation support. The game uses the Raylib-CsLo library for rendering via a custom rendering framework called BIERKELLER.

**BIERKELLER** stands for "Brilliant Integrated Engine for Rendering Kernels with Extreme Logic Layer and Engine Routing" - a custom game framework built on top of Raylib-CsLo.

## Solution Structure

The solution contains three projects:

1. **4Chess** (main executable): The chess game implementation
   - Entry point: `Program.cs` creates a `_4ChessGame` instance and runs it
   - Depends on BIERKELLER project
   - Uses unsafe code blocks for performance-critical rendering operations

2. **BIERKELLER** (library): Custom game framework
   - Provides the core game loop via `BIERGame` abstract class
   - Rendering system (`BIERRenderer`, `BIERRenderObject` hierarchy)
   - UI components (`BIERButton`, `BIERLabel`, `BIERInput`)
   - Dependencies: Raylib-CsLo NuGet package

3. **ChessAi** (library): Chess AI implementation (currently under development)

## Build & Run Commands

```bash
# Build the solution
dotnet build 4Chess.sln

# Build in Release mode with AOT compilation
dotnet publish 4Chess/4Chess.csproj -c Release

# Run the game (Debug)
dotnet run --project 4Chess/4Chess.csproj

# Clean build artifacts
dotnet clean 4Chess.sln
```

## Key Architectural Patterns

### Game Loop Architecture

All games inherit from `BIERGame` (BIERKELLER/BIERGaming/BIERGame.cs) which provides:
- `GameInit()`: Initialize resources, load textures, setup game state
- `GameUpdate()`: Handle input and update game logic each frame
- `GameRender()`: Render game objects and UI
- `GameDispose()`: Clean up resources
- `Run()`: Main loop that orchestrates Init → Update/Render loop → Dispose

The `_4ChessGame` class implements this pattern and extends it with:
- `CustomPreRenderFuncs`: Actions executed before rendering objects (e.g., board rendering)
- `CustomPostRenderFuncs`: Actions executed after rendering objects (e.g., UI, dragged pieces)

### Rendering System

BIERKELLER uses a render object pattern:
- All renderable objects inherit from `BIERRenderObject`
- Concrete types: `BIERRenderRect`, `BIERRenderTexture`, `BIERRenderText`
- UI components (`BIERUIComponent`) contain lists of `BIERRenderObject` for their visuals
- `BIERRenderer.Render()` is called once per frame with a list of objects to draw

The game runs in fullscreen mode (toggled in `BIERRenderer.Init()`) at 1920x1080 with vsync enabled.

### Chess Game Logic

**Board Representation**: 8x8 2D list (`List<List<Piece?>>`) where `Board[y][x]` holds a piece or null
- Y-axis: rows (0=top/black, 7=bottom/white in standard orientation)
- X-axis: columns (0-7 from left to right)

**Piece System**: Abstract `Piece` base class with:
- `GetMoves(bool validate = true, bool rocharde = true)`: Returns legal moves as `Vector2` positions
- `ValidateMoves()`: Filters moves that would put own king in check
- Position tracked via `X`, `Y` properties
- Each piece references its parent `Game` instance

**Move Validation Flow**:
1. `Piece.GetMoves()` generates raw moves based on piece type
2. `ValidateMoves()` simulates each move and checks if it exposes king
3. Move is removed if opponent can capture king in resulting position
4. King position tracking: `_4ChessGame.WhiteKingPosition` / `BlackKingPosition`

**Input Handling** (_4ChessMove.cs):
- Drag-and-drop system: Click piece → drag → release on valid square
- Turn-based system enforced via `isWhiteTurn` flag
- Castling logic in `KingCasteling()`
- Pawn promotion triggers UI button selection (`SwitchPiece()`)
- Debug mode (`debugMoveMode`) allows moving any piece regardless of turn

### Multiplayer Architecture

WebSocket-based peer-to-peer multiplayer via `MultiplayerManager`:
- **Host mode**: Creates HTTP listener, upgrades to WebSocket, waits for client
- **Client mode**: Connects to host's IP address via WebSocket
- Board state serialization: `MoveCounter.SerializeBoard()` / `DeserializeBoard()`
- Turn management: `IsLocalTurn` flag controls when local player can move
- Messages queued via `ConcurrentQueue<string>` and processed in `GameUpdate()`

### AI Integration

Two AI implementations planned:
- **StockfishAi**: Integration with Stockfish engine
- **CustomAi**: Custom evaluation-based AI
- Difficulty levels: Low, Medium, High, Ultra, UltraPlus, Overthinker
- AI selected via menu buttons, stored in static properties on `_4ChessGame`

## Resource Management

**Texture Loading**:
- All piece textures loaded in `GameInit()` into `_pieceTextureDict`
- Black pieces have additional "SELECTED" variants (white silhouettes for drag visual)
- Textures resized to `TILE_SIZE` at load time
- Resources copied from `Res/img/` to output directory via MSBuild PreBuild target

**Font Loading**:
- Custom font: `res/font_romulus.png` (Raylib bitmap font format)
- Used for all UI text rendering

**Cleanup**:
- All textures unloaded in `GameDispose()`
- Fonts unloaded explicitly
- Window cleanup handled by `BIERRenderer.Dispose()`

## Debug Features

When debugger is attached (F-key shortcuts):
- **F1**: Toggle UI hitbox visualization (`_debugUiHitboxes`)
- **F2**: Toggle debug move mode (no turn restrictions)
- **F3**: Load test position (checkmate setup)

Game restart: Press ENTER, SPACE, or R when game ends (checkmate/stalemate)

## Important Implementation Details

**Coordinate System**:
- Board coordinates: `Board[row][column]` where row=Y, column=X
- Screen positions: Calculated as `BOARDXPos + x * TILE_SIZE`, `BOARDYPos + y * TILE_SIZE`
- Mouse to board conversion: `(mouseX - BOARDXPos) / TILE_SIZE`

**Turn Management**:
- Normal mode: Enforces white/black alternation via `isWhiteTurn`
- Multiplayer: `IsLocalTurn` controls when local player can interact
- Debug mode: Allows any piece to move regardless of color

**Castling**:
- Tracked via `King.IsUnmoved` and `Rook.IsUnmoved` properties
- King moves 2 squares, rook teleports to other side
- Animation state stored in `CastlingRook`, `CastlingRookStart`, `CastlingRookTarget`

**En Passant**:
- Pawn stores `IsEnPassant` flag when moving 2 squares
- Checked during diagonal pawn moves when no piece on target square
- Captured pawn removed from board (one rank above/below target)

**Pawn Promotion**:
- Triggered when `Pawn.IsAtEnd()` returns true (reached opposite end)
- UI buttons displayed to select Bishop, Rook, Queen, or Knight
- New piece replaces pawn at same position

## Development Branch Strategy

- Main branch for PRs: **DEV**
- Feature branches created off DEV
- Current branch: ChessAi (AI implementation work in progress)

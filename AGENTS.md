# Repository Guidelines

>This guide applies to the entire repository rooted here. It’s optimized for contributors and automation agents working on the .NET 8 Raylib-based game 4Chess with optional Stockfish AI.

## Project Structure & Module Organization
- `4Chess/` — main game project (`net8.0`, `WinExe`).
  - `Game/`, `Pieces/`, `ChessAi/` — core logic and AI glue.
  - `Res/img/` assets; `Res/engine/` external engines (e.g., `stockfish.exe`).
  - `4Chess.csproj` — copies `Res/**` to output via a PreBuild target.
- `BIERKELLER/` — local UI/render dependency referenced by the game.
- `ChessAi/` — auxiliary C# project (experiments/utilities).
- `4Chess.sln` — open this solution for multi-project work.

## Build, Test, and Development Commands
- Restore: `dotnet restore 4Chess.sln`
- Build (Debug): `dotnet build 4Chess.sln -c Debug`
- Run game: `dotnet run --project 4Chess/4Chess.csproj -c Debug`
- Publish (AOT, Windows x64): `dotnet publish 4Chess/4Chess.csproj -c Release -r win-x64 /p:PublishAot=true`
- Tests: no test project yet; see Testing Guidelines below.

## Coding Style & Naming Conventions
- C# with nullable enabled; 4-space indentation; UTF-8.
- Names: `PascalCase` for public types/members; `camelCase` for locals/parameters; private fields `_camelCase`.
- One public type per file; file name matches type.
- Prefer `var` when the type is obvious; use early returns; avoid static state.
- Use `Logger.Log*` for diagnostics (not `Console.WriteLine`).

## Testing Guidelines
- Create `4Chess.Tests` (xUnit or NUnit) when adding behavior.
- Name tests `ClassName_Method_Scenario_Expected()`; put files under `Tests/` mirroring source folders.
- Run all tests: `dotnet test` (from repo root once a test project exists).
- Target high-value logic: move generation, rules, AI adapters.

## Commit & Pull Request Guidelines
- Commits: imperative, concise, scoped (e.g., `fix: prevent illegal castle` or `feat: add Ultrathink overlay`).
- PRs: clear description, linked issues, repro steps, and screenshots/GIFs for UI changes.
- Include notes for engine/config changes and update docs when paths or flags change.

## Security & Configuration Tips
- Place `stockfish.exe` in `4Chess/Res/engine/` (preferred); it is copied to output. See `STOCKFISH_INTEGRATION.md` and `STOCKFISH_CONFIG.md`.
- Do not commit secrets or user data. Generated logs live under the app’s `logs/` at runtime and should not be versioned.

## Agent-Specific Instructions
- Keep diffs minimal and focused; preserve project layout and naming. Update docs alongside code.

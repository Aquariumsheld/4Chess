using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using _4Chess.Game;
using _4Chess.Game.Input;
using _4Chess.Pieces;

namespace _4Chess.ChessAi
{
    public class StockfishAi
    {
        private ChessAiDifficulty ChessAiDifficulty { get; set; }
        private Process? _stockfishProcess;
        private StreamWriter? _stockfishInput;
        private StreamReader? _stockfishOutput;
        private bool _isInitialized = false;
        private _4ChessGame? _game;

        // Schwierigkeitsgrade
        private int _depth;
        private int _skillLevel;
        private int _multiPV; // Anzahl der Varianten für ultrathink
        private int _moveTimeMs; // Zeit in Millisekunden pro Zug (0 = unbegrenzt)

        // Spielzug-Historie
        private List<string> _moveHistory = new List<string>();

        // Ultrathink Daten
        public List<StockfishThinkingLine> ThinkingLines { get; private set; } = new List<StockfishThinkingLine>();
        public bool ShowThinking { get; set; } = false;
        public int CurrentDepth { get; private set; } = 0;
        public int TargetDepth { get; private set; } = 0;
        public int CurrentMoveTimeMs { get; private set; } = 0;
        public DateTime ThinkingStartTime { get; private set; } = DateTime.Now;
        public bool IsThinking { get; private set; } = false;

        public StockfishAi(ChessAiDifficulty chessAiDifficulty)
        {
            ChessAiDifficulty = chessAiDifficulty;
            ConfigureDifficulty();
            InitializeStockfish();
        }

        private void ConfigureDifficulty()
        {
            switch (ChessAiDifficulty)
            {
                case ChessAiDifficulty.Low:
                    _depth = 5;
                    _skillLevel = 5;
                    _multiPV = 1;
                    _moveTimeMs = 5000; // 5 Sekunden
                    ShowThinking = true;
                    break;
                case ChessAiDifficulty.Medium:
                    _depth = 10;
                    _skillLevel = 10;
                    _multiPV = 1;
                    _moveTimeMs = 10000; // 10 Sekunden
                    ShowThinking = true;
                    break;
                case ChessAiDifficulty.High:
                    _depth = 15;
                    _skillLevel = 15;
                    _multiPV = 1;
                    _moveTimeMs = 15000; // 15 Sekunden
                    ShowThinking = true;
                    break;
                case ChessAiDifficulty.Ultra:
                    _depth = 20;
                    _skillLevel = 20;
                    _multiPV = 3;
                    _moveTimeMs = 15000; // 15 Sekunden
                    ShowThinking = true;
                    break;
                case ChessAiDifficulty.UltraPlus:
                    _depth = 25;
                    _skillLevel = 20;
                    _multiPV = 5;
                    _moveTimeMs = 22000; // 22 Sekunden
                    ShowThinking = true;
                    break;
                case ChessAiDifficulty.Overthinker:
                    _depth = 60;
                    _skillLevel = 20;
                    _multiPV = 10;
                    _moveTimeMs = 30000; // 30 Sekunden
                    ShowThinking = true;
                    break;
            }
        }

        private void InitializeStockfish()
        {
            try
            {
                // Versuche Stockfish zu finden
                string stockfishPath = FindStockfishPath();

                if (string.IsNullOrEmpty(stockfishPath))
                {
                    _4Chess.Logger.LogError("Stockfish executable not found!");
                    _4Chess.Logger.LogError("Please place stockfish.exe in the game directory or add it to PATH.");
                    return;
                }

                _stockfishProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = stockfishPath,
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                _stockfishProcess.Start();
                _stockfishInput = _stockfishProcess.StandardInput;
                _stockfishOutput = _stockfishProcess.StandardOutput;

                // UCI initialisieren
                SendCommand("uci");
                WaitForResponse("uciok");

                // Optionen setzen
                SendCommand($"setoption name Skill Level value {_skillLevel}");
                SendCommand($"setoption name MultiPV value {_multiPV}");
                SendCommand("isready");
                WaitForResponse("readyok");

                _isInitialized = true;
                _4Chess.Logger.LogInfo($"Stockfish initialized! Difficulty: {ChessAiDifficulty}, Depth: {_depth}, Skill: {_skillLevel}, MultiPV: {_multiPV}, TimeLimit: {_moveTimeMs}ms");
            }
            catch (Exception ex)
            {
                _4Chess.Logger.LogError($"Error initializing Stockfish", ex);
            }
        }
        private string FindStockfishPath()
        {
            // Mögliche Pfade für Stockfish
            string[] possiblePaths = new[]
            {
                "stockfish.exe",
                "stockfish_17.1.exe",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stockfish.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stockfish_17.1.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", "engine", "stockfish-windows-x86-64-avx2.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", "engine", "stockfish_17.1.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", "stockfish.exe"),
                Path.Combine(Environment.CurrentDirectory, "res", "engine", "stockfish.exe"),
                Path.Combine(Environment.CurrentDirectory, "stockfish.exe"),
                "C:\\Program Files\\Stockfish\\stockfish.exe",
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    _4Chess.Logger.LogInfo($"Found Stockfish at: {path}");
                    return path;
                }
            }

            // Versuche über PATH zu finden
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "where",
                        Arguments = "stockfish.exe",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrWhiteSpace(output))
                {
                    string path = output.Split('\n')[0].Trim();
                    if (File.Exists(path))
                    {
                        _4Chess.Logger.LogInfo($"Found Stockfish in PATH: {path}");
                        return path;
                    }
                }
            }
            catch { }

            return string.Empty;
        }

        private void SendCommand(string command)
        {
            if (_stockfishInput != null)
            {
                _stockfishInput.WriteLine(command);
                _stockfishInput.Flush();
                _4Chess.Logger.Log($">> {command}");
            }
        }

        private void WaitForResponse(string expectedResponse)
        {
            if (_stockfishOutput == null) return;

            while (true)
            {
                string? line = _stockfishOutput.ReadLine();
                if (line != null)
                {
                    _4Chess.Logger.Log($"<< {line}");
                    if (line.Contains(expectedResponse))
                    {
                        break;
                    }
                }
            }
        }

        public async Task<string?> GetBestMoveAsync(_4ChessGame game, bool isWhiteTurn, int moveCounter)
        {
            if (!_isInitialized || _stockfishInput == null || _stockfishOutput == null)
            {
                _4Chess.Logger.LogError("Stockfish not initialized!");
                return null;
            }

            _game = game;

            try
            {
                // Setze UCI-Optionen für diesen Zug (wichtig: muss vor jedem go-Befehl gesetzt werden!)
                SendCommand($"setoption name Skill Level value {_skillLevel}");
                SendCommand($"setoption name MultiPV value {_multiPV}");
                SendCommand("isready");
                WaitForResponse("readyok");

                _4Chess.Logger.LogInfo($"Stockfish calculating - Difficulty: {ChessAiDifficulty}, Depth: {_depth}, Skill: {_skillLevel}, MultiPV: {_multiPV}, TimeLimit: {_moveTimeMs}ms");

                // Setze öffentliche Properties für UI-Anzeige
                TargetDepth = _depth;
                CurrentMoveTimeMs = _moveTimeMs;
                CurrentDepth = 0;
                ThinkingStartTime = DateTime.Now;
                IsThinking = true;

                string fen = ChessAiHelper.BoardToFEN(game.Board, isWhiteTurn, moveCounter);
                SendCommand($"position fen {fen}");

                // Berechnung starten (mit Depth und Zeitlimit)
                var startTime = DateTime.Now;
                if (_moveTimeMs > 0)
                {
                    // Zeitlimit + max Depth (stoppt bei was auch immer zuerst erreicht wird)
                    SendCommand($"go depth {_depth} movetime {_moveTimeMs}");
                }
                else
                {
                    // Nur Depth (unbegrenzte Zeit)
                    SendCommand($"go depth {_depth}");
                }

                // Thinking Lines zurücksetzen (Thread-sicher)
                lock (ThinkingLines)
                {
                    ThinkingLines.Clear();
                }

                // Auf bestmove warten und Thinking-Lines sammeln
                string? bestMove = null;
                int maxDepthReached = 0;
                while (true)
                {
                    string? line = await Task.Run(() => _stockfishOutput.ReadLine());
                    if (line == null) break;

                    _4Chess.Logger.Log($"<< {line}");

                    // Parse thinking lines für ultrathink
                    if (line.StartsWith("info") && line.Contains("pv"))
                    {
                        ParseThinkingLine(line);

                        // Tracke maximale erreichte Depth
                        if (line.Contains("depth"))
                        {
                            var parts = line.Split(' ');
                            for (int i = 0; i < parts.Length - 1; i++)
                            {
                                if (parts[i] == "depth" && int.TryParse(parts[i + 1], out int depth))
                                {
                                    maxDepthReached = Math.Max(maxDepthReached, depth);
                                    CurrentDepth = maxDepthReached; // Update für UI
                                    break;
                                }
                            }
                        }
                    }

                    // Best move gefunden
                    if (line.StartsWith("bestmove"))
                    {
                        IsThinking = false; // Stockfish ist fertig
                        var elapsed = DateTime.Now - startTime;
                        _4Chess.Logger.LogInfo($"Stockfish finished - Elapsed: {elapsed.TotalSeconds:F2}s, Max Depth: {maxDepthReached}/{_depth}, Target Time: {_moveTimeMs}ms");

                        string[] parts = line.Split(' ');
                        if (parts.Length >= 2)
                        {
                            bestMove = parts[1];
                        }
                        break;
                    }
                }

                return bestMove;
            }
            catch (Exception ex)
            {
                _4Chess.Logger.LogError($"Error getting best move", ex);
                return null;
            }
        }

        private void ParseThinkingLine(string line)
        {
            try
            {
                var parts = line.Split(' ');
                int pvIndex = 1;
                int depth = 0;
                int score = 0;
                string scoreType = "cp"; // centipawns oder mate
                List<string> moves = new List<string>();

                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i] == "multipv" && i + 1 < parts.Length)
                    {
                        int.TryParse(parts[i + 1], out pvIndex);
                    }
                    else if (parts[i] == "depth" && i + 1 < parts.Length)
                    {
                        int.TryParse(parts[i + 1], out depth);
                    }
                    else if (parts[i] == "score" && i + 2 < parts.Length)
                    {
                        scoreType = parts[i + 1];
                        int.TryParse(parts[i + 2], out score);
                    }
                    else if (parts[i] == "pv")
                    {
                        // Alle nachfolgenden Teile sind die Zugfolge
                        for (int j = i + 1; j < parts.Length; j++)
                        {
                            moves.Add(parts[j]);
                        }
                        break;
                    }
                }

                // Aktualisiere oder füge Thinking Line hinzu (Thread-sicher)
                lock (ThinkingLines)
                {
                    var existingLine = ThinkingLines.FirstOrDefault(l => l.MultiPVIndex == pvIndex);
                    if (existingLine != null)
                    {
                        existingLine.Depth = depth;
                        existingLine.Score = score;
                        existingLine.ScoreType = scoreType;
                        existingLine.PrincipalVariation = moves;
                    }
                    else
                    {
                        ThinkingLines.Add(new StockfishThinkingLine
                        {
                            MultiPVIndex = pvIndex,
                            Depth = depth,
                            Score = score,
                            ScoreType = scoreType,
                            PrincipalVariation = moves
                        });
                    }

                    // Sortiere nach MultiPV Index
                    ThinkingLines = ThinkingLines.OrderBy(l => l.MultiPVIndex).ToList();
                }
            }
            catch (Exception ex)
            {
                _4Chess.Logger.LogError($"Error parsing thinking line", ex);
            }
        }

        public async Task MakeMoveAsync(_4ChessGame game, bool isWhiteTurn, int moveCounter)
        {
            _4Chess.Logger.LogInfo($"AI MakeMove called - isWhiteTurn: {isWhiteTurn}, moveCounter: {moveCounter}");

            string? bestMove = await GetBestMoveAsync(game, isWhiteTurn, moveCounter);

            if (bestMove == null || bestMove == "(none)")
            {
                _4Chess.Logger.LogWarning("No valid move found by Stockfish!");
                return;
            }

            _4Chess.Logger.LogInfo($"Stockfish best move: {bestMove}");

            // Parse UCI move
            var (from, to, promotion) = ChessAiHelper.ParseUCIMove(bestMove);

            // Finde die Figur auf dem Board
            var piece = game.Board[from.y][from.x];
            if (piece == null)
            {
                _4Chess.Logger.LogError($"No piece at ({from.x},{from.y})");
                return;
            }

            _4Chess.Logger.LogInfo($"Moving piece from ({from.x},{from.y}) to ({to.x},{to.y})");

            // Führe den Zug aus
            game.Board[from.y][from.x] = null;
            game.Board[to.y][to.x] = piece;
            piece.X = to.x;
            piece.Y = to.y;

            // Handle special moves
            HandleSpecialMoves(game, piece, from, to, promotion);

            // Speichere Zug in Historie
            _moveHistory.Add(bestMove);

            // Trigger turn change
            _4Chess.Logger.LogInfo($"Before TurnChange - IsWhiteTurn: {_4ChessMove.IsWhiteTurn}");
            _4ChessMove.TurnChange();
            _4Chess.Logger.LogInfo($"After TurnChange - IsWhiteTurn: {_4ChessMove.IsWhiteTurn}");

            // Increment move counter
            _4ChessMove.MoveCounter++;
            _4Chess.Logger.LogInfo($"AI move completed - MoveCounter: {_4ChessMove.MoveCounter}");
        }

        private void HandleSpecialMoves(_4ChessGame game, Piece piece, (int x, int y) from, (int x, int y) to, char promotion)
        {
            // Bauernumwandlung
            if (promotion != '\0' && piece is Pawn)
            {
                Piece newPiece = promotion switch
                {
                    'q' => new Queen(to.y, to.x, piece.Alignment, game),
                    'r' => new Rook(to.y, to.x, piece.Alignment, game),
                    'b' => new Bishop(to.y, to.x, piece.Alignment, game),
                    'n' => new Knight(to.y, to.x, piece.Alignment, game),
                    _ => new Queen(to.y, to.x, piece.Alignment, game)
                };
                game.Board[to.y][to.x] = newPiece;
            }

            // Rochade
            if (piece is King)
            {
                int xDiff = to.x - from.x;

                // Kingside castling
                if (xDiff == 2)
                {
                    var kingsideRook = game.Board[from.y][7];
                    if (kingsideRook != null)
                    {
                        game.Board[from.y][7] = null;
                        game.Board[from.y][5] = kingsideRook;
                        kingsideRook.X = 5;
                        kingsideRook.Y = from.y;
                    }
                }
                // Queenside castling
                else if (xDiff == -2)
                {
                    var queensideRook = game.Board[from.y][0];
                    if (queensideRook != null)
                    {
                        game.Board[from.y][0] = null;
                        game.Board[from.y][3] = queensideRook;
                        queensideRook.X = 3;
                        queensideRook.Y = from.y;
                    }
                }

                // Update King position
                if (piece.Alignment == Piece.Color.White)
                    game.WhiteKingPosition = new System.Numerics.Vector2(to.x, to.y);
                else
                    game.BlackKingPosition = new System.Numerics.Vector2(to.x, to.y);
            }

            // En Passant
            if (piece is Pawn pawn)
            {
                int xDiff = Math.Abs(to.x - from.x);
                if (xDiff == 1 && game.Board[to.y][to.x] == null)
                {
                    // Diagonaler Zug ohne Figur = En Passant
                    int capturedPawnY = pawn.Alignment == Piece.Color.White ? to.y + 1 : to.y - 1;
                    if (capturedPawnY >= 0 && capturedPawnY < 8)
                    {
                        game.Board[capturedPawnY][to.x] = null;
                    }
                }

                // Markiere 2-Felder-Zug
                int yDiff = Math.Abs(to.y - from.y);
                pawn.IsEnPassant = (yDiff == 2);
                pawn.IsUnmoved = false;
            }

            // Update IsUnmoved für Rochade-Rechte
            if (piece is King kingPiece) kingPiece.IsUnmoved = false;
            if (piece is Rook rookPiece) rookPiece.IsUnmoved = false;
        }
        public void Dispose()
        {
            try
            {
                if (_stockfishProcess != null && !_stockfishProcess.HasExited)
                {
                    SendCommand("quit");
                    _stockfishProcess.WaitForExit(1000);
                    if (!_stockfishProcess.HasExited)
                    {
                        _stockfishProcess.Kill();
                    }
                }
                _stockfishInput?.Dispose();
                _stockfishOutput?.Dispose();
                _stockfishProcess?.Dispose();
            }
            catch (Exception ex)
            {
                _4Chess.Logger.LogError($"Error disposing Stockfish", ex);
            }
        }
    }

    public class StockfishThinkingLine
    {
        public int MultiPVIndex { get; set; }
        public int Depth { get; set; }
        public int Score { get; set; }
        public string ScoreType { get; set; } = "cp"; // "cp" für Centipawns, "mate" für Matt
        public List<string> PrincipalVariation { get; set; } = new List<string>();

        public string GetScoreDisplay()
        {
            if (ScoreType == "mate")
            {
                return $"Matt in {Math.Abs(Score)}";
            }
            else
            {
                double pawns = Score / 100.0;
                return $"{pawns:+0.00;-0.00}";
            }
        }

        public string GetMovesDisplay(int maxMoves = 5)
        {
            var moves = PrincipalVariation.Take(maxMoves);
            return string.Join(" ", moves);
        }
    }
}

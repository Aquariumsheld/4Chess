using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _4Chess.Game;

namespace _4Chess.ChessAi
{
    public class AiTournamentResult
    {
        public string WhiteAiName { get; set; } = "";
        public string BlackAiName { get; set; } = "";
        public int WhiteWins { get; set; } = 0;
        public int BlackWins { get; set; } = 0;
        public int Draws { get; set; } = 0;
        public int TotalGames { get; set; } = 0;
        public int CurrentGame { get; set; } = 0;
        public List<string> GameHistory { get; set; } = new List<string>();
        public bool IsFinished { get; set; } = false;
    }

    public class AiTournament
    {
        private ChessAiDifficulty _whiteAiDifficulty;
        private ChessAiDifficulty _blackAiDifficulty;
        private int _numberOfGames;
        private int _maxMovesPerGame;

        public AiTournamentResult Result { get; private set; }
        public bool IsRunning { get; private set; } = false;

        public AiTournament(ChessAiDifficulty whiteAi, ChessAiDifficulty blackAi, int numberOfGames, int maxMovesPerGame = 100)
        {
            _whiteAiDifficulty = whiteAi;
            _blackAiDifficulty = blackAi;
            _numberOfGames = numberOfGames;
            _maxMovesPerGame = maxMovesPerGame;

            Result = new AiTournamentResult
            {
                WhiteAiName = $"Stockfish ({whiteAi})",
                BlackAiName = $"Stockfish ({blackAi})",
                TotalGames = numberOfGames
            };
        }

        public async Task RunTournamentAsync(_4ChessGame game)
        {
            IsRunning = true;
            _4Chess.Logger.LogInfo($"Starting AI Tournament: {Result.WhiteAiName} vs {Result.BlackAiName} - {_numberOfGames} games");

            for (int gameNum = 1; gameNum <= _numberOfGames; gameNum++)
            {
                Result.CurrentGame = gameNum;
                _4Chess.Logger.LogInfo($"Game {gameNum}/{_numberOfGames} starting...");

                var gameResult = await PlaySingleGameAsync(game, gameNum);

                Result.GameHistory.Add(gameResult);
                _4Chess.Logger.LogInfo($"Game {gameNum} finished: {gameResult}");

                await Task.Delay(1000);
            }

            Result.IsFinished = true;
            IsRunning = false;
            _4Chess.Logger.LogInfo($"Tournament finished! White: {Result.WhiteWins}, Black: {Result.BlackWins}, Draws: {Result.Draws}");
        }

        private async Task<string> PlaySingleGameAsync(_4ChessGame game, int gameNumber)
        {
            _4Chess.Game.Input._4ChessMove.IsWhiteTurn = true;
            _4Chess.Game.Input._4ChessMove.MoveCounter = 1;

            game.Board =
            [
                [new _4Chess.Pieces.Rook(0, 0, _4Chess.Pieces.Piece.Color.Black, game), new _4Chess.Pieces.Knight(0, 1, _4Chess.Pieces.Piece.Color.Black, game), new _4Chess.Pieces.Bishop(0, 2, _4Chess.Pieces.Piece.Color.Black, game), new _4Chess.Pieces.Queen(0, 3, _4Chess.Pieces.Piece.Color.Black, game), new _4Chess.Pieces.King(0, 4, _4Chess.Pieces.Piece.Color.Black, game), new _4Chess.Pieces.Bishop(0, 5, _4Chess.Pieces.Piece.Color.Black, game), new _4Chess.Pieces.Knight(0, 6, _4Chess.Pieces.Piece.Color.Black, game), new _4Chess.Pieces.Rook(0, 7, _4Chess.Pieces.Piece.Color.Black, game)],
                [new _4Chess.Pieces.Pawn(1, 0, _4Chess.Pieces.Piece.Color.Black, game), new _4Chess.Pieces.Pawn(1, 1, _4Chess.Pieces.Piece.Color.Black, game), new _4Chess.Pieces.Pawn(1, 2, _4Chess.Pieces.Piece.Color.Black, game), new _4Chess.Pieces.Pawn(1, 3, _4Chess.Pieces.Piece.Color.Black, game), new _4Chess.Pieces.Pawn(1, 4, _4Chess.Pieces.Piece.Color.Black, game), new _4Chess.Pieces.Pawn(1, 5, _4Chess.Pieces.Piece.Color.Black, game), new _4Chess.Pieces.Pawn(1, 6, _4Chess.Pieces.Piece.Color.Black, game), new _4Chess.Pieces.Pawn(1, 7, _4Chess.Pieces.Piece.Color.Black, game)],
                [null,null,null,null,null,null,null,null],
                [null,null,null,null,null,null,null,null],
                [null,null,null,null,null,null,null,null],
                [null,null,null,null,null,null,null,null],
                [new _4Chess.Pieces.Pawn(6, 0, _4Chess.Pieces.Piece.Color.White, game), new _4Chess.Pieces.Pawn(6, 1, _4Chess.Pieces.Piece.Color.White, game), new _4Chess.Pieces.Pawn(6, 2, _4Chess.Pieces.Piece.Color.White, game), new _4Chess.Pieces.Pawn(6, 3, _4Chess.Pieces.Piece.Color.White, game), new _4Chess.Pieces.Pawn(6, 4, _4Chess.Pieces.Piece.Color.White, game), new _4Chess.Pieces.Pawn(6, 5, _4Chess.Pieces.Piece.Color.White, game), new _4Chess.Pieces.Pawn(6, 6, _4Chess.Pieces.Piece.Color.White, game), new _4Chess.Pieces.Pawn(6, 7, _4Chess.Pieces.Piece.Color.White, game)],
                [new _4Chess.Pieces.Rook(7, 0, _4Chess.Pieces.Piece.Color.White, game), new _4Chess.Pieces.Knight(7, 1, _4Chess.Pieces.Piece.Color.White, game), new _4Chess.Pieces.Bishop(7, 2, _4Chess.Pieces.Piece.Color.White, game), new _4Chess.Pieces.Queen(7, 3, _4Chess.Pieces.Piece.Color.White, game), new _4Chess.Pieces.King(7, 4, _4Chess.Pieces.Piece.Color.White, game), new _4Chess.Pieces.Bishop(7, 5, _4Chess.Pieces.Piece.Color.White, game), new _4Chess.Pieces.Knight(7, 6, _4Chess.Pieces.Piece.Color.White, game), new _4Chess.Pieces.Rook(7, 7, _4Chess.Pieces.Piece.Color.White, game)],
            ];

            StockfishAi whiteAi = new StockfishAi(_whiteAiDifficulty);
            StockfishAi blackAi = new StockfishAi(_blackAiDifficulty);

            int moveCount = 0;
            bool gameFinished = false;
            string result = "Draw (Max moves reached)";

            while (!gameFinished && moveCount < _maxMovesPerGame)
            {
                bool isWhiteTurn = _4Chess.Game.Input._4ChessMove.IsWhiteTurn;
                StockfishAi currentAi = isWhiteTurn ? whiteAi : blackAi;

                try
                {
                    await currentAi.MakeMoveAsync(game, isWhiteTurn, _4Chess.Game.Input._4ChessMove.MoveCounter);
                    moveCount++;

                   
                    var pieces = game.Board
                        .SelectMany(row => row)
                        .Where(p => p != null)
                        .Cast<_4Chess.Pieces.Piece>()
                        .ToList();

                    var whitePieces = pieces.Where(p => p.Alignment == _4Chess.Pieces.Piece.Color.White).ToList();
                    var blackPieces = pieces.Where(p => p.Alignment == _4Chess.Pieces.Piece.Color.Black).ToList();

                    var whiteMoves = whitePieces.SelectMany(p => p.GetMoves()).ToList();
                    var blackMoves = blackPieces.SelectMany(p => p.GetMoves()).ToList();

                    var whiteKing = whitePieces.FirstOrDefault(p => p is _4Chess.Pieces.King);
                    var blackKing = blackPieces.FirstOrDefault(p => p is _4Chess.Pieces.King);
                    var whiteKingPos = whiteKing != null ? new System.Numerics.Vector2(whiteKing.X, whiteKing.Y) : new System.Numerics.Vector2(-1, -1);
                    var blackKingPos = blackKing != null ? new System.Numerics.Vector2(blackKing.X, blackKing.Y) : new System.Numerics.Vector2(-1, -1);

                    bool whiteHasMoves = whiteMoves.Count > 0;
                    bool blackHasMoves = blackMoves.Count > 0;

                    if (!whiteHasMoves)
                    {
                        bool whiteInCheck = blackMoves.Contains(whiteKingPos);
                        if (whiteInCheck)
                        {
                            result = "Black wins (checkmate)";
                            Result.BlackWins++;
                        }
                        else
                        {
                            result = "Draw (stalemate)";
                            Result.Draws++;
                        }
                        gameFinished = true;
                    }
                    else if (!blackHasMoves)
                    {
                        bool blackInCheck = whiteMoves.Contains(blackKingPos);
                        if (blackInCheck)
                        {
                            result = "White wins (checkmate)";
                            Result.WhiteWins++;
                        }
                        else
                        {
                            result = "Draw (stalemate)";
                            Result.Draws++;
                        }
                        gameFinished = true;
                    }


                }
                catch (Exception ex)
                {
                    _4Chess.Logger.LogError($"Error in game {gameNumber}, move {moveCount}", ex);
                    result = isWhiteTurn ? "Black wins (White error)" : "White wins (Black error)";
                    gameFinished = true;

                    if (isWhiteTurn)
                        Result.BlackWins++;
                    else
                        Result.WhiteWins++;
                }
            }

            if (!gameFinished)
            {
                Result.Draws++;
            }

            whiteAi.Dispose();
            blackAi.Dispose();

            return $"Game {gameNumber}: {result} ({moveCount} moves)";
        }
    }
}


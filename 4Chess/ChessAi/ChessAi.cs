using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _4Chess.Game;

namespace _4Chess.ChessAi
{
    public class ChessAi
    {
        private ChessAiDifficulty ChessAiDifficulty { get; set; }
        public ChessAi(int aiType, int difficulty = 0) 
        {
            SetAiDifficulty(difficulty);

            // AiType 1: Stockfish
            // AiType 2: CustomAi
            switch (aiType)
            {
                case 1:
                    _4ChessGame.StockfishAi = new StockfishAi(ChessAiDifficulty);
                    break;
                case 2:
                    _4ChessGame.CustomAi = new CustomAi(ChessAiDifficulty);
                    break;
                default:
                    _4ChessGame.StockfishAi = new StockfishAi(ChessAiDifficulty);
                    break;
            }
        }

        private void SetAiDifficulty(int difficulty)
        {
            switch (difficulty)
            {
                case 0:
                    ChessAiDifficulty = ChessAiDifficulty.Low; break;
                case 1:
                    ChessAiDifficulty = ChessAiDifficulty.Medium; break;
                case 2:
                    ChessAiDifficulty = ChessAiDifficulty.High; break;
                case 3:
                    ChessAiDifficulty = ChessAiDifficulty.Expert; break;
                case 4:
                    ChessAiDifficulty = ChessAiDifficulty.Ultra; break;
                case 5:
                    ChessAiDifficulty = ChessAiDifficulty.UltraPlus; break;
                case 6:
                    ChessAiDifficulty = ChessAiDifficulty.Godlike; break;
                case 7:
                    ChessAiDifficulty = ChessAiDifficulty.Overthinker; break;
                default:
                    ChessAiDifficulty = ChessAiDifficulty.Low; break;
            }
        }
    }
}

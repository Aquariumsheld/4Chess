using _4Chess.Game;
using BIERKELLER.BIERRender;
using Raylib_CsLo;
using System.Runtime.InteropServices;
using static Raylib_CsLo.Raylib;
namespace _4Chess;

internal class Program
{
    [DllImport("kernel32.dll")]
    static extern bool AllocConsole();

    private static void Main()
    {
        // Konsole für Debug-Ausgaben aktivieren
        AllocConsole();

        // Logger initialisieren
        Logger.Initialize();
        Logger.LogInfo("4Chess gestartet");

        try
        {
            var syzygyCheck = System.Environment.GetEnvironmentVariable("SYZYGY_CHECK");
            if (!string.IsNullOrWhiteSpace(syzygyCheck) && syzygyCheck.Trim() == "1")
            {
                Logger.LogInfo("Running headless Syzygy check (SYZYGY_CHECK=1)");
                var game = new _4ChessGame();
                var tournament = new _4Chess.ChessAi.AiTournament(
                    _4Chess.ChessAi.ChessAiDifficulty.Low,
                    _4Chess.ChessAi.ChessAiDifficulty.Low,
                    numberOfGames: 1,
                    maxMovesPerGame: 1);
                tournament.RunTournamentAsync(game).GetAwaiter().GetResult();
                Logger.LogInfo("Syzygy check finished. Exiting.");
                return;
            }
            var _4chessGame = new _4ChessGame();
            _4chessGame.Run();
        }
        catch (Exception ex)
        {
            Logger.LogError("Kritischer Fehler beim Ausführen des Spiels", ex);
        }
        finally
        {
            Logger.Close();
        }
    }
}

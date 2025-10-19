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

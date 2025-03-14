using _4Chess.Game;
using BIERKELLER.BIERRender;
using Raylib_CsLo;
using static Raylib_CsLo.Raylib;
namespace _4Chess;

internal class Program
{
    private static void Main()
    {
        var _4chessGame = new _4ChessGame();
        _4chessGame.Run();
    }
}

using Domain;
namespace GameBrain;

public class GamePlacementManager
{
    public CheckersGame Game { get; set; }

    private int BoardRows { get; set; }

    private int BoardColumns { get; set; }

    public (int x, int y)? LastCursorPosistion { get; set; }

    public static readonly (int x, int y) SaveGame = (-2, -2);

    public List<(int x, int y)> PossibleMoves = new List<(int x, int y)>();

    public List<(int startX, int startY, int eatX, int eatY, int destX, int destY)> EatingCoordinates { get; set; }
    
        
    public (int x, int y) CheckersCoordinatesForMove { get; set; }

    public GamePlacementManager(CheckersGame game)
    {
        Game = game;
        BoardColumns = game.CheckersOption!.Width;
        BoardRows = game.CheckersOption.Height;
        EatingCoordinates = new List<(int startX, int startY, int eatX, int eatY, int destX, int destY)>();
    }

    public (int x, int y) GetInputForCheckersMove()
    {
        var done = false;
        var result = (0, 0);
        var boardX = 0;
        var boardY = 0;
        
        var pos = Console.GetCursorPosition();
        
        if (LastCursorPosistion != null)
        {
            boardX = LastCursorPosistion.Value.x;
            boardY = LastCursorPosistion.Value.y;
        }
        else
        {
            Console.SetCursorPosition(pos.Left + 5, pos.Top + 1);
        }
        
        while (!done)
        {
           
            pos = Console.GetCursorPosition();
            var key = Console.ReadKey(true).Key;
            switch (key)
            {
                case ConsoleKey.DownArrow when pos.Top < BoardRows * 2 - 2:
                    Console.SetCursorPosition(pos.Left, pos.Top + 2);
                    boardY++;
                    break;

                case ConsoleKey.RightArrow when pos.Left < BoardColumns * 5 + 5:
                    Console.SetCursorPosition(pos.Left + 6, pos.Top);
                    boardX++;
                    break;
                
                case ConsoleKey.LeftArrow when pos.Left >= 6:
                    Console.SetCursorPosition(pos.Left - 6, pos.Top);
                    boardX--;
                    break;
                
                case ConsoleKey.UpArrow when pos.Top >= 2:
                    Console.SetCursorPosition(pos.Left, pos.Top - 2);
                    boardY--;
                    break;
                
                case ConsoleKey.Enter:
                    Console.Clear();
                    result = (boardX, boardY);
                    done = true;
                    break;
                
                case ConsoleKey.S:
                    Console.Clear();
                    return SaveGame;
            }
        }
        Console.Clear();
        LastCursorPosistion = result;
        return result;
    }
}
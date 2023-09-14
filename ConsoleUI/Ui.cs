
using Domain;
using GameBrain;

namespace ConsoleUI;

public static class Ui
{
    public static void DrawGameBoard(CheckersGame game)
    {
        var board = game.CheckersState!.GameBoard;
        var rows = board!.GetLength(0);
        var cols = board[0].GetLength(0);
        
        var counter = rows;
        var characterCounter = 0;
        var characters = new List<string>();
        for (var c = 'A'; c <= 'Z'; c++)
            characters.Add("" + c);

        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < cols; col++)
            {
                if (col == 0 && row == 0)
                {
                    Console.Write("  +--" + characters[characterCounter] + "--");
                    characterCounter++;
                }
                
                else if (row == 0)
                {
                    Console.Write("+--" + characters[characterCounter] + "--");
                    characterCounter++;
                }
                
                if (row != 0 && col == 0)
                {
                    Console.Write("  +-----");
                }
                else if(row != 0)
                {
                    Console.Write("+-----");
                }

            }
            Console.WriteLine("+");
            
            
            if (counter < 10)
            {
                Console.Write(" " + counter);
            }
            else
            {
                Console.Write(counter);
            }
            counter--;
            
            for (int col = 0; col < cols; col++)
            {
                Console.Write("|  ");
                switch (board[row][col])
                {
                    case ESquareState.Black:
                        Console.Write("O  ");
                        break;
                    case ESquareState.White:
                        Console.Write("X  ");
                        break;
                    case ESquareState.Blank:
                        Console.Write("   ");
                        break;
                }
            }
            Console.WriteLine("|");
            
            
        }
        for (int col = 0; col < cols; col++)
        {
            Console.Write(col == 0 ? "  +-----" : "+-----");
        }
        Console.WriteLine("+");
        
        var playerName = game.CheckersState!.NextMoveByWhite ? game.Player1Name : game.Player2Name;
        
        Console.WriteLine("Current turn: {0}", playerName);
        //Console.WriteLine();
        Console.WriteLine("To save game and exit press: S");
               
        
    }
}
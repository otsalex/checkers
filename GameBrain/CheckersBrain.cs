using System.Security.Cryptography;
using DAL;
using Domain;

namespace GameBrain;

public class CheckersBrain
{
    public CheckersGame Game { get; set; }
    private GamePlacementManager? _manager;
    private (int x, int y) EMPTY = (-2, -2);

    public CheckersBrain(CheckersGame game)
    {
        Game = game;
        _manager = new GamePlacementManager(game);
        if (game.CheckersState == null)
        {
            CreateStartingBoard();
        }
    }
    
    public (int x, int y)? GetManagerLastCursorPosition()
    {
        if (_manager!.LastCursorPosistion == null)
        {
            return null;
        }
        return (_manager.LastCursorPosistion.Value.x * 6, _manager.LastCursorPosistion.Value.y * 2);
    }

    public void CreateStartingBoard()
    {
        if (Game.CheckersState == null)
        {
            Game.CheckersState = new CheckersGameState();
        }
        Game.CheckersState!.GameBoard = new ESquareState[Game.CheckersOption!.Width][];
        for (int i = 0; i < Game.CheckersOption!.Width; i++)
        {
            Game.CheckersState!.GameBoard![i] = new ESquareState[Game.CheckersOption!.Height];
        }

        var pieceCount = (Game.CheckersOption!.Height - 2) * Game.CheckersOption!.Width / 4;

        if (pieceCount % 2 != 0)
            pieceCount -= 2;

        var whiteCount = pieceCount;
        var blackCount = pieceCount;

        for (var row = 0; row < Game.CheckersOption.Width; row++)

        {
            var startingPoint = (pieceCount % 2 == 0) switch
            {
                true => row % 2 == 0 ? 0 : 1,
                false => row % 2 != 0 ? 0 : 1
            };

            for (var col = startingPoint; col < Game.CheckersOption.Height; col += 2)
            {
                if (whiteCount > 0)
                {
                    Game.CheckersState!.GameBoard[row][col] = ESquareState.White;
                    whiteCount--;
                }
            }
        }

        for (var row = Game.CheckersOption.Width - 1; row >= 0; row--)
        {
            var startingPoint = row % 2 == 0 ? 1 : 0;
            for (var col = Game.CheckersOption.Height - 1 - startingPoint; col >= 0; col -= 2)
            {
                if (blackCount > 0)
                {
                    Game.CheckersState!.GameBoard[row][col] = ESquareState.Black;
                    blackCount--;
                }
            }
        }
    }

    public ESquareState[][]? GetBoard()
    {
        return Game.CheckersState!
        .GameBoard;

    }
    public List<(int x, int y)> GetManagerPossibleMoves()
    {
        return _manager!.PossibleMoves;
    }
    public List<(int startX, int startY, int eatX, int eatY, int destX, int destY)> GetManagerEatingCoordinates()
    {
        return _manager!.EatingCoordinates;
    }
    
    public string StartCheckersGame()
    {
        var done = false;
        while (!done)
        {
            if (Game.Player2Type == EPlayerType.Ai && !Game.CheckersState!.NextMoveByWhite)
            {
                var returnVal = PerformAiMove();
                _manager!.EatingCoordinates.Clear();
                return returnVal;
            }
            
            GetMandatoryCheckersMoves(); // fills the EatingCoordinates list
            
            var userInput = EMPTY;
            
            if (_manager!.EatingCoordinates.Count > 0)
            {
                // küsi inputi
                userInput = _manager.GetInputForCheckersMove();
                // kontorlli kas käik on õiges kohas!
                foreach (var set in _manager.EatingCoordinates)
                {
                    if (userInput.x == set.destX && userInput.y == set.destY)
                    {
                        EatButton(set.startX, set.startY, set.eatX, set.eatY, set.destX, set.destY);
                        _manager.EatingCoordinates.Clear();
                        GetMandatoryCheckersMoves();
                        return "piece taken!";
                    }
                }
                return "invalid turn, piece has to eat!";
            }
            
            userInput = _manager.GetInputForCheckersMove();
            
            if (userInput == GamePlacementManager.SaveGame)
            {
                StoreOldState();
                return "save";
            }

            // Move piece 
            if (_manager.PossibleMoves.Contains(userInput))
            {
                if (!IsCurrentPlayerTurn(Game.CheckersState!.GameBoard![_manager.CheckersCoordinatesForMove.y][_manager.CheckersCoordinatesForMove.x]))
                {
                    _manager.EatingCoordinates.Clear();
                    return "select correct piece!";
                }
                MoveCheckersPiece(_manager.CheckersCoordinatesForMove.x, _manager.CheckersCoordinatesForMove.y, userInput.x, userInput.y);
                _manager.PossibleMoves.Clear();
                GetMandatoryCheckersMoves();
                return "move done";
            }

            _manager.CheckersCoordinatesForMove = userInput;

            if (IsCurrentPlayerTurn(Game.CheckersState!.GameBoard![userInput.y][userInput.x]))
            {
                _manager.PossibleMoves = GetAllPossibleCheckersMoves(Game, userInput.x, userInput.y);
            }
            GetMandatoryCheckersMoves();

            return "none";

        }
        return "x";
    }
    public void MoveCheckersPiece(int startX, int startY, int destinationX, int destinationY)
    {
        // Makes copy of board and adds new game state
        StoreOldState();

        Game.CheckersState!.GameBoard![destinationY][destinationX] = Game.CheckersState!.GameBoard[startY][startX];
        Game.CheckersState!.GameBoard[startY][startX] = ESquareState.Blank;
        
        UpdateCurrentStateTime();

        ChangeTurn();
    }
    public void StoreOldState()
    {
        var stateToSave = new CheckersGameState
        {
            CreatedAt = Game.CheckersState!.CreatedAt,
            GameBoard = Game.CheckersState.GameBoard,
            CheckersGameId = Game.Id,
            NextMoveByWhite = Game.CheckersState!.NextMoveByWhite
        };

        // if game has no states list, create it
        Game.CheckersGameStates ??= new List<CheckersGameState>();
        // add state
        Game.CheckersGameStates.Add(stateToSave);
    }
    
    public void UpdateCurrentStateTime()
    {
        Game.CheckersState!.CreatedAt = DateTime.Now;
    }
    private void ChangeTurn()
    {
        Game.CheckersState!.NextMoveByWhite = Game.CheckersState!.NextMoveByWhite ? false : true;
    }
    public bool IsCurrentPlayerTurn(ESquareState piece)
    {
        switch (piece)
        {
            case ESquareState.Black when !Game.CheckersState!.NextMoveByWhite:
            case ESquareState.White when Game.CheckersState!.NextMoveByWhite:
                return true;
            default:
                return false;
        }
    }
    
    public List<(int x, int y)> GetAllPossibleCheckersMoves(CheckersGame game, int x, int y)
    {
        List<(int x, int y)> result = new List<(int x, int y)>();
        var width = Game.CheckersOption!.Width;
        var heigth = Game.CheckersOption!.Height;
        var board = game.CheckersState!.GameBoard;

        if (board![y][x] == ESquareState.White)
        {
            // Black player
            if (x-1 >= 0 && y + 1 <= heigth -1)
            {
                if (board[y + 1][x - 1] == ESquareState.Blank)
                {
                    result.Add((x-1, y+1));
                }
            }
            if (x + 1 <= width - 1 && y + 1 <= heigth - 1)
            {
                if (board[y+1][x+1] == ESquareState.Blank)
                {
                    result.Add((x+1,y+1));   
                }
            }
        }
        if (board[y][x] == ESquareState.Black)
        {
            if (x-1 >= 0 && y-1 >= 0)
            {
                if (board[y - 1][x - 1] == ESquareState.Blank)
                {
                    result.Add((x-1, y-1));
                }
            }
            if (x+1 <= width -1 && y - 1 >= 0)
            {
                if (board[y - 1][x + 1] == ESquareState.Blank)
                {
                    result.Add((x+1, y-1));
                }
            }
        }
        return result;
    }
    public void EatButton(int startX, int startY, int eatX, int eatY, int destX, int destY)
    {
        var board = Game.CheckersState!.GameBoard;
        StoreOldState();
        if (board![startY][startX] is ESquareState.Black or ESquareState.White)
        {
            board[eatY][eatX] = ESquareState.Blank;
            board[destY][destX] = board[startY][startX];
            board[startY][startX] = ESquareState.Blank;
        }
        UpdateCurrentStateTime();
        ChangeTurn();
    }
    public bool GetMandatoryCheckersMoves()
    {
        var eatingIsPossible = false;
        var board = GetBoard();
        for (int y = 0; y < board!.GetLength(0); y++)
        {
            for (int x = 0; x < board[0].Length; x++)
            {
                var cords = GetForcedCheckersMove(x, y, Game);
                if (cords.Count > 0)
                {
                    cords.ForEach(set => _manager!.EatingCoordinates.Add(set));
                    eatingIsPossible = true;
                }
            }
        }
        return eatingIsPossible;
    }
    
    public List<(int startX, int startY, int eatX, int eatY, int destX, int destY)> 
        GetForcedCheckersMove(int x, int y, CheckersGame game)
    {
        var result = new List<(int startX, int startY, int eatX, int eatY, int destX, int destY)>();
        var gameBoard = game.CheckersState!.GameBoard;
        // valge nupp!
        if(gameBoard![y][x] == ESquareState.White && game.CheckersState.NextMoveByWhite) 
        {
            try
            {
                if ((gameBoard[y + 1][x + 1] == ESquareState.Black) && gameBoard[y+2][x+2] == ESquareState.Blank)
                {
                    result.Add((x, y, x+1, y+1, x+2, y+2));
                }
            }
            catch (IndexOutOfRangeException){}
            try
            {
                if ((gameBoard[y + 1][x - 1] == ESquareState.Black) && gameBoard[y+2][x-2] == ESquareState.Blank)
                {
                    result.Add((x, y, x-1, y+1, x-2, y+2));
                }
            }
            catch (IndexOutOfRangeException){}
        }
        // Must nupp
        if(gameBoard[y][x] == ESquareState.Black && !game.CheckersState.NextMoveByWhite) 
        {
            try
            {
                if ((gameBoard[y - 1][x - 1] is ESquareState.White)  && gameBoard[y-2][x-2] == ESquareState.Blank)
                {
                    result.Add((x, y, x-1, y-1, x-2, y-2));
                }
            }
            catch (IndexOutOfRangeException){}
            try
            {
                if (gameBoard[y-1][x+1] is ESquareState.White && gameBoard[y-2][x+2] == ESquareState.Blank)
                {
                    result.Add((x, y, x+1, y-1, x+2, y-2));
                }
            }
            catch (IndexOutOfRangeException){}
        }
        
        return result;
    }
    public bool IsGameOver()
    {
        var gameOver = false;
        
        var whiteButtons = 0;
        var blackButtons = 0;

        var board = Game.CheckersState!.GameBoard;
        foreach (var row in board!)
        {
            for (int i = 0; i < row.Length; i++)
            {
                if (row[i] is ESquareState.Black) blackButtons++;
                if (row[i] is ESquareState.White) whiteButtons++;
            }
        }
        
        // Kontrolli kas praegusel käigu tegijal on käike!
        
        var currentTurn = Game.CheckersState.NextMoveByWhite ? ESquareState.White : ESquareState.Black;
        var currentTurnMoves = 0;
        
        for (int y = 0; y < board.Length; y++)
        {
            for (int x = 0; x < board[0].Length; x++)
            {
                if (board[y][x] == currentTurn)
                {
                    currentTurnMoves += GetAllPossibleCheckersMoves(Game, x, y).Count;
                }
            }
        }
        // if player whose turn is it, has no more possible moves!

        if (currentTurnMoves == 0)
        {
            Game.GameWonByPlayer = Game.CheckersState.NextMoveByWhite ? Game.Player1Name : Game.Player2Name;
            Game.GameOverAt = DateTime.Now;
            gameOver = true;
        }

        if (whiteButtons == 0 || blackButtons == 0)
        {
            Game.GameWonByPlayer = whiteButtons == 0 ? Game.Player2Name : Game.Player1Name;
            Game.GameOverAt = DateTime.Now;
            gameOver = true;
        }
        return gameOver;
    }
    public bool CanPieceMoveForward(int x, int y, CheckersGame game)
    {
        var width = game.CheckersOption!.Width;
        var heigth = game.CheckersOption.Height;
        var board = game.CheckersState!.GameBoard;
        if (board![y][x] == ESquareState.Blank)
        {
            return false;
        }
        if (board[y][x] == ESquareState.White && game.CheckersState!.NextMoveByWhite)
        {
            // Black player
            if (x-1 >= 0 && y + 1 <= heigth -1 && board[y+1][x - 1] == ESquareState.Blank) // A
            {
                return true;
            }

            if (x + 1 <= width - 1 && y + 1 <= heigth - 1 && board[y+1][x+1] == ESquareState.Blank) // B
            {
                return true;
            }
        }
        
        if (board[y][x] == ESquareState.Black && game.CheckersState!.NextMoveByWhite == false)
        {
            if (x-1 >= 0 && y-1 >= 0 && board[y - 1][x - 1] == ESquareState.Blank) // C
            {
                return true;
            }

            if (x+1 <= width -1 && y - 1 >= 0 && board[y - 1][x + 1] == ESquareState.Blank) // D
            {
                return true;
            }
        }
        return false;
    }
    public string PerformAiMove()
    {
        // AI is always PLAYER 2, Black
        var board = Game.CheckersState!.GameBoard;

        if (Game.GameOverAt != null)
        {
            return "gameOver";
        }
        var eatingList = new List<(int startX, int startY, int eatX, int eatY, int destX, int destY)>();
        var normalMovesList = new List<(int startX, int startY, int destX, int destY)>();
        // kontrolli, kas on mõni nupp millega peab käima
        for (int y = 0; y < board!.GetLength(0); y++)
        {
            for (int x = 0; x < board[0].Length; x++)
            {
                if (board[y][x] is ESquareState.Black)
                {
                    GetForcedCheckersMove(x, y, Game).ForEach(move => eatingList.Add(move));
                }

                if (board[y][x] is ESquareState.Black && eatingList.Count == 0)
                {
                    GetAllPossibleCheckersMoves(Game, x, y).ForEach(move =>
                    {
                        (int startX, int startY, int destX, int destY) possibleMove = (x, y, move.x, move.y);
                        normalMovesList.Add(possibleMove);
                    });
                }
            }
        }
        // JAH ->
        if (eatingList.Count > 0)
        {
            AiPerformRandomEatingMove(eatingList);
            return "AI";
        }
        
        AiPerformRandomMove(normalMovesList);
        return "AI.";
    }

    public void AiPerformRandomMove(List<(int startX, int startY, int destX, int destY)> moves)
    {
        var randomNumber = moves.Count == 1 ? 0 : RandomNumberGenerator.GetInt32(0, moves.Count);
        var choice = moves[randomNumber];
        MoveCheckersPiece(choice.startX, choice.startY, choice.destX, choice.destY);
    }

    public void AiPerformRandomEatingMove(
        List<(int startX, int startY, int eatX, int eatY, int destX, int destY)> moves)
    {
        var randomNumber = moves.Count == 1 ? 0 : RandomNumberGenerator.GetInt32(0, moves.Count);
        var choice = moves[randomNumber];
        EatButton(choice.startX, choice.startY, choice.eatX, choice.eatY, choice.destX, choice.destY);
    }
}
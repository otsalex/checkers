using ConsoleUI;
using DAL;
using DAL.Db;
using Domain;
using MenuSystem;
using GameBrain;
using Microsoft.EntityFrameworkCore;

// Create and load default game into brain on startup
CheckersGame game = new CheckersGame
{
    Name = "Default game",
    Player1Name = "player",
    Player1Type = EPlayerType.Human,
    Player2Name = "Bot",
    Player2Type = EPlayerType.Ai,
    CheckersOption = new CheckersOption
    {
        Name = "American Checkers",
        Width = 8,
        Height = 8,
        WhiteStarts = true
    }
};

CheckersBrain brain = new CheckersBrain(game);

var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite("Data source=C:/Users/Kasutaja/IdeaProjects/icd0008/checkers.db")
    .Options;
var ctx = new AppDbContext(dbOptions);


// set up options repos
IGameOptionsRepository optionsRepoF = new GameOptionsRepositoryFileSystem();
IGameOptionsRepository optionsRepoDb = new GameOptionsRepositoryDb(ctx);

IGameOptionsRepository optionsRepo = optionsRepoDb; // set on DB initially

// set up game repos
IGameRepository gamesRepoF = new GameRepositoryFileSystem();
IGameRepository gamesRepoDb = new GameRepositoryDb(ctx);

IGameRepository gamesRepo = gamesRepoDb; // set on DB initially

gamesRepoF.SaveGame("Default game", game);

// save preset options to filesystem(ones in Db are already saved manually)
optionsRepoF.SaveGameOptions(game.CheckersOption.Name, game.CheckersOption);
optionsRepoF.SaveGameOptions("International draughts", new CheckersOption
{
    Name = "International draughts",
    Width = 10,
    Height = 10,
    WhiteStarts = true
    // TODO lots of more options to set!
});

void LoadMenus()
{
    var optionsChoice = new Menu(EMenuLevel.Other, ">>--- OPTIONS ---<<", menuItems: ListGameOptionsAsMenuItems());
    var loadsChoice = new Menu(EMenuLevel.Other, ">>--- LOADS ---<<", menuItems: ListLoadsAsMenuItems());

    var loadsMenu = new Menu(EMenuLevel.Other,">>--- LOADS ---<<", menuItems: new List<MenuItem>()
    {
        new MenuItem("Save current game", SaveGame),
        new MenuItem("Load game", loadsChoice.RunMenu)
    });

    var optionsMenu = new Menu(EMenuLevel.Other,">>--- OPTIONS ---<<", menuItems: new List<MenuItem>()
    {
        new MenuItem("Create options", CreateNewOptions),
        new MenuItem("Load options", optionsChoice.RunMenu),
        new MenuItem("Persistence method swap", SwapPersistenceEngine),
    });

    var mainMenu = new Menu(EMenuLevel.Main,">>--- CHECKERS ---<<", menuItems: new List<MenuItem>()
    {
        new MenuItem("New Game", CreateGame),
        new MenuItem("Continue game", PlayGame),
        new MenuItem("Loads", loadsMenu.RunMenu),
        new MenuItem("Options", optionsMenu.RunMenu)
    });
    mainMenu.RunMenu();
}

LoadMenus();

MenuItem SwapPersistenceEngine()
{
    optionsRepo = optionsRepo == optionsRepoDb ? optionsRepoF : optionsRepoDb;
    gamesRepo = gamesRepo == gamesRepoDb ? gamesRepoF : gamesRepoDb;
    
    Console.WriteLine("Persistence engine:" + optionsRepo.Name);
    LoadMenus();
    return new MenuItem("Main menu", methodToRun:null);
}

MenuItem CreateGame()
{
    var loadedOptions = brain.Game.CheckersOption;
    var newGame = new CheckersGame
    {
        CheckersOption = loadedOptions
    };
    brain.Game = newGame;

    Console.WriteLine("Enter games name:");
    newGame.Name = Console.ReadLine()!;
    Console.WriteLine("Enter player 1 name:");
    newGame.Player1Name = Console.ReadLine()!;
    Console.WriteLine("Enter player 2 name:");
    newGame.Player2Name = Console.ReadLine()!;
    Console.WriteLine("Play against AI? Y/N");
    var opponent = Console.ReadLine();
    newGame.Player2Type = opponent?.ToUpper().Trim() switch
    {
        "N" => EPlayerType.Human,
        "Y" => EPlayerType.Ai,
        _ => EPlayerType.Ai
    };
    Console.WriteLine("Use default options(1) or loaded options(2)?");
    var options = Console.ReadLine();
    if (int.Parse(options!) == 1)
    {
        newGame.CheckersOption = optionsRepo.GetGameOptions("American Checkers");
    }
    else
    {
        newGame.CheckersOption = optionsRepo.GetGameOptions("American Checkers");
        Console.WriteLine("Add options manually after creating the game!");
    }
    
    new CheckersBrain(newGame);
    gamesRepo.SaveGame(newGame.Name, newGame);
    
    LoadMenus();
    LoadGameState(newGame.Name); // also starts the game
    return new MenuItem("Exit", methodToRun: null);
}

MenuItem PlayGame()
{
    var returnValue = "none";
    var doNext = "";
    
    while (returnValue != "done")
    {
        Console.Clear();
        
        Ui.DrawGameBoard(brain.Game);
        
        if (brain.GetManagerLastCursorPosition() != null)
        {
            var cursorCords = brain.GetManagerLastCursorPosition();
            Console.SetCursorPosition(cursorCords!.Value.x + 5, cursorCords.Value.y + 1);
        }
        else
        {
            Console.SetCursorPosition(0,0);
        }
        

        // get user input!
        returnValue = brain.StartCheckersGame();

        // If AI places a marker!
        if (returnValue == "AI")
        {
            continue;
        }
        
        // If user exits game
        if (returnValue == "save")
        {
            doNext = "save";
            returnValue = "done";
        }

        if (returnValue == "gameOver")
        {
            returnValue = "done";
            Console.Clear();
            Ui.DrawGameBoard(brain.Game);
            Console.SetCursorPosition(0, game.CheckersOption!.Height + 1);
            Console.WriteLine("Game over at " + game.GameOverAt);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Winner: " + game.GameWonByPlayer);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.ResetColor();
            Console.WriteLine("Press any key to continue..");
            gamesRepo.SaveGame(brain.Game.Id.ToString(), brain.Game);
            Console.ReadKey();
        }

        if (returnValue == "invalid turn, piece has to eat!")
        {
            DisplayError("Eating is mandatory!");
        }

        if (returnValue == "select correct piece!")
        {
            DisplayError("Select your piece before moving!");
        }
    }
    if (doNext == "save")
    {
        game = brain.Game;
        SaveGame();
    }

    return new MenuItem("Back", methodToRun:null);
}

List<MenuItem> ListGameOptionsAsMenuItems()
{
    var optionsList = new List<MenuItem>(); 
    foreach (var option in optionsRepo.GetGameOptionsList())
    {
        MenuItem Func() => LoadGameOptions(option);
        var o = new MenuItem(option, Func);
        optionsList.Add(o);
    }
    return optionsList;
}

MenuItem LoadGameOptions(string option)
{
    var newGame = new CheckersGame
    {
        Name = brain.Game.Name,
        CheckersOption = optionsRepo.GetGameOptions(option)
    };
    brain = new CheckersBrain(newGame);
    //brain.CreateStartingBoard(); // create new board because options changed!

    return new MenuItem("Main menu", methodToRun: null);
}

MenuItem CreateNewOptions()
{
    var boardHeight = GetBoardMeasures("height");
    var boardWidth = GetBoardMeasures("width");
    var whiteStarts = true;
    var startingPieceChosen = false;
    do
    {
        Console.WriteLine("White starts? Y/N");
        var option = Console.ReadLine();
        
        switch (option?.ToUpper().Trim())
        {
            case "N":
                whiteStarts = false;
                startingPieceChosen = true;
                break;
            case "Y":
                whiteStarts = true;
                startingPieceChosen = true;
                break;
            default:
                Console.WriteLine("Invalid choice. Try again!");
                break;
        }
        
    } while (startingPieceChosen == false);
    
    Console.WriteLine("Enter options name:");
    var optionsName = Console.ReadLine() ?? "new option";
    var options = new CheckersOption
    {
        Height = boardHeight,
        Width = boardWidth,
        WhiteStarts = whiteStarts
    };

    optionsRepo.SaveGameOptions(optionsName, options);

    LoadGameOptions(optionsName);
    LoadMenus();
    return new MenuItem("Exit", methodToRun: null);
}

int GetBoardMeasures(string s)
{
    var saved = false;
    var measure = 0;
    do
    {
        Console.WriteLine("Board " + s + ":");
        var optionWidth = Console.ReadLine() ?? "0";
        if (Int32.Parse(optionWidth) >= 3 &&
            Int32.Parse(optionWidth) <= 30)
        {
            measure = Int32.Parse(optionWidth);
            saved = true;
        }
        else
        {
            Console.WriteLine("Invalid choice. Try again!");
        }
    } while (saved == false);

    return measure;
}

MenuItem LoadGameState(string load)
{
    // load game and latest state to brain
    brain = new CheckersBrain(gamesRepo.GetGame(load));
    Console.Clear();
    return new MenuItem("Play Game", methodToRun: null);
}

List<MenuItem> ListLoadsAsMenuItems()
{
    var loadsList = new List<MenuItem>(); 
    foreach (var load in gamesRepo.GetGamesList())
    {
        MenuItem Func() => LoadGameState(load);
        var o = new MenuItem(load, Func);
        loadsList.Add(o);
    }
    return loadsList;
}

MenuItem SaveGame()
{
    gamesRepo.SaveGame(brain.Game.Name, brain.Game);
    LoadMenus();
    return new MenuItem("Exit", methodToRun:null);
}

void DisplayError(string msg)
{
    Console.SetCursorPosition(0, game.CheckersOption!.Height + 2);
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write(msg);
    Console.ResetColor();
    Console.SetCursorPosition(0,0);
    Thread.Sleep(1000);
}
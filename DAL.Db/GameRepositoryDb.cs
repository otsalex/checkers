using Domain;
using GameBrain;

namespace DAL.Db;

public class GameRepositoryDb : IGameRepository
{
    public string Name { get; } = "DB";
    private readonly AppDbContext _dbContext;

    public GameRepositoryDb(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public List<string> GetGamesList()
    {
        return _dbContext.CheckersGames
            .OrderBy(o => o.Name)
            .Select(o => o.Name)
            .ToList();
    }

    public CheckersGame GetGame(string id)
    {
        // Kui konsooli programm võtab DB-st nime järgi siis veebis võtab id järgi
        
        var game = _dbContext.CheckersGames.First(g => g.Name == id || g.Id.ToString() == id);
        
        var options = _dbContext.CheckersOptions.First(o => o.Id == game.CheckersOptionId);
        
        // Viimane state
        var state = _dbContext.CheckersGameStates.Where(s => s.CheckersGameId == game.Id)
            .OrderByDescending(s => s.CreatedAt).First();

        var board = System.Text.Json.JsonSerializer.Deserialize<ESquareState[][]>(state.BoardSerializedString!);
        
        game.CheckersOption = options;
        game.CheckersState = state;
        game.CheckersState.GameBoard = board;
        return game;
    }

    public void SaveGame(string gameName, CheckersGame game)
    {
        var jsonBoard = System.Text.Json.JsonSerializer.Serialize<ESquareState[][]>(game.CheckersState!.GameBoard!);
        game.CheckersState!.BoardSerializedString = jsonBoard;
        var gameFromDb = _dbContext.CheckersGames.FirstOrDefault(o => o.Name == gameName || o.Id.ToString() == gameName);
        // if its a new game!
        if (gameFromDb == null)
        {
            // If game only has one current session state!
            if (game.CheckersGameStates == null) 
            {
                game.CheckersGameStates = new List<CheckersGameState>
                {
                    game.CheckersState!
                };
                _dbContext.Add(game);
                _dbContext.SaveChanges();
                return;
            }
            
            // if game has more than one current session state!
            var entity = _dbContext.Add(game);
            var gameId = entity.Entity.Id;
            foreach (var state in game.CheckersGameStates)
            {
                jsonBoard = System.Text.Json.JsonSerializer.Serialize<ESquareState[][]>(state.GameBoard!);
                state.BoardSerializedString = jsonBoard;
                state.CheckersGameId = gameId;
                _dbContext.CheckersGameStates.Add(state);
            }
            _dbContext.SaveChanges();
            return;
        }
        if (game.CheckersGameStates != null)
        {
            var allStates = _dbContext.CheckersGameStates.Where(s => s.CheckersGameId == game.Id).ToList();
            
            foreach (var state in game.CheckersGameStates)
            {
                if (allStates.Contains(state))
                {
                    // if there are duplicate states
                    continue;
                }
                jsonBoard = System.Text.Json.JsonSerializer.Serialize<ESquareState[][]>(state.GameBoard!);
                state.BoardSerializedString = jsonBoard;
                state.CheckersGameId = gameFromDb.Id;
                _dbContext.CheckersGameStates.Add(state);
            }
        }
        _dbContext.SaveChanges();
    }

    public void DeleteGameById(string id)
    {
        if (id is "" or null)
        {
            return;
        }

        try
        {
            var gameFromDb = GetGame(id.Trim());
            _dbContext.CheckersGames.Remove(gameFromDb);
            _dbContext.SaveChanges();
        }
        catch (Exception)
        {
            Console.Clear();
            Console.WriteLine($"Game \"{id.Trim()}\" does not exist.");
            Thread.Sleep(2000);
        }
    }
}
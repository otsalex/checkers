using Domain;

namespace DAL.Db;

public class GameOptionsRepositoryDb : IGameOptionsRepository
{
    public string Name { get; } = "DB";
    private readonly AppDbContext _dbContext;

    public GameOptionsRepositoryDb(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<string> GetGameOptionsList()
    {
        return _dbContext.CheckersOptions
            .OrderBy(o => o.Name)
            .Select(o => o.Name)
            .ToList();
    }

    public CheckersOption GetGameOptions(string id)
    {
        return _dbContext.CheckersOptions.First(g => g.Name == id || g.Id.ToString() == id);
    }

    public void SaveGameOptions(string id, CheckersOption option)
    {
        var optionsFromDb = _dbContext.CheckersOptions.FirstOrDefault(o => o.Name == id);
        
        if (optionsFromDb == null)
        {
            _dbContext.CheckersOptions.Add(option);
            _dbContext.SaveChanges();
            return;
        }

        optionsFromDb.Name = option.Name;
        optionsFromDb.Width = option.Width;
        optionsFromDb.Height = option.Height;
        optionsFromDb.CaptureMandatory = option.CaptureMandatory;
        optionsFromDb.WhiteStarts = option.WhiteStarts;
        optionsFromDb.AllowCaptureBackwards = option.AllowCaptureBackwards;
        optionsFromDb.LongestCaptureMandatory = option.LongestCaptureMandatory;
        optionsFromDb.BecomesKingSameTurn = option.BecomesKingSameTurn;
        optionsFromDb.KingMovesByOne = option.KingMovesByOne;

        _dbContext.SaveChanges();
    }

    public void DeleteGameOptions(string id)
    {
        var optionsFromDb = GetGameOptions(id);
        _dbContext.CheckersOptions.Remove(optionsFromDb);
        _dbContext.SaveChanges();
    }

    public void CheckOrCreateDirectory()
    {
        // implemented because abstract class. fix it
    }
}
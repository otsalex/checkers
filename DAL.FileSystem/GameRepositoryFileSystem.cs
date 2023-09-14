using Domain;

namespace DAL;

public class GameRepositoryFileSystem : IGameRepository
{
    private const string FileExtension = "json";
    private readonly string _loadsDirectory = "." + Path.DirectorySeparatorChar + "loads";
    
    public List<string> GetGamesList()
    {
        if (!Directory.Exists(_loadsDirectory))
        {
            Directory.CreateDirectory(_loadsDirectory);
        }
        
        var res = new List<string>();

        foreach (var fileName in Directory.GetFileSystemEntries(_loadsDirectory, "*." + FileExtension))
        {
            res.Add(Path.GetFileNameWithoutExtension(fileName));
        }

        return res;
    }

    public CheckersGame GetGame(string id)
    {
        var fileName = _loadsDirectory + Path.DirectorySeparatorChar + id + "." + FileExtension;
        var fileContent = File.ReadAllText(fileName);
        var load = System.Text.Json.JsonSerializer.Deserialize<CheckersGame>(fileContent);
        if (load == null)
            throw new NullReferenceException("Could not serialize");
        return load;
    }

    public void SaveGame(string gameName, CheckersGame game)
    {
        if (!Directory.Exists(_loadsDirectory))
        {
            Directory.CreateDirectory(_loadsDirectory);
        }
        
        var fileContent = System.Text.Json.JsonSerializer.Serialize(game);
        File.WriteAllText(GetFileName(gameName), fileContent);
    }

    public void DeleteGameById(string id)
    {
        File.Delete(GetFileName(id));
    }


    private string GetFileName(string id)
    {
        return _loadsDirectory + Path.DirectorySeparatorChar + id + "." + FileExtension;
    }
}

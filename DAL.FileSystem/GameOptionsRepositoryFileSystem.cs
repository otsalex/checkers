using Domain;

namespace DAL;

public class GameOptionsRepositoryFileSystem : IGameOptionsRepository
{
    public string Name { get; } = "FileSystem";
    private const string FileExtension = "json";
    private readonly string _optionsDirectory = "." + System.IO.Path.DirectorySeparatorChar + "options";

    public List<string> GetGameOptionsList()
    {
        if (!System.IO.Directory.Exists(_optionsDirectory))
        {
            System.IO.Directory.CreateDirectory(_optionsDirectory);
        }
        
        var res = new List<string>();

        foreach (var fileName in System.IO.Directory.GetFileSystemEntries(_optionsDirectory, "*." + FileExtension))
        {
            res.Add(Path.GetFileNameWithoutExtension(fileName));
        }

        return res;
    }

    
    public CheckersOption GetGameOptions(string id)
    {
        var fileName = _optionsDirectory + Path.DirectorySeparatorChar + id + "." + FileExtension;
        var fileContent = File.ReadAllText(fileName);
        var options = System.Text.Json.JsonSerializer.Deserialize<CheckersOption>(fileContent);
        if (options == null)
            throw new NullReferenceException("Could not serialize");
        return options!;
    }

    public void SaveGameOptions(string id, CheckersOption option)
    {
        if (!System.IO.Directory.Exists(_optionsDirectory))
        {
            System.IO.Directory.CreateDirectory(_optionsDirectory);
        }
        
        var fileContent = System.Text.Json.JsonSerializer.Serialize(option);
        File.WriteAllText(GetFileName(id), fileContent);
    }

    public void DeleteGameOptions(string id)
    {
        File.Delete(GetFileName(id));
    }

    private string GetFileName(string id)
    {
        return _optionsDirectory + Path.DirectorySeparatorChar + id + "." + FileExtension;
    }
    
}
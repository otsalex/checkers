using Domain;

namespace DAL;

public interface IGameOptionsRepository
{
    string Name { get; }

    // read
    List<string> GetGameOptionsList();
    CheckersOption GetGameOptions(string id);

    // create and update
    void SaveGameOptions(string id, CheckersOption option);

    // delete
    void DeleteGameOptions(string id);
    
}
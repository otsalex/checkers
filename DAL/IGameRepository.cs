using Domain;

namespace DAL;

public interface IGameRepository
{
    List<string> GetGamesList();
    
    CheckersGame GetGame(string id);

    void SaveGame(string gameName, CheckersGame game);
    void DeleteGameById(string id);

}
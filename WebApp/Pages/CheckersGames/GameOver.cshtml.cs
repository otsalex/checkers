using DAL;
using DAL.Db;
using Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Games;

public class GameOver : PageModel
{
    public IGameRepository GameRepository { get; set; }
    public CheckersGame? Game { get; set; }

    public GameOver(IGameRepository gameRepository)
    {
        GameRepository = gameRepository;
    }
    
    public void OnGet()
    {
        var id = HttpContext.Request.Query["id"].ToString();
        var game = GameRepository.GetGame(id);
        Game = game;
    }
}
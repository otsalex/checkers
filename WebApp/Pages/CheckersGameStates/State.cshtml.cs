using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL.Db;
using Domain;

namespace WebApp.Pages.CheckersGameStates;

public class StateModel : PageModel
{
    private readonly DAL.Db.AppDbContext _context;
    
    public StateModel(DAL.Db.AppDbContext context)
    {
        _context = context;
    }

    public IList<CheckersGameState> CheckersGameState { get;set; } = default!;
    public CheckersGame? Game { get; set; }
    public async Task OnGetAsync(int id)
    {
        if (_context.CheckersGameStates != null)
        {
            CheckersGameState = await _context.CheckersGameStates
                .Include(c => c.Game)
                .Where(c => c.CheckersGameId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();;
        }

        Game = _context.CheckersGames.First(g => g.Id == id);
    }
}
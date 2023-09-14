using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Domain;

namespace WebApp.Pages.CheckersGameStates
{
    public class IndexModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public IndexModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

        public IList<CheckersGameState> CheckersGameState { get;set; } = default!;
        public IList<CheckersGame> CheckersGame { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.CheckersGameStates != null)
            {
                CheckersGameState = await _context.CheckersGameStates
                    .Include(c => c.Game).ToListAsync();;
            }
            if (_context.CheckersGames != null)
            {
                CheckersGame = await _context.CheckersGames.ToListAsync();;
            }
        }
    }
}

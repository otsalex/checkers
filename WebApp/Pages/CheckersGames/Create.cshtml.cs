using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Domain;
using DAL;
using GameBrain;

namespace WebApp.Pages.CheckersGames
{
    public class CreateModel : PageModel
    {
        private readonly IGameRepository _gameRepository;
        private List<CheckersOption> _checkersOptions = new List<CheckersOption>();
        private readonly DAL.Db.AppDbContext _context;

        public CreateModel(IGameRepository repoContext, DAL.Db.AppDbContext context)
        {
            _gameRepository = repoContext;
            _context = context;
        }

        public IActionResult OnGet()
        {
            OptionsSelectList = new SelectList(_context.CheckersOptions, "Id", "Name");
            return Page();
        }

        [BindProperty]
        public CheckersGame CheckersGame { get; set; } = default!;
        

        public SelectList OptionsSelectList { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            // At first game has no state. Generate initial board!
            var options = _context.CheckersOptions.FirstOrDefault(x => x.Id == CheckersGame.CheckersOptionId);
            CheckersGame.CheckersOption = options;
            new CheckersBrain(CheckersGame);

            if (!ModelState.IsValid || CheckersGame == null)
            {
                return Page();
            }
            _gameRepository.SaveGame(CheckersGame.Name, CheckersGame);
            return Redirect("./Play?id=" + CheckersGame.Id);
        }
    }
}

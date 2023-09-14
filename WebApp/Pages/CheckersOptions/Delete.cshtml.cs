using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL.Db;
using Domain;

namespace WebApp.Pages.CheckersOptions
{
    public class DeleteModel : PageModel
    {
        private readonly DAL.Db.AppDbContext _context;

        public DeleteModel(DAL.Db.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
      public CheckersOption CheckersOption { get; set; } = default!;
      public string? Msg { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id, string? msg)
        {
            if (msg != null)
            {
                Msg = msg;
            }

            if (id == null || _context.CheckersOptions == null)
            {
                return NotFound();
            }

            var checkersoption = await _context.CheckersOptions.FirstOrDefaultAsync(m => m.Id == id);

            if (checkersoption == null)
            {
                return NotFound();
            }
            else 
            {
                CheckersOption = checkersoption;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.CheckersOptions == null)
            {
                return NotFound();
            }
            var checkersoption = await _context.CheckersOptions.FindAsync(id);
            try
            {
                var game = _context.CheckersGames.First(g => g.CheckersOptionId == id);
                if (game != null)
                {
                    return Redirect("?msg=error&id=" + id);
                }
            }
            catch(Exception)
            {
                if (checkersoption != null)
                {
                    CheckersOption = checkersoption;
                    _context.CheckersOptions.Remove(CheckersOption);
                    await _context.SaveChangesAsync();
                }

                return RedirectToPage("./Index");   
            }

            return Page();
        }
    }
}

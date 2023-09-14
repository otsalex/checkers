using System.Net.Mime;
using DAL;
using DAL.Db;

using Domain;
using GameBrain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Pages.CheckersGames;

public class PlayModel : PageModel
{
        private IGameRepository GameRepository { get; set; }
        public CheckersGame Game = default!;
        public List<(int x, int y)> PossibleMoves = new List<(int x, int y)>();
        public CheckersBrain Brain = default!;
        public bool EatIsMandatory { get; set; }
        public List<(int startX, int startY, int eatX, int eatY, int destX, int destY)> EatingCoordinates =
            new List<(int startX, int startY, int eatX, int eatY, int destX, int destY)>();
        private readonly DAL.Db.AppDbContext _context;
    

        public PlayModel(IGameRepository gameRepository, DAL.Db.AppDbContext context)
        {
            GameRepository = gameRepository;
            _context = context;
        }
        
        public ActionResult OnGet(string id, int? x, int? y, int? startX, int? startY,
            int? destX, int? destY, int? eatX, int? eatY, string? eatDone, string? stateId, string? gameId)
        {
            // for starting game from loads
            if (stateId != null)
            {
                Game = GameRepository.GetGame(gameId!);
                Game.CheckersState = _context.CheckersGameStates.First(m => m.Id == Int32.Parse(stateId));
                var gameBoard = System.Text.Json.JsonSerializer.Deserialize<ESquareState[][]>(Game.CheckersState.BoardSerializedString!);
                Game.CheckersState.GameBoard = gameBoard;
            }
            else
            {
                Game = GameRepository.GetGame(id);
            }

            
            Brain = new CheckersBrain(Game);
            
            if (Brain.IsGameOver())
            {
                return Redirect("GameOver?id="+Game.Id);
            }
            IsEatMandatory(Game);

        if (Brain.Game.Player2Type == EPlayerType.Ai && !Game.CheckersState!.NextMoveByWhite) {
         
         Brain.PerformAiMove();
         GameRepository.SaveGame(Brain.Game.Id.ToString(), Brain.Game);
         // Because AI is always player 2.
         return Redirect("Play?id=" + Game.Id);
        }

        if (startX.HasValue && startY.HasValue && eatX.HasValue && eatY.HasValue && destX.HasValue && destY.HasValue)
        {
                Brain.EatButton(startX.Value, startY.Value,
                    eatX.Value, eatY.Value,
                    destX.Value, destY.Value);
                
                Brain.IsGameOver();

                GameRepository.SaveGame(Game.Id.ToString(), Game);
                return Redirect("Play?id=" + Game.Id); 
        }
        if (startX.HasValue && startY.HasValue && destX.HasValue && destY.HasValue)
        {
            Brain.MoveCheckersPiece(startX.Value, startY.Value, destX.Value, destY.Value);
            GameRepository.SaveGame(Game.Id.ToString(), Game);
            
            return Redirect("Play?id=" + Game.Id);
        }
        if (x.HasValue && y.HasValue)
        {
            PossibleMoves = Brain.GetAllPossibleCheckersMoves(Game, x.Value, y.Value);
            Console.WriteLine("CURRENT TURN IS - " + (Game.CheckersState!.NextMoveByWhite ? ESquareState.White : ESquareState.Black));
            return Page();
        }


        return Page();
        }

        public void IsEatMandatory(CheckersGame game)
        {
            var board = game.CheckersState!.GameBoard;
            EatIsMandatory = false;
            for (int y = 0; y < board!.Length; y++)
            {
                for (int x = 0; x < board[0].Length; x++)
                {
                    // Kui on mõni nupp, mis PEAB sööma
                    if (Brain.GetForcedCheckersMove(x, y, game).Count > 0)
                    {
                        foreach (var eatCordinatesSet in Brain.GetForcedCheckersMove(x, y, game))
                        {
                            EatingCoordinates.Add(eatCordinatesSet);
                            EatIsMandatory = true;
                        }
                    }
                }
            }
        }
        public ActionResult OnPost(string id, int x, int y)
        {
            Game = GameRepository.GetGame(id);
            if (Brain.GetAllPossibleCheckersMoves(Game, x, y).Count != 0)
            {
                return Page();
            }
            return RedirectToPage("WebApp/Pages/Error.cshtml");
        }
    }

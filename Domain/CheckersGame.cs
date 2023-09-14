using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain;

public class CheckersGame
{
     [Display(Name = "Game name")]
     public String Name { get; set; } = default!;
     public int Id { get; set; }
     [Display(Name="Game started at")]
     public DateTime StartedAt { get; set; } = DateTime.Now;
     [Display(Name="Game finished at")]
     public  DateTime? GameOverAt { get; set; }
     [Display(Name="Game won by")]
     public string? GameWonByPlayer { get; set; }
     
     [Display(Name="Player 1 name")]
     [MaxLength(128)]
     public string Player1Name { get; set; } = default!;
     public EPlayerType Player1Type { get; set; }
     
     [Display(Name="Player 2 name")]
     [MaxLength(128)]
     public string Player2Name { get; set; } = default!;
     
     [Display(Name="Player 2 type")]
     public EPlayerType Player2Type { get; set; }

     [ForeignKey("CheckersOptionsId")] 
     public int CheckersOptionId { get; set; }
     [Display(Name="Checkers options name")]
     public CheckersOption? CheckersOption { get; set; }
     
     [NotMapped]
     public CheckersGameState? CheckersState { get; set; }
     
     public ICollection<CheckersGameState>? CheckersGameStates { get; set; }
}
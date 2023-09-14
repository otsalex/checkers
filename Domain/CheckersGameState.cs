
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using GameBrain;

namespace Domain;

public class CheckersGameState
{
    public int Id { get; set; }
    [ForeignKey("CheckersGameId")] 
    public int? CheckersGameId { get; set; }
    
    [Display(Name="Load saved at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? BoardSerializedString { get; set; }
    
    [NotMapped] 
    public ESquareState[][]? GameBoard { get; set; }
    
    [Display(Name="Next move by white piece")]
    public bool NextMoveByWhite { get; set; }
    public CheckersGame? Game { get; set; }
    public string ToString()
    {
        int white = 0;
        int black = 0;
        for (int i = 0; i < GameBoard!.Length; i++)
        {
            for (int j = 0; j < GameBoard![0].Length; j++)
            {
                if (GameBoard![i][j] == ESquareState.White) white++;
                if (GameBoard![i][j] == ESquareState.Black) black++;
            }
        }

        return $"{white} White : {black} Black";
    }

}
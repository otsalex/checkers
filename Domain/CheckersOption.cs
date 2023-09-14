using System.ComponentModel.DataAnnotations;
namespace Domain;

public class CheckersOption
{
    public int Id { get; set; }
    
    [Required]
    [MinLength(1)]
    public string Name { get; set; } = "option";
    [Required]
    [Range(6, 30)]
    public int Width { get; set; } = 8;
    [Required]
    [Range(6, 99)]
    public int Height { get; set; } = 8;

    [Display(Name="White starts")]
    public bool WhiteStarts { get; set; } = true;
    
    [Display(Name="Capture is mandatory")]
    public bool CaptureMandatory  { get; set; } = true;
    [Display(Name="Longest Capture Mandatory")]
    public bool LongestCaptureMandatory { get; set; } = true;
    [Display(Name="Becomes King Same Turn")]
    
    public bool BecomesKingSameTurn { get; set; } = true;
    [Display(Name="Allow Capture Backwards")]
    
    public bool AllowCaptureBackwards { get; set; } = true;
    [Display(Name="King Moves By One")]
    
    public  bool KingMovesByOne { get; set; } = true;
    
    public ICollection<CheckersGame>? CheckersGames { get; set; }

    public override string ToString()
    {
        return $"Board: {Height}x{Width} White Starts: {WhiteStarts}";
    }


}   
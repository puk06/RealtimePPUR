namespace RealtimePPUR.Models;

public class RuntimeSettings
{
    public bool EnableLossMode { get; set; } = true;
    public int CalculationInterval { get; set; } = 15;
    public string CustomSongsFolder { get; set; } = string.Empty;

    public bool EnableOverlay { get; set; } = true;
    public int OverlayLeft { get; set; } = 0;
    public int OverlayTop { get; set; } = 80;
    
    public InGameOverlayValues InGameOverlayValues { get; set; } =
        InGameOverlayValues.StarRatings |
        InGameOverlayValues.CurrentPerformancePoint |
        InGameOverlayValues.OffsetHelp |
        InGameOverlayValues.RemainingNotes;
}

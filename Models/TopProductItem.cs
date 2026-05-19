namespace PharmaDesk.Models;

public class TopProductItem
{
    public int    Rank        { get; set; }
    public string ImagePath   { get; set; } = string.Empty;
    public string Name        { get; set; } = string.Empty;
    public int    UnitsSold   { get; set; }
    public int    MaxUnits    { get; set; }
    public double Progress    => MaxUnits > 0 ? (double)UnitsSold / MaxUnits : 0;
}

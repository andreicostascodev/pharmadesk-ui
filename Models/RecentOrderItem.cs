using System;

namespace PharmaDesk.Models;

public class RecentOrderItem
{
    public string   OrderId    { get; set; } = string.Empty;
    public string   Customer   { get; set; } = string.Empty;
    public string   Initials   { get; set; } = string.Empty;
    public int      ItemCount  { get; set; }
    public decimal  Total      { get; set; }
    public string   Status     { get; set; } = string.Empty;
    public DateTime CreatedAt  { get; set; }
    public string   TimeAgo    => CreatedAt.ToString("dd MMM yyyy HH:mm");
}

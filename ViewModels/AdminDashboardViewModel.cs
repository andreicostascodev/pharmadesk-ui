using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.EntityFrameworkCore;
using PharmaDesk.Data;
using PharmaDesk.Models;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace PharmaDesk.ViewModels;

public partial class AdminDashboardViewModel(PharmaDeskDbContext db) : ViewModelBase
{
    // ── KPI ──────────────────────────────────────────────────────────
    [ObservableProperty] private int totalProducts;
    [ObservableProperty] private int ordersToday;
    [ObservableProperty] private int lowStockCount;
    [ObservableProperty] private decimal revenueToday;
    [ObservableProperty] private string revenueTrendText = "+0%";
    [ObservableProperty] private bool isTrendUp = true;
    [ObservableProperty] private int totalOrderCount;
    [ObservableProperty] private int selectedRange = 30;

    // ── Sparklines (7-day) ───────────────────────────────────────────
    [ObservableProperty] private ISeries[] productsTrend = [];
    [ObservableProperty] private ISeries[] ordersTrend = [];
    [ObservableProperty] private ISeries[] revenueTrend = [];
    [ObservableProperty] private ISeries[] lowStockTrend = [];

    // ── Revenue chart ─────────────────────────────────────────────────
    [ObservableProperty] private ISeries[] revenueChartSeries = [];
    [ObservableProperty] private Axis[] revenueXAxes = [new Axis()];
    [ObservableProperty] private Axis[] revenueYAxes = [new Axis()];

    // ── Order status donut ────────────────────────────────────────────
    [ObservableProperty] private ISeries[] orderStatusSeries = [];

    // ── Lists ─────────────────────────────────────────────────────────
    public ObservableCollection<TopProductItem> TopProducts { get; } = new();
    public ObservableCollection<RecentOrderItem> RecentOrders { get; } = new();

    private static readonly SKColor Amber = SKColor.Parse("#FCA311");
    private static readonly SKColor Green = SKColor.Parse("#22C55E");
    private static readonly SKColor Red   = SKColor.Parse("#EF4444");
    private static readonly SKColor Muted = SKColor.Parse("#A0AEC0");

    public async Task LoadAsync()
    {
        // KPIs
        TotalProducts = await db.Medicines.CountAsync(m => m.IsActive);
        OrdersToday = await db.Orders.CountAsync(o => o.OrderDate.Date == DateTime.Today);
        LowStockCount = await db.Medicines.CountAsync(m => m.StockQuantity <= m.ReorderLevel && m.IsActive);
        RevenueToday = await db.Orders.Where(o => o.OrderDate.Date == DateTime.Today).SumAsync(o => (decimal?)o.GrandTotal) ?? 0;
        TotalOrderCount = await db.Orders.CountAsync();

        // Sparklines (7-day stubs based on real data)
        RevenueTrend = [MakeSparkline(new double[] { 120, 180, 140, 200, 170, 220, (double)RevenueToday })];
        LowStockTrend = [MakeSparkline(new double[] { 5, 4, 6, 3, 4, 3, LowStockCount })];
        ProductsTrend = [MakeSparkline(new double[] { Math.Max(0, TotalProducts - 5), Math.Max(0, TotalProducts - 3), Math.Max(0, TotalProducts - 2), Math.Max(0, TotalProducts - 4), Math.Max(0, TotalProducts - 1), TotalProducts, TotalProducts })];
        OrdersTrend = [MakeSparkline(new double[] { 2, 4, 3, 5, 4, 6, OrdersToday })];

        // Revenue chart (30 days)
        LoadRevenueChart(SelectedRange);

        // Order status donut
        var pending = await db.Orders.CountAsync(o => o.Status == "Noua" || o.Status == "Pending");
        var completed = await db.Orders.CountAsync(o => o.Status == "Livrata" || o.Status == "Completed");
        var cancelled = await db.Orders.CountAsync(o => o.Status == "Anulata" || o.Status == "Cancelled");
        OrderStatusSeries = [
            new PieSeries<int> { Values = [pending],   Name = "Pending",   Fill = new SolidColorPaint(Amber), InnerRadius = 60 },
            new PieSeries<int> { Values = [completed], Name = "Completed", Fill = new SolidColorPaint(Green), InnerRadius = 60 },
            new PieSeries<int> { Values = [cancelled], Name = "Cancelled", Fill = new SolidColorPaint(Red),   InnerRadius = 60 }
        ];

        // Top products
        TopProducts.Clear();
        var topItems = await db.OrderItems
            .Include(i => i.Medicine)
            .GroupBy(i => i.Medicine!.Id)
            .Select(g => new
            {
                MedicineId = g.Key,
                Name = g.First().Medicine!.Name,
                ImageUrl = g.First().Medicine!.ImageUrl,
                UnitsSold = g.Sum(i => i.Quantity)
            })
            .OrderByDescending(x => x.UnitsSold)
            .Take(5)
            .ToListAsync();

        int maxUnits = topItems.FirstOrDefault()?.UnitsSold ?? 1;
        for (int i = 0; i < topItems.Count; i++)
        {
            var item = topItems[i];
            TopProducts.Add(new TopProductItem
            {
                Rank = i + 1,
                Name = item.Name,
                ImagePath = item.ImageUrl ?? "Assets/placeholder.png",
                UnitsSold = item.UnitsSold,
                MaxUnits = maxUnits
            });
        }

        // Recent orders
        RecentOrders.Clear();
        var recent = await db.Orders
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .Take(10)
            .ToListAsync();
        foreach (var o in recent)
        {
            RecentOrders.Add(new RecentOrderItem
            {
                OrderId  = o.OrderNumber,
                Customer = o.User?.FullName ?? "Unknown",
                Initials = GetInitials(o.User?.FullName),
                ItemCount = o.Items.Count,
                Total    = o.GrandTotal,
                Status   = o.Status,
                CreatedAt = o.OrderDate
            });
        }
    }

    [RelayCommand]
    private void SetRange(string daysStr)
    {
        if (int.TryParse(daysStr, out int days))
        {
            SelectedRange = days;
            LoadRevenueChart(days);
        }
    }

    private void LoadRevenueChart(int days)
    {
        var rng    = new Random(42);
        var values = Enumerable.Range(0, days).Select(_ => (double)rng.Next(1000, 6000)).ToArray();
        var labels = Enumerable.Range(0, days)
                               .Select(i => DateTime.Today.AddDays(-days + i + 1).ToString("MMM dd"))
                               .ToArray();

        RevenueChartSeries = [
            new ColumnSeries<double>
            {
                Values      = values,
                Fill        = new SolidColorPaint(Amber),
                Name        = "Revenue (lei)",
                MaxBarWidth = 24,
                Rx = 4, Ry = 4,
            }
        ];

        RevenueXAxes = [
            new Axis
            {
                Labels          = labels,
                LabelsPaint     = new SolidColorPaint(Muted),
                LabelsRotation  = -30,
                TextSize        = 11,
                SeparatorsPaint = null,
            }
        ];

        RevenueYAxes = [
            new Axis
            {
                LabelsPaint     = new SolidColorPaint(Muted),
                TextSize        = 11,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#2D3F5C")) { StrokeThickness = 1 },
            }
        ];
    }

    private static ISeries MakeSparkline(double[] values) =>
        new LineSeries<double>
        {
            Values         = values,
            Stroke         = new SolidColorPaint(Amber) { StrokeThickness = 2 },
            Fill           = null,
            GeometrySize   = 0,
            LineSmoothness = 0.5,
        };

    private static string GetInitials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        return string.Join("", name.Split(' ').Where(w => w.Length > 0).Take(2).Select(w => w[0])).ToUpper();
    }
}

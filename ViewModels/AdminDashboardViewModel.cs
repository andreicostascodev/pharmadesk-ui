using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using PharmaDesk.Models;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace PharmaDesk.ViewModels;

public partial class AdminDashboardViewModel : ObservableObject
{
    // ── KPI ──────────────────────────────────────────────────────────
    [ObservableProperty] private int     totalProducts;
    [ObservableProperty] private int     ordersToday;
    [ObservableProperty] private int     lowStockCount;
    [ObservableProperty] private decimal revenueToday;
    [ObservableProperty] private string  revenueTrendText = "+0%";
    [ObservableProperty] private bool    isTrendUp        = true;

    // ── Sparklines (7-day) ───────────────────────────────────────────
    [ObservableProperty] private ISeries[] productsTrend = [];
    [ObservableProperty] private ISeries[] ordersTrend   = [];
    [ObservableProperty] private ISeries[] revenueTrend  = [];
    [ObservableProperty] private ISeries[] lowStockTrend = [];

    // ── Revenue chart ─────────────────────────────────────────────────
    [ObservableProperty] private ISeries[] revenueChartSeries = [];
    [ObservableProperty] private Axis[]    revenueXAxes       = [];
    [ObservableProperty] private Axis[]    revenueYAxes       = [];
    [ObservableProperty] private int       selectedRange       = 30;

    // ── Order status donut ────────────────────────────────────────────
    [ObservableProperty] private ISeries[] orderStatusSeries = [];
    [ObservableProperty] private int       totalOrderCount;

    // ── Lists ─────────────────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<TopProductItem>  topProducts  = [];
    [ObservableProperty] private ObservableCollection<RecentOrderItem> recentOrders = [];

    private static readonly SKColor Amber = SKColor.Parse("#FCA311");
    private static readonly SKColor Green = SKColor.Parse("#22C55E");
    private static readonly SKColor Red   = SKColor.Parse("#EF4444");
    private static readonly SKColor Muted = SKColor.Parse("#A0AEC0");

    public AdminDashboardViewModel()
    {
        LoadData(30);
    }

    [RelayCommand]
    private void SetRange(int days)
    {
        SelectedRange = days;
        LoadRevenueChart(days);
    }

    private void LoadData(int rangeDays)
    {
        TotalProducts   = 42;
        OrdersToday     = 18;
        LowStockCount   = 3;
        RevenueToday    = 4_280.50m;
        RevenueTrendText = "+12.4%";
        IsTrendUp        = true;
        TotalOrderCount  = 284;

        ProductsTrend = MakeSparkline(new double[] { 38, 39, 40, 40, 41, 41, 42 });
        OrdersTrend   = MakeSparkline(new double[] { 12, 15, 10, 18, 14, 16, 18 });
        RevenueTrend  = MakeSparkline(new double[] { 3100, 3800, 2900, 4100, 3700, 4000, 4280 });
        LowStockTrend = MakeSparkline(new double[] { 5, 4, 6, 3, 4, 3, 3 });

        LoadRevenueChart(rangeDays);
        LoadOrderStatusDonut();
        LoadTopProducts();
        LoadRecentOrders();
    }

    private void LoadRevenueChart(int days)
    {
        var rng    = new Random(42);
        var values = Enumerable.Range(0, days).Select(_ => (double)rng.Next(1000, 6000)).ToArray();
        var labels = Enumerable.Range(0, days)
                               .Select(i => DateTime.Today.AddDays(-days + i + 1).ToString("MMM dd"))
                               .ToArray();

        RevenueChartSeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Values      = values,
                Fill        = new SolidColorPaint(Amber),
                Name        = "Revenue (lei)",
                MaxBarWidth = 24,
                Rx = 4, Ry = 4,
            }
        };

        RevenueXAxes = new Axis[]
        {
            new Axis
            {
                Labels         = labels,
                LabelsPaint    = new SolidColorPaint(Muted),
                LabelsRotation = -30,
                TextSize       = 11,
                SeparatorsPaint = null,
            }
        };

        RevenueYAxes = new Axis[]
        {
            new Axis
            {
                LabelsPaint     = new SolidColorPaint(Muted),
                TextSize        = 11,
                SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#2D3F5C"))
                                  { StrokeThickness = 1 },
            }
        };
    }

    private void LoadOrderStatusDonut()
    {
        OrderStatusSeries = new ISeries[]
        {
            new PieSeries<double>
            {
                Name        = "Pending",
                Values      = new double[] { 84 },
                Fill        = new SolidColorPaint(Amber),
                InnerRadius = 60,
            },
            new PieSeries<double>
            {
                Name        = "Completed",
                Values      = new double[] { 176 },
                Fill        = new SolidColorPaint(Green),
                InnerRadius = 60,
            },
            new PieSeries<double>
            {
                Name        = "Cancelled",
                Values      = new double[] { 24 },
                Fill        = new SolidColorPaint(Red),
                InnerRadius = 60,
            },
        };
    }

    private void LoadTopProducts()
    {
        TopProducts = new ObservableCollection<TopProductItem>
        {
            new() { Rank=1, Name="Paracetamol Forte", UnitsSold=847, MaxUnits=900,
                    ImagePath="Assets/Products/paracetamol-forte.png" },
            new() { Rank=2, Name="Omega-3",           UnitsSold=723, MaxUnits=900,
                    ImagePath="Assets/Products/omega-3.png" },
            new() { Rank=3, Name="Ibuprofen Rapid",   UnitsSold=612, MaxUnits=900,
                    ImagePath="Assets/Products/ibuprofen-rapid.png" },
            new() { Rank=4, Name="Vitamina C + Zinc", UnitsSold=598, MaxUnits=900,
                    ImagePath="Assets/Products/vitamina-c-zinc.png" },
            new() { Rank=5, Name="Magneziu B6",       UnitsSold=504, MaxUnits=900,
                    ImagePath="Assets/Products/magneziu-b6.png" },
        };
    }

    private void LoadRecentOrders()
    {
        RecentOrders = new ObservableCollection<RecentOrderItem>
        {
            new() { OrderId="#001284", Customer="Maria Ionescu",    Initials="MI",
                    ItemCount=3, Total=245.00m,  Status="Completed",
                    CreatedAt=DateTime.Now.AddHours(-2) },
            new() { OrderId="#001283", Customer="Alexandru Popa",   Initials="AP",
                    ItemCount=1, Total=89.50m,   Status="Pending",
                    CreatedAt=DateTime.Now.AddHours(-4) },
            new() { OrderId="#001282", Customer="Elena Dumitrescu", Initials="ED",
                    ItemCount=5, Total=412.00m,  Status="Completed",
                    CreatedAt=DateTime.Now.AddHours(-6) },
            new() { OrderId="#001281", Customer="Ion Constantin",   Initials="IC",
                    ItemCount=2, Total=178.00m,  Status="Cancelled",
                    CreatedAt=DateTime.Now.AddDays(-1) },
            new() { OrderId="#001280", Customer="Gabriela Stan",    Initials="GS",
                    ItemCount=4, Total=320.50m,  Status="Completed",
                    CreatedAt=DateTime.Now.AddDays(-1) },
        };
    }

    private static ISeries[] MakeSparkline(double[] values) => new ISeries[]
    {
        new LineSeries<double>
        {
            Values         = values,
            Stroke         = new SolidColorPaint(Amber) { StrokeThickness = 2 },
            Fill           = null,
            GeometrySize   = 0,
            LineSmoothness = 0.5,
        }
    };
}

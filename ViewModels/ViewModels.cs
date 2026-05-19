using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using PharmaDesk.Data;
using PharmaDesk.Models;
using PharmaDesk.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace PharmaDesk.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string title = string.Empty;
}

public class ProductSection(string name, IEnumerable<Medicine> products)
{
    public string Name { get; } = name;
    public ObservableCollection<Medicine> Products { get; } = new(products);
}

public partial class MainViewModel(IServiceProvider provider, AppSession session, IToastService toast) : ViewModelBase
{
    [ObservableProperty] private ViewModelBase? currentViewModel;
    [ObservableProperty] private string toastMessage = string.Empty;
    [ObservableProperty] private bool isToastVisible;
    [ObservableProperty] private bool isDarkTheme;
    [ObservableProperty] private bool isSidebarExpanded = true;

    public bool IsLoggedIn => session.IsAuthenticated;
    public bool IsAdmin => session.IsAdmin;
    public bool CanManageMedicines => session.CanManageMedicines;
    public string CurrentUserName => session.CurrentUser?.FullName ?? "Vizitator";

    public void Initialize()
    {
        Title = "PharmaDesk";
        toast.ToastRaised += ShowToast;
        CurrentViewModel = provider.GetRequiredService<LoginViewModel>();
    }

    [RelayCommand] private void GoLogin() => CurrentViewModel = provider.GetRequiredService<LoginViewModel>();
    [RelayCommand] private void GoRegister() => CurrentViewModel = provider.GetRequiredService<RegisterViewModel>();
    [RelayCommand] private async Task GoHome() { var vm = provider.GetRequiredService<UserHomeViewModel>(); await vm.LoadAsync(); CurrentViewModel = vm; RefreshChrome(); }
    [RelayCommand] private async Task GoCart() { var vm = provider.GetRequiredService<CartViewModel>(); await vm.LoadAsync(); CurrentViewModel = vm; RefreshChrome(); }
    [RelayCommand] private async Task GoOrders() { var vm = provider.GetRequiredService<OrderHistoryViewModel>(); await vm.LoadAsync(); CurrentViewModel = vm; RefreshChrome(); }
    [RelayCommand] private async Task GoProfile() { var vm = provider.GetRequiredService<ProfileViewModel>(); await vm.LoadAsync(); CurrentViewModel = vm; RefreshChrome(); }
    [RelayCommand] private async Task GoAdminDashboard() { var vm = provider.GetRequiredService<AdminDashboardViewModel>(); await vm.LoadAsync(); CurrentViewModel = vm; RefreshChrome(); }
    [RelayCommand] private async Task GoProducts() { var vm = provider.GetRequiredService<ProductManagementViewModel>(); await vm.LoadAsync(); CurrentViewModel = vm; RefreshChrome(); }
    [RelayCommand] private async Task GoCategories() { var vm = provider.GetRequiredService<CategoryManagementViewModel>(); await vm.LoadAsync(); CurrentViewModel = vm; RefreshChrome(); }
    [RelayCommand] private async Task GoUsers() { var vm = provider.GetRequiredService<UserManagementViewModel>(); await vm.LoadAsync(); CurrentViewModel = vm; RefreshChrome(); }
    [RelayCommand] private async Task GoAdminOrders() { var vm = provider.GetRequiredService<AdminOrdersViewModel>(); await vm.LoadAsync(); CurrentViewModel = vm; RefreshChrome(); }
    [RelayCommand] private async Task GoReports() { var vm = provider.GetRequiredService<ReportsViewModel>(); await vm.LoadAsync(); CurrentViewModel = vm; RefreshChrome(); }
    [RelayCommand] private async Task GoAudit() { var vm = provider.GetRequiredService<AuditLogViewModel>(); await vm.LoadAsync(); CurrentViewModel = vm; RefreshChrome(); }
    [RelayCommand] private void ToggleTheme()
    {
        ThemeService.Instance.Toggle();
        IsDarkTheme = ThemeService.Instance.CurrentTheme == AppTheme.Dark;
    }
    [RelayCommand] private void ToggleSidebar() => IsSidebarExpanded = !IsSidebarExpanded;

    [RelayCommand]
    private void Logout()
    {
        session.SignOut();
        CurrentViewModel = provider.GetRequiredService<LoginViewModel>();
        RefreshChrome();
        toast.Show("Te-ai delogat.");
    }

    public void RefreshChrome()
    {
        OnPropertyChanged(nameof(IsLoggedIn));
        OnPropertyChanged(nameof(IsAdmin));
        OnPropertyChanged(nameof(CanManageMedicines));
        OnPropertyChanged(nameof(CurrentUserName));
    }

    private async void ShowToast(string message)
    {
        ToastMessage = message;
        IsToastVisible = true;
        await Task.Delay(2600);
        IsToastVisible = false;
    }
}

public partial class LoginViewModel(IAuthService auth, AppSession session, IToastService toast, MainViewModel main) : ViewModelBase
{
    [ObservableProperty] private string username = "client";
    [ObservableProperty] private string password = "Client123!";
    [ObservableProperty] private string errorMessage = string.Empty;

    [RelayCommand]
    private async Task LoginAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        var user = await auth.LoginAsync(Username, Password);
        IsBusy = false;
        if (user is null)
        {
            ErrorMessage = "Credentiale invalide sau cont dezactivat.";
            return;
        }

        toast.Show($"Bine ai venit, {user.FullName}!");
        if (session.CanManageMedicines) await main.GoAdminDashboardCommand.ExecuteAsync(null);
        else await main.GoHomeCommand.ExecuteAsync(null);
    }
}

public partial class RegisterViewModel(IAuthService auth, IToastService toast, MainViewModel main) : ViewModelBase
{
    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string message = string.Empty;

    [RelayCommand]
    private async Task RegisterAsync()
    {
        var result = await auth.RegisterAsync(Username, Password, Email, FullName);
        Message = result.Message;
        if (result.Success)
        {
            toast.Show("Cont creat. Te poti autentifica acum.");
            main.GoLoginCommand.Execute(null);
        }
    }
}

public partial class UserHomeViewModel(ICatalogService catalog, ICartService cart, AppSession session, IToastService toast) : ViewModelBase
{
    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<Medicine> Products { get; } = new();
    public ObservableCollection<Medicine> Featured { get; } = new();
    public ObservableCollection<ProductSection> Sections { get; } = new();
    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private Category? selectedCategory;
    [ObservableProperty] private Medicine? selectedMedicine;
    [ObservableProperty] private Medicine? heroProduct;
    [ObservableProperty] private int cartCount;

    public async Task LoadAsync()
    {
        Title = "PharmaDesk+";
        Categories.Clear();
        foreach (var c in await catalog.GetCategoriesAsync()) Categories.Add(c);

        var allProducts = await catalog.SearchMedicinesAsync(null, null, 0, 120);
        HeroProduct = allProducts.OrderByDescending(x => x.DiscountPercent).ThenByDescending(x => x.Rating).FirstOrDefault();

        Products.Clear();
        foreach (var product in allProducts.Take(36)) Products.Add(product);

        Featured.Clear();
        foreach (var p in allProducts.Where(x => x.IsPromotion).OrderByDescending(x => x.DiscountPercent).Take(12)) Featured.Add(p);

        Sections.Clear();
        AddSection("Top 10 recomandate azi", allProducts.OrderByDescending(x => x.Rating).Take(10));
        AddSection("Promotii intense", allProducts.Where(x => x.IsPromotion).OrderByDescending(x => x.DiscountPercent).Take(12));
        AddSection("Noutati in farmacie", allProducts.Where(x => x.IsNew).OrderByDescending(x => x.CreatedAt).Take(12));
        foreach (var group in allProducts.GroupBy(x => x.Category?.Name ?? "Alte produse").Where(x => x.Count() >= 2).OrderBy(x => x.Key))
        {
            AddSection(group.Key, group.Take(12));
        }

        if (session.CurrentUser is not null) CartCount = (await cart.GetCartAsync(session.CurrentUser.Id)).Sum(x => x.Quantity);
    }

    [RelayCommand] private async Task SearchAsync()
    {
        Products.Clear();
        foreach (var p in await catalog.SearchMedicinesAsync(SearchText, SelectedCategory?.Id)) Products.Add(p);
    }

    [RelayCommand]
    private async Task AddToCartAsync(Medicine medicine)
    {
        if (session.CurrentUser is null) return;
        await cart.AddToCartAsync(session.CurrentUser.Id, medicine.Id);
        CartCount = (await cart.GetCartAsync(session.CurrentUser.Id)).Sum(x => x.Quantity);
        toast.Show($"{medicine.Name} a fost adaugat in cos.");
    }

    private void AddSection(string name, IEnumerable<Medicine> products)
    {
        var list = products.ToList();
        if (list.Count > 0) Sections.Add(new ProductSection(name, list));
    }
}

public partial class CartViewModel(ICartService cart, IOrderService orders, AppSession session, IToastService toast) : ViewModelBase
{
    public ObservableCollection<CartItem> Items { get; } = new();
    [ObservableProperty] private string shippingAddress = "Str. Exemplu 10, Bucuresti";
    [ObservableProperty] private string paymentMethod = "Card";
    [ObservableProperty] private string? prescriptionPath;
    [ObservableProperty] private decimal total;

    public decimal Subtotal => Items.Sum(x => x.LineTotal);
    public decimal DeliveryFee => Items.Count > 0 ? 15m : 0m;
    public bool RequiresPrescription => Items.Any(x => x.Medicine?.IsPrescriptionRequired == true);

    public async Task LoadAsync()
    {
        Title = "Cosul meu";
        Items.Clear();
        if (session.CurrentUser is null) return;
        foreach (var item in await cart.GetCartAsync(session.CurrentUser.Id)) Items.Add(item);
        Total = Items.Sum(x => x.LineTotal);
        OnPropertyChanged(nameof(RequiresPrescription));
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(DeliveryFee));
    }

    [RelayCommand] private async Task IncreaseAsync(CartItem item) { await cart.UpdateQuantityAsync(item.Id, item.Quantity + 1); await LoadAsync(); }
    [RelayCommand] private async Task DecreaseAsync(CartItem item) { await cart.UpdateQuantityAsync(item.Id, Math.Max(1, item.Quantity - 1)); await LoadAsync(); }
    [RelayCommand] private async Task RemoveAsync(CartItem item) { await cart.RemoveAsync(item.Id); await LoadAsync(); toast.Show("Produs eliminat din cos."); }

    [RelayCommand]
    private void PickPrescription()
    {
        var dialog = new OpenFileDialog { Filter = "PDF (*.pdf)|*.pdf" };
        if (dialog.ShowDialog() == true) PrescriptionPath = dialog.FileName;
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (session.CurrentUser is null) return;
        var order = await orders.CheckoutAsync(session.CurrentUser.Id, ShippingAddress, PaymentMethod, PrescriptionPath);
        await LoadAsync();
        toast.Show($"Comanda {order.OrderNumber} a fost plasata cu succes.");
    }
}

public partial class OrderHistoryViewModel(IOrderService orders, AppSession session) : ViewModelBase
{
    public ObservableCollection<Order> Orders { get; } = new();
    public async Task LoadAsync()
    {
        Title = "Istoric comenzi";
        Orders.Clear();
        if (session.CurrentUser is null) return;
        foreach (var order in await orders.GetOrdersForUserAsync(session.CurrentUser.Id)) Orders.Add(order);
    }
}

public partial class ProfileViewModel(AppSession session, PharmaDeskDbContext db, IAuthService auth, IToastService toast) : ViewModelBase
{
    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string oldPassword = string.Empty;
    [ObservableProperty] private string newPassword = string.Empty;

    [ObservableProperty] private int totalOrdersCount;
    [ObservableProperty] private decimal totalSpentAmount;
    [ObservableProperty] private string memberSinceText = string.Empty;

    public string Initials => string.Join("", (FullName ?? "U").Split(' ').Where(w => w.Length > 0).Take(2).Select(w => w[0])).ToUpper();

    public async Task LoadAsync()
    {
        Title = "Profil";
        FullName = session.CurrentUser?.FullName ?? string.Empty;
        Email = session.CurrentUser?.Email ?? string.Empty;
        MemberSinceText = session.CurrentUser?.CreatedAt.Year.ToString() ?? string.Empty;

        if (session.CurrentUser is not null)
        {
            var userOrders = await db.Orders.Where(o => o.UserId == session.CurrentUser.Id).ToListAsync();
            TotalOrdersCount = userOrders.Count;
            TotalSpentAmount = userOrders.Sum(o => o.GrandTotal);
        }

        OnPropertyChanged(nameof(Initials));
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        if (session.CurrentUser is null) return;
        var user = await db.Users.FirstAsync(x => x.Id == session.CurrentUser.Id);
        user.FullName = FullName;
        user.Email = Email;
        await db.SaveChangesAsync();
        session.CurrentUser.FullName = FullName;
        session.CurrentUser.Email = Email;
        OnPropertyChanged(nameof(Initials));
        toast.Show("Profil actualizat.");
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        if (session.CurrentUser is null) return;
        await auth.ChangePasswordAsync(session.CurrentUser.Id, OldPassword, NewPassword);
        OldPassword = NewPassword = string.Empty;
        toast.Show("Parola a fost schimbata.");
    }
}

public partial class ProductManagementViewModel(ICatalogService catalog) : ViewModelBase
{
    public ObservableCollection<Medicine> Products { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    [ObservableProperty] private Medicine editor = new();
    [ObservableProperty] private Medicine? selectedProduct;

    public string EditName
    {
        get => Editor.Name;
        set { Editor.Name = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNameError)); }
    }

    public string EditPrice
    {
        get => Editor.UnitPrice.ToString("N2");
        set { if (decimal.TryParse(value, out var d)) { Editor.UnitPrice = d; OnPropertyChanged(); OnPropertyChanged(nameof(HasPriceError)); } }
    }

    public string EditCategory
    {
        get => Editor.Category?.Name ?? string.Empty;
        set { OnPropertyChanged(); }
    }

    public string EditStock
    {
        get => Editor.StockQuantity.ToString();
        set { if (int.TryParse(value, out var i)) { Editor.StockQuantity = i; OnPropertyChanged(); } }
    }

    public bool HasNameError => string.IsNullOrWhiteSpace(Editor.Name);
    public bool HasPriceError => Editor.UnitPrice <= 0;
    public string NameError => "Name is required.";
    public string PriceError => "Price must be greater than 0.";

    public void SetImagePath(string path) { Editor.ImageUrl = path; OnPropertyChanged(nameof(Editor)); }

    public async Task LoadAsync()
    {
        Title = "Medicamente si stoc";
        Products.Clear();
        Categories.Clear();
        foreach (var c in await catalog.GetCategoriesAsync()) Categories.Add(c);
        foreach (var p in await catalog.SearchMedicinesAsync(null, null, 0, 500)) Products.Add(p);
        NewProduct();
    }

    [RelayCommand] private void Edit(Medicine medicine) => Editor = new Medicine { Id = medicine.Id, Name = medicine.Name, GenericName = medicine.GenericName, Barcode = medicine.Barcode, CategoryId = medicine.CategoryId, Category = medicine.Category, DosageForm = medicine.DosageForm, Strength = medicine.Strength, UnitPrice = medicine.UnitPrice, StockQuantity = medicine.StockQuantity, ReorderLevel = medicine.ReorderLevel, IsPrescriptionRequired = medicine.IsPrescriptionRequired, IsActive = medicine.IsActive, ImageUrl = medicine.ImageUrl, Description = medicine.Description, Rating = medicine.Rating, IsNew = medicine.IsNew, IsPromotion = medicine.IsPromotion, DiscountPercent = medicine.DiscountPercent, CreatedAt = medicine.CreatedAt };
    [RelayCommand] private void EditProduct(Medicine medicine) => Edit(medicine);
    [RelayCommand] private void NewProduct() => Editor = new Medicine { IsActive = true, ImageUrl = "pack://application:,,,/Resources/logo.png", Rating = 4.6m, CreatedAt = DateTime.UtcNow, CategoryId = Categories.FirstOrDefault()?.Id ?? 0 };
    [RelayCommand] private async Task SaveAsync() { await catalog.SaveMedicineAsync(Editor); await LoadAsync(); }
    [RelayCommand] private async Task SaveProduct() => await SaveAsync();
    [RelayCommand] private async Task DeleteAsync(Medicine medicine) { await catalog.DeleteMedicineAsync(medicine.Id); await LoadAsync(); }
    [RelayCommand] private async Task DeleteProduct(Medicine? medicine) { if (medicine is not null) await DeleteAsync(medicine); }

    [RelayCommand]
    private void UploadImage()
    {
        var dialog = new OpenFileDialog { Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*", Title = "Selecteaza imaginea produsului" };
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var fileName = System.IO.Path.GetFileName(dialog.FileName);
                var destPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Products", fileName);
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destPath)!);
                System.IO.File.Copy(dialog.FileName, destPath, true);
                Editor.ImageUrl = $"pack://application:,,,/Assets/Products/{fileName}";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Eroare la upload: {ex.Message}");
            }
        }
    }
}

public partial class CategoryManagementViewModel(ICatalogService catalog) : ViewModelBase
{
    public ObservableCollection<Category> Categories { get; } = new();
    [ObservableProperty] private Category editor = new();
    public async Task LoadAsync() { Title = "Categorii"; Categories.Clear(); foreach (var c in await catalog.GetCategoriesAsync()) Categories.Add(c); Editor = new(); }
    [RelayCommand] private void Edit(Category category) => Editor = new Category { Id = category.Id, Name = category.Name, Description = category.Description, ParentCategoryId = category.ParentCategoryId };
    [RelayCommand] private void EditCategory(Category category) => Edit(category);
    [RelayCommand] private void AddCategory() => Editor = new Category();
    [RelayCommand] private async Task SaveAsync() { await catalog.SaveCategoryAsync(Editor); await LoadAsync(); }
}

public partial class UserManagementViewModel(PharmaDeskDbContext db, IToastService toast) : ViewModelBase
{
    public ObservableCollection<User> Users { get; } = new();
    public async Task LoadAsync() { Title = "Utilizatori"; Users.Clear(); foreach (var u in await db.Users.Include(x => x.Role).AsNoTracking().OrderBy(x => x.Username).ToListAsync()) Users.Add(u); }
    [RelayCommand] private async Task ToggleActiveAsync(User user) { var entity = await db.Users.FirstAsync(x => x.Id == user.Id); entity.IsActive = !entity.IsActive; await db.SaveChangesAsync(); toast.Show(entity.IsActive ? "Utilizator activat." : "Utilizator dezactivat."); await LoadAsync(); }
    [RelayCommand] private void ViewUser(User u) { /* placeholder — show details dialog in future */ }
    [RelayCommand] private void EditRole(User u) { /* placeholder — toggle role in future */ }
}

public partial class AdminOrdersViewModel(IOrderService orders, IToastService toast) : ViewModelBase
{
    public ObservableCollection<Order> Orders { get; } = new();
    public ObservableCollection<Order> FilteredOrders { get; } = new();

    [ObservableProperty] private string selectedFilter = "All";
    [ObservableProperty] private Order? selectedOrder;

    public int AllCount => Orders.Count;
    public int PendingCount => Orders.Count(o => o.Status == "Noua" || o.Status == "Pending");
    public int CompletedCount => Orders.Count(o => o.Status == "Livrata" || o.Status == "Completed");
    public int CancelledCount => Orders.Count(o => o.Status == "Anulata" || o.Status == "Cancelled");

    public async Task LoadAsync()
    {
        Title = "Comenzi primite";
        Orders.Clear();
        foreach (var o in await orders.GetAllOrdersAsync()) Orders.Add(o);
        ApplyFilter();
    }

    [RelayCommand] private void Filter(string filter) { SelectedFilter = filter; ApplyFilter(); }
    [RelayCommand] private void ViewOrder(Order order) { SelectedOrder = order; }
    [RelayCommand] private async Task UpdateStatus(Order order) { await orders.MarkShippedAsync(order.Id); toast.Show($"Status actualizat."); await LoadAsync(); }
    [RelayCommand] private void Export() { toast.Show("Export not yet implemented."); }

    // Keep backward compat alias
    [RelayCommand] private async Task ShipAsync(Order order) { await orders.MarkShippedAsync(order.Id); toast.Show($"Comanda {order.OrderNumber} marcata ca expediata."); await LoadAsync(); }

    private void ApplyFilter()
    {
        FilteredOrders.Clear();
        var filtered = SelectedFilter switch
        {
            "Pending" => Orders.Where(o => o.Status == "Noua" || o.Status == "Pending"),
            "Completed" => Orders.Where(o => o.Status == "Livrata" || o.Status == "Completed"),
            "Cancelled" => Orders.Where(o => o.Status == "Anulata" || o.Status == "Cancelled"),
            _ => Orders.AsEnumerable()
        };
        foreach (var o in filtered) FilteredOrders.Add(o);
        OnPropertyChanged(nameof(AllCount));
        OnPropertyChanged(nameof(PendingCount));
        OnPropertyChanged(nameof(CompletedCount));
        OnPropertyChanged(nameof(CancelledCount));
    }
}

public partial class ReportsViewModel(IReportService reports, IToastService toast, PharmaDeskDbContext db) : ViewModelBase
{
    [ObservableProperty] private DateTime from = DateTime.Today.AddMonths(-1);
    [ObservableProperty] private DateTime to = DateTime.Today;
    [ObservableProperty] private string lastReportPath = string.Empty;

    [ObservableProperty] private ISeries[] revenueByMonthSeries = [];
    [ObservableProperty] private Axis[] revenueXAxes = [new Axis()];
    [ObservableProperty] private Axis[] revenueYAxes = [new Axis()];
    [ObservableProperty] private ISeries[] orderStatusSeries = [];
    [ObservableProperty] private ISeries[] topProductsSeries = [];
    [ObservableProperty] private Axis[] topProductsXAxes = [new Axis()];
    [ObservableProperty] private Axis[] topProductsYAxes = [new Axis()];
    [ObservableProperty] private decimal totalRevenue;
    [ObservableProperty] private int totalOrders;
    [ObservableProperty] private int totalProducts;
    [ObservableProperty] private int totalUsers;

    public async Task LoadAsync()
    {
        Title = "Rapoarte";

        TotalOrders = await db.Orders.CountAsync();
        TotalProducts = await db.Medicines.CountAsync();
        TotalUsers = await db.Users.CountAsync();
        TotalRevenue = await db.Orders.SumAsync(o => (decimal?)o.GrandTotal) ?? 0;

        // Revenue by month (last 6 months)
        var sixMonthsAgo = DateTime.Today.AddMonths(-6);
        var revenueData = await db.Orders
            .Where(o => o.OrderDate >= sixMonthsAgo)
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(o => o.GrandTotal) })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        RevenueByMonthSeries = [new ColumnSeries<decimal>
        {
            Values = revenueData.Select(x => x.Total).ToArray(),
            Fill = new SolidColorPaint(new SKColor(0xFC, 0xA3, 0x11))
        }];
        RevenueXAxes = [new Axis { Labels = revenueData.Select(x => $"{x.Month}/{x.Year % 100}").ToArray(), TextSize = 11 }];
        RevenueYAxes = [new Axis { TextSize = 11 }];

        // Order status donut
        var pending = await db.Orders.CountAsync(o => o.Status == "Noua" || o.Status == "Pending");
        var completed = await db.Orders.CountAsync(o => o.Status == "Livrata" || o.Status == "Completed");
        var cancelled = await db.Orders.CountAsync(o => o.Status == "Anulata" || o.Status == "Cancelled");
        OrderStatusSeries = [
            new PieSeries<int> { Values = [pending], Name = "Pending", Fill = new SolidColorPaint(new SKColor(0xFC,0xA3,0x11)), InnerRadius = 60 },
            new PieSeries<int> { Values = [completed], Name = "Completed", Fill = new SolidColorPaint(new SKColor(0x22,0xC5,0x5E)), InnerRadius = 60 },
            new PieSeries<int> { Values = [cancelled], Name = "Cancelled", Fill = new SolidColorPaint(new SKColor(0xEF,0x44,0x44)), InnerRadius = 60 }
        ];

        // Top products (by order items quantity)
        var topProducts = await db.OrderItems
            .GroupBy(i => i.Medicine!.Name)
            .Select(g => new { Name = g.Key, Count = g.Sum(i => i.Quantity) })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        TopProductsSeries = [new ColumnSeries<int>
        {
            Values = topProducts.Select(x => x.Count).ToArray(),
            Fill = new SolidColorPaint(new SKColor(0xFC, 0xA3, 0x11))
        }];
        TopProductsXAxes = [new Axis { Labels = topProducts.Select(x => x.Name).ToArray(), TextSize = 10 }];
        TopProductsYAxes = [new Axis { TextSize = 11 }];
    }

    [RelayCommand] private async Task ExportExcelAsync() { LastReportPath = await reports.ExportSalesExcelAsync(From, To); toast.Show("Raport Excel generat."); }
    [RelayCommand] private async Task ExportPdfAsync() { LastReportPath = await reports.ExportSalesPdfAsync(From, To); toast.Show("Raport PDF generat."); }
}

public partial class AuditLogViewModel(IAuditService audit) : ViewModelBase
{
    public ObservableCollection<AuditLog> Logs { get; } = new();
    public ObservableCollection<AuditLog> LogEntries { get; } = new();

    [ObservableProperty] private DateTime? dateFrom;
    [ObservableProperty] private DateTime? dateTo;
    [ObservableProperty] private string selectedActionType = "All";
    public ObservableCollection<string> ActionTypes { get; } = new(new[] { "All", "Create", "Update", "Delete", "Login" });

    public async Task LoadAsync()
    {
        Title = "Audit log";
        Logs.Clear();
        LogEntries.Clear();
        foreach (var l in await audit.GetLogsAsync())
        {
            Logs.Add(l);
            LogEntries.Add(l);
        }
    }

    [RelayCommand] private async Task ApplyFilter() => await LoadAsync();
}

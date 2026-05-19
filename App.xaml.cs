using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using PharmaDesk.Data;
using PharmaDesk.Services;
using PharmaDesk.ViewModels;

namespace PharmaDesk;

public partial class App : Application
{
    private IHost? host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ThemeService.Instance.LoadSaved();

        try
        {
            host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(config => config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true))
                .ConfigureServices((context, services) =>
                {
                    var connection = context.Configuration.GetConnectionString("DefaultConnection")!;
                    var serverVersion = MySqlServerVersion.Parse(context.Configuration["App:MySqlServerVersion"] ?? "8.0.36");
                    services.AddDbContext<PharmaDeskDbContext>(options => options.UseMySql(connection, serverVersion));
                    services.AddSingleton<AppSession>();
                    services.AddTransient<DatabaseInitializer>();
                    services.AddTransient<IAuthService, AuthService>();
                    services.AddTransient<ICatalogService, CatalogService>();
                    services.AddTransient<ICartService, CartService>();
                    services.AddTransient<IOrderService, OrderService>();
                    services.AddTransient<IReportService, ReportService>();
                    services.AddTransient<IAuditService, AuditService>();
                    services.AddSingleton<IToastService, ToastService>();
                    services.AddSingleton<MainViewModel>();
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<RegisterViewModel>();
                    services.AddTransient<UserHomeViewModel>();
                    services.AddTransient<CartViewModel>();
                    services.AddTransient<OrderHistoryViewModel>();
                    services.AddTransient<ProfileViewModel>();
                    services.AddTransient<AdminDashboardViewModel>();
                    services.AddTransient<ProductManagementViewModel>();
                    services.AddTransient<CategoryManagementViewModel>();
                    services.AddTransient<UserManagementViewModel>();
                    services.AddTransient<AdminOrdersViewModel>();
                    services.AddTransient<ReportsViewModel>();
                    services.AddTransient<AuditLogViewModel>();
                    services.AddSingleton<MainWindow>();
                })
                .Build();

            await host.StartAsync();
            await host.Services.GetRequiredService<DatabaseInitializer>().InitializeAsync();
            var mainViewModel = host.Services.GetRequiredService<MainViewModel>();
            mainViewModel.Initialize();
            var window = host.Services.GetRequiredService<MainWindow>();
            window.DataContext = mainViewModel;
            window.Show();
        }
        catch (MySqlException ex)
        {
            MessageBox.Show(
                "Nu ma pot conecta la MySQL.\n\n" +
                "Verifica in appsettings.json linia DefaultConnection: userul, parola, portul si daca serverul MySQL este pornit.\n\n" +
                $"Detalii MySQL: {ex.Message}",
                "Eroare conectare MySQL",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Startup failed:\n{ex}", "Startup error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (host is not null)
        {
            await host.StopAsync();
            host.Dispose();
        }
        base.OnExit(e);
    }
}

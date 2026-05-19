using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using PharmaDesk.Services;

namespace PharmaDesk;

public partial class MainWindow : Window
{
    private bool _sidebarExpanded = true;
    private Button? _activeNavButton;

    public MainWindow()
    {
        InitializeComponent();
        // Default: show Admin nav, navigate to Dashboard
        NavigateTo(new Views.AdminDashboardView());
        SetActiveNav(NavDashboard);
        UpdateThemeIcon();
    }

    // ── Sidebar toggle ──────────────────────────────────────────
    private void BtnToggle_Click(object sender, RoutedEventArgs e)
    {
        _sidebarExpanded = !_sidebarExpanded;
        double target = _sidebarExpanded ? 220 : 64;

        var anim = new DoubleAnimation(target, TimeSpan.FromMilliseconds(200))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        SidebarColumn.BeginAnimation(ColumnDefinition.WidthProperty, anim);

        // Show/hide text labels
        var visibility = _sidebarExpanded ? Visibility.Visible : Visibility.Collapsed;
        LogoPanel.Visibility     = visibility;
        AppNameLabel.Visibility  = visibility;
        ThemeLabel.Visibility    = visibility;
        LogoutLabel.Visibility   = visibility;
        UserNameLabel.Visibility = visibility;

        // Hide all nav text labels
        foreach (var lbl in new[] { LblDashboard, LblOrders, LblProducts, LblCategories,
                                     LblUsers, LblReports, LblAudit,
                                     LblHome, LblCart, LblOrderHistory, LblProfile })
            lbl.Visibility = visibility;
    }

    // ── Theme toggle ────────────────────────────────────────────
    private void BtnTheme_Click(object sender, RoutedEventArgs e)
    {
        ThemeService.Instance.Toggle();
        UpdateThemeIcon();
    }

    private void UpdateThemeIcon()
    {
        bool isDark = ThemeService.Instance.CurrentTheme == AppTheme.Dark;
        ThemeIcon.Text  = isDark ? "☾" : "☀";
        ThemeLabel.Text = isDark ? " Dark Mode" : " Light Mode";
    }

    // ── Logout ──────────────────────────────────────────────────
    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        var login = new Views.LoginView();
        login.Show();
        Close();
    }

    // ── Navigation helpers ──────────────────────────────────────
    private void NavigateTo(Page page)
    {
        ContentFrame.Navigate(page);
    }

    private void SetActiveNav(Button btn)
    {
        if (_activeNavButton != null)
            _activeNavButton.Style = (Style)FindResource("NavItem");
        _activeNavButton = btn;
        btn.Style = (Style)FindResource("NavItemActive");
    }

    // ── Nav click handlers ──────────────────────────────────────
    private void NavDashboard_Click(object sender, RoutedEventArgs e)
    {
        NavigateTo(new Views.AdminDashboardView());
        SetActiveNav(NavDashboard);
    }

    private void NavOrders_Click(object sender, RoutedEventArgs e)
    {
        NavigateTo(new Views.AdminOrdersView());
        SetActiveNav(NavOrders);
    }

    private void NavProducts_Click(object sender, RoutedEventArgs e)
    {
        NavigateTo(new Views.ProductManagementView());
        SetActiveNav(NavProducts);
    }

    private void NavCategories_Click(object sender, RoutedEventArgs e)
    {
        NavigateTo(new Views.CategoryManagementView());
        SetActiveNav(NavCategories);
    }

    private void NavUsers_Click(object sender, RoutedEventArgs e)
    {
        NavigateTo(new Views.UserManagementView());
        SetActiveNav(NavUsers);
    }

    private void NavReports_Click(object sender, RoutedEventArgs e)
    {
        NavigateTo(new Views.ReportsView());
        SetActiveNav(NavReports);
    }

    private void NavAudit_Click(object sender, RoutedEventArgs e)
    {
        NavigateTo(new Views.AuditLogView());
        SetActiveNav(NavAudit);
    }

    private void NavHome_Click(object sender, RoutedEventArgs e)
    {
        NavigateTo(new Views.UserHomeView());
        SetActiveNav(NavHome);
    }

    private void NavCart_Click(object sender, RoutedEventArgs e)
    {
        NavigateTo(new Views.CartView());
        SetActiveNav(NavCart);
    }

    private void NavOrderHistory_Click(object sender, RoutedEventArgs e)
    {
        NavigateTo(new Views.OrderHistoryView());
        SetActiveNav(NavOrderHistory);
    }

    private void NavProfile_Click(object sender, RoutedEventArgs e)
    {
        NavigateTo(new Views.ProfileView());
        SetActiveNav(NavProfile);
    }
}

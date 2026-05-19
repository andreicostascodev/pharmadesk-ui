using PharmaDesk.Models;
using System.Windows;
using System.Windows.Controls;

namespace PharmaDesk.Views;

public partial class AdminDashboardView : Page
{
    public AdminDashboardView()
    {
        InitializeComponent();
    }

    private void RecentOrdersGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (RecentOrdersGrid.SelectedItem is RecentOrderItem order)
        {
            // TODO: wire to MainWindow navigation frame in Task 8
            var window = Window.GetWindow(this);
            // Navigation to AdminOrdersView will be connected when MainWindow shell is complete (Task 8)
        }
    }
}

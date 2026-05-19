using System.Windows;

namespace PharmaDesk.Views;

public partial class RegisterView : Window
{
    public RegisterView()
    {
        InitializeComponent();
    }

    private void BtnCreateAccount_Click(object sender, RoutedEventArgs e)
    {
        // TODO: wire to existing registration service
        new LoginView().Show();
        Close();
    }

    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        new LoginView().Show();
        Close();
    }
}

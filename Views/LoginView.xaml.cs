using System.Windows;

namespace PharmaDesk.Views;

public partial class LoginView : Window
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        // TODO: wire to existing auth service
        var main = new MainWindow();
        main.Show();
        Close();
    }

    private void BtnRegister_Click(object sender, RoutedEventArgs e)
    {
        new RegisterView().Show();
        Close();
    }
}

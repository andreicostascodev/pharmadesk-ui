using System.Windows;
using PharmaDesk.ViewModels;

namespace PharmaDesk.Views;

public partial class LoginView : Window
{
    public LoginView()
    {
        InitializeComponent();
    }

    private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
            vm.Password = TxtPassword.Password;
    }
}

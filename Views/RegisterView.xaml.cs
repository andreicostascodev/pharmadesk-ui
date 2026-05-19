using System.Windows;
using PharmaDesk.ViewModels;

namespace PharmaDesk.Views;

public partial class RegisterView : Window
{
    public RegisterView()
    {
        InitializeComponent();
    }

    private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel vm)
            vm.Password = TxtPassword.Password;
    }
}

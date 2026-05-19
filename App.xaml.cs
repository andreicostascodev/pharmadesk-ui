using System.Windows;
using PharmaDesk.Services;

namespace PharmaDesk;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ThemeService.Instance.LoadSaved();
    }
}

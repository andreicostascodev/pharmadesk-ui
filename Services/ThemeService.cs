using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace PharmaDesk.Services;

public enum AppTheme { Light, Dark }

public class ThemeService
{
    private static ThemeService? _instance;
    public static ThemeService Instance => _instance ??= new ThemeService();

    private const string SettingsPath   = "appsettings.json";
    private const string LightThemeUri  = "Styles/LightTheme.xaml";
    private const string DarkThemeUri   = "Styles/DarkTheme.xaml";

    public AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    private ThemeService() { }

    public void LoadSaved()
    {
        try
        {
            var json = File.ReadAllText(SettingsPath);
            var doc  = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("Theme", out var val) &&
                Enum.TryParse<AppTheme>(val.GetString(), out var theme))
            {
                Apply(theme);
                return;
            }
        }
        catch { /* fall through to default */ }

        Apply(AppTheme.Light);
    }

    public void Toggle() =>
        Apply(CurrentTheme == AppTheme.Light ? AppTheme.Dark : AppTheme.Light);

    public void Apply(AppTheme theme)
    {
        CurrentTheme = theme;
        var uri      = theme == AppTheme.Light ? LightThemeUri : DarkThemeUri;
        var newDict  = new ResourceDictionary { Source = new Uri(uri, UriKind.Relative) };

        var mergedDicts = Application.Current.Resources.MergedDictionaries;
        var existing    = mergedDicts.FirstOrDefault(d =>
            d.Source?.OriginalString is LightThemeUri or DarkThemeUri);

        if (existing != null) mergedDicts.Remove(existing);
        mergedDicts.Add(newDict);

        Persist();
    }

    private void Persist()
    {
        try
        {
            string raw  = File.Exists(SettingsPath) ? File.ReadAllText(SettingsPath) : "{}";
            var doc     = JsonDocument.Parse(raw);
            var dict    = doc.RootElement.EnumerateObject()
                             .ToDictionary(p => p.Name, p => (object)p.Value.ToString());
            dict["Theme"] = CurrentTheme.ToString();
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(dict,
                new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { /* non-fatal — theme preference loss is acceptable */ }
    }
}

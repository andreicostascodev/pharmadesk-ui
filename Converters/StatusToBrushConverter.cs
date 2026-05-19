using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PharmaDesk.Converters;

public class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value?.ToString() switch
        {
            "Completed" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E")),
            "Cancelled" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
            _           => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCA311")),
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

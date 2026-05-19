using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PharmaDesk.Converters;

public class RoleToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value?.ToString() == "Admin"
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#14213D"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCA311"));

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

using System;
using System.Globalization;
using System.Windows.Data;

namespace PharmaDesk.Converters;

public class ProgressToWidthConverter : IValueConverter
{
    private const double MaxWidth = 160.0;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is double d ? d * MaxWidth : 0.0;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

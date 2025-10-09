using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace LightspeedNexus.Converters;

/// <summary>
/// If the value is true, the opacity is low.
/// </summary>
public class TrueToFadedConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b)
            return parameter ?? (value is double d ? d : 0.5);
        return 1.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value is double d && d >= 1.0;
}

/// <summary>
/// If the value is true, the opacity is zero.
/// </summary>
public class TrueToInvisibleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b)
            return parameter ?? 0.0;
        return 1.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is double d && d >= 1.0;
    }
}

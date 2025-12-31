using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace LightspeedNexus.Converters;

public class StringsAreEqualConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is string s1 && parameter is string s2 && s1 == s2;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
}

public class UppercaseConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value?.ToString()?.ToUpper(culture) ?? value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value;
}

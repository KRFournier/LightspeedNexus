using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using System;
using System.Globalization;

namespace LightspeedNexus.Converters;

/// <summary>
/// Generic value converter for radio buttons
/// </summary>
public class RadioButtonValueConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value?.Equals(parameter);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? parameter : BindingOperations.DoNothing;

    public override object ProvideValue(IServiceProvider serviceProvider)
        => this;
}

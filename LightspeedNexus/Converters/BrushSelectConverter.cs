using Avalonia;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Globalization;

namespace LightspeedNexus.Converters;

/// <summary>
/// Chooses a color based on a boolean value
/// </summary>
public class BrushSelectConverter : AvaloniaObject, IValueConverter
{
    public static readonly StyledProperty<IBrush> TrueBrushProperty =
        AvaloniaProperty.Register<BrushSelectConverter, IBrush>(nameof(TrueBrush));

    public static readonly StyledProperty<IBrush> FalseBrushProperty =
        AvaloniaProperty.Register<BrushSelectConverter, IBrush>(nameof(FalseBrush));

    public IBrush TrueBrush
    {
        get => GetValue(TrueBrushProperty);
        set => SetValue(TrueBrushProperty, value);
    }

    public IBrush FalseBrush
    {
        get => GetValue(FalseBrushProperty);
        set => SetValue(FalseBrushProperty, value);
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
            return boolValue ? TrueBrush : FalseBrush;

        // Handle null, unset, or incorrect types gracefully
        if (value == AvaloniaProperty.UnsetValue)
            return FalseBrush;

        // Attempt conversion for non-bool types (e.g., int to bool)
        try
        {
            bool convertedBool = System.Convert.ToBoolean(value, culture);
            return convertedBool ? TrueBrush : FalseBrush;
        }
        catch
        {
            // Fallback for types that can't be converted
            return FalseBrush;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Converting a Brush back to a bool is usually not required.
        // If it were, you would implement the logic here.
        throw new NotSupportedException();
    }
}

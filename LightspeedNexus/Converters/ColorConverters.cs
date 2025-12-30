using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace LightspeedNexus.Converters;

/// <summary>
/// Base class for color converters. Includes predefined colors.
/// </summary>
public abstract class ColorConverter
{
    public static readonly SolidColorBrush Red = new(Color.Parse("#b52d25"));
    public static readonly SolidColorBrush Orange = new(Color.Parse("#b56d24"));
    public static readonly SolidColorBrush Yellow = new(Color.Parse("#b5b524"));
    public static readonly SolidColorBrush Lime = new(Color.Parse("#6db524"));
    public static readonly SolidColorBrush Green = new(Color.Parse("#218d21"));
    public static readonly SolidColorBrush Teal = new(Color.Parse("#24b5b5"));
    public static readonly SolidColorBrush Blue = new(Color.Parse("#21458d"));
    public static readonly SolidColorBrush Purple = new(Color.Parse("#44208c"));
    public static readonly SolidColorBrush Violet = new(Color.Parse("#8c208c"));
    public static readonly SolidColorBrush Magenta = new(Color.Parse("#8c2044"));
    public static readonly SolidColorBrush Gray = new(Color.Parse("#4c4c4c"));
    public static readonly SolidColorBrush White = new(Color.Parse("#dfdfdf"));
}

/// <summary>
/// Converts a WeaponClass to a corresponding color.
/// </summary>
public class WeaponToColorConverter : ColorConverter, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Models.WeaponClass weaponClass)
        {
            return weaponClass switch
            {
                Models.WeaponClass.Rey => Teal,
                Models.WeaponClass.Ren => Red,
                Models.WeaponClass.Tano => Yellow,
                _ => White
            };
        }
        return White;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

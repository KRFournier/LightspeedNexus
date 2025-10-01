using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightspeedNexus.Converters;

public class VenueToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            null => string.Empty,
            ViewModels.VenueViewModel vm => vm.Name,
            Models.Venue v => v.Name,
            _ => throw new ArgumentException("Unsupported type for VenueViewModelToString converter", nameof(value))
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

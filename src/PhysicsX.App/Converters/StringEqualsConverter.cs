using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PhysicsX.App.Converters;

public class StringEqualsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str && parameter is string param)
        {
            return str.Equals(param, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public static class ObjectConverters
{
    public static FuncValueConverter<object?, bool> IsNotNull { get; } =
        new FuncValueConverter<object?, bool>(x => x != null);
}

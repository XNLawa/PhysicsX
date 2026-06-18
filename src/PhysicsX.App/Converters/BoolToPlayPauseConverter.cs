using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PhysicsX.App.Converters;

public class BoolToPlayPauseConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isRunning)
        {
            return isRunning ? "⏸ 暂停" : "▶ 播放";
        }
        return "▶ 播放";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

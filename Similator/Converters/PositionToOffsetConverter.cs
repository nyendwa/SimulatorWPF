using System;
using System.Globalization;
using System.Windows.Data;

namespace Simulator.Converters
{
    public class PositionToOffsetConverter : IValueConverter
    {
        public double OffsetA { get; set; } = -80;
        public double OffsetB { get; set; } = 80;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? pos = value as string;
            return pos == "B" ? OffsetB : OffsetA;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}

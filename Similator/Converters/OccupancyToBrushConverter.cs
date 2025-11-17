using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Simulator.Converters
{
    public class OccupancyToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Brushes.Transparent : Brushes.CadetBlue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace NetworkService.Helpers
{
    public class ValueToColorConverter : IValueConverter
    {
        private int MinValue = 250;
        private int MaxValue = 350;
        private int MaxGreen = 255;
        private int MinGreen = 200;
        private int MaxRed = 100;
        private int MinRed = 50;
        private int MaxBlue = 180;
        private int MinBlue = 80;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Assuming value is the reading value and MinValue/MaxValue are available
            int proportion = (int)value / (int)(MaxValue - MinValue);

            // Map the proportion to the desired color range
            Color desiredColor = Color.FromRgb((byte)(proportion * (MaxRed - MinRed) + MinRed),
                                               (byte)(proportion * (MaxGreen - MinGreen) + MinGreen),
                                               (byte)(proportion * (MaxBlue - MinBlue) + MinBlue));

            return new SolidColorBrush(desiredColor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}

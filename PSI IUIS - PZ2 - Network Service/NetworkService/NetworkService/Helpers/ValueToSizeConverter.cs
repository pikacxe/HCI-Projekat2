using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NetworkService.Helpers
{
    public class ValueToSizeConverter : IValueConverter
    {
        private double MinValue = 250;
        private double MaxValue = 350;
        private double MaxSize = 100;
        private double MinSize = 50;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((int)value < MinValue || (int)value > MaxValue)
            {
                return 0;
            }
            // Perform the linear interpolation
            double slope = (MaxSize - MinSize) / (MaxValue - MinValue);
            double mappedValue = MinSize + slope * ((int)value - MinValue);

            // Clamp the mapped value within the output range
            mappedValue = Math.Max(MinSize, Math.Min(MaxSize, mappedValue));

            return mappedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}

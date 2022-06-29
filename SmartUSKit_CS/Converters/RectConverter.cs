using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SmartUSKit_CS.Converters
{
    //[ValueConversion(typeof(decimal), typeof(string))]
    public class RectConverter : IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double width = (double)value;
            return (double)value - 28;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //string price = value.ToString();

            //decimal result;
            //if (Decimal.TryParse(price, System.Globalization.NumberStyles.Any, culture, out result))
            //{
            //    return result;
            //}
            return value;
        }


        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double w = (double)values[0];
                double h = (double)values[1];
                switch (parameter as string)
                {
                    //case "FirstLast":
                    //    return values[0] + " " + values[1];
                    //case "LastFirst":
                    //    return values[1] + "," + values[0];
                    //default:
                    //    return "";
                }
                return new Rect(0, 0, w, h);
            }
            catch (Exception ex)
            {
            }
            return new Rect(0,0,100,20);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

}

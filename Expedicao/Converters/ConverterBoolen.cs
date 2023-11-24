using System;
using System.Globalization;
using System.Windows.Data;

namespace Expedicao
{
    public class ConverterBoolen : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            switch (value.ToString().Trim())
            {
                case "-1":
                    return true;
                case "1":
                    return true;
                case "0":
                    return false;
                default:
                    break;
            }
            return false;
            //-1   

        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case true:
                    return "-1";
                case false:
                    return "0";
            }
            return "0";
        }
    }
}

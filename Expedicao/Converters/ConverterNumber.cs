using System;
using System.Globalization;
using System.Windows.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Expedicao
{
    public class ConverterNumber : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                //Convert.ToDouble(value);
                var valor = Convert.ToDouble(value) % 1 == 0 ? value : string.Format(CultureInfo.CurrentCulture, "{0:N}", value);
                //var valorFormatado = valor; //string.Format(new CultureInfo("pt-BR"), "{0:F}", value)
                return valor;

            }
            catch (Exception)
            {
                throw;
            }
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using NorthwindClient.Northwind;

namespace NorthwindClient
{
    class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return value;
            return ((decimal)value).ToString("C");            
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (string.IsNullOrEmpty((string)value)) return value;
            return decimal.Parse(((string)value).TrimStart('$'));
        }
    }
    public class DateConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return value;
            return ((DateTime)value).ToString("d");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return value;
            return DateTime.Parse((string)value);
        }
    }
    public class ProductConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                var product = (from p in App.ViewModel.Products
                               where p.ProductID == (int)value
                               select p).FirstOrDefault();

                return product.ProductName;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
    public class ProductPriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                var product = (from p in App.ViewModel.Products
                               where p.ProductID == (int)value
                               select p).FirstOrDefault();

                return ((decimal)product.UnitPrice).ToString("C");
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (string.IsNullOrEmpty((string)value)) return value;
            return decimal.Parse(((string)value).TrimStart('$'));
        }
    }
}

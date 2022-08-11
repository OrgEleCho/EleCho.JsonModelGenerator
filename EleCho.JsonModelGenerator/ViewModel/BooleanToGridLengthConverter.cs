using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Collections.Generic;

namespace EleCho.JsonModelGenerator.ViewModel
{
    public class BooleanToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean && boolean)
            {
                return new GridLength(1, GridUnitType.Star);
            }

            return new GridLength(0, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GridLength gridLen)
            {
                return gridLen.Value != 0;
            }

            return false;
        }
    }
}

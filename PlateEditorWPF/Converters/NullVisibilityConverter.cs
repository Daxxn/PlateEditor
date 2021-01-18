using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace PlateEditorWPF.Converters
{
   public class NullVisibilityConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value is null)
         {
            return Visibility.Hidden;
         }
         if (value is string str)
         {
            if (String.IsNullOrEmpty(str))
            {
               return Visibility.Hidden;
            }
         }
         return Visibility.Visible;
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException("Theres no need to convert back.");
      }
   }
}

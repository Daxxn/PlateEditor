using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace PlateEditorWPF.Converters
{
   public class VisibilityConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value is bool b)
         {
            return b ? Visibility.Visible : Visibility.Hidden;
         }
         else
         {
            return Visibility.Hidden;
         }
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value is Visibility vis)
         {
            return vis == Visibility.Visible;
         }
         else
         {
            return false;
         }
      }
   }
}

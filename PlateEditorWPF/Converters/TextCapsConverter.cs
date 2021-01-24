using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace PlateEditorWPF.Converters
{
   class TextCapsConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value is string str)
         {
            return str.ToUpper();
         }
         else
         {
            return value;
         }
      }

      public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         if (value is string str)
         {
            return str.ToUpper();
         }
         return value;
      }
   }
}

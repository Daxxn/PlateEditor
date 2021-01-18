using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PlateEditorWPF
{
   /// <summary>
   /// Interaction logic for SaveCompletedDialog.xaml
   /// </summary>
   public partial class SaveCompletedDialog : Window
   {
      public SaveCompletedDialog(string message, string source, string save, List<SavedFile> savedFiles)
      {
         DataContext = new SaveCompletedViewModel(message, source, save, savedFiles);
         InitializeComponent();
      }
   }
}

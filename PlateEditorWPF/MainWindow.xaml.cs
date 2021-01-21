﻿using PlateEditorWPF.Events;
using PlateEditorWPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlateEditorWPF
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public RotateTransform ImageRotation { get; set; }
      public MainWindow()
      {
         ImageRotation = new RotateTransform(0);
         var VM = new MainViewModel();
         DataContext = VM;
         InitializeComponent();
         plateTypeComb.ItemsSource = Enum.GetValues(typeof(PlateType));
         plateRegionComb.ItemsSource = PlateMetaData.AllRegions;

         allPlatesView.SelectionChanged += VM.AllPlatesViewSelectEvent;
         CurrentPlateApproachTypeCM.Opened += VM.GetAllApproachTypes;
         AllApproachTypesView.SelectionChanged += VM.AllApproachTypesSelection;

         PageSizeSlider.ValueChanged += VM.PageSizeSliderChangeEvent;

         VM.UpdateImage += VM_UpdateImage;
      }

      private void VM_UpdateImage(object sender, UpdateImageEventArgs e)
      {
         if (!e.ClearImage)
         {
            if (e.NewUri.IsFile)
            {
               currentImageElement.Source = new BitmapImage(e.NewUri);
            }
         }
         else
         {
            currentImageElement.Source = null;
         }
      }

      private void RotateButtons_Click(object sender, RoutedEventArgs e)
      {
         if (sender is Button btn)
         {
            if (btn.DataContext is string direction)
            {
               switch (direction)
               {
                  case "Horz":
                     ImageRotation.Angle = 90;
                     break;
                  case "Vert":
                     ImageRotation.Angle = 0;
                     break;
                  default:
                     goto case "Horz";
               } 
            } 
         }
         currentImageElement.RenderTransform = ImageRotation;
      }

      private void TextBox_GotFocus(object sender, RoutedEventArgs e)
      {
         if (sender is TextBox box)
         {
            box.SelectAll();
         }
      }
   }
}

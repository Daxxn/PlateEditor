using MVVMLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using PlateEditorWPF.Events;
using PlateEditorWPF.Models;
using System.Threading.Tasks;
using System.Threading;
using JsonReaderLibrary;
using System.Windows.Media;

namespace PlateEditorWPF
{
   public class MainViewModel : ViewModel
   {
      #region - Fields & Properties
      private Page<PlateMetaData> _page;

      private readonly string _null = "NA-";
      public event EventHandler<UpdateImageEventArgs> UpdateImage;
      private string _rootDir = @"B:\Games\OtherGames\FS 2020\Airport Plates\All Plate Images Test\A";
      private string _saveFile = @"B:\Games\OtherGames\FS 2020\Airport Plates\AllPlates.json";
      private string _bookmarkFilePath = $"{Directory.GetCurrentDirectory()}\\Bookmark.json";

      private ObservableCollection<PlateMetaData> _allPlates;
      private PlateMetaData _currentPlate;

      private Dictionary<string, string> _approachTypes;

      #region Commands
      public Command OpenRootDirCmd { get; private set; }
      public Command AppendRootDirCmd { get; private set; }
      public Command PrevImageCmd { get; private set; }
      public Command NextImageCmd { get; private set; }
      public Command SavePlatesCmd { get; private set; }
      public Command SavePlatesJsonCmd { get; private set; }
      public Command OpenPlatesJsonCmd { get; private set; }
      public Command SaveBookmarkCmd { get; private set; }
      public Command OpenBookmarkCmd { get; private set; }
      public Command PrevPageCmd { get; private set; }
      public Command NextPageCmd { get; private set; }
      public Command ArrDepParseCmd { get; private set; }

      public Command TestCmd { get; private set; }
      #endregion

      private bool _toggleOverwrite;
      private ObservableCollection<string> _allApproachTypes;
      #endregion

      #region - Constructors
      public MainViewModel()
      {
         OpenRootDirCmd = new Command(OpenRootDir);
         AppendRootDirCmd = new Command(AppendPlates);
         PrevImageCmd = new Command(PrevPlate);
         NextImageCmd = new Command(NextPlate);
         SavePlatesJsonCmd = new Command(SaveJsonPlates);
         OpenPlatesJsonCmd = new Command(OpenJsonPlates);

         SaveBookmarkCmd = new Command(SaveBookmark);
         OpenBookmarkCmd = new Command(OpenBookmark);

         PrevPageCmd = new Command(PrevPage);
         NextPageCmd = new Command(NextPage);

         ArrDepParseCmd = new Command(ArrDepParse);

         TestCmd = new Command(Test);

         ApproachTypes = PlateMetaData.ApproachTypes;
      }
      #endregion

      #region - Methods
      /// <summary>
      /// Just for testing stuff. Nothin else.
      /// </summary>
      public void Test(object p)
      {
         var allHashCodes = new List<int>();
         foreach (var plate in Page.AllData)
         {
            allHashCodes.Add(plate.GetHashCode());
         }
      }

      #region Command Methods
      private void OpenRootDir(object p)
      {
         if (RootDirExists)
         {
            GetRootFiles();
         }
      }

      private void PrevPlate(object p)
      {
         if (Page is null) return;

         if (CurrentPlateIndex - 1 < 0)
         {
            CurrentPlate = Page.PageData[^1];
         }
         else
         {
            CurrentPlate = Page.PageData[CurrentPlateIndex - 1];
         }
      }

      private void NextPlate(object p)
      {
         if (Page is null) return;

         if (CurrentPlateIndex + 1 >= Page.PageData.Count)
         {
            CurrentPlate = Page.PageData[0];
         }
         else
         {
            CurrentPlate = Page.PageData[CurrentPlateIndex + 1];
         }
      }

      private void PrevPage(object p)
      {
         if (Page is null) return;
         Page.PageNumber--;
         CurrentPlate = Page.PageData[0];
      }

      private void NextPage(object p)
      {
         if (Page is null) return;
         Page.PageNumber++;
         CurrentPlate = Page.PageData[0];
      }

      private void OpenJsonPlates(object p)
      {
         try
         {
            Page = new Page<PlateMetaData>((Page is null ? 50 : Page.PageSize), JsonReader.OpenJsonFile<List<PlateMetaData>>(SaveFile));
            if (Page.AllData.Count > 0)
            {
               CurrentPlate = Page.PageData[0];
            }
         }
         catch (Exception e)
         {
            MessageBox.Show(e.Message);
         }
      }

      private void SaveJsonPlates(object p)
      {
         try
         {
            JsonReader.SaveJsonFile(SaveFile, Page.AllData, true, true);

            MessageBox.Show("Saved Plates", "Done");
         }
         catch (Exception e)
         {
            MessageBox.Show(e.Message);
         }
      }

      private void AppendPlates(object p)
      {
         var rel = Path.GetRelativePath(Page.AllData[^1].PlateFile, RootDirectory);
         if (Page.AllData.Count > 0 && rel != "..")
         {
            GetRootFiles(true);
         }
         else
         {
            MessageBox.Show($"Folder was already opened in this file, {rel}", "");
         }
      }

      private void SaveBookmark(object p)
      {
         try
         {
            SaveJsonPlates(p);
            JsonReader.SaveJsonFile(
               _bookmarkFilePath,
               new Bookmark(CurrentPlateIndex, Page.PageNumber, RootDirectory, SaveFile),
               true,
               true
               );
         }
         catch (Exception e)
         {
            MessageBox.Show(e.Message, "Bookmark Save Error");
         }
      }

      /// <summary>
      /// Broken. Just returns null values.
      /// </summary>
      private void OpenBookmark(object p)
      {
         try
         {
            var bookmark = JsonReader.OpenJsonFile<Bookmark>(_bookmarkFilePath);
            RootDirectory = bookmark.CurrentSourceFolder;
            SaveFile = bookmark.CurrentSaveFile;
            GetRootFiles();

            if (Page.AllData.Count > 0)
            {
               CurrentPlate = Page.AllData[bookmark.CurrentPlateIndex];
            }
         }
         catch (Exception e)
         {
            MessageBox.Show(e.Message, "Bookmark Open Error");
         }
      }

      private void ArrDepParse(object p)
      {
         var arrDepName = "";
         var arrDepOther = "";
         var name = CurrentPlate.ApproachType;
         if (name.Contains('_'))
         {
            var nameSplit = name.Split('_');
            arrDepName = nameSplit[0];
            arrDepOther = $"-{nameSplit[1]}";
         }
         else
         {
            arrDepName = name;
         }
         CurrentPlate.ApproachType = "RNAV";
         CurrentPlate.Name = arrDepName;
         CurrentPlate.Other = arrDepOther;
      }
      #endregion

      #region Helper Methods
      /// <summary>
      /// Checks for null or empty strings and replaces.
      /// </summary>
      public void CheckValues()
      {
         if (CurrentPlate != null)
         {
            if (String.IsNullOrEmpty(CurrentPlate.IATACode))
            {
               CurrentPlate.IATACode = _null;
            }
            if (String.IsNullOrEmpty(CurrentPlate.Name))
            {
               CurrentPlate.Name = _null;
            }
            if (String.IsNullOrEmpty(CurrentPlate.Runway))
            {
               CurrentPlate.Runway = _null;
            }
            if (String.IsNullOrEmpty(CurrentPlate.ApproachOption))
            {
               CurrentPlate.ApproachOption = _null;
            }
            if (String.IsNullOrEmpty(CurrentPlate.Other))
            {
               CurrentPlate.Other = _null;
            }
            if (String.IsNullOrEmpty(CurrentPlate.ApproachType))
            {
               CurrentPlate.ApproachType = _null;
            }
         }
      }

      private void ApproachTypeSelected(string value)
      {
         if (CurrentPlate != null)
         {
            CurrentPlate.GetApproachType(value);
         }
      }

      private void GetRootFiles(bool append = false)
      {
         try
         {
            if (append)
            {
               Page.Append(
                  PlateMetaData.BuildMetaData(
                     Directory.GetFiles(RootDirectory)
                     )
                  );
            }
            else
            {
               Page = new Page<PlateMetaData>(
                  50,
                  PlateMetaData.BuildMetaData(
                     Directory.GetFiles(RootDirectory)
                     )
                  );
            }

            if (Page.AllData.Count > 0)
            {
               Page.SelectPage(0);
               CurrentPlate = Page.PageData[0];
            }
         }
         catch (Exception e)
         {
            MessageBox.Show($"{e.Message} - Unable to select file.", "Error!");
         }
      }
      #endregion

      #region Event Handlers
      public void ApproachTypeSelectionEvent(object sender, SelectionChangedEventArgs e)
      {
         if (CurrentPlate != null && e.AddedItems.Count == 1)
         {
            if (e.AddedItems[0] is string selectedKey)
            {
               CurrentPlate.ApproachType = PlateMetaData.ApproachTypes[selectedKey];
            }
         }
      }

      public void AllPlatesViewSelectEvent(object sender, SelectionChangedEventArgs e)
      {
         if (e.AddedItems.Count == 1)
         {
            if (e.AddedItems[0] is PlateMetaData plate)
            {
               CurrentPlate = plate;
            }
         }
      }

      public void AllApproachTypesSelection(object sender, SelectionChangedEventArgs e)
      {
         if (e.AddedItems.Count == 1 && e.AddedItems[0] is string newAp)
         {
            CurrentPlate.ApproachType = newAp;
         }
         e.Handled = true;
      }

      public void PageSizeSliderChangeEvent(object sender, RoutedPropertyChangedEventArgs<double> e)
      {
         CurrentPlate = Page.PageData[0];
         e.Handled = true;
      }

      public void GetAllApproachTypes(object sender, EventArgs e)
      {
         if (Page is null) return;

         var apTypes = new List<string>();
         foreach (var plate in Page.AllData)
         {
            if (!apTypes.Contains(plate.ApproachType))
            {
               apTypes.Add(plate.ApproachType);
            }
         }
         AllApproachTypes = new ObservableCollection<string>(apTypes);
      }
      #endregion

      private void Update()
      {
         if (CurrentPlate != null)
         {
            UpdateImage?.Invoke(this, new UpdateImageEventArgs(new Uri(CurrentPlate.PlateFile)));
         }
      }

      #endregion

      #region - Full Properties
      public Page<PlateMetaData> Page
      {
         get { return _page; }
         set
         {
            _page = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PlateCount));
         }
      }

      public string RootDirectory
      {
         get { return _rootDir; }
         set
         {
            _rootDir = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RootDirExists));
         }
      }

      public bool RootDirExists
      {
         get
         {
            return Directory.Exists(RootDirectory);
         }
      }

      public string SaveFile
      {
         get { return _saveFile; }
         set
         {
            _saveFile = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SaveFileExists));
         }
      }

      public bool SaveFileExists
      {
         get
         {
            return File.Exists(SaveFile);
         }
      }

      public ObservableCollection<PlateMetaData> AllPlates
      {
         get { return _allPlates; }
         set
         {
            _allPlates = value;
            OnPropertyChanged();
         }
      }

      public PlateMetaData CurrentPlate
      {
         get { return _currentPlate; }
         set
         {
            CheckValues();
            _currentPlate = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(AllPlates));
            OnPropertyChanged(nameof(CurrentPlateIndex));
            Update();
         }
      }

      public int CurrentPlateIndex
      {
         get
         {
            if (Page is null) return 0;
            return Page.PageData.IndexOf(CurrentPlate);
         }
      }

      public Dictionary<string, string> ApproachTypes
      {
         get { return _approachTypes; }
         set
         {
            _approachTypes = value;
            OnPropertyChanged();
         }
      }

      public ObservableCollection<string> AllApproachTypes
      {
         get
         {
            return _allApproachTypes;
         }
         set
         {
            _allApproachTypes = value;
            OnPropertyChanged();
         }
      }

      public int PlateCount
      {
         get
         {
            if (Page is null) return 0;
            return Page.AllData.Count;
         }
      }

      public bool ToggleOverwrite
      {
         get { return _toggleOverwrite; }
         set
         {
            _toggleOverwrite = value;
            OnPropertyChanged();
         }
      }
      #endregion
   }
}

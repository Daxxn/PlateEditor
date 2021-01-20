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

namespace PlateEditorWPF
{
   public class MainViewModel : ViewModel
   {
      #region - Fields & Properties
      private readonly string _null = "NA-";
      public event EventHandler<UpdateImageEventArgs> UpdateImage;
      private string _rootDir = @"B:\Games\OtherGames\FS 2020\Airport Plates\Plate Editor Test Images";
      private string _saveDir = @"B:\Games\OtherGames\FS 2020\Airport Plates\Cleaned Plates";
      private string _bookmarkFilePath = $"{Directory.GetCurrentDirectory()}\\Bookmark.txt";

      private ObservableCollection<PlateMetaData> _allPlates;
      private PlateMetaData _currentPlate;

      private Dictionary<string, string> _approachTypes;

      #region Commands
      public Command OpenRootDirCmd { get; private set; }
      public Command PrevImageCmd { get; private set; }
      public Command NextImageCmd { get; private set; }
      public Command SavePlatesCmd { get; private set; }
      public Command SavePlatesJsonCmd { get; private set; }
      public Command SaveBookmarkCmd { get; private set; }
      public Command OpenBookmarkCmd { get; private set; }
      #endregion

      private bool _toggleSaveAll;
      private bool _toggleOverwrite;
      private ObservableCollection<string> _allApproachTypes;
      #endregion

      #region - Constructors
      public MainViewModel()
      {
         OpenRootDirCmd = new Command(OpenRootDir);
         PrevImageCmd = new Command(PrevImage);
         NextImageCmd = new Command(NextImage);
         SavePlatesCmd = new Command(SavePlates);
         SavePlatesJsonCmd = new Command(SaveJsonPlates);
         SaveBookmarkCmd = new Command(SaveBookmark);
         OpenBookmarkCmd = new Command(OpenBookmark);

         ApproachTypes = PlateMetaData.ApproachTypes;
      }
      #endregion

      #region - Methods
      public void CheckValues()
      {
         if (CurrentPlate != null)
         {
            if (String.IsNullOrEmpty(CurrentPlate.IATACode))
            {
               CurrentPlate.IATACode = _null;
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
         }
      }

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

      private void ApproachTypeSelected(string value)
      {
         if (CurrentPlate != null)
         {
            CurrentPlate.GetApproachType(value);
         }
      }

      private void OpenRootDir(object p)
      {
         if (RootDirExists)
         {
            GetRootFiles();
         }
      }

      private void GetRootFiles()
      {
         try
         {
            AllPlates = new ObservableCollection<PlateMetaData>(PlateMetaData.BuildMetaData(Directory.GetFiles(RootDirectory)));
            if (AllPlates.Count > 0)
            {
               CurrentPlate = AllPlates[0];
            }
         }
         catch (Exception e)
         {
            MessageBox.Show($"{e.Message} - Unable to select file.", "Error!");
         }
      }

      private void NextImage(object p)
      {
         if (AllPlates != null)
         {
            if (CurrentPlateIndex + 1 >= AllPlates.Count)
            {
               CurrentPlate = AllPlates[0];
            }
            else
            {
               CurrentPlate = AllPlates[CurrentPlateIndex + 1];
            }
         }
      }

      private void PrevImage(object p)
      {
         if (AllPlates != null)
         {
            if (CurrentPlateIndex - 1 < 0)
            {
               CurrentPlate = AllPlates[^1];
            }
            else
            {
               CurrentPlate = AllPlates[CurrentPlateIndex - 1];
            }
         }
      }

      private void SavePlates(object p)
      {
         var savedFiles = new List<SavedFile>();
         var errOccured = false;
         if (ToggleOverwrite)
         {
            var delResult = Parallel.ForEach(Directory.GetFiles(SaveDirectory), (filePath) =>
            {
               File.Delete(filePath);
            });
         }

         var result = Parallel.ForEach(AllPlates, (plate) =>
         {
               try
               {
                  plate.Save(SaveDirectory);
                  savedFiles.Add(new SavedFile(plate.ToString(), plate.PlateFile, null));
               }
               catch (Exception e)
               {
                  errOccured = true;
                  savedFiles.Add(new SavedFile(plate.ToString(), plate.PlateFile, e));
               }
         });

         var newSaveCmpDialog = new SaveCompletedDialog(errOccured ? "Save Completed, some errors occured." : "Save Completed.", RootDirectory, SaveDirectory, savedFiles);
         newSaveCmpDialog.ShowDialog();
      }

      public void SaveJsonPlates(object p)
      {
         var errOccured = false;
         try
         {
            JsonReader.SaveJsonFile(SaveDirectory, AllPlates, true, true);
         }
         catch (Exception e)
         {
            MessageBox.Show(e.Message);
         }
      }

      private void Update()
      {
         if (CurrentPlate != null)
         {
            UpdateImage?.Invoke(this, new UpdateImageEventArgs(new Uri(CurrentPlate.PlateFile)));
         }
      }

      public void ToggleSaveAllEvent(object sender, RoutedEventArgs e)
      {
         foreach (var plate in AllPlates)
         {
            plate.WillSavePlate = ToggleSaveAll;
         }
      }

      private void SaveBookmark(object p)
      {
         StringBuilder bookmarkFileBuilder = new StringBuilder();
         bookmarkFileBuilder.AppendLine(RootDirectory);
         bookmarkFileBuilder.AppendLine(SaveDirectory);
         bookmarkFileBuilder.AppendLine(CurrentPlate.PlateFile);

         try
         {
            File.WriteAllText(_bookmarkFilePath, bookmarkFileBuilder.ToString());
         }
         catch (Exception e)
         {
            MessageBox.Show(e.Message, "Bookmark Save Error");
         }
      }

      private void OpenBookmark(object p)
      {
         try
         {
            string[] lines = File.ReadAllLines(_bookmarkFilePath);
            if (lines.Length == 3)
            {
               RootDirectory = lines[0];
               SaveDirectory = lines[1];
               string bookmarkFilePath = lines[2];

               if (RootDirExists)
               {
                  GetRootFiles();
               }

               if (AllPlates.Count > 0)
               {
                  CurrentPlate = AllPlates.First(plate => plate.PlateFile == bookmarkFilePath);
               }
            }
            else
            {
               MessageBox.Show("Bookmark file is corrupted.", "Bookmark Open Error");
            }
         }
         catch (Exception e)
         {
            MessageBox.Show(e.Message, "Bookmark Open Error");
         }
      }

      public void GetAllApproachTypes(object sender, EventArgs e)
      {
         var apTypes = new List<string>();
         foreach (var plate in AllPlates)
         {
            if (!apTypes.Contains(plate.ApproachType))
            {
               apTypes.Add(plate.ApproachType);
            }
         }
         AllApproachTypes = new ObservableCollection<string>(apTypes);
      }

      public void AllApproachTypesSelection(object sender, SelectionChangedEventArgs e)
      {
         if (e.AddedItems.Count == 1 && e.AddedItems[0] is string newAp)
         {
            CurrentPlate.ApproachType = newAp;
         }
      }
      #endregion

      #region - Full Properties
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

      public string SaveDirectory
      {
         get { return _saveDir; }
         set
         {
            _saveDir = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SaveDirExists));
         }
      }

      public bool SaveDirExists
      {
         get
         {
            return Directory.Exists(SaveDirectory);
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
            OnPropertyChanged(nameof(CurrentPlateIndex));
            Update();
         }
      }

      public int CurrentPlateIndex
      {
         get
         {
            return AllPlates.IndexOf(CurrentPlate);
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

      public bool ToggleSaveAll
      {
         get { return _toggleSaveAll; }
         set
         {
            _toggleSaveAll = value;
            OnPropertyChanged();
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
      #endregion
   }
}

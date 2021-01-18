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
using System.Threading.Tasks;
using System.Threading;

namespace PlateEditorWPF
{
   public class MainViewModel : ViewModel
   {
      #region - Fields & Properties
      private readonly string _null = "NA-";
      public event EventHandler<UpdateImageEventArgs> UpdateImage;
      private string _rootDir = @"B:\Games\OtherGames\FS 2020\Airport Plates\All Plate Images";
      private string _backupDir = @"B:\Games\OtherGames\FS 2020\Airport Plates\Cleaned Plates";
      private string _bookmarkFilePath = $"{Directory.GetCurrentDirectory()}\\Bookmark.txt";

      private ObservableCollection<PlateMetaData> _allPlates;
      //private Pl _currentFilePath;
      private PlateMetaData _currentPlate;
      private int _currentFilePathIndex;

      private Dictionary<string, string> _approachTypes;

      #region Commands
      public Command OpenRootDirCmd { get; private set; }
      public Command PrevImageCmd { get; private set; }
      public Command NextImageCmd { get; private set; }
      public Command SavePlatesCmd { get; private set; }
      public Command SaveBookmarkCmd { get; private set; }
      public Command OpenBookmarkCmd { get; private set; }
      #endregion

      private string _selectedApproachType;
      private bool _toggleSaveAll;
      #endregion

      #region - Constructors
      public MainViewModel()
      {
         OpenRootDirCmd = new Command(OpenRootDir);
         PrevImageCmd = new Command(PrevImage);
         NextImageCmd = new Command(NextImage);
         SavePlatesCmd = new Command(SavePlates);
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
               //ApproachTypeSelected(selectedKey);
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
               CurrentPlateIndex = AllPlates.IndexOf(plate);
            }
         }
      }

      private void ApproachTypeSelected(string value)
      {
         if (CurrentPlate != null)
         {
            //CurrentPlate.ApproachType = PlateMetaData.ApproachTypes[value];
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
               CurrentPlateIndex = 0;
            }
         }
         catch (Exception e)
         {
            MessageBox.Show($"{e.Message} - Unable to select file.", "Error!");
         }
      }

      //private void SetSelectedFileData()
      //{
      //   try
      //   {
      //      //CurrentFilePath = AllPlates[CurrentFilePathIndex];
      //      CurrentPlate = AllPlates[CurrentPlateIndex];
      //      if (AllPlates.Count > 0)
      //      {
      //         UpdateImage?.Invoke(this, new UpdateImageEventArgs(CurrentPlate.PlateFile));
      //      }
      //   }
      //   catch (Exception e)
      //   {
      //      MessageBox.Show($"Unable to select file.\n\n{e.Message}", "Error!");
      //   }
      //}

      private void NextImage(object p)
      {
         CurrentPlateIndex++;
      }

      private void PrevImage(object p)
      {
         CurrentPlateIndex--;
      }

      public async void SaveFilesEvent(object sender, EventArgs e)
      {
         await RenameFiles();
      }
      /// <summary>
      /// IT NO WORKIEE!! Cant fire an event delegate on a different thread.
      /// Going to have to make a copy of the file and send it to another folder.
      /// </summary>
      private async Task RenameFiles()
      {
         try
         {
            await Task.Run(() =>
            {
               foreach (var plate in AllPlates)
               {
                  if (plate.IsNameChanged)
                  {
                     string newPath = Path.Combine(SaveDirectory, plate.FileName);
                     File.Copy(plate.PlateFile.LocalPath, newPath, true);
                  }
               }
            });

            MessageBox.Show("All Plates Saved.", "Done");
         }
         catch (Exception e)
         {
            MessageBox.Show(e.Message);
         }
      }

      private void SavePlates(object p)
      {
         var savedFiles = new List<SavedFile>();
         var errOccured = false;
         var result = Parallel.ForEach(AllPlates, (plate) =>
         {
            if (plate.WillSavePlate)
            {
               try
               {
                  plate.Save(SaveDirectory);
                  savedFiles.Add(new SavedFile(plate.ToString(), plate.PlateFile.LocalPath, null));
               }
               catch (Exception e)
               {
                  errOccured = true;
                  savedFiles.Add(new SavedFile(plate.ToString(), plate.PlateFile.LocalPath, e));
               }
            }
         });

         var newSaveCmpDialog = new SaveCompletedDialog(errOccured ? "Save Completed, some errors occured." : "Save Completed.", RootDirectory, SaveDirectory, savedFiles);
         newSaveCmpDialog.ShowDialog();
      }

      private void Update()
      {
         if (CurrentPlate != null)
         {
            UpdateImage?.Invoke(this, new UpdateImageEventArgs(CurrentPlate.PlateFile));
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
         bookmarkFileBuilder.AppendLine(CurrentPlate.PlateFile.LocalPath);

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
                  CurrentPlate = AllPlates.First(plate => plate.PlateFile.LocalPath == bookmarkFilePath);
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
         get { return _backupDir; }
         set
         {
            _backupDir = value;
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
            if (value != null)
            {
               SelectedApproachType = value.GetApproachTypeKey(value.ApproachType);
            }
            else
            {
               SelectedApproachType = _null;
            }
            //ApproachTypeSelected(value.ApproachType);
            OnPropertyChanged();
            Update();
         }
      }

      public int CurrentPlateIndex
      {
         get { return _currentFilePathIndex; }
         set
         {
            if (AllPlates != null)
            {
               if (value >= AllPlates.Count)
               {
                  _currentFilePathIndex = 0;
                  CurrentPlate = AllPlates[0];
               }
               else if (value < 0)
               {
                  _currentFilePathIndex =
                     AllPlates != null || AllPlates.Count > 0
                     ? AllPlates.Count - 1
                     : 0;
                  CurrentPlate = AllPlates[AllPlates.Count - 1];
               }
               else
               {
                  _currentFilePathIndex = value;
                  CurrentPlate = AllPlates[value];
               }
               OnPropertyChanged(nameof(CurrentPlate));
               //SetSelectedFileData();
            }
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

      public string SelectedApproachType
      {
         get { return _selectedApproachType; }
         set
         {
            _selectedApproachType = value;
            CurrentPlate.ApproachType = PlateMetaData.ApproachTypes[value];
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
      #endregion
   }
}

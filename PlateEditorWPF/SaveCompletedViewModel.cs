using MVVMLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace PlateEditorWPF
{
   public class SaveCompletedViewModel : ViewModel
   {
      #region - Fields & Properties
      private string _message;
      private string _source;
      private string _save;
      private ObservableCollection<SavedFile> _savedFiles;
      #endregion

      #region - Constructors
      public SaveCompletedViewModel(string message, string source, string save, List<SavedFile> savedFiles)
      {
         Message = message;
         Source = source;
         Save = save;
         SavedFiles = new ObservableCollection<SavedFile>(savedFiles);
      }
      #endregion

      #region - Methods

      #endregion

      #region - Full Properties
      public string Message
      {
         get { return _message; }
         set
         {
            _message = value;
            OnPropertyChanged();
         }
      }

      public string Source
      {
         get { return _source; }
         set
         {
            _source = value;
            OnPropertyChanged();
         }
      }

      public string Save
      {
         get { return _save; }
         set
         {
            _save = value;
            OnPropertyChanged();
         }
      }

      public ObservableCollection<SavedFile> SavedFiles
      {
         get { return _savedFiles; }
         set
         {
            _savedFiles = value;
            OnPropertyChanged();
         }
      }
      #endregion
   }
}

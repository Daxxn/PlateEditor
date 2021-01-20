using System;
using System.Collections.Generic;
using System.Text;

namespace PlateEditorWPF.Models
{
   public struct SavedFile
   {
      #region - Fields & Properties
      public string Name { get; private set; }
      public string Path { get; private set; }
      public Exception Error { get; private set; }
      #endregion

      #region - Constructors
      public SavedFile(string name, string path, Exception error)
      {
         Name = name;
         Path = path;
         Error = error;
      }
      #endregion

      #region - Methods
      #endregion

      #region - Full Properties
      #endregion
   }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace PlateEditorWPF.Models
{
   /// <summary>
   /// Saves some basic state for loading later.
   /// </summary>
   public class Bookmark
   {
      #region - Fields & Properties
      public int CurrentPlateIndex { get; }
      public string CurrentSourceFolder { get; }
      public string CurrentSaveFile { get; }
      public int CurrentPageNumber { get; }
      #endregion

      #region - Constructors
      public Bookmark() { }
      public Bookmark(
         int currentPlateIndex,
         int currentPageNumber,
         string currentSourceFolder,
         string currentSaveFile
         )
      {
         CurrentPlateIndex = currentPlateIndex;
         CurrentPageNumber = currentPageNumber;
         CurrentSourceFolder = currentSourceFolder;
         CurrentSaveFile = currentSaveFile;
      }
      #endregion

      #region - Methods

      #endregion

      #region - Full Properties

      #endregion
   }
}

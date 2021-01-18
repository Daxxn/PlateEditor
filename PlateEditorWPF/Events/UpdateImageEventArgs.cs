using System;
using System.Collections.Generic;
using System.Text;

namespace PlateEditorWPF.Events
{
   public class UpdateImageEventArgs : EventArgs
   {
      public Uri NewUri { get; private set; }
      public bool ClearImage { get; private set; } = false;
      public UpdateImageEventArgs(Uri newUri)
      {
         NewUri = newUri;
      }
      public UpdateImageEventArgs(bool clear = true)
      {
         ClearImage = clear;
      }
   }
}

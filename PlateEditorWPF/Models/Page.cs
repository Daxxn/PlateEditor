using MVVMLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace PlateEditorWPF.Models
{
   /// <summary>
   /// Controls a large list of items with the page design principle.
   /// </summary>
   /// <typeparam name="T">The object stored in the page object</typeparam>
   public class Page<T> : Model
   {
      #region - Fields & Properties
      private List<string> _allFiles;
      private List<T> _allData;
      private ObservableCollection<T> _pageData;
      private int _pageSize;
      private int _pageStart = 0;
      private int _pageEnd;
      private int _pageNumber;
      #endregion

      #region - Constructors
      public Page(int pageSize, IEnumerable<T> data)
      {
         AllData = data.ToList();
         PageSize = pageSize;
         PageStart = 0;
         PageNumber = 0;
         //PageEnd = pageSize;
      }
      #endregion

      #region - Methods
      /// <summary>
      /// Selects the next page based on the PageNumber property.
      /// <para/>
      /// creates a new <see cref="ObservableCollection{T}"/> with items between the current
      /// and next multiple of the PageSize and PageNumber properties.
      /// This reduces the number of <see cref="ObservableCollection{T}"/>s stored in memory at any time,
      /// reducing the load on the UI thread.
      /// </summary>
      public void SelectPage()
      {
         PageData = new ObservableCollection<T>();
         int tempPageStart = PageSize * PageNumber;
         int tempPageEnd = PageSize * (PageNumber + 1);
         if (tempPageStart < AllData.Count)
         {
            PageStart = tempPageStart;
            PageEnd = tempPageEnd;
            PageData = new ObservableCollection<T>();
            for (int i = PageStart; i < PageEnd; i++)
            {
               if (i < AllData.Count && i >= 0)
               {
                  PageData.Add(AllData[i]);
               }
            }
         }
      }

      /// <summary>
      /// Manually selects a page based on the provided value.
      /// <para/>
      /// creates a new <see cref="ObservableCollection{T}"/> with items between the current
      /// and next multiple of the PageSize property and the <paramref name="pageNumber"/> value.
      /// This reduces the number of <see cref="ObservableCollection{T}"/>s stored in memory at any time,
      /// reducing the load on the UI thread.
      /// </summary>
      /// <param name="pageNumber">The manually selected page to display.</param>
      public void SelectPage(int pageNumber)
      {
         PageData = new ObservableCollection<T>();
         int tempPageStart = PageSize * pageNumber;
         int tempPageEnd = PageSize * (pageNumber + 1);
         if (tempPageStart < AllData.Count)
         {
            PageStart = tempPageStart;
            PageEnd = tempPageEnd;
            PageData = new ObservableCollection<T>();
            for (int i = PageStart; i < PageEnd; i++)
            {
               PageData.Add(AllData[i]);
            }
         }
      }
      #endregion

      #region - Full Properties
      public List<string> AllFiles
      {
         get { return _allFiles; }
         set
         {
            _allFiles = value;
            OnPropertyChanged();
         }
      }

      public List<T> AllData
      {
         get { return _allData; }
         set
         {
            _allData = value;
            OnPropertyChanged();
         }
      }

      public ObservableCollection<T> PageData
      {
         get { return _pageData; }
         set
         {
            _pageData = value;
            OnPropertyChanged();
         }
      }

      public int PageSize
      {
         get { return _pageSize; }
         set
         {
            _pageSize = value;
            OnPropertyChanged();
         }
      }

      public int PageStart
      {
         get { return _pageStart; }
         set
         {
            _pageStart = value;
            OnPropertyChanged();
         }
      }

      public int PageEnd
      {
         get { return _pageEnd; }
         set
         {
            _pageEnd = value;
            OnPropertyChanged();
         }
      }

      public int PageCount
      {
         get => (int)Math.Floor((double)AllData.Count / (double)PageSize);
      }

      public int PageNumber
      {
         get { return _pageNumber; }
         set
         {
            if (value < 0)
            {
               _pageNumber = PageCount;
            }
            else if (value > PageCount)
            {
               _pageNumber = 0;
            }
            else
            {
               _pageNumber = value;
            }
            SelectPage();
            OnPropertyChanged();
            OnPropertyChanged(nameof(PageNumberDisp));
            OnPropertyChanged(nameof(PageData));
         }
      }

      public int PageNumberDisp
      {
         get => PageNumber + 1;
         set => PageNumber = value - 1;
      }
      #endregion
   }
}

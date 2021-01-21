using MVVMLibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PlateEditorWPF.Models
{
   /// <summary>
   /// Controls a large list of items with the page design principle.
   /// </summary>
   /// <typeparam name="T">The object stored in the page object</typeparam>
   public class Page<T> : Model
   {
      #region - Fields & Properties
      private List<T> _allData;
      private ObservableCollection<T> _pageData;
      private int _pageSize;
      private int _pageStart = 0;
      private int _pageEnd;
      private int _pageNumber;
      #endregion

      #region - Constructors
      /// <summary>
      /// Initializes a new instance of a <see cref="Page{T}"/>, sets the data to be used.
      /// </summary>
      /// <param name="pageSize">The number of items stored in a page.</param>
      /// <param name="data">All the data the pages will use.</param>
      public Page(int pageSize, IEnumerable<T> data)
      {
         AllData = data.ToList();
         PageSize = pageSize;
         PageStart = 0;
         PageNumber = 0;
      }
      #endregion

      #region - Methods
      /// <summary>
      /// Appends new data to old.
      /// </summary>
      /// <param name="newData">The new data.</param>
      public void Append(IEnumerable<T> newData)
      {
         if (newData.Count() > 0)
         {
            AllData.AddRange(newData);
            SelectPage(0);
         }
      }

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
      /// <summary>
      /// All the data to display. <para/> this should only be set once, with the constructor.
      /// </summary>
      public List<T> AllData
      {
         get { return _allData; }
         private set
         {
            _allData = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PageCount));
            OnPropertyChanged(nameof(DataCount));
            OnPropertyChanged(nameof(PageCountDisp));
         }
      }
      public int DataCount
      {
         get
         {
            return AllData.Count;
         }
      }

      /// <summary>
      /// The items this page contains.
      /// </summary>
      public ObservableCollection<T> PageData
      {
         get { return _pageData; }
         set
         {
            _pageData = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PageCount));
            OnPropertyChanged(nameof(DataCount));
            OnPropertyChanged(nameof(PageCountDisp));
         }
      }

      /// <summary>
      /// The number of items stored in a page.
      /// </summary>
      public int PageSize
      {
         get { return _pageSize; }
         set
         {
            _pageSize = value;
            SelectPage(0);
            OnPropertyChanged();
            OnPropertyChanged(nameof(PageCount));
            OnPropertyChanged(nameof(DataCount));
            OnPropertyChanged(nameof(PageCountDisp));
         }
      }

      /// <summary>
      /// The starting index of the current page.
      /// </summary>
      public int PageStart
      {
         get { return _pageStart; }
         set
         {
            _pageStart = value;
            OnPropertyChanged();
         }
      }

      /// <summary>
      /// The ending index of the current page.
      /// </summary>
      public int PageEnd
      {
         get { return _pageEnd; }
         set
         {
            _pageEnd = value;
            OnPropertyChanged();
         }
      }

      /// <summary>
      /// The number of pages total.
      /// </summary>
      public int PageCount
      {
         get => (int)Math.Floor((double)AllData.Count / (double)PageSize);
      }

      /// <summary>
      /// The current page number. <para/> Used to calc the current page indeces.
      /// </summary>
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
            OnPropertyChanged(nameof(DataCount));
            OnPropertyChanged(nameof(PageCountDisp));
         }
      }

      /// <summary>
      /// The corrected page number to display.
      /// </summary>
      public int PageNumberDisp
      {
         get => PageNumber + 1;
      }

      public int PageCountDisp
      {
         get => PageCount + 1;
      }
      #endregion
   }
}

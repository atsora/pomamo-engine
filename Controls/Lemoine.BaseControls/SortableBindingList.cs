// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms;

using Lemoine.Core.Log;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Provides a generic collection that supports data binding and additionally supports sorting.
  /// See http://msdn.microsoft.com/en-us/library/ms993236.aspx
  /// If the elements are IComparable it uses that; otherwise compares the ToString()
  /// </summary>
  /// <typeparam name="T">The type of elements in the list.</typeparam>

  [Serializable()]
  public class SortableBindingList<T> : BindingList<T>, ITypedList
  {
    #region Members
    private bool                            _isSorted;
    private ListSortDirection               _dir = ListSortDirection.Ascending;
    private bool                            _sortColumns = true;

    [NonSerialized()]
    private PropertyDescriptorCollection    _shape=null;

    [NonSerialized()]
    private PropertyDescriptor              _sort = null;

    #endregion // Members
    
    static readonly ILog log = LogManager.GetLogger(typeof (SortableBindingList<T>).FullName);
    
    #region Constructor
    /// <summary>
    /// Constructor
    /// </summary>
    public SortableBindingList() : base()
    {
      /* Default to sorted columns */
      _sortColumns = true;

      /* Get shape (only get public properties marked browsable true) */
      _shape = GetShape();
    }
    #endregion // Constructor

    #region SortedBindingList<T> Column Sorting API
    /// <summary>
    /// SortColumns
    /// </summary>
    public bool SortColumns
    {
      get { return _sortColumns; }
      set
      {
        if (value != _sortColumns)
        {
          /* Set Column Sorting */
          _sortColumns = value;

          /* Set shape */
          _shape = GetShape();

          /* Fire MetaDataChanged */
          OnListChanged(new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, -1));
        }
      }
    }
    #endregion

    #region BindingList<T> Public Sorting API

    /// <summary>
    /// Sort
    /// </summary>
    public void Sort()
    {
      ApplySortCore(_sort, _dir);
    }

    /// <summary>
    /// Sort
    /// </summary>
    public void Sort(string property)
    {
      /* Get the PD */
      _sort = FindPropertyDescriptor(property);

      /* Sort */
      ApplySortCore(_sort, _dir);
    }

    /// <summary>
    /// Sort
    /// </summary>
    public void Sort(string property, ListSortDirection direction)
    {
      /* Get the sort property */
      _sort = FindPropertyDescriptor(property);
      _dir = direction;

      /* Sort */
      ApplySortCore(_sort, _dir);
    }
    #endregion

    #region BindingList<T> Sorting Overrides
    /// <summary>
    /// SupportsSortingCore
    /// </summary>
    protected override bool SupportsSortingCore
    {
      get { return true; }
    }

    /// <summary>
    /// ApplySortCore
    /// </summary>
    protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
    {
      List<T>     items = this.Items as List<T>;

      if ((null != items) && (null != property))
      {
        PropertyComparer<T> pc = new PropertyComparer<T>(property, direction);
        items.Sort(pc);

        /* Set sorted */
        _isSorted = true;
      }
      else
      {
        /* Set sorted */
        _isSorted = false;
      }
    }

    /// <summary>
    /// IsSortedCore
    /// </summary>
    protected override bool IsSortedCore
    {
      get { return _isSorted; }
    }

    /// <summary>
    /// RemoveSortCore
    /// </summary>
    protected override void RemoveSortCore()
    {
      _isSorted = false;
    }
    #endregion

    #region SortedBindingList<T> Private Sorting API
    /// <summary>
    /// FindPropertyDescriptor
    /// </summary>
    private PropertyDescriptor FindPropertyDescriptor(string property)
    {
      PropertyDescriptor prop = null;

      if (null != _shape)
      {
        prop = _shape.Find(property, true);
      }

      return prop;
    }

    /// <summary>
    /// PropertyDescriptorCollection
    /// </summary>
    private PropertyDescriptorCollection GetShape()
    {
      /* Get shape (only get public properties marked browsable true) */
      PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(typeof(T), new Attribute[] { new BrowsableAttribute(true) });

      /* Sort if required */
      if (_sortColumns)
      {
        pdc = pdc.Sort();
      }

      return pdc;
    }
    #endregion

    #region PropertyComparer<TKey>
    internal class PropertyComparer<TKey> : System.Collections.Generic.IComparer<TKey>
    {
      /*
       * The following code contains code implemented by Rockford Lhotka:
       * msdn.microsoft.com/library/default.asp?url=/library/en-us/dnadvnet/html/vbnet01272004.asp"
       */

      private PropertyDescriptor  _property;
      private ListSortDirection   _direction;

      public PropertyComparer(PropertyDescriptor property, ListSortDirection direction)
      {
        _property = property;
        _direction = direction;
      }

      public int Compare(TKey xVal, TKey yVal)
      {
        /* Get property values */
        object xValue = GetPropertyValue(xVal, _property);
        object yValue = GetPropertyValue(yVal, _property);

        /* Determine sort order */
        if (_direction == ListSortDirection.Ascending)
        {
          return CompareAscending(xValue, yValue);
        }
        else
        {
          return CompareDescending(xValue, yValue);
        }
      }

      public bool Equals(TKey xVal, TKey yVal)
      {
        return xVal.Equals(yVal);
      }

      public int GetHashCode(TKey obj)
      {
        return obj.GetHashCode();
      }

      /* Compare two property values of any type */
      private int CompareAscending(object xValue, object yValue)
      {
        int result;

        if (xValue == null) {
          if (yValue == null) {
            return 0;
          }
          else {
            return -1;
          }
        }
        else if (yValue == null) {
          return 1;
        }
        else  if (xValue is IComparable) {
          /* If values implement IComparer */
          result = ((IComparable)xValue).CompareTo(yValue);
        }
        else if (xValue.Equals(yValue)) {
          /* If values don't implement IComparer but are equivalent */
          result = 0;
        }
        else {
          /* Values don't implement IComparer and are not equivalent, so compare as string values */
          result = xValue.ToString().CompareTo(yValue.ToString());
        }

        /* Return result */
        return result;
      }

      private int CompareDescending(object xValue, object yValue)
      {
        return CompareAscending(yValue, xValue);
      }

      private object GetPropertyValue(TKey value, PropertyDescriptor property)
      {
        /* Get property */
        return property.GetValue(value);
      }
    }
    #endregion

    #region ITypedList Implementation
    /// <summary>
    /// GetItemProperties
    /// </summary>
    /// <param name="listAccessors"></param>
    /// <returns></returns>
    public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
    {
      PropertyDescriptorCollection    pdc = null;

      if (null == listAccessors)
      {
        /* Return properties in sort order */
        pdc = _shape;
      }
      else
      {
        /* Return child list shape */
        pdc = ListBindingHelper.GetListItemProperties(listAccessors[0].PropertyType);
      }

      return pdc;
    }

    /// <summary>
    /// GetListName
    /// </summary>
    /// <param name="listAccessors"></param>
    /// <returns></returns>
    public string GetListName(PropertyDescriptor[] listAccessors)
    {
      /* Not really used anywhere other than DT and the old DataGrid */
      return typeof(T).Name;
    }
    #endregion
  }
}
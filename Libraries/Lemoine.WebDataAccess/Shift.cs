// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of Shift.
  /// </summary>
  public class Shift
    : Lemoine.Model.IShift
    , IDisplayPriorityCodeNameComparerItem
  {
    #region ISerializableModel implementation
    public void Unproxy()
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IDisplayable implementation
    public string GetDisplay(string variant)
    {
      throw new NotImplementedException();
    }
    public string Display {
      get; set;
    }
    #endregion
    #region IShift implementation
    public int Id {
      get; set;
    }
    public string Name {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public string Code {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public string ExternalCode {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public string Color {
      get; set;
    }
    public int? DisplayPriority {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    #endregion
    #region IDataWithVersion implementation
    public int Version {
      get {
        throw new NotImplementedException();
      }
    }
    #endregion
    #region ISelectionable implementation
    public string SelectionText {
      get {
        throw new NotImplementedException();
      }
    }
    #endregion
    #region IComparable implementation
    /// <summary>
    /// <see cref="IComparable"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int CompareTo (object obj)
    {
      if (obj == null) {
        return int.MinValue;
      }

      var other = obj as IShift;
      if (other != null) {
        return new DisplayPriorityCodeNameComparer<IShift, Shift> ()
          .Compare (this, other);
      }
      else {
        throw new ArgumentException ("other is not a Shift", "obj");
      }
    }
    #endregion // IComparable implementation
    #region IComparable<IMachine> implementation
    /// <summary>
    /// <see cref="IComparable{T}"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo (IShift other)
    {
      if (other is null) {
        return int.MinValue;
      }
      return new DisplayPriorityCodeNameComparer<IShift, Shift> ()
        .Compare (this, other);
    }
    /// <summary>
    /// <see cref="IComparer{T}"/>
    /// </summary>
    public class DisplayComparer : IComparer<IShift>
    {
      /// <summary>
      /// Implementation of DisplayComparer
      /// 
      /// Use the following properties to sort the machines:
      /// <item>Display priority</item>
      /// <item>Code</item>
      /// <item>Name</item>
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      int IComparer<IShift>.Compare (IShift x, IShift y)
      {
        return (new DisplayPriorityCodeNameComparer<IShift, Shift> ())
          .Compare (x, y);
      }
    }
    #endregion // IComparable<IShift> implementation
  }
}

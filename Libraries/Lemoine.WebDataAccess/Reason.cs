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
  /// Description of Reason.
  /// </summary>
  public class Reason
    : Lemoine.Model.IReason
    , IDisplayPriorityComparerItem
  {
    #region ISerializableModel implementation
    public void Unproxy()
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IReason implementation
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
    public string TranslationKey {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public string NameOrTranslation {
      get {
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
    public string Description {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public string DescriptionTranslationKey {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public string DescriptionOrTranslation {
      get {
        throw new NotImplementedException();
      }
    }
    public string Color {
      get; set;
    }
    public string CustomColor {
      get {
        throw new NotImplementedException ();
      }
      set {
        throw new NotImplementedException ();
      }
    }
    public Lemoine.Model.LinkDirection LinkOperationDirection {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public Lemoine.Model.IReasonGroup ReasonGroup {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public string ReportColor {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public string CustomReportColor {
      get {
        throw new NotImplementedException ();
      }
      set {
        throw new NotImplementedException ();
      }
    }
    public int? DisplayPriority
    {
      get {
        throw new NotImplementedException ();
      }
      set {
        throw new NotImplementedException ();
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
    #region IDisplayable implementation
    public string Display {
      get; set;
    }
    public string GetDisplay (string variant)
    {
      throw new NotImplementedException ();
    }
    #endregion
    public string LongDisplay {
      get; set;
    }
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

      var other = obj as IReason;
      if (other != null) {
        return new DisplayPriorityComparer<IReason, Reason> ()
          .Compare (this, other);
      }
      else {
        throw new ArgumentException ("other is not a Reason", "obj");
      }
    }
    #endregion // IComparable implementation
    #region IComparable<IReason> implementation
    /// <summary>
    /// <see cref="IComparable{T}"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo (IReason other)
    {
      if (other is null) {
        return int.MinValue;
      }
      return new DisplayPriorityComparer<IReason, Reason> ()
        .Compare (this, other);
    }
    /// <summary>
    /// <see cref="IComparer{T}"/>
    /// </summary>
    public class DisplayComparer : IComparer<IReason>
    {
      /// <summary>
      /// Implementation of DisplayComparer
      /// 
      /// Use the following properties to sort the reasons:
      /// <item>Display priority</item>
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      int IComparer<IReason>.Compare (IReason x, IReason y)
      {
        return (new DisplayPriorityComparer<IReason, Reason> ())
          .Compare (x, y);
      }
    }
    #endregion // IComparable<IReason> implementation
  }
}

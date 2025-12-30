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
  /// Description of Machine.
  /// </summary>
  public class Machine
    : Lemoine.Model.IMachine
    , IDisplayPriorityCodeNameComparerItem
  {
    #region IMachine implementation
    public bool IsMonitored()
    {
      throw new NotImplementedException();
    }
    public bool IsObsolete()
    {
      throw new NotImplementedException();
    }
    public int Id {
      get; set;
    }
    public string Name {
      get
      {
        throw new NotImplementedException ();
      }
      set
      {
        throw new NotImplementedException ();
      }
    }
    public string Code {
      get
      {
        throw new NotImplementedException ();
      }
      set
      {
        throw new NotImplementedException ();
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
    public Lemoine.Model.IMachineMonitoringType MonitoringType {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public int? DisplayPriority {
      get; set;
    }
    public Lemoine.Model.ICompany Company {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public Lemoine.Model.IDepartment Department {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public Lemoine.Model.ICell Cell {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public Lemoine.Model.IMachineCategory Category {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public Lemoine.Model.IMachineSubCategory SubCategory {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    #endregion
    #region ISerializableModel implementation
    public void Unproxy()
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IDataWithVersion implementation
    public int Version {
      get
      {
        throw new NotImplementedException ();
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
    #region IDataWithIdentifiers implementation
    public string[] Identifiers {
      get {
        throw new NotImplementedException();
      }
    }

    public double? CostOff { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }
    public double? CostInactive { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }
    public double? CostActive { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }
    public IMachineStateTemplate DefaultMachineStateTemplate { get => throw new NotImplementedException (); set => throw new NotImplementedException (); }
    #endregion
    #region IComparable implementation
    /// <summary>
    /// <see cref="IComparable"/>
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int CompareTo (object obj)
    {
      if (obj == null) {
        return int.MinValue;
      }

      var other = obj as IMachine;
      if (other != null) {
        return new DisplayPriorityCodeNameComparer<IMachine, Machine> ()
          .Compare (this, other);
      }
      else {
        throw new ArgumentException ("other is not a Machine", "obj");
      }
    }
    #endregion // IComparable implementation
    #region IComparable<IMachine> implementation
    /// <summary>
    /// <see cref="IComparable{T}"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo (IMachine other)
    {
      if (other is null) {
        return int.MinValue;
      }
      return new DisplayPriorityCodeNameComparer<IMachine, Machine> ()
        .Compare (this, other);
    }
    /// <summary>
    /// <see cref="IComparer{T}"/>
    /// </summary>
    public class DisplayComparer : IComparer<IMachine>
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
      int IComparer<IMachine>.Compare (IMachine x, IMachine y)
      {
        return (new DisplayPriorityCodeNameComparer<IMachine, Machine> ())
          .Compare (x, y);
      }
    }
    #endregion // IComparable<IMachine> implementation
  }
}

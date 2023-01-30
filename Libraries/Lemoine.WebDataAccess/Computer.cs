// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of Computer.
  /// </summary>
  public class Computer : Lemoine.Model.IComputer
  {
    #region ISerializableModel implementation
    public void Unproxy ()
    {
      throw new NotImplementedException ();
    }
    #endregion
    #region IComputer implementation
    public int Id {
      get; set;
    }
    public string Name {
      get; set;
    }
    public string Address {
      get; set;
    }
    public bool IsLctr { get; set; }
    public bool IsLpst { get; set; }
    public bool IsCnc { get; set; }
    public bool IsWeb { get; set; }
    public bool IsAutoReason { get; set; }
    public bool IsAlert { get; set; }
    public bool IsSynchronization { get; set; }

    public string WebServiceUrl { get; set; }
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
      get {
        throw new NotImplementedException();
      }
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
    #endregion
  }
}

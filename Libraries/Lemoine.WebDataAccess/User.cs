// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of User.
  /// </summary>
  public class User : Lemoine.Model.IUser
  {
    #region IUpdater implementation
    public T As<T> () where T : class, Lemoine.Model.IUpdater
    {
      throw new NotImplementedException ();
    }
    public System.Collections.Generic.ICollection<Lemoine.Model.IRevision> Revisions
    {
      get {
        throw new NotImplementedException ();
      }
    }
    #endregion
    #region ISerializableModel implementation
    public void Unproxy ()
    {
      throw new NotImplementedException ();
    }
    #endregion
    #region IUser implementation
    public int Id
    {
      get; set;
    }
    public string Name
    {
      get; set;
    }
    public string Code
    {
      get {
        throw new NotImplementedException ();
      }
      set {
        throw new NotImplementedException ();
      }
    }
    public string ExternalCode
    {
      get {
        throw new NotImplementedException ();
      }
      set {
        throw new NotImplementedException ();
      }
    }
    public string Login
    {
      get; set;
    }
    public string Password
    {
      get {
        throw new NotImplementedException ();
      }
      set {
        throw new NotImplementedException ();
      }
    }
    public Lemoine.Model.IShift Shift
    {
      get {
        throw new NotImplementedException ();
      }
      set {
        throw new NotImplementedException ();
      }
    }
    public string MobileNumber
    {
      get {
        throw new NotImplementedException ();
      }
      set {
        throw new NotImplementedException ();
      }
    }
    public IRole Role
    {
      get {
        throw new NotImplementedException ();
      }
      set {
        throw new NotImplementedException ();
      }
    }
    public string EMail
    {
      get {
        throw new NotImplementedException ();
      }
      set {
        throw new NotImplementedException ();
      }
    }
    public ICompany Company
    {
      get {
        throw new NotImplementedException ();
      }
      set {
        throw new NotImplementedException ();
      }
    }

    public TimeSpan? DisconnectionTime
    {
      get => throw new NotImplementedException ();
      set => throw new NotImplementedException ();
    }
    #endregion

    #region IDisplayable implementation
    public string Display
    {
      get {
        throw new NotImplementedException ();
      }
    }
    public string GetDisplay (string variant)
    {
      throw new NotImplementedException ();
    }
    #endregion
    #region IDataWithVersion implementation
    public int Version
    {
      get {
        throw new NotImplementedException ();
      }
    }

    #endregion
  }
}

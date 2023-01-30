// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using Lemoine.Core.Log;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of MachineStateTemplate.
  /// </summary>
  public class MachineStateTemplate: Lemoine.Model.IMachineStateTemplate
  {
    #region IMachineStateTemplate implementation
    public Lemoine.Model.IMachineStateTemplateItem AddItem(Lemoine.Model.IMachineObservationState machineObservationState)
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IMachineStateTemplateItem InsertItem(int position, Lemoine.Model.IMachineObservationState machineObservationState)
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IMachineStateTemplateStop AddStop()
    {
      throw new NotImplementedException();
    }
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

    public Lemoine.Model.MachineStateTemplateCategory? Category {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }

    public bool UserRequired {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public bool ShiftRequired {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public bool? OnSite {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public Lemoine.Model.IMachineStateTemplate SiteAttendanceChange {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public System.Collections.Generic.IList<Lemoine.Model.IMachineStateTemplateItem> Items {
      get {
        throw new NotImplementedException();
      }
    }
    public ISet<Lemoine.Model.IMachineStateTemplateStop> Stops {
      get {
        throw new NotImplementedException();
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
    #endregion
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
    #region IDataWithIdentifiers implementation
    public string[] Identifiers {
      get {
        throw new NotImplementedException();
      }
    }
    #endregion
  }
}
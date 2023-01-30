// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of Revision.
  /// </summary>
  public class Revision: Lemoine.Model.IRevision
  {
    #region IRevision implementation
    public void AddModification(Lemoine.Model.IModification modification)
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IUpdater Updater {
      get; set;
    }
    public DateTime DateTime {
      get; set;
    }
    public string Comment {
      get; set;
    }
    public string IPAddress {
      get; set;
    }
    public string Application {
      get; set;
    }
    public System.Collections.Generic.ICollection<Lemoine.Model.IGlobalModification> GlobalModifications {
      get {
        throw new NotImplementedException();
      }
    }
    public System.Collections.Generic.ICollection<Lemoine.Model.IMachineModification> MachineModifications {
      get {
        throw new NotImplementedException();
      }
    }
    public System.Collections.Generic.IEnumerable<Lemoine.Model.IModification> Modifications {
      get {
        throw new NotImplementedException();
      }
    }
    #endregion
    #region IDataWithId implementation
    public int Id {
      get; set;
    }
    #endregion
  }
}

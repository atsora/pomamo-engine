// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table Revision
  /// </summary>
  public interface IRevision: Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Associated updater if known
    /// </summary>
    IUpdater Updater { get; set; }
    
    /// <summary>
    /// Date/time of the revision
    /// </summary>
    DateTime DateTime { get; set; }
    
    /// <summary>
    /// Comment
    /// </summary>
    string Comment { get; set; }

    /// <summary>
    /// IP address (source of the revision)
    /// </summary>
    string IPAddress { get; set; }
    
    /// <summary>
    /// Application (source of the revision)
    /// </summary>
    string Application { get; set; }
    
    /// <summary>
    /// Associated global modifications
    /// </summary>
    ICollection<IGlobalModification> GlobalModifications { get; }

    /// <summary>
    /// Associated machine modifications
    /// </summary>
    ICollection<IMachineModification> MachineModifications { get; }
    
    /// <summary>
    /// Associated modifications (global and machine)
    /// </summary>
    IEnumerable<IModification> Modifications { get; }
    
    /// <summary>
    /// Add a modification to the revision
    /// </summary>
    void AddModification (IModification modification);
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table TaskMachineAssociation
  /// </summary>
  public interface ITaskMachineAssociation: IMachineModification
  {
    /// <summary>
    /// UTC date/time range
    /// </summary>
    UtcDateTimeRange Range { get; }

    /// <summary>
    /// Task to associate to a machine with a work order
    /// </summary>
    ITask Task { get; set; }
    
    /// <summary>
    /// Association option
    /// </summary>
    AssociationOption? Option { get; set; }
  }
}

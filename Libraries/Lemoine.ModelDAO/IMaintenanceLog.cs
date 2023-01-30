// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Maintenance state
  /// </summary>
  public enum MaintenanceState
  {
    /// <summary>
    /// Started
    /// </summary>
    Started = 0,
    /// <summary>
    /// Completed successfully
    /// </summary>
    Success = 1,
    /// <summary>
    /// In error
    /// </summary>
    Error = 2,
    /// <summary>
    /// Re-index of the cncvalue table is completed
    /// </summary>
    ReindexCncValueCompleted = 3,
    /// <summary>
    /// Re-index of the fact table is completed
    /// </summary>
    ReindexFactCompleted = 4,
    /// <summary>
    /// Re-index of the reasonslot table is completed
    /// </summary>
    ReindexReasonSlotCompleted = 5,
    /// <summary>
    /// Re-index of the sequenceslot table is completed
    /// </summary>
    ReindexSequenceSlotCompleted = 6,
    /// <summary>
    /// Re-index of the operationslot table is completed
    /// </summary>
    ReindexOperationSlotCompleted = 7,
    /// <summary>
    /// Re-index is completed
    /// </summary>
    ReindexCompleted = 8,
    /// <summary>
    /// The backend was terminated
    /// </summary>
    TerminateBackendCompleted = 9,
  }
  
  /// <summary>
  /// Description of IMaintenanceLog.
  /// </summary>
  public interface IMaintenanceLog
  {
    /// <summary>
    /// Maintenance state
    /// </summary>
    string State { get; }
  }
}

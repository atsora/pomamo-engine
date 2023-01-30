// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Virtual machine mode sub-slot for IReasonOnlySlot
  /// </summary>
  public interface IMachineModeSubSlot
    : IMergeableItem<IMachineModeSubSlot>
  {
    /// <summary>
    /// Associated machine mode
    /// </summary>
    IMachineMode MachineMode { get; }
  }
  
  /// <summary>
  /// Virtual machine observation state sub-slot for IReasonOnlySlot
  /// </summary>
  public interface IMachineObservationStateSubSlot
    : IMergeableItem<IMachineObservationStateSubSlot>
  {
    /// <summary>
    /// Associated machine mode
    /// </summary>
    IMachineObservationState MachineObservationState { get; }
  }

  /// <summary>
  /// Virtual reason sub-slot for IMachineModeSlot
  /// </summary>
  public interface IReasonSubSlot
    : IMergeableItem<IReasonSubSlot>
  {
    /// <summary>
    /// Associated reason
    /// </summary>
    IReason Reason { get; }
    
    /// <summary>
    /// Associated details
    /// </summary>
    string ReasonDetails { get; }
    
    /// <summary>
    /// Default reason
    /// </summary>
    bool DefaultReason { get; }
  }
  
}

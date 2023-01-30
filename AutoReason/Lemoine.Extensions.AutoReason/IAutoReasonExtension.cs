// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.AutoReason
{
  /// <summary>
  /// Extension for an auto reason
  /// </summary>
  public interface IAutoReasonExtension
    : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Associated machine
    /// </summary>
    IMonitoredMachine Machine { get; }

    /// <summary>
    /// Default auto-reason
    /// </summary>
    IReason Reason { get; }

    /// <summary>
    /// Default reason score
    /// </summary>
    double ReasonScore { get; }

    /// <summary>
    /// Initialize the auto-reason with a machine
    /// 
    /// The plugin is de-activated if false is returned
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="caller"></param>
    /// <returns>Return if the plugin should be activated</returns>
    bool Initialize (IMonitoredMachine machine, Lemoine.Threading.IChecked caller);

    /// <summary>
    /// Optional manual score that can be applied in case you want to force that reason
    /// </summary>
    double? ManualScore { get; }

    /// <summary>
    /// Does the reason of the specified data may come from this auto-reason ?
    /// 
    /// This method is mainly to check an auto-reason is compatible with
    /// a new machine mode or new machine observation state
    /// 
    /// Because it is used only for some optimization purpose,
    /// return true, in doubt, in case it is not easy to determine it
    /// </summary>
    /// <param name="machineMode">not null</param>
    /// <param name="machineObservationState">not null</param>
    /// <param name="reason"></param>
    /// <param name="score"></param>
    /// <returns></returns>
    bool IsValidMatch (IMachineMode machineMode, IMachineObservationState machineObservationState, IReason reason, double score);

    /// <summary>
    /// Does the reason can be overridden by this auto-reason,
    /// meaning the resulted score will be higher
    /// for the specified machine mode and machine observation state
    /// 
    /// Because it is used only for some optimization purpose,
    /// return true, in doubt, in case it is not easy to determine it
    /// </summary>
    /// <param name="reasonSlot">not null</param>
    /// <returns></returns>
    bool CanOverride (IReasonSlot reasonSlot);

    /// <summary>
    /// May this auto-reason correspond to one of the possible extra auto-reasons of the specified reason slot
    /// 
    /// Because it is used only for some optimization purpose,
    /// return true, in doubt, in case it is not easy to determine it
    /// </summary>
    /// <param name="reasonSlot">not null</param>
    /// <returns></returns>
    bool IsValidExtraAutoReason (IReasonSlot reasonSlot);

    /// <summary>
    /// Check the data
    /// 
    /// One or several transactions may be created in this method
    /// </summary>
    void RunOnce ();
  }
}

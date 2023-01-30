// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Extensions.Database;
using Lemoine.Model;
using Lemoine.Core.Log;
using System.Collections.Generic;
using System.Linq;
using Pulse.Extensions.Database;

namespace Lemoine.Plugin.DefaultReasonEndObservationStateSlot
{
  /// <summary>
  /// Description of DefaultReasonExtension.
  /// </summary>
  public class ReasonExtension
    : Lemoine.Extensions.UniqueInstanceConfigurableExtension<Configuration>
    , Pulse.Extensions.Database.IReasonExtension
  {
    #region Members
    IEnumerable<Configuration> m_configurations = new List<Configuration> ();
    #endregion // Members

    static ILog log = LogManager.GetLogger (typeof (ReasonExtension).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ReasonExtension ()
    {
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IResetReasonExtension implementation
    public bool Initialize (IMachine machine)
    {
      log = LogManager.GetLogger (typeof (ReasonExtension).FullName + "." + machine.Id);

      m_configurations = LoadConfigurations ();

      return true;
    }

    IEnumerable<Configuration> GetValidConfigurations (IReasonSlot reasonSlot)
    {
      return m_configurations
        .Where (c => c.CurrentMachineObservationStateId.Equals (reasonSlot.MachineObservationState.Id))
        .Where (c => c.MachineModeIds.Any (id => ActivityAnalysisExtension.IsMachineModeIdValid (id, reasonSlot.MachineMode, log)));
    }

    public void StartBatch ()
    {
      // No optimization for the moment here
      return;
    }

    public void PreLoad (UtcDateTimeRange range)
    {
      // No optimization for the moment here
      return;
    }

    public void EndBatch ()
    {
      // No optimization for the moment here
      return;
    }


    public double? GetMaximumScore (IReasonSlot newReasonSlot)
    {
      var validConfigurations = GetValidConfigurations (newReasonSlot);
      if (!validConfigurations.Any ()) {
        return -1.0;
      }

      return validConfigurations
        .Select (c => c.Score)
        .Max ();
    }

    /// <summary>
    /// Lemoine.Extensions.Database.IResetReasonExtension implementation
    /// </summary>
    /// <param name="newReasonSlot"></param>
    public bool MayApplyAutoReasons (IReasonSlot newReasonSlot)
    {
      var validConfigurations = GetValidConfigurations (newReasonSlot);
      return validConfigurations.Any ();
    }

    /// <summary>
    /// Lemoine.Extensions.Database.IResetReasonExtension implementation
    /// </summary>
    /// <param name="newReasonSlot"></param>
    public bool MayApplyManualReasons (IReasonSlot newReasonSlot)
    {
      return false;
    }

    /// <summary>
    /// Lemoine.Extensions.Database.IResetReasonExtension implementation
    /// </summary>
    /// <param name="oldReasonSlot"></param>
    /// <param name="newReasonSlot"></param>
    /// <param name="modification"></param>
    /// <param name="reasonSlotChange"></param>
    /// <returns></returns>
    public RequiredResetKind GetRequiredResetKind (IReasonSlot oldReasonSlot, IReasonSlot newReasonSlot, IModification modification, ReasonSlotChange reasonSlotChange)
    {
      if (GetValidConfigurations (newReasonSlot).Any ()) {
        // TODO: RequiredResetKind.None or ExtraAuto could be returned instead in some cases (optimization)
        return RequiredResetKind.Full;
      }
      else if (!GetValidConfigurations (oldReasonSlot).Any ()) { // This auto-reason was not the old main auto-reason
        // TODO: RequiredResetKind.None could be returned in some cases (optimization)
        // TODO: and some additional optimizations are possible...
        return RequiredResetKind.ExtraAuto;
      }
      else { // Potentially the active reason matches this auto-reason
        return RequiredResetKind.Full;
      }
    }

    public bool IsResetApplicable (ReasonSource source, double score, int autoReasonNumber)
    {
      // TODO: ...
      return true;
    }

    public void TryResetReason (ref Lemoine.Model.IReasonSlot reasonSlot)
    {
      UpdateDefaultReasonNotifier.NotifyUpdateDefaultReason (reasonSlot);
    }

    public IEnumerable<IPossibleReason> TryGetActiveAt (DateTime at, IMachineMode machineMode, IMachineObservationState machineObservationState, bool autoManualOnly)
    {
      // TODO: to write
      return new List<IPossibleReason> ();
    }

    /// <summary>
    /// Is a reason compatible with the machine mode and the machine observation state ?
    /// 
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="range"></param>
    /// <param name="machineMode">not null</param>
    /// <param name="machineObservationState">not null</param>
    /// <param name="reason">not null</param>
    /// <param name="reasonScore"></param>
    /// <param name="reasonSource"></param>
    /// <returns></returns>
    public bool IsCompatible (UtcDateTimeRange range, IMachineMode machineMode, IMachineObservationState machineObservationState, IReason reason, double reasonScore, ReasonSource reasonSource)
    {
      // TODO: to write
      return false;
    }

    #endregion
  }
}

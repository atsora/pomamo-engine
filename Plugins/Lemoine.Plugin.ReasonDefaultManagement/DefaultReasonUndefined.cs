// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Pulse.Extensions.Database;
using Lemoine.Extensions;

namespace Lemoine.Plugin.ReasonDefaultManagement
{
  /// <summary>
  /// Fallback to undefined in case nothing is set in the database
  /// </summary>
  public sealed class DefaultReasonUndefined
    : Lemoine.Extensions.NotConfigurableExtension
    , IReasonExtension
    , IExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (DefaultReasonUndefined).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public DefaultReasonUndefined ()
    {
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IReasonExtension implementation
    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public bool Initialize (IMachine machine)
    {
      return true;
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    public void StartBatch ()
    {
      // Not necessary here
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="range"></param>
    public void PreLoad (UtcDateTimeRange range)
    {
      // Not necessary here
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    public void EndBatch ()
    {
      // Not necessary here
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="newReasonSlot"></param>
    public double? GetMaximumScore (IReasonSlot newReasonSlot)
    {
      return 0.0;
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="newReasonSlot"></param>
    public bool MayApplyAutoReasons (IReasonSlot newReasonSlot)
    {
      return false;
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="newReasonSlot"></param>
    public bool MayApplyManualReasons (IReasonSlot newReasonSlot)
    {
      return false;
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="oldReasonSlot"></param>
    /// <param name="newReasonSlot"></param>
    /// <param name="modification"></param>
    /// <param name="reasonSlotChange"></param>
    /// <returns></returns>
    public RequiredResetKind GetRequiredResetKind (IReasonSlot oldReasonSlot, IReasonSlot newReasonSlot, IModification modification, ReasonSlotChange reasonSlotChange)
    {
      return RequiredResetKind.None;
    }

    /// <summary>
    /// <see cref="IReasonExtension"/>
    /// </summary>
    /// <param name="reasonSource"></param>
    /// <param name="reasonScore"></param>
    /// <param name="autoReasonNumber"></param>
    /// <returns></returns>
    public bool IsResetApplicable (ReasonSource reasonSource, double reasonScore, int autoReasonNumber)
    {
      return true;
    }

    /// <summary>
    /// Update the default reason following a change in the reason
    /// </summary>
    /// <param name="reasonSlot">new reason slot (not null)</param>
    /// <param name="initialSource"></param>
    /// <param name="initialScore"></param>
    /// <param name="initialAutoReasonNumber"></param>
    /// <returns></returns>
    public void TryResetReason (ref Lemoine.Model.IReasonSlot reasonSlot)
    {
      Debug.Assert (null != reasonSlot);

      if ((null != reasonSlot.Reason)
        && ((int)ReasonId.Undefined == reasonSlot.Reason.Id)
        && (0 <= reasonSlot.ReasonScore)) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("TryResetReason: the reason is already 'undefined' in reasonslot id {0}",
                           reasonSlot.Id);
        }
      }
      else {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("TryResetReason: try to set undefined to reasonSlot id {0}",
            reasonSlot.Id);
        }
        IReason undefined = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Undefined);
        var oldSlot = (ISlot)reasonSlot.Clone ();
        this.TryDefaultReason (reasonSlot, undefined, 0.0, true, false, new UpperBound<DateTime> ());
        reasonSlot.Consolidate (oldSlot, null);
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonExtension"/>
    /// </summary>
    /// <param name="at"></param>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="autoManualOnly"></param>
    /// <returns></returns>
    public IEnumerable<IPossibleReason> TryGetActiveAt (DateTime at, IMachineMode machineMode, IMachineObservationState machineObservationState, bool autoManualOnly)
    {
      if (autoManualOnly) {
        return new List<IPossibleReason> ();
      }
      else {
        var reason = ModelDAOHelper.DAOFactory.ReasonDAO.FindById ((int)ReasonId.Undefined);
        var range = new UtcDateTimeRange (new LowerBound<DateTime> (null),
          new UpperBound<DateTime> (null));
        var possibleReason = new PossibleReason (reason, null, 0.0, ReasonSource.Default, true, range);
        return new List<IPossibleReason> { possibleReason };
      }
    }

    /// <summary>
    /// Is a reason compatible with the machine mode and the machine observation state ?
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
      return reasonScore.Equals (ReasonSource.Default) && reason.Id == (int)ReasonId.Undefined && 0.0 == reasonScore;
    }
    #endregion
  }
}

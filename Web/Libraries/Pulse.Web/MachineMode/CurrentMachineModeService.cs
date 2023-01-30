// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web;

namespace Pulse.Web.MachineMode
{
  /// <summary>
  /// Current period match
  /// </summary>
  [Flags]
  public enum CurrentMachineModePeriod
  {
    /// <summary>
    /// None
    /// </summary>
    None = 0,
    /// <summary>
    /// MachineModeCategory
    /// </summary>
    MachineModeCategory = 2,
    /// <summary>
    /// Running
    /// </summary>
    Running = 4,
  }

  /// <summary>
  /// Extensions to CurrentMachineModePeriod
  /// </summary>
  public static class CurrentMachineModePeriodExtensions
  {
    static ILog log = LogManager.GetLogger (typeof (CurrentMachineModePeriodExtensions).ToString ());

    /// <summary>
    /// Check if other is included into t
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool HasFlag (this CurrentMachineModePeriod t, CurrentMachineModePeriod other)
    {
      return other == (t & other);
    }
  }

  /// <summary>
  /// Description of MachineModeColorService
  /// </summary>
  public class CurrentMachineModeService
    : GenericCachedService<CurrentMachineModeRequestDTO>
  {
    static readonly string REASON_SLOT_IS_CURRENT_MARGIN_KEY = "Web.CurrentMachineMode.UseReasonSlotMargin";
    static readonly TimeSpan REASON_SLOT_IS_CURRENT_MARGIN_DEFAULT = TimeSpan.FromSeconds (2);

    static readonly string REASON_SLOT_IS_CURRENT_MARGIN_IF_PERIOD_KEY = "Web.CurrentMachineMode.UseReasonSlotMarginIfPeriod";
    static readonly TimeSpan REASON_SLOT_IS_CURRENT_MARGIN_IF_PERIOD_DEFAULT = TimeSpan.FromSeconds (10);

    static readonly string CURRENT_MACHINE_MODE_MARGIN_KEY = "Web.CurrentMachineMode.CurrentMachineModeMargin";
    static readonly TimeSpan CURRENT_MACHINE_MODE_MARGIN_DEFAULT = TimeSpan.FromSeconds (30);

    static readonly string LIMIT_MARGIN_KEY = "Web.CurrentMachineMode.LimitMargin"; // For fact and currentmachinemode
    static readonly TimeSpan LIMIT_MARGIN_DEFAULT = TimeSpan.FromMinutes (1);

    static readonly TimeSpan REASON_SLOT_STEP = TimeSpan.FromHours (8);
    static readonly TimeSpan MAX_PERIOD_DURATION = TimeSpan.FromDays (1);

    static readonly ILog log = LogManager.GetLogger (typeof (CurrentMachineModeService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public CurrentMachineModeService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    #region Methods
    CurrentMachineModePeriod ConvertPeriodParameter (string periodParameter)
    {
      if (string.IsNullOrEmpty (periodParameter)) {
        return CurrentMachineModePeriod.None;
      }
      switch (periodParameter.ToLowerInvariant ()) {
        case "none":
        case "":
          return CurrentMachineModePeriod.None;
        case "running":
          return CurrentMachineModePeriod.Running;
        case "running_machinemodecategory":
          return CurrentMachineModePeriod.Running | CurrentMachineModePeriod.MachineModeCategory;
        default:
          log.ErrorFormat ("ConvertPeriodParameter: {0} is not a supported parameter", periodParameter);
          throw new ArgumentException ("Not supported period", "periodParameter");
      }
    }

    /// <summary>
    /// Does the reason slot can be considered as part of the current period ?
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <param name="period">not None</param>
    /// <param name="machineMode">not null</param>
    /// <returns></returns>
    bool IsPeriodMatch (IReasonSlot reasonSlot, CurrentMachineModePeriod period, IMachineMode machineMode)
    {
      Debug.Assert (!period.Equals (CurrentMachineModePeriod.None));
      Debug.Assert (null != machineMode);

      if (period.HasFlag (CurrentMachineModePeriod.MachineModeCategory)
        && !reasonSlot.MachineMode.MachineModeCategory.Equals (machineMode.MachineModeCategory)) {
        return false;
      }
      if (period.HasFlag (CurrentMachineModePeriod.Running)
        && !object.Equals (reasonSlot.MachineMode.Running, machineMode.Running)) {
        return false;
      }

      return true;
    }

    /// <summary>
    /// Does the fact can be considered as part of the current period ?
    /// </summary>
    /// <param name="fact"></param>
    /// <param name="period">not None and does not has flag Reason</param>
    /// <param name="machineMode">not null</param>
    /// <returns></returns>
    bool IsPeriodMatch (IFact fact, CurrentMachineModePeriod period, IMachineMode machineMode)
    {
      Debug.Assert (!period.Equals (CurrentMachineModePeriod.None));
      Debug.Assert (null != machineMode);

      if (period.HasFlag (CurrentMachineModePeriod.MachineModeCategory)
        && !fact.CncMachineMode.MachineModeCategory.Equals (machineMode.MachineModeCategory)) {
        return false;
      }
      if (period.HasFlag (CurrentMachineModePeriod.Running)
        && !object.Equals (fact.CncMachineMode.Running, machineMode.Running)) {
        return false;
      }

      return true;
    }

    void SetPeriodStartFromReasonSlots (ref DateTime? periodStart, IMachine machine, CurrentMachineModePeriod period, IMachineMode machineMode, DateTime dateTime)
    {
      if (!period.Equals (CurrentMachineModePeriod.None)) {
        var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindOverlapsRangeDescending (machine, new UtcDateTimeRange (DateTime.UtcNow.Subtract (MAX_PERIOD_DURATION), dateTime), REASON_SLOT_STEP);
        var firstNotMatching = reasonSlots
          .FirstOrDefault (s => !IsPeriodMatch (s, period, machineMode));
        if (null != firstNotMatching) {
          Debug.Assert (firstNotMatching.DateTimeRange.Upper.HasValue);
          periodStart = firstNotMatching.DateTimeRange.Upper.Value;
        }
      }
    }

    void SetPeriodStartFromFacts (ref DateTime? periodStart, IMonitoredMachine machine, CurrentMachineModePeriod period, IMachineMode machineMode, DateTime dateTime)
    {
      if (!period.Equals (CurrentMachineModePeriod.None)) {
        var facts = ModelDAOHelper.DAOFactory.FactDAO
          .FindAllInUtcRange (machine, new UtcDateTimeRange (DateTime.UtcNow.Subtract (MAX_PERIOD_DURATION), dateTime));
        var firstNotMatching = facts
          .FirstOrDefault (s => !IsPeriodMatch (s, period, machineMode));
        if (null != firstNotMatching) {
          periodStart = firstNotMatching.End;
        }
      }
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (CurrentMachineModeRequestDTO request)
    {
      CurrentMachineModeResponseDTO response = new CurrentMachineModeResponseDTO ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.CurrentMachineMode")) {
          int machineId = request.MachineId;
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (machineId);
          if (null == machine) {
            log.ErrorFormat ("GetWithoutCache: " +
                             "unknown machine with ID {0}",
                             machineId);
            return new ErrorDTO ("No monitored machine with the specified ID",
                                 ErrorStatus.WrongRequestParameter);
          }

          IMachineMode machineMode = null;
          DateTime dateTime = DateTime.UtcNow;
          bool manualActivity = false;
          DateTime? periodStart = null;

          // Manual machine status ? / Current reason slot ok ?
          IMachineStatus machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO
            .FindByIdWithMachineMode (machine.Id);
          if (null != machineStatus) {
            if (machineStatus.ManualActivity
                && (Bound.Compare<DateTime> (DateTime.UtcNow,
                                             new UpperBound<DateTime> (machineStatus.ManualActivityEnd)) <= 0)) {
              log.DebugFormat ("GetWithoutCache: " +
                               "return a manual activity {0}",
                               machineStatus.MachineMode);
              machineMode = machineStatus.MachineMode;
              dateTime = DateTime.UtcNow;
              manualActivity = true;
              if (!request.NotRunningOnlyDuration || !machineMode.Running.HasValue || !machineMode.Running.Value) {
                SetPeriodStartFromReasonSlots (ref periodStart, machine, ConvertPeriodParameter (request.Period), machineMode, dateTime);
              }
            }
            else { // Is current reason slot ok for the current machine mode ?
              var period = ConvertPeriodParameter (request.Period);
              TimeSpan reasonSlotIsCurrentMargin;
              if (period.Equals (CurrentMachineModePeriod.None)) {
                reasonSlotIsCurrentMargin = Lemoine.Info.ConfigSet
                  .LoadAndGet<TimeSpan> (REASON_SLOT_IS_CURRENT_MARGIN_KEY,
                                         REASON_SLOT_IS_CURRENT_MARGIN_DEFAULT);
              }
              else {
                reasonSlotIsCurrentMargin = Lemoine.Info.ConfigSet
                  .LoadAndGet<TimeSpan> (REASON_SLOT_IS_CURRENT_MARGIN_IF_PERIOD_KEY,
                                         REASON_SLOT_IS_CURRENT_MARGIN_IF_PERIOD_DEFAULT);
              }
              if (DateTime.UtcNow <= machineStatus.ReasonSlotEnd.Add (reasonSlotIsCurrentMargin)) {
                log.DebugFormat ("GetWithoutCache: " +
                                 "get current machine {0} mode from reason slot",
                                 machineStatus.MachineMode);
                machineMode = machineStatus.MachineMode;
                dateTime = machineStatus.ReasonSlotEnd;
                if (!request.NotRunningOnlyDuration || !machineMode.Running.HasValue || !machineMode.Running.Value) {
                  SetPeriodStartFromReasonSlots (ref periodStart, machine, ConvertPeriodParameter (request.Period), machineMode, dateTime);
                }
              }
            }
          }

          // currentMachineMode
          if (null == machineMode) {
            ICurrentMachineMode currentMachineMode = ModelDAOHelper.DAOFactory.CurrentMachineModeDAO
              .FindWithMachineMode (machine);
            if (null != currentMachineMode) {
              Debug.Assert (null != currentMachineMode.MachineMode);
              TimeSpan currentMachineModeMargin = Lemoine.Info.ConfigSet
                .LoadAndGet<TimeSpan> (CURRENT_MACHINE_MODE_MARGIN_KEY,
                                       CURRENT_MACHINE_MODE_MARGIN_DEFAULT);
              if (DateTime.UtcNow <= currentMachineMode.DateTime.Add (currentMachineModeMargin)) {
                log.DebugFormat ("GetWithoutCache: " +
                                 "consider machine mode {0} from currentmachinemode",
                                 currentMachineMode.MachineMode);
                machineMode = currentMachineMode.MachineMode;
                dateTime = currentMachineMode.DateTime;
              }
            }

            // Fact
            if (null == machineMode) {
              TimeSpan limitMargin = Lemoine.Info.ConfigSet
                .LoadAndGet<TimeSpan> (LIMIT_MARGIN_KEY,
                                       LIMIT_MARGIN_DEFAULT);
              IFact fact = ModelDAOHelper.DAOFactory.FactDAO
                .GetLast (machine);
              if ((null != fact)
                  && ((null == currentMachineMode)
                      || (Bound.Compare<DateTime> (currentMachineMode.DateTime, fact.End) <= 0))) {
                if (DateTime.UtcNow <= fact.End.Add (limitMargin)) {
                  // Fact is more recent than currentMachineMode
                  log.DebugFormat ("GetWithoutCache: " +
                                   "consider machine mode {0} from fact",
                                   fact.CncMachineMode);
                  machineMode = fact.CncMachineMode;
                  dateTime = fact.End;
                  if (!request.NotRunningOnlyDuration || !machineMode.Running.HasValue || !machineMode.Running.Value) {
                    SetPeriodStartFromFacts (ref periodStart, machine, ConvertPeriodParameter (request.Period), machineMode, dateTime);
                  }
                }
              }
              else if ((null != currentMachineMode)
                       && (DateTime.UtcNow <= currentMachineMode.DateTime.Add (limitMargin))) {
                log.DebugFormat ("GetWithoutCache: " +
                                 "consider machine mode {0} from currentmachinemode with the limit margin {1}",
                                 currentMachineMode.MachineMode, limitMargin);
                machineMode = currentMachineMode.MachineMode;
                dateTime = currentMachineMode.DateTime;
              }
            }
          }

          if (null != machineMode) {
            response.CurrentDateTime = ConvertDTO.DateTimeUtcToIsoString (DateTime.UtcNow);
            response.MachineMode = new MachineModeDTOAssembler ().Assemble (machineMode);
            response.DateTime = ConvertDTO.DateTimeUtcToIsoString (dateTime);
            if (manualActivity) {
              response.ManualActivity = true;
            }
            if (periodStart.HasValue) {
              response.PeriodStart = ConvertDTO.DateTimeUtcToIsoString (periodStart.Value);
            }
          }
          else {
            log.ErrorFormat ("GetWithoutCache: " +
                             "unable to get a current machine mode for machine {0}",
                             machineId);
            return new ErrorDTO ("No current machine was recorded",
                                 ErrorStatus.ProcessingDelay);
          }

        } // transaction
      } // session

      return response;
    }
    #endregion // Methods
  }
}

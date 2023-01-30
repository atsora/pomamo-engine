// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System.Linq;
using Lemoine.Extensions.Database;
using System.Threading.Tasks;
using Pulse.Extensions.Database;

namespace Lemoine.Business.Reason
{
  /// <summary>
  /// Current period match
  /// </summary>
  [Flags]
  public enum CurrentReasonPeriod
  {
    /// <summary>
    /// None
    /// </summary>
    None = 0,
    /// <summary>
    /// Reason
    /// </summary>
    Reason = 1,
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
  /// Extensions to CurrentReasonPeriod
  /// </summary>
  public static class CurrentReasonPeriodExtensions
  {
    readonly static ILog log = LogManager.GetLogger (typeof (CurrentReasonPeriodExtensions).ToString ());

    /// <summary>
    /// Check if other is included into t
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool HasFlag (this CurrentReasonPeriod t, CurrentReasonPeriod other)
    {
      return other == (t & other);
    }
  }

  /// <summary>
  /// Request class to get ...
  /// </summary>
  public sealed class CurrentReason
    : IRequest<CurrentReasonResponse>
  {
    static readonly string REASON_SLOT_IS_CURRENT_MARGIN_KEY = "Business.Reason.CurrentReason.UseReasonSlotMargin";
    static readonly TimeSpan REASON_SLOT_IS_CURRENT_MARGIN_DEFAULT = TimeSpan.FromSeconds (10);

    static readonly string CURRENT_MACHINE_MODE_MARGIN_KEY = "Business.Reason.CurrentReason.CurrentMachineModeMargin";
    static readonly TimeSpan CURRENT_MACHINE_MODE_MARGIN_DEFAULT = TimeSpan.FromSeconds (30);

    static readonly string LIMIT_MARGIN_KEY = "Business.Reason.CurrentReason.LimitMargin"; // For fact and currentmachinemode
    static readonly TimeSpan LIMIT_MARGIN_DEFAULT = TimeSpan.FromMinutes (1);

    static readonly TimeSpan REASON_SLOT_STEP = TimeSpan.FromHours (8);
    static readonly TimeSpan FACT_STEP = TimeSpan.FromHours (8);
    static readonly TimeSpan MAX_PERIOD_DURATION = TimeSpan.FromDays (1);

    #region Members
    readonly IMonitoredMachine m_machine;
    readonly CurrentReasonPeriod m_period = CurrentReasonPeriod.None;
    readonly bool m_notRunningOnlyDuration = false;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (CurrentReason).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    public CurrentReason (IMonitoredMachine machine)
      : this (machine, CurrentReasonPeriod.None, false)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="period"></param>
    /// <param name="notRunningOnlyDuration"></param>
    public CurrentReason (IMonitoredMachine machine, CurrentReasonPeriod period, bool notRunningOnlyDuration)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
      m_period = period;
      m_notRunningOnlyDuration = notRunningOnlyDuration;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public CurrentReasonResponse Get ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IReason reason = null;
        IMachineMode machineMode = null;
        DateTime dateTime = DateTime.UtcNow;
        double? reasonScore = null;
        ReasonSource? reasonSource = null;
        int? autoReasonNumber = null;
        DateTime? periodStart = null;

        // Transaction 1
        using (IDAOTransaction transaction = session.BeginReadOnlyDeferrableTransaction ("Web.CurrentReason")) {

          // Current reason slot ok ?
          var machineStatus = GetFromMachineStatus (m_machine, m_period, m_notRunningOnlyDuration, ref reason, ref machineMode, ref dateTime, ref reasonScore, ref reasonSource, ref autoReasonNumber, ref periodStart);

          // Last reason slots
          if  (reason is null) {
            GetFromLastReasonSlots (m_machine, m_period, m_notRunningOnlyDuration, ref reason, ref machineMode, ref dateTime, ref reasonScore, ref reasonSource, ref autoReasonNumber, ref periodStart);
          }

          // currentMachineMode
          if (reason is null) {
            var currentMachineMode = GetFromCurrentMachineMode (m_machine, m_period, machineStatus, m_notRunningOnlyDuration, ref reason, ref machineMode, ref dateTime, ref reasonScore, ref reasonSource, ref autoReasonNumber, ref periodStart);

            // Fact
            if (null == machineMode) {
              TimeSpan limitMargin = Lemoine.Info.ConfigSet
                .LoadAndGet<TimeSpan> (LIMIT_MARGIN_KEY,
                                       LIMIT_MARGIN_DEFAULT);
              if ((null != machineStatus)
                && ((null == currentMachineMode)
                    || (Bound.Compare<DateTime> (currentMachineMode.Change, machineStatus.ReasonSlotEnd) < 0))) {
                FillFromMachineStatusWithMarginCondition (m_machine, m_period, machineStatus, limitMargin, m_notRunningOnlyDuration, ref reason, ref machineMode, ref dateTime, ref reasonScore, ref reasonSource, ref autoReasonNumber, ref periodStart);
              }
              if (null == machineMode) {
                GetMachineModeFromLastFact (m_machine, m_period, currentMachineMode, limitMargin, m_notRunningOnlyDuration, ref machineMode, ref dateTime, ref periodStart);
              }
            }
          }
        } // Transaction 1

        // Transaction 2
        if ((null != machineMode) && (reason is null)) {
          using (IDAOTransaction transaction = session.BeginReadOnlyDeferrableTransaction ("Web.CurrentReason.GuessReasonFromMachineMode")) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: try to guess reason from machineMode {machineMode.Id}");
            }
            GuessReasonFromMachineMode (m_machine, machineMode, dateTime, ref reason, ref reasonScore, ref reasonSource, ref autoReasonNumber, ref periodStart);
          } // Transaction 2
        }

        var response = new CurrentReasonResponse ();
        if (null != reason) {
          Debug.Assert (null != machineMode);
          response.CurrentDateTime = DateTime.UtcNow;
          response.MachineMode = machineMode;
          response.Reason = reason;
          response.DateTime = dateTime;
          response.ReasonScore = reasonScore;
          response.ReasonSource = reasonSource;
          response.AutoReasonNumber = autoReasonNumber;
          response.PeriodStart = periodStart;
        }
        return response;
      } // session
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<CurrentReasonResponse> GetAsync ()
    {
      // TODO: make it asynchronous
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Reason.Current." + m_period.ToString ()
        + "." + m_notRunningOnlyDuration
        + "." + m_machine.Id;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (CurrentReasonResponse data)
    {
      return CacheTimeOut.CurrentShort.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<CurrentReasonResponse> data)
    {
      return true;
    }
    #endregion // IRequest implementation

    /// <summary>
    /// Does the reason slot can be considered as part of the current period ?
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <param name="period">not None</param>
    /// <param name="machineMode">not null</param>
    /// <param name="reason">not null</param>
    /// <returns></returns>
    bool IsPeriodMatch (IReasonSlot reasonSlot, CurrentReasonPeriod period, IMachineMode machineMode, IReason reason)
    {
      Debug.Assert (!period.Equals (CurrentReasonPeriod.None));
      Debug.Assert (null != machineMode);
      Debug.Assert (null != reason);

      if (period.HasFlag (CurrentReasonPeriod.MachineModeCategory)
        && !reasonSlot.MachineMode.MachineModeCategory.Equals (machineMode.MachineModeCategory)) {
        return false;
      }
      if (period.HasFlag (CurrentReasonPeriod.Reason)
        && (reasonSlot.Reason.Id != reason.Id)) {
        return false;
      }
      if (period.HasFlag (CurrentReasonPeriod.Running)
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
    bool IsPeriodMatch (IFact fact, CurrentReasonPeriod period, IMachineMode machineMode)
    {
      Debug.Assert (!period.Equals (CurrentReasonPeriod.None));
      Debug.Assert (!period.HasFlag (CurrentReasonPeriod.Reason));
      Debug.Assert (null != machineMode);

      if (period.HasFlag (CurrentReasonPeriod.MachineModeCategory)
        && !fact.CncMachineMode.MachineModeCategory.Equals (machineMode.MachineModeCategory)) {
        return false;
      }
      if (period.HasFlag (CurrentReasonPeriod.Running)
        && !object.Equals (fact.CncMachineMode.Running, machineMode.Running)) {
        return false;
      }

      return true;
    }

    void SetPeriodStartFromReasonSlots (ref DateTime? periodStart, IMachine machine, CurrentReasonPeriod period, IMachineMode machineMode, IReason reason, DateTime dateTime)
    {
      Debug.Assert (null != reason);
      if (!period.Equals (CurrentReasonPeriod.None)) {
        var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindOverlapsRangeDescending (machine, new UtcDateTimeRange (dateTime.Subtract (MAX_PERIOD_DURATION), dateTime), REASON_SLOT_STEP);
        var firstNotMatching = reasonSlots
          .FirstOrDefault (s => !IsPeriodMatch (s, period, machineMode, reason));
        if (null != firstNotMatching) {
          Debug.Assert (firstNotMatching.DateTimeRange.Upper.HasValue);
          periodStart = firstNotMatching.DateTimeRange.Upper.Value;
        }
      }
    }

    void SetPeriodStartFromFacts (ref DateTime? periodStart, IMonitoredMachine machine, CurrentReasonPeriod period, IMachineMode machineMode, DateTime dateTime)
    {
      if (!period.Equals (CurrentReasonPeriod.None) && !period.HasFlag (CurrentReasonPeriod.Reason)) {
        var facts = ModelDAOHelper.DAOFactory.FactDAO
          .FindOverlapsRangeDescending (machine, new UtcDateTimeRange (dateTime.Subtract (MAX_PERIOD_DURATION), dateTime), FACT_STEP);
        var firstNotMatching = facts
          .FirstOrDefault (s => !IsPeriodMatch (s, period, machineMode));
        if (null != firstNotMatching) {
          periodStart = firstNotMatching.End;
        }
      }
    }


    IMachineStatus GetFromMachineStatus (IMonitoredMachine machine, CurrentReasonPeriod period, bool notRunningOnlyDuration, ref IReason reason, ref IMachineMode machineMode, ref DateTime dateTime, ref double? reasonScore, ref ReasonSource? reasonSource, ref int? autoReasonNumber, ref DateTime? periodStart)
    {
      IMachineStatus machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO
        .FindByIdWithMachineModeReasonGroup (machine.Id);
      TimeSpan margin = Lemoine.Info.ConfigSet
        .LoadAndGet<TimeSpan> (REASON_SLOT_IS_CURRENT_MARGIN_KEY,
                               REASON_SLOT_IS_CURRENT_MARGIN_DEFAULT);
      FillFromMachineStatusWithMarginCondition (machine, period, machineStatus, margin, notRunningOnlyDuration, ref reason, ref machineMode, ref dateTime, ref reasonScore, ref reasonSource, ref autoReasonNumber, ref periodStart);
      return machineStatus;
    }

    void FillFromMachineStatusWithMarginCondition (IMonitoredMachine machine, CurrentReasonPeriod period, IMachineStatus machineStatus, TimeSpan margin, bool notRunningOnlyDuration, ref IReason reason, ref IMachineMode machineMode, ref DateTime dateTime, ref double? reasonScore, ref ReasonSource? reasonSource, ref int? autoReasonNumber, ref DateTime? periodStart)
    {
      if (null != machineStatus) {
        if (DateTime.UtcNow <= machineStatus.ReasonSlotEnd.Add (margin)) {
          FillFromMachineStatus (machine, period, machineStatus, notRunningOnlyDuration, ref reason, ref machineMode, ref dateTime, ref reasonScore, ref reasonSource, ref autoReasonNumber, ref periodStart);
        }
      }
    }

    void FillFromMachineStatus (IMonitoredMachine machine, CurrentReasonPeriod period, IMachineStatus machineStatus, bool notRunningOnlyDuration, ref IReason reason, ref IMachineMode machineMode, ref DateTime dateTime, ref double? reasonScore, ref ReasonSource? reasonSource, ref int? autoReasonNumber, ref DateTime? periodStart)
    {
      Debug.Assert (null != machineStatus);
      Debug.Assert (null != machineStatus.Reason);
      if (log.IsDebugEnabled) {
        log.Debug ($"FillFromMachineStatus: get current reason {machineStatus.Reason.Id} from machine status");
      }
      if ((int)ReasonId.Processing != machineStatus.Reason.Id) {
        reason = machineStatus.Reason;
      }
      machineMode = machineStatus.MachineMode;
      dateTime = machineStatus.ReasonSlotEnd;
      reasonScore = machineStatus.ReasonScore;
      reasonSource = machineStatus.ReasonSource;
      autoReasonNumber = machineStatus.AutoReasonNumber;
      if (!notRunningOnlyDuration || !machineMode.Running.HasValue || !machineMode.Running.Value) {
        SetPeriodStartFromReasonSlots (ref periodStart, machine, period, machineMode, machineStatus.Reason, dateTime);
      }
    }

    void GetFromLastReasonSlots (IMonitoredMachine machine, CurrentReasonPeriod period, bool notRunningOnlyDuration, ref IReason reason, ref IMachineMode machineMode, ref DateTime dateTime, ref double? reasonScore, ref ReasonSource? reasonSource, ref int? autoReasonNumber, ref DateTime? periodStart)
    {
      Debug.Assert (reason is null);

      TimeSpan reasonSlotIsCurrentMargin = Lemoine.Info.ConfigSet
        .LoadAndGet<TimeSpan> (REASON_SLOT_IS_CURRENT_MARGIN_KEY,
                               REASON_SLOT_IS_CURRENT_MARGIN_DEFAULT);
      var range = new UtcDateTimeRange (DateTime.UtcNow.Subtract (reasonSlotIsCurrentMargin));
      var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindOverlapsRangeDescending (machine, range, reasonSlotIsCurrentMargin);
      bool first = true;
      foreach (var reasonSlot in reasonSlots) {
        Debug.Assert (null != reasonSlot.MachineMode);
        Debug.Assert (reason is null);
        IReason notProcessingReason = null;
        if ((null != reasonSlot.Reason)
          && ((int)ReasonId.Processing != reasonSlot.Reason.Id)) {
          notProcessingReason = reasonSlot.Reason;
        }
        if (first || (null != notProcessingReason)) {
          reason = notProcessingReason;
          machineMode = reasonSlot.MachineMode;
          dateTime = reasonSlot.EndDateTime.Value;
          reasonScore = reasonSlot.ReasonScore;
          reasonSource = reasonSlot.ReasonSource;
          autoReasonNumber = reasonSlot.AutoReasonNumber;
          if (!notRunningOnlyDuration || !machineMode.Running.HasValue || !machineMode.Running.Value) {
            if (null != notProcessingReason) {
              SetPeriodStartFromReasonSlots (ref periodStart, machine, period, machineMode, notProcessingReason, dateTime);
            }
          }
        }
        if (null != reason) {
          return;
        }
        else if (first) { // Give it another chance to another period
          first = false;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="period"></param>
    /// <param name="machineStatus">nullable</param>
    /// <param name="notRunningOnlyDuration"></param>
    /// <param name="reason"></param>
    /// <param name="machineMode"></param>
    /// <param name="dateTime"></param>
    /// <param name="reasonScore"></param>
    /// <param name="reasonSource"></param>
    /// <param name="autoReasonNumber"></param>
    /// <param name="periodStart"></param>
    /// <returns></returns>
    ICurrentMachineMode GetFromCurrentMachineMode (IMonitoredMachine machine, CurrentReasonPeriod period, IMachineStatus machineStatus, bool notRunningOnlyDuration, ref IReason reason, ref IMachineMode machineMode, ref DateTime dateTime, ref double? reasonScore, ref ReasonSource? reasonSource, ref int? autoReasonNumber, ref DateTime? periodStart)
    {
      ICurrentMachineMode currentMachineMode = ModelDAOHelper.DAOFactory.CurrentMachineModeDAO
        .FindWithMachineMode (machine);
      if (null != currentMachineMode) {
        Debug.Assert (null != currentMachineMode.MachineMode);
        TimeSpan currentMachineModeMargin = Lemoine.Info.ConfigSet
          .LoadAndGet<TimeSpan> (CURRENT_MACHINE_MODE_MARGIN_KEY,
                                 CURRENT_MACHINE_MODE_MARGIN_DEFAULT);
        if (DateTime.UtcNow <= currentMachineMode.DateTime.Add (currentMachineModeMargin)) {
          if ((null != machineStatus) && Bound.Compare<DateTime> (currentMachineMode.Change, machineStatus.ReasonSlotEnd) < 0) {
            FillFromMachineStatus (machine, period, machineStatus, notRunningOnlyDuration, ref reason, ref machineMode, ref dateTime, ref reasonScore, ref reasonSource, ref autoReasonNumber, ref periodStart);
          }
          else {
            Debug.Assert (null != currentMachineMode.MachineMode);
            if (log.IsDebugEnabled) {
              log.DebugFormat ("GetFromCurrentMachineMode: " +
                               "consider machine mode {0} from currentmachinemode",
                               currentMachineMode.MachineMode.Id);
            }
            machineMode = currentMachineMode.MachineMode;
            dateTime = currentMachineMode.DateTime;
          }
        } // currentMachineMode date/time is ok
      }

      return currentMachineMode;
    }

    void GetMachineModeFromLastFact (IMonitoredMachine machine, CurrentReasonPeriod period, ICurrentMachineMode currentMachineMode, TimeSpan limitMargin, bool notRunningOnlyDuration, ref IMachineMode machineMode, ref DateTime dateTime, ref DateTime? periodStart)
    {
      if (null == machineMode) {
        IFact fact = ModelDAOHelper.DAOFactory.FactDAO
          .GetLast (machine);
        if ((null != fact)
            && ((null == currentMachineMode)
                || (Bound.Compare<DateTime> (currentMachineMode.DateTime, fact.End) <= 0))) {
          if (DateTime.UtcNow <= fact.End.Add (limitMargin)) {
            // Fact is more recent than currentReason
            Debug.Assert (null != fact.CncMachineMode);
            log.DebugFormat ("GetMachineModeFromLastFact: " +
                             "consider machine mode {0} from fact",
                             fact.CncMachineMode.Id);
            machineMode = fact.CncMachineMode;
            dateTime = fact.End;
            if (!notRunningOnlyDuration || !machineMode.Running.HasValue || !machineMode.Running.Value) {
              SetPeriodStartFromFacts (ref periodStart, machine, period, machineMode, dateTime);
            }
          }
        }
        else if ((null != currentMachineMode)
                 && (DateTime.UtcNow <= currentMachineMode.DateTime.Add (limitMargin))) {
          Debug.Assert (null != currentMachineMode.MachineMode);
          log.DebugFormat ("GetMachineModeFromLastFact: " +
                           "consider machine mode {0} from currentmachinemode with the limit margin {1}",
                           currentMachineMode.MachineMode.Id, limitMargin);
          machineMode = currentMachineMode.MachineMode;
          dateTime = currentMachineMode.DateTime;
        }
      }
    }

    void GuessReasonFromMachineMode (IMonitoredMachine machine, IMachineMode machineMode, DateTime dateTime, ref IReason reason, ref double? reasonScore, ref ReasonSource? reasonSource, ref int? autoReasonNumber, ref DateTime? periodStart)
    {
      Debug.Assert (null != machineMode);
      Debug.Assert (null == reason);

      IObservationStateSlot observationStateSlot = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
        .FindAt (machine, dateTime);
      if (null != observationStateSlot?.MachineObservationState) {
        var reasonExtensionRequest = new Lemoine.Business.Extension
          .MonitoredMachineExtensions<IReasonExtension> (machine, (ext, m) => ext.Initialize (m));
        var reasonExtensions = Lemoine.Business.ServiceProvider
          .Get (reasonExtensionRequest);
        var possibleReasons = reasonExtensions
          .SelectMany (ext => ext.TryGetActiveAt (dateTime, machineMode, observationStateSlot.MachineObservationState, false))
          .OrderByDescending (p => p.ReasonScore);
        if (!possibleReasons.Any ()) {
          if (log.IsWarnEnabled) {
            log.Warn ($"GuessReasonFromMachineMode: could not get a reason from reason extensions for machine mode {machineMode.Id}");
          }
        }
        else { // Any ()
          var first = possibleReasons.First ();
          if (!ModelDAOHelper.DAOFactory.IsInitialized (first.Reason)) {
            reason = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (first.Reason.Id);
          }
          else {
            reason = first.Reason;
          }
          reasonScore = first.ReasonScore;
          reasonSource = first.ReasonSource;
          autoReasonNumber = possibleReasons
            .Count (p => p.ReasonSource.IsAuto ());
          if (m_period.HasFlag (CurrentReasonPeriod.Reason)) {
            // Stop at the latest reasonslot that does not match
            if (null != first.RestrictedRange) {
              UtcDateTimeRange rangeToCheck;
              if (periodStart.HasValue
                && (Bound.Compare<DateTime> (first.RestrictedRange.Lower, periodStart.Value) < 0)) {
                rangeToCheck = new UtcDateTimeRange (periodStart.Value, dateTime);
              }
              else {
                rangeToCheck = new UtcDateTimeRange (first.RestrictedRange.Lower, dateTime);
              }
              var step = TimeSpan.FromHours (8);
              var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
                .FindOverlapsRangeDescending (m_machine, rangeToCheck, step);
              bool firstReasonSlot = true;
              foreach (var reasonSlot in reasonSlots) {
                if ((firstReasonSlot
                  || Bound.Equals (reasonSlot.DateTimeRange.Upper, periodStart.Value))
                  && IsPeriodMatch (reasonSlot, m_period, machineMode, reason)) {
                  periodStart = reasonSlot.DateTimeRange.Lower.Value;
                }
                else if (firstReasonSlot) { // && !IsPeriodMatch
                  periodStart = reasonSlot.DateTimeRange.Upper.Value;
                  break;
                }
                else { // Different reason or gap
                  break;
                }
                firstReasonSlot = false;
              }
              if (!periodStart.HasValue && first.RestrictedRange.Lower.HasValue) { // Default if no reason slot
                periodStart = first.RestrictedRange.Lower.Value;
              }
            }
          }
          else if (!periodStart.HasValue && !m_period.Equals (CurrentReasonPeriod.None)) {
            SetPeriodStartFromFacts (ref periodStart, m_machine, m_period, machineMode, dateTime);
          }
          if (log.IsDebugEnabled) {
            log.Debug ($"GuessReasonFromMachineMode: consider reason {reason.Id} from reason extensions machinemode={machineMode.Id}");
          }
        }
      }
    }
  }

  /// <summary>
  /// Response of the business request CurrentReason
  /// </summary>
  public sealed class CurrentReasonResponse
    : Lemoine.Extensions.Business.Reason.ICurrentReasonData
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CurrentReasonResponse).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    internal CurrentReasonResponse ()
    {
    }

    /// <summary>
    /// Current date/time (now)
    /// </summary>
    public DateTime CurrentDateTime { get; internal set; }

    /// <summary>
    /// Associated reason
    /// </summary>
    public IReason Reason { get; internal set; }

    /// <summary>
    /// Associated machine mode
    /// </summary>
    public IMachineMode MachineMode { get; internal set; }

    /// <summary>
    /// UTC Date/time of the period start if the Period parameter is set
    /// </summary>
    public DateTime? PeriodStart { get; internal set; }

    /// <summary>
    /// UTC Date/time of the response
    /// </summary>
    public DateTime DateTime { get; internal set; }

    /// <summary>
    /// Associated reason score
    /// </summary>
    public double? ReasonScore { get; internal set; }

    /// <summary>
    /// Reason source
    /// </summary>
    public ReasonSource? ReasonSource { get; internal set; }

    /// <summary>
    /// Auto-reason number
    /// </summary>
    public int? AutoReasonNumber { get; set; }
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Extensions.AutoReason.Action;

namespace Lemoine.Plugin.AutoReasonStopBetweenCycles
{
  /// <summary>
  /// Detect stops between cycles and apply an autoreason
  /// </summary>
  public sealed class AutoReasonExtension
    : Lemoine.Extensions.AutoReason.AutoReasonExtensionBase<Configuration>
    , Lemoine.Extensions.AutoReason.IAutoReasonExtension
  {
    ILog log = LogManager.GetLogger (typeof (AutoReasonExtension).FullName);

    double m_reasonScoreExtended;
    TimeSpan? m_extendedStopLimit = null;
    IReason m_reasonForExtendedStops = null;

    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension () : base (Plugin.DEFAULT_REASON_TRANSLATION_KEY, "AutoReason.StopBetweenCycles")
    {
    }

    /// <summary>
    /// Additional configuration of the plugin that comes from the database or from the plugin configuration
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    protected override bool InitializeAdditionalConfigurations (Configuration configuration)
    {
      log = LogManager
        .GetLogger ($"{typeof (AutoReasonExtension).FullName}.{this.Machine.Id}");

      // Durations
      m_reasonScoreExtended = configuration.ReasonScoreExtended;
      m_extendedStopLimit = configuration.ExtendedPeriod;

      // Reason for the extended period
      if (m_extendedStopLimit.HasValue) {
        int reasonId = configuration.ExtendedReasonId;
        if (reasonId == 0) {
          m_reasonForExtendedStops = ModelDAOHelper.DAOFactory.ReasonDAO
            .FindAll ()
            .FirstOrDefault (r => string.Equals (DefaultReasonTranslationKey + "Extended", r.TranslationKey, StringComparison.InvariantCultureIgnoreCase));
        }
        else { // 0 != reasonId
          m_reasonForExtendedStops = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (reasonId);
        }
        if (m_reasonForExtendedStops is null) {
          GetLogger ().ErrorFormat ("InitializeAdditionalConfigurations: reason {0} or {1} for an extended stop could not be loaded",
                                    reasonId, DefaultReasonTranslationKey + "Extended");
          return false;
        }
        else {
          GetLogger ().InfoFormat ("InitializeAdditionalConfigurations: successfully loaded reason {0}: id {1}", DefaultReasonTranslationKey + "Extended", reasonId);
        }
      }

      // Everything is ok
      return true;
    }

    /// <summary>
    /// <see cref="IAutoReasonExtension"/>
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <returns></returns>
    public override bool CanOverride (IReasonSlot reasonSlot)
    {
      return reasonSlot.ReasonScore < this.ReasonScore
        || ( (null != m_reasonForExtendedStops) && (reasonSlot.ReasonScore < m_reasonScoreExtended));
    }

    public override bool IsValidExtraAutoReason (IReasonSlot reasonSlot)
    {
      return true;
    }

    public override bool IsValidMatch (IMachineMode machineMode, IMachineObservationState machineObservationState, IReason reason, double score)
    {
      if ((null != reason)
        && (reason.Id != this.Reason.Id)
        && ((null == m_reasonForExtendedStops) || (reason.Id != m_reasonForExtendedStops.Id))) {
        return false;
      }

      if ((score != this.ReasonScore)
        && ((null == m_reasonForExtendedStops) || (score != m_reasonScoreExtended))) {
        return false;
      }

      return true;
    }

    protected override void Check ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("AutoReason.StopBetweenCycles.Check")) {
          // Find the next cycle end
          IDynamicTimeResponse dynamicStartResponse = Lemoine.Business.DynamicTimes.DynamicTime
            .GetDynamicTime ("CycleEnd", this.Machine, this.DateTime, notApplicableExpected: true);
          Debug.Assert (null != dynamicStartResponse);

          if (dynamicStartResponse.NotApplicable) {
            // Not applicable: datetime is not in a cycle
            if (GetLogger ().IsInfoEnabled) {
              log.Info ($"Check: cycle end not searched because {this.DateTime} is not in a cycle");
            }
            var cycleAfter = ModelDAOHelper.DAOFactory.OperationCycleDAO.GetFirstStrictlyAfter (this.Machine, this.DateTime);
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"Check: cycle after dateTime={this.DateTime} is {cycleAfter}");
            }
            if (cycleAfter != null && cycleAfter.Begin.HasValue) {
              GoOn (cycleAfter.Begin.Value.AddSeconds (1));
            }
          }
          else if (dynamicStartResponse.NoData) {
            // NoData: we are in a cycle that has no end and another cycle began after it => we skip the current cycle
            log.InfoFormat ("Check: skip the current cycle at {0} that has no end", this.DateTime);
            var cycleAfter = ModelDAOHelper.DAOFactory.OperationCycleDAO.GetFirstStrictlyAfter (this.Machine, this.DateTime);
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"Check: cycle after dateTime={this.DateTime} is {cycleAfter}");
            }
            if (cycleAfter != null && cycleAfter.Begin.HasValue) {
              GoOn (cycleAfter.Begin.Value.AddSeconds (1));
            }
          }
          else if (dynamicStartResponse.Final.HasValue) {
            // A next production end is found
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"Check: dynamic time CycleEnd={dynamicStartResponse.Final} for dateTime={this.DateTime}");
            }
            StartReason (dynamicStartResponse.Final.Value);
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"Check: reason started, move dateTime to {dynamicStartResponse.Final.Value.AddSeconds (1)}");
            }
            GoOn (dynamicStartResponse.Final.Value.AddSeconds (1));
          }
          else {
            // We don't know yet if there is a cycle end, we need to wait
            // "Pending" falls also in this case
            log.InfoFormat ("Check: don't know yet if there is a cycle end after {0}", this.DateTime);
          }
        }
      }
    }

    void GoOn (DateTime dateTime)
    {
      if (dateTime <= this.DateTime) {
        GetLogger ().Error ($"GoOn: not requesting a date/time change, dateTime={dateTime}");
        dateTime = this.DateTime.AddSeconds (1);
      }
      this.AddUpdateDateTimeDelayedAction (dateTime);
    }

    void StartReason (DateTime start)
    {
      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ($"StartReason: start={start} extendedStopLimit={m_extendedStopLimit}");
      }
      // Extended stop possible?
      if (m_extendedStopLimit.HasValue) {
        { // Extended
          var range = new UtcDateTimeRange (start, start.AddDays (1));
          AddReason (m_reasonForExtendedStops, m_reasonScoreExtended, range);
        }
        { // Normal
          var range = new UtcDateTimeRange (start, start.Add (m_extendedStopLimit.Value));
          AddReason (range);
        }
      }
      else {
        // Only normal
        var range = new UtcDateTimeRange (start, start.AddDays (1));
        AddReason (range);
      }
    }

    void AddReason (IReason reason, double reasonScore, UtcDateTimeRange range)
    {
      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ($"AddReason: range={range} reason={reason?.Id} score={reasonScore}");
      }

      var action = new ApplyReasonAction (this, reason, reasonScore, range, ",NextCycleSameOperation");
      AddDelayedAction (action);
    }

    void AddReason (UtcDateTimeRange range)
    {
      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ($"AddReason: range={range}");
      }

      var action = new ApplyReasonAction (this, range, ",NextCycleSameOperation");
      AddDelayedAction (action);
    }
  }
}

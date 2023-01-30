// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Linq;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Extensions.AutoReason.Action;

namespace Lemoine.Plugin.AutoReasonShiftEnd
{
  /// <summary>
  /// Detect idle periods at the end of a shift and apply an auto-reason
  /// </summary>
  public sealed class AutoReasonExtension
    : Lemoine.Extensions.AutoReason.AutoReasonExtensionBase<Configuration>
    , Lemoine.Extensions.AutoReason.IAutoReasonExtension
  {
    ILog log = LogManager.GetLogger (typeof (AutoReasonExtension).FullName);

    // ID of the previous shift
    static readonly string PREVIOUS_SHIFT_ID = "PreviousShiftID";
    int m_previousShiftId = -2; // -2 is not initialized, -1 will be "no shift"
    TimeSpan m_maxDuration;

    /// <summary>
    /// Set to true for a testing purpose (to remove the getLastEffective condition)
    /// False by default
    /// </summary>
    public bool TestMode { get; set; }

    public override ILog GetLogger ()
    {
      return log;
    }

    public AutoReasonExtension () : base (Plugin.DEFAULT_REASON_TRANSLATION_KEY, "AutoReason.ShiftEnd")
    {
      TestMode = false;
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

      // Previous shift id
      var previousShiftId = ModelDAOHelper.DAOFactory.AutoReasonStateDAO.GetAutoReasonState (this.Machine, GetKey (PREVIOUS_SHIFT_ID));
      m_previousShiftId = (previousShiftId != null) ? (int)previousShiftId.Value : -2;

      m_maxDuration = configuration.MaxDuration;

      // Everything is ok
      return true;
    }

    public override bool IsValidExtraAutoReason (IReasonSlot reasonSlot)
    {
      return true;
    }

    protected override void Check ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("AutoReason.ShiftEnd.Check")) {

          // Find an observation state slot after the current date time
          DateTime maxForDetection = DateTime.UtcNow;
          var nextOSSs = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO.FindFirstOverlapsRange (
            this.Machine, new UtcDateTimeRange (this.DateTime.AddSeconds (1), maxForDetection, true, true), 1, false);

          if (nextOSSs != null && nextOSSs.Any ()) {
            IObservationStateSlot nextOSS = nextOSSs.First ();

            // Is there a beginning of a slot?
            if (nextOSS.BeginDateTime.HasValue && nextOSS.BeginDateTime.Value > this.DateTime && nextOSS.BeginDateTime.Value <= maxForDetection) {
              int newShiftId = (nextOSS.Shift != null) ? nextOSS.Shift.Id : -1;
              bool canGoOn = true;
              if (m_previousShiftId != -2 && m_previousShiftId != newShiftId) {
                canGoOn = CreateAutoReason (nextOSS.BeginDateTime.Value);
              }

              // Datetime will be the beginning of the OSS
              if (canGoOn) { // Otherwise we wait
                AddUpdatePreviousShiftIdAction (newShiftId);
                GoOn (nextOSS.BeginDateTime.Value);
              }
            }
            else if (nextOSS.EndDateTime.HasValue && nextOSS.EndDateTime.Value > this.DateTime && nextOSS.EndDateTime.Value <= maxForDetection) {
              // Datetime will be the end of the OSS
              GoOn (nextOSS.EndDateTime.Value.AddSeconds (-1));
            }
            else {
              // Datetime will be the end of the studied range
              GoOn (maxForDetection);
            }
          }
        }
      }
    }

    void GoOn (DateTime dateTime)
    {
      if (dateTime <= this.DateTime) {
        dateTime = this.DateTime.AddSeconds (1);
      }
      this.AddUpdateDateTimeDelayedAction (dateTime);
    }

    void AddUpdatePreviousShiftIdAction (int previousShiftId)
    {
      var action = new UpdatePreviousShiftIdAction (this, previousShiftId);
      AddDelayedAction (action);
    }

    internal int GetPreviousShiftId ()
    {
      return m_previousShiftId;
    }

    internal void UpdatePreviousShiftId (int id)
    {
      m_previousShiftId = id;
      log.InfoFormat ("AutoReasonShiftEnd: updated '{0}' to '{1}'", GetKey (PREVIOUS_SHIFT_ID), m_previousShiftId);
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.Save (this.Machine, GetKey (PREVIOUS_SHIFT_ID), m_previousShiftId);
    }

    internal void ResetPreviousShiftId (int id)
    {
      m_previousShiftId = id;
    }

    bool CreateAutoReason (DateTime dateTime)
    {
      if (TestMode) { // TODO: to remove
        // Bouchon
        var range = new UtcDateTimeRange (dateTime.Subtract (m_maxDuration), dateTime);
        AddReason (range);
        return true;
      }

      // Find the last production end
      UtcDateTimeRange limit = new UtcDateTimeRange (dateTime.Subtract (m_maxDuration), dateTime, "[]");
      IDynamicTimeResponse dynamicResponse = Business.DynamicTimes.DynamicTime
       .GetDynamicTime ("LastProductionEnd", this.Machine, dateTime, new UtcDateTimeRange ("(,)"), limit);
      Debug.Assert (dynamicResponse != null);

      if (dynamicResponse.NotApplicable) {
        // Not applicable: end datetime is in a production
        log.InfoFormat ("ShiftEnd: last production not searched between {0} and {1} because the end is in a production",
          dateTime.Subtract (m_maxDuration), dateTime);
      } else if (dynamicResponse.NoData) {
        // The answer is: no last stop in the limit, we do nothing
        log.InfoFormat ("ShiftEnd: no last production end between {0} and {1}", dateTime.Subtract (m_maxDuration), dateTime);
      } else if (dynamicResponse.Final.HasValue) {
        // A last stop has been found, we create a reason machine association
        var lastStop = dynamicResponse.Final.Value;
        if (lastStop == dateTime) {
          log.InfoFormat ("ShiftEnd: the end of the shift is also the end of the production at {0}", dateTime);
        } else if (!limit.ContainsElement (lastStop)) {
          log.ErrorFormat ("ShiftEnd: LastProductionEnd returned a value {0} that is not in the specified range ", dateTime, limit);
        } else {
          var range = new UtcDateTimeRange (lastStop, dateTime);
          AddReason (range);
          log.InfoFormat ("ShiftEnd: found a last production end between {0} and {1}: {2}",
            dateTime.Subtract (m_maxDuration), dateTime, lastStop);
        }
      } else {
        // We don't know yet if there is a last stop, we need to wait
        log.InfoFormat ("ShiftEnd: don't know yet if there is a last production end between {0} and {1}",
          dateTime.Subtract (m_maxDuration), dateTime);
        return false;
      }

      return true;
    }

    void AddReason (UtcDateTimeRange range)
    {
      var action = new ApplyReasonAction (this, range);
      AddDelayedAction (action);
    }
  }
}

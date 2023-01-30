// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemoine.Plugin.MaintenanceAction
{
  sealed class MaintenanceActionEstimatedDateTime : Lemoine.Business.IRequest<DateTime?>
  {
    static readonly string CACHE_TIMEOUT_KEY = "MaintenanceAction.EstimatedDateTime.CacheTimeOut";
    static readonly TimeSpan CACHE_TIMEOUT_DEFAULT = TimeSpan.FromHours (1);

    #region Members
    double? m_utilizationPercentage = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (MaintenanceActionEstimatedDateTime).FullName);

    #region Getters / Setters
    /// <summary>
    /// Maintenance action (not null)
    /// </summary>
    MaintenanceAction MaintenanceAction { get; set; }

    double UtilizationPercentage
    {
      get
      {
        if (!m_utilizationPercentage.HasValue) {
          m_utilizationPercentage = Lemoine.Business.ServiceProvider.Get<double> (new Lemoine.Business.MachineMode.UtilizationPercentage (MaintenanceAction.Machine));
        }
        return m_utilizationPercentage.Value;
      }
    }
    #endregion // Getters / Setters

    public MaintenanceActionEstimatedDateTime (MaintenanceAction maintenanceAction)
    {
      this.MaintenanceAction = maintenanceAction;
    }

    public DateTime? Get ()
    {
      if (this.MaintenanceAction.CompletionDateTime.HasValue) {
        return this.MaintenanceAction.CompletionDateTime.Value;
      }

      DateTime? estimatedDateTime = null;

      UpdateEstimatedDateTime (this.MaintenanceAction.PlannedDateTime, ref estimatedDateTime);

      if (this.MaintenanceAction.RemainingMachiningDuration.HasValue
        || this.MaintenanceAction.StandardMachiningFrequency.HasValue) {
        // Get the elapsed machining duration
        var nowMinutes = DateTime.UtcNow;
        nowMinutes = nowMinutes.Subtract (TimeSpan.FromMilliseconds (nowMinutes.Millisecond).Add (TimeSpan.FromSeconds (nowMinutes.Second)));
        Debug.Assert (DateTime.UtcNow.Subtract (TimeSpan.FromMinutes (2)) < nowMinutes);
        var elapsedRange = new UtcDateTimeRange (this.MaintenanceAction.CreationDateTime, nowMinutes);
        TimeSpan elapsedMachiningDuration;
        if (elapsedRange.IsEmpty ()) {
          log.WarnFormat ("Get: elapsedRange {0} is empty", elapsedRange);
          elapsedMachiningDuration = TimeSpan.FromTicks (0);
        }
        else {
          elapsedMachiningDuration = Lemoine.Business.ServiceProvider.Get<TimeSpan> (new Lemoine.Business.MachineMode.RunningDuration (MaintenanceAction.Machine, elapsedRange));
        }

        if (this.MaintenanceAction.RemainingMachiningDuration.HasValue) {
          if (this.MaintenanceAction.RemainingMachiningDuration.Value < elapsedMachiningDuration) {
            var t = elapsedMachiningDuration.Subtract (this.MaintenanceAction.RemainingMachiningDuration.Value); // machining time to subtract
            UpdateEstimatedDateTime (GetTimeFromExceededMachiningDuration (t), ref estimatedDateTime);
          }
          else {
            var remainingDuration = this.MaintenanceAction.RemainingMachiningDuration.Value.Subtract (elapsedMachiningDuration);
            UpdateEstimatedDateTime (GetTimeFromRemainingMachiningDuration (remainingDuration),
              ref estimatedDateTime);
          }
        }

        if (this.MaintenanceAction.StandardMachiningFrequency.HasValue) {
          if (this.MaintenanceAction.StandardMachiningFrequency.Value < elapsedMachiningDuration) {
            var t = elapsedMachiningDuration.Subtract (this.MaintenanceAction.StandardMachiningFrequency.Value); // machining time to subtract
            UpdateEstimatedDateTime (GetTimeFromExceededMachiningDuration (t), ref estimatedDateTime);
          }
          else {
            var remainingDuration = this.MaintenanceAction.StandardMachiningFrequency.Value.Subtract (elapsedMachiningDuration);
            UpdateEstimatedDateTime (GetTimeFromRemainingMachiningDuration (remainingDuration),
              ref estimatedDateTime);
          }
        }
      }

      if (this.MaintenanceAction.StandardTotalFrequency.HasValue) {
        UpdateEstimatedDateTime (this.MaintenanceAction.CreationDateTime.Add (this.MaintenanceAction.StandardTotalFrequency.Value),
          ref estimatedDateTime);
      }

      return estimatedDateTime;
    }

    public Task<DateTime?> GetAsync ()
    {
      return Task.FromResult (Get ());
    }

    static void UpdateEstimatedDateTime (DateTime? newValue, ref DateTime? estimatedDateTime)
    {
      if (newValue.HasValue) {
        if (estimatedDateTime.HasValue) {
          if (newValue.Value < estimatedDateTime.Value) {
            estimatedDateTime = newValue;
          }
        }
        else {
          estimatedDateTime = newValue;
        }
      }
    }

    DateTime GetTimeFromRemainingMachiningDuration (TimeSpan remainingMachiningDuration)
    {
      return DateTime.UtcNow.AddSeconds (this.UtilizationPercentage * remainingMachiningDuration.TotalSeconds);
    }

    DateTime GetTimeFromExceededMachiningDuration (TimeSpan machiningDuration)
    {
      var t = machiningDuration;
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Plugin.MaintenanceAction.EstimatedDateTime.ExceededRemaingMachiningDuration")) {
          var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRangeDescending (this.MaintenanceAction.Machine, new UtcDateTimeRange (new LowerBound<DateTime> (null), new UpperBound<DateTime> (null)), TimeSpan.FromDays (1))
            .Where (s => s.Running);
          foreach (var reasonSlot in reasonSlots) {
            if (reasonSlot.Duration.HasValue) {
              log.FatalFormat ("GetTimeFromExceededMachiningDuration: " +
                "reason slot with no duration");
              Debug.Assert (false);
            }
            else {
              if (reasonSlot.Duration.Value < t) {
                t = t.Subtract (reasonSlot.Duration.Value);
              }
              else {
                Debug.Assert (reasonSlot.DateTimeRange.Upper.HasValue);
                return reasonSlot.DateTimeRange.Upper.Value.Subtract (t);
              }
            }
          }
        }
      }

      log.FatalFormat ("GetTimeFromExceededMachiningDuration: incoherent data, missing reason slots");
      Debug.Assert (false);
      throw new InvalidProgramException ("Missing reason slots");
    }

    public string GetCacheKey ()
    {
      Debug.Assert (null != this.MaintenanceAction);
      return "Plugin.MaintenanceAction.EstimatedDateTime." + this.MaintenanceAction.Id.ToString ();
    }

    public bool IsCacheValid (Business.CacheValue<DateTime?> cache)
    {
      return true;
    }

    public TimeSpan GetCacheTimeout (DateTime? data)
    {
      return Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (CACHE_TIMEOUT_KEY,
        CACHE_TIMEOUT_DEFAULT);
    }

    public static DateTime? GetEstimatedDateTime (MaintenanceAction maintenanceAction)
    {
      return Lemoine.Business.ServiceProvider.Get<DateTime?> (new MaintenanceActionEstimatedDateTime (maintenanceAction));
    }

  }
}

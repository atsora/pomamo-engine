// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.DefaultAccumulators
{
  /// <summary>
  /// 
  /// </summary>
  public class CustomAction
    : Lemoine.Extensions.Configuration.IConfiguration
    , Lemoine.Extensions.CustomAction.ICustomAction
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (CustomAction).FullName);

    #region Getters / Setters
    /// <summary>
    /// Help text for the configuration
    /// </summary>
    public string Help => "Select a period in which data will be computed, then validate. This operation may take time.";

    /// <summary>
    /// Machine
    /// </summary>
    [PluginConf ("Machine", "Machine", Optional = true)]
    public int MachineId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [PluginConf ("DateRange", "Period", Optional = false)]
    public IRange<DateTime> Range { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [PluginConf ("Bool", "Reason summary", Optional = false)]
    public bool ReasonSummary { get; set; } = true;

    /// <summary>
    /// 
    /// </summary>
    [PluginConf ("Bool", "Machine activity summary", Optional = false)]
    public bool MachineActivitySummary { get; set; } = true;

    /// <summary>
    /// 
    /// </summary>
    [PluginConf ("Bool", "Production state summary", Optional = false)]
    public bool ProductionStateSummary { get; set; } = true;

    /// <summary>
    /// 
    /// </summary>
    [PluginConf ("Bool", "Production rate summary", Optional = false)]
    public bool ProductionRateSummary { get; set; } = true;
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CustomAction ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Return true if the configuration is valid
    /// </summary>
    public bool IsValid (out IEnumerable<string> errors)
    {
      var errorList = new List<string> ();

      if (this.Range.IsEmpty ()) {
        errorList.Add ("Empty period");
      }
      if (!this.Range.Lower.HasValue) {
        errorList.Add ("No lower value");
      }
      if (!this.Range.Upper.HasValue) {
        errorList.Add ("No upper value");
      }

      errors = errorList;
      return (0 == errorList.Count);
    }

    public void DoAction (IEnumerable<Extensions.Configuration.Implementation.EmptyConfiguration> configurations, ref IList<string> warnings, ref int revisionId)
    {
      // Limits of computation
      var range = this.Range;

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IList<IMachine> machines;
        if (0 != this.MachineId) {
          var machine = ModelDAOHelper.DAOFactory.MachineDAO
            .FindById (this.MachineId);
          if (machine is null) {
            log.Error ($"DoAction: no machine with id {this.MachineId} => do nothing");
            return;
          }
          machines = new List<IMachine> { machine };
        }
        else {
          machines = ModelDAOHelper.DAOFactory.MachineDAO
            .FindAll ();
        }

        foreach (var day in this.Range.GetDays ()) {
          var daySlotRequest = new Lemoine.Business.Time.DaySlotFromDay (day);
          var daySlot = Lemoine.Business.ServiceProvider
            .Get (daySlotRequest);
          foreach (var machine in machines) {
            if (this.ReasonSummary) {
              ComputeReasonSummary (machine, day, daySlot);
            }
            if (this.MachineActivitySummary) {
              ComputeMachineActivitySummary (machine, day, daySlot);
            }
            if (this.ProductionStateSummary) {
              ComputeProductionStateSummary (machine, day, daySlot);
            }
            if (this.ProductionRateSummary) {
              ComputeProductionRateSummary (machine, day, daySlot);
            }
            // TODO: Production and ShiftByMachine accumulators
          }
        }
      }
    }

    TimeSpan? GetDayDuration (IReasonSlot reasonSlot, DateTime day, IDaySlot daySlot)
    {
      if (new DayRange (day, day, true, true).ContainsRange (reasonSlot.DayRange)) {
        return reasonSlot.Duration;
      }
      else {
        if (daySlot is null) {
          log.Error ($"GetDayDuration: day slot was null for {day}");
          return null;
        }
        var intersection = new UtcDateTimeRange (reasonSlot.DateTimeRange.Intersects (daySlot.DateTimeRange));
        if (log.IsDebugEnabled) {
          log.Debug ($"GetDayDuration: intersection={intersection}, duration={intersection.Duration}, day={day}, daySlot={daySlot.DateTimeRange}, reasonSlot={reasonSlot.DateTimeRange}");
        }
        return intersection.Duration;
      }
    }

    void ComputeReasonSummary (IMachine machine, DateTime day, IDaySlot daySlot, int remainingAttempt = 2)
    {
      if (0 == remainingAttempt) {
        log.Error ($"ComputeReasonSummary: last attempt reached for machine={machine.Id} day={day}");
        return;
      }

      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (var transaction = session.BeginTransaction ("DefaultAccumulators.Action.ReasonSummary")) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ComputeReasonSummary: machine={machine.Id} day={day}");
          }
          var reasonSummaries = ModelDAOHelper.DAOFactory.ReasonSummaryDAO
            .FindInDayRange (machine, new DayRange (day, day));
          foreach (var reasonSummary in reasonSummaries) {
            ModelDAOHelper.DAOFactory.ReasonSummaryDAO
              .MakeTransient (reasonSummary);
          }
          var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindAtDay (machine, day)
            .Where (s => s.Reason.Id != (int)ReasonId.Processing);
          var groups = reasonSlots.GroupBy (x => new ReasonSummaryKey (day, x));
          foreach (var group in groups) {
            var reasonSummary = ModelDAOHelper.ModelFactory.CreateReasonSummary (machine, day, group.Key.Shift, group.Key.MachineObservationState, group.Key.Reason);
            reasonSummary.Number = group.Count ();
            reasonSummary.Time = TimeSpan.FromSeconds (group.Sum (x => GetDayDuration (x, day, daySlot)?.TotalSeconds ?? 0.0));
            ModelDAOHelper.DAOFactory.ReasonSummaryDAO
              .MakePersistent (reasonSummary);
          }
          transaction.Commit ();
        }
      }
      catch (Exception ex) {
        log.Exception (ex, "ComputeReasonSummary");
        ComputeReasonSummary (machine, day, daySlot, remainingAttempt - 1);
      }
    }

    void ComputeMachineActivitySummary (IMachine machine, DateTime day, IDaySlot daySlot, int remainingAttempt = 2)
    {
      if (0 == remainingAttempt) {
        log.Error ($"ComputeMachineActivitySummary: last attempt reached for machine={machine.Id} day={day}");
        return;
      }

      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (var transaction = session.BeginTransaction ("DefaultAccumulators.Action.MachineActivitySummary")) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ComputeMachineActivitySummary: machine={machine.Id} day={day}");
          }
          var machineActivitySummaries = ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO
            .FindInDayRange (machine, new DayRange (day, day));
          foreach (var machineActivitySummary in machineActivitySummaries) {
            ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO
              .MakeTransient (machineActivitySummary);
          }
          var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindAtDay (machine, day);
          var groups = reasonSlots.GroupBy (x => new MachineActivitySummaryKey (day, x));
          foreach (var group in groups) {
            var machineActivitySummary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine, day, group.Key.MachineObservationState, group.Key.MachineMode, group.Key.Shift);
            machineActivitySummary.Time = TimeSpan.FromSeconds (group.Sum (x => GetDayDuration (x, day, daySlot)?.TotalSeconds ?? 0.0));
            ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO
              .MakePersistent (machineActivitySummary);
          }
          transaction.Commit ();
        }
      }
      catch (Exception ex) {
        log.Exception (ex, "ComputeMachineActivitySummary");
        ComputeMachineActivitySummary (machine, day, daySlot, remainingAttempt - 1);
      }
    }

    void ComputeProductionStateSummary (IMachine machine, DateTime day, IDaySlot daySlot, int remainingAttempt = 2)
    {
      if (0 == remainingAttempt) {
        log.Error ($"ComputeProductionStateSummary: last attempt reached for machine={machine.Id} day={day}");
        return;
      }

      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (var transaction = session.BeginTransaction ("DefaultAccumulators.Action.ProductionStateSummary")) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ComputeProductionStateSummary: machine={machine.Id} day={day}");
          }
          var summaries = ModelDAOHelper.DAOFactory.ProductionStateSummaryDAO
            .FindInDayRange (machine, new DayRange (day, day));
          foreach (var summary in summaries) {
            ModelDAOHelper.DAOFactory.ProductionStateSummaryDAO
              .MakeTransient (summary);
          }
          var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindAtDay (machine, day);
          var groups = reasonSlots
            .Where (x => null != x.ProductionState)
            .GroupBy (x => new ProductionStateSummaryKey (day, x));
          foreach (var group in groups) {
            var summary = ModelDAOHelper.ModelFactory.CreateProductionStateSummary (machine, day, group.Key.Shift, group.Key.MachineObservationState, group.Key.ProductionState);
            summary.Duration = TimeSpan.FromSeconds (group.Sum (x => GetDayDuration (x, day, daySlot)?.TotalSeconds ?? 0.0));
            ModelDAOHelper.DAOFactory.ProductionStateSummaryDAO
              .MakePersistent (summary);
          }
          transaction.Commit ();
        }
      }
      catch (Exception ex) {
        log.Exception (ex, "ComputeProductionStateSummary");
        ComputeProductionStateSummary (machine, day, daySlot, remainingAttempt - 1);
      }
    }

    void ComputeProductionRateSummary (IMachine machine, DateTime day, IDaySlot daySlot, int remainingAttempt = 2)
    {
      if (0 == remainingAttempt) {
        log.Error ($"ComputeProductionRateSummary: last attempt reached for machine={machine.Id} day={day}");
        return;
      }

      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (var transaction = session.BeginTransaction ("DefaultAccumulators.Action.ProductionRateSummary")) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ComputeProductionRateSummary: machine={machine.Id} day={day}");
          }
          var summaries = ModelDAOHelper.DAOFactory.ProductionRateSummaryDAO
            .FindInDayRange (machine, new DayRange (day, day));
          foreach (var summary in summaries) {
            ModelDAOHelper.DAOFactory.ProductionRateSummaryDAO
              .MakeTransient (summary);
          }
          var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindAtDay (machine, day);
          var groups = reasonSlots.GroupBy (x => new ProductionRateSummaryKey (day, x));
          foreach (var group in groups) {
            var totalDurationWithRateSeconds = group.Where (x => x.ProductionRate.HasValue).Sum (x => x.Duration?.TotalSeconds ?? 0.0);
            if (0 < totalDurationWithRateSeconds) {
              var summary = ModelDAOHelper.ModelFactory.CreateProductionRateSummary (machine, day, group.Key.Shift, group.Key.MachineObservationState);
              summary.Duration = TimeSpan.FromSeconds (group.Sum (x => GetDayDuration (x, day, daySlot)?.TotalSeconds ?? 0.0));
              summary.ProductionRate = group.Where (x => x.ProductionRate.HasValue).Sum (x => x.ProductionRate.Value / totalDurationWithRateSeconds * x.Duration.Value.TotalSeconds);
              ModelDAOHelper.DAOFactory.ProductionRateSummaryDAO
                .MakePersistent (summary);
            }
          }
          transaction.Commit ();
        }
      }
      catch (Exception ex) {
        log.Exception (ex, "ComputeProductionRateSummary");
        ComputeProductionRateSummary (machine, day, daySlot, remainingAttempt - 1);
      }
    }
    #endregion // Methods
  }

}

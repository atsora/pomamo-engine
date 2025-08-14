// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Business;
using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Extensions.Configuration.Implementation;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.HourlyOperationSummary
{
  /// <summary>
  /// CustomAction
  /// </summary>
  public class CustomAction
    : Lemoine.Extensions.Configuration.IConfiguration
    , Lemoine.Extensions.CustomAction.ICustomActionNoConfig
  {
    readonly ILog log = LogManager.GetLogger (typeof (CustomAction).FullName);

    #region Getters / Setters
    public string Help => "Select a period in which data will be computed, then validate. This operation may take time.";

    /// <summary>
    /// 
    /// </summary>
    [PluginConf ("UtcDateTimeRange", "Period", Optional = false)]
    public UtcDateTimeRange Range { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public CustomAction ()
    {
    }


    public void DoAction (IEnumerable<EmptyConfiguration> configurations, ref IList<string> warnings, ref int revisionId)
    {
      Debug.Assert (!this.Range.IsEmpty ());
      if (this.Range.IsEmpty ()) {
        return;
      }

      Debug.Assert (this.Range.Lower.HasValue);
      if (!this.Range.Lower.HasValue) {
        log.Error ("DoAction: no lower bound");
        return;
      }

      LowerBound<DateTime> lower;
      if (this.Range.Lower.HasValue) {
        lower = this.Range.Lower.Value.ToLocalDateHour ();
      }
      else {
        lower = DateTime.Now.AddDays (-31).ToLocalDateHour ();
        log.Error ($"DoAction: no lower bound was defined: use {lower} instead");
      }
      UpperBound<DateTime> upper;
      if (this.Range.Upper.HasValue) {
        upper = this.Range.Upper.Value.ToLocalDateHour ();
      }
      else {
        upper = DateTime.Now.ToLocalDateHour ();
        log.Info ($"DoAction: no upper bound was defined: use {upper} instead");
      }
      var localDateHourRange = new LocalDateTimeRange (lower, upper, "[]");
      var preLoadRange = new UtcDateTimeRange (localDateHourRange.Lower, localDateHourRange.UpperInclusive ? localDateHourRange.Upper.Value.AddHours (1) : localDateHourRange.Upper.Value);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var dao = new HourlyOperationSummaryDAO ();

        using (var transaction = session.BeginTransaction ("Plugin.HourlyOperationSummary.Action.Delete")) {
          dao.Delete (localDateHourRange);
          transaction.Commit ();
        }

        var monitoredMachines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindAll ();
        foreach (var machine in monitoredMachines) {
          if (log.IsDebugEnabled) {
            log.Debug ($"DoAction: machine {machine.Id}");
          }

          foreach (var localDateHour in GetLocalDateHours (localDateHourRange)) {
            var r = new UtcDateTimeRange (localDateHour, localDateHour.AddHours (1));
            using (var transaction = session.BeginTransaction ("Plugin.HourlyOperationSummary.Action.Insert")) {
              if (log.IsDebugEnabled) {
                log.Debug ($"DoAction machine {machine.Id} localDateHour={localDateHour}");
              }
              var operationCycles = ModelDAOHelper.DAOFactory.OperationCycleDAO.FindOverlapsRange (machine, r);
              var groupByHourlyOperationSummaries = operationCycles
                .GroupBy (s => new HourlyOperationSummary (machine, s.OperationSlot?.Operation, s.OperationSlot?.Component, s.OperationSlot?.WorkOrder, s.OperationSlot?.Line, s.OperationSlot?.ManufacturingOrder, s.OperationSlot?.Day, s.OperationSlot?.Shift, localDateHour), new HourlyOperationSummaryReferenceDataComparer ());
              foreach (var group in groupByHourlyOperationSummaries) {
                var hourlyOperationSummary = group.Key;
                hourlyOperationSummary.TotalCycles = group.Count (c => c.Full);
                hourlyOperationSummary.AdjustedCycles = group.Count (c => c.Full && c.Quantity.HasValue);
                hourlyOperationSummary.AdjustedQuantity = group
                  .Where (c => c.Full && c.Quantity.HasValue)
                  .Select (c => c.Quantity.Value)
                  .DefaultIfEmpty (0)
                  .Sum ();
                hourlyOperationSummary.Duration = TimeSpan.FromSeconds (group
                  .Select (g => g.OperationSlot)
                  .Where (x => !(x is null))
                  .Distinct ()
                  .Where (o => !(o.DateTimeRange is null) && !o.DateTimeRange.IsEmpty ())
                  .Select (o => new UtcDateTimeRange (o.DateTimeRange.Intersects (r)).Duration)
                  .Where (d => d.HasValue)
                  .Select (d => d.Value.TotalSeconds)
                  .DefaultIfEmpty (0.0)
                  .Sum ());
                dao.MakePersistent (hourlyOperationSummary);
              }
              transaction.Commit ();
            }
          }
        }
      }
    }

    public bool IsValid (out IEnumerable<string> errors)
    {
      var errorList = new List<string> ();

      if (this.Range.IsEmpty ()) {
        errorList.Add ("Empty period");
      }
      if (!this.Range.Lower.HasValue) {
        errorList.Add ("No lower bound was defined");
      }

      errors = errorList;
      return (0 == errorList.Count);
    }
    #endregion // Constructors

    IEnumerable<DateTime> GetLocalDateHours (LocalDateTimeRange localDateTimeRange)
    {
      Debug.Assert (localDateTimeRange.Lower.HasValue);
      Debug.Assert (localDateTimeRange.Upper.HasValue);

      var x = localDateTimeRange.Lower.Value;
      while (localDateTimeRange.ContainsElement (x)) {
        yield return x;
        x = x.AddHours (1).ToLocalDateHour ();
      }
    }

  }
}

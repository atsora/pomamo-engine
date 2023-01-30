// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Extensions.Database;

namespace Lemoine.Plugin.HourlyOperationSummary
{
  public class ComponentExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IComponentExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (ComponentExtension).FullName);

    public void Merge (IComponent oldComponent, IComponent newComponent)
    {
      { // - Update HourlyIntermediateWorkPieceByMachineSummary
        IList<IHourlyOperationSummary> operationSummaries =
          new HourlyOperationSummaryDAO ()
          .FindByComponent (oldComponent);
        foreach (IHourlyOperationSummary operationSummary in operationSummaries) {
          new HourlyOperationSummaryDAO ()
            .MakeTransient (operationSummary);
          IHourlyOperationSummary existing =
            new HourlyOperationSummaryDAO ()
            .FindByKey (operationSummary.Machine, operationSummary.Operation, newComponent, operationSummary.WorkOrder, operationSummary.Line, operationSummary.Task, operationSummary.Day, operationSummary.Shift, operationSummary.LocalDateHour);
          if (null != existing) {
            existing.Duration += operationSummary.Duration;
            existing.TotalCycles += operationSummary.TotalCycles;
            existing.AdjustedCycles += operationSummary.AdjustedCycles;
            existing.AdjustedQuantity += operationSummary.AdjustedQuantity;
            new HourlyOperationSummaryDAO ()
              .MakePersistent (existing);
          }
          else {
            IHourlyOperationSummary created =
              new HourlyOperationSummary (operationSummary.Machine,
                                          operationSummary.Operation,
                                          newComponent,
                                          operationSummary.WorkOrder,
                                          operationSummary.Line,
                                          operationSummary.Task,
                                          operationSummary.Day,
                                          operationSummary.Shift, operationSummary.LocalDateHour);
            created.Duration = operationSummary.Duration;
            created.TotalCycles = operationSummary.TotalCycles;
            created.AdjustedCycles = created.AdjustedCycles;
            created.AdjustedQuantity = created.AdjustedQuantity;
            new HourlyOperationSummaryDAO ()
              .MakePersistent (created);
          }
        }
      }

    }
  }
}

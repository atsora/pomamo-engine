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
  public class OperationExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IOperationExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (WorkOrderExtension).FullName);

    public void Merge (IOperation oldOperation, IOperation newOperation)
    {
      Debug.Assert (null != oldOperation);
      Debug.Assert (null != newOperation);

      IList<IHourlyOperationSummary> operationSummaries =
        new HourlyOperationSummaryDAO ()
        .FindByOperation (oldOperation);
      foreach (IHourlyOperationSummary operationSummary in operationSummaries) {
        new HourlyOperationSummaryDAO ()
          .MakeTransient (operationSummary);
        IHourlyOperationSummary existing =
          new HourlyOperationSummaryDAO ()
          .FindByKey (operationSummary.Machine, newOperation, operationSummary.Component, operationSummary.WorkOrder,
                      operationSummary.Line, operationSummary.ManufacturingOrder,
                      operationSummary.Day, operationSummary.Shift, operationSummary.LocalDateHour);
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
                                        newOperation,
                                        operationSummary.Component,
                                        operationSummary.WorkOrder,
                                        operationSummary.Line,
                                        operationSummary.ManufacturingOrder,
                                        operationSummary.Day,
                                        operationSummary.Shift,
                                        operationSummary.LocalDateHour);
          created.Duration = operationSummary.Duration;
          created.TotalCycles = operationSummary.TotalCycles;
          created.AdjustedCycles = operationSummary.AdjustedCycles;
          created.AdjustedQuantity = operationSummary.AdjustedQuantity;
          new HourlyOperationSummaryDAO ()
            .MakePersistent (created);
        }
      }
    }
  }
}

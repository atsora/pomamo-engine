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

namespace Lemoine.Plugin.CycleCountSummary
{
  public class WorkOrderExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IWorkOrderExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (WorkOrderExtension).FullName);

    public void Merge (IWorkOrder oldWorkOrder, IWorkOrder newWorkOrder)
    {
      IList<ICycleCountSummary> oldSummarys = (new CycleCountSummaryDAO ())
        .FindByWorkOrder (oldWorkOrder);
      foreach (ICycleCountSummary oldSummary in oldSummarys) {
        (new CycleCountSummaryDAO ()).MakeTransient (oldSummary);
        (new CycleCountSummaryDAO ()).Update (oldSummary.Machine,
                                                 oldSummary.Day,
                                                 oldSummary.Shift,
                                                 newWorkOrder,
                                                 oldSummary.Line,
                                                 oldSummary.Task,
                                                 oldSummary.Component,
                                                 oldSummary.Operation,
                                                 oldSummary.Full,
                                                 oldSummary.Partial);
      }
    }
  }
}

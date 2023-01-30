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

namespace Lemoine.Plugin.CycleDurationSummary
{
  public class WorkOrderExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IWorkOrderExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (WorkOrderExtension).FullName);

    public void Merge (IWorkOrder oldWorkOrder, IWorkOrder newWorkOrder)
    {
      IList<ICycleDurationSummary> oldSummarys = (new CycleDurationSummaryDAO ())
        .FindByWorkOrder (oldWorkOrder);
      foreach (ICycleDurationSummary oldSummary in oldSummarys) {
        (new CycleDurationSummaryDAO ()).MakeTransient (oldSummary);
        (new CycleDurationSummaryDAO ()).UpdateDay (oldSummary.Machine,
                                                 oldSummary.Day,
                                                 oldSummary.Shift,
                                                 newWorkOrder,
                                                 oldSummary.Line,
                                                 oldSummary.Task,
                                                 oldSummary.Component,
                                                 oldSummary.Operation,
                                                 oldSummary.Offset,
                                                 oldSummary.Number,
                                                 oldSummary.Partial);
      }
    }
  }
}

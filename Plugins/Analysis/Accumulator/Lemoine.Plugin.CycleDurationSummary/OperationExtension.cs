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
  public class OperationExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IOperationExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (OperationExtension).FullName);

    public void Merge (IOperation oldOperation, IOperation newOperation)
    {
      IList<ICycleDurationSummary> oldSummarys = (new CycleDurationSummaryDAO ())
        .FindByOperation (oldOperation);
      foreach (ICycleDurationSummary oldSummary in oldSummarys) {
        (new CycleDurationSummaryDAO ()).MakeTransient (oldSummary);
        (new CycleDurationSummaryDAO ()).UpdateDay (oldSummary.Machine,
                                              oldSummary.Day,
                                              oldSummary.Shift,
                                              oldSummary.WorkOrder,
                                              oldSummary.Line,
                                              oldSummary.Task,
                                              oldSummary.Component,
                                              newOperation,
                                              oldSummary.Offset,
                                              oldSummary.Number,
                                              oldSummary.Partial);
      }
    }
  }
}

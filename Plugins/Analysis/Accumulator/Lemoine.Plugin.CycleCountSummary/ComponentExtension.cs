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
  public class ComponentExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IComponentExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (ComponentExtension).FullName);

    public void Merge (IComponent oldComponent, IComponent newComponent)
    {
      IList<ICycleCountSummary> oldSummarys = (new CycleCountSummaryDAO ())
        .FindByComponent (oldComponent);
      foreach (ICycleCountSummary oldSummary in oldSummarys) {
        (new CycleCountSummaryDAO ()).MakeTransient (oldSummary);
        (new CycleCountSummaryDAO ()).Update (oldSummary.Machine,
                                                 oldSummary.Day,
                                                 oldSummary.Shift,
                                                 oldSummary.WorkOrder,
                                                 oldSummary.Line,
                                                 oldSummary.ManufacturingOrder,
                                                 newComponent,
                                                 oldSummary.Operation,
                                                 oldSummary.Full,
                                                 oldSummary.Partial);
      }
    }
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
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
  public class ComponentExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IComponentExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (ComponentExtension).FullName);

    public void Merge (IComponent oldComponent, IComponent newComponent)
    {
      IList<ICycleDurationSummary> oldSummarys = (new CycleDurationSummaryDAO ())
        .FindByComponent (oldComponent);
      foreach (ICycleDurationSummary oldSummary in oldSummarys) {
        (new CycleDurationSummaryDAO ()).MakeTransient (oldSummary);
        (new CycleDurationSummaryDAO ()).UpdateDay (oldSummary.Machine,
                                                 oldSummary.Day,
                                                 oldSummary.Shift,
                                                 oldSummary.WorkOrder,
                                                 oldSummary.Line,
                                                 oldSummary.ManufacturingOrder,
                                                 newComponent,
                                                 oldSummary.Operation,
                                                 oldSummary.Offset,
                                                 oldSummary.Number,
                                                 oldSummary.Partial);
      }
    }
  }
}

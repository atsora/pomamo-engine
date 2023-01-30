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

namespace Lemoine.Plugin.IntermediateWorkPieceSummary
{
  public class ComponentExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IComponentExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (ComponentExtension).FullName);

    public void Merge (IComponent oldComponent, IComponent newComponent)
    {
      { // - Update IntermediateWorkPieceByMachineSummary
        IList<IIntermediateWorkPieceByMachineSummary> iwpSummarys =
          new IntermediateWorkPieceByMachineSummaryDAO ()
          .FindByComponent (oldComponent);
        foreach (IIntermediateWorkPieceByMachineSummary iwpSummary in iwpSummarys) {
          new IntermediateWorkPieceByMachineSummaryDAO ()
            .MakeTransient (iwpSummary);
          IIntermediateWorkPieceByMachineSummary existing =
            new IntermediateWorkPieceByMachineSummaryDAO ()
            .FindByKey (iwpSummary.Machine, iwpSummary.IntermediateWorkPiece, newComponent, iwpSummary.WorkOrder, iwpSummary.Line, iwpSummary.Task, iwpSummary.Day, iwpSummary.Shift);
          if (null != existing) {
            existing.Counted += iwpSummary.Counted;
            existing.Corrected += iwpSummary.Corrected;
            existing.Checked += iwpSummary.Checked;
            existing.Scrapped += iwpSummary.Scrapped;
            existing.Targeted += iwpSummary.Targeted;
            new IntermediateWorkPieceByMachineSummaryDAO ()
              .MakePersistent (existing);
          }
          else {
            IIntermediateWorkPieceByMachineSummary created =
              new IntermediateWorkPieceByMachineSummary (iwpSummary.Machine,
                                                                                       iwpSummary.IntermediateWorkPiece,
                                                                                       newComponent,
                                                                                       iwpSummary.WorkOrder,
                                                                                       iwpSummary.Line,
                                                                                       iwpSummary.Task,
                                                                                       iwpSummary.Day,
                                                                                       iwpSummary.Shift);
            created.Counted = iwpSummary.Counted;
            created.Corrected = iwpSummary.Corrected;
            created.Checked = iwpSummary.Checked;
            created.Scrapped = iwpSummary.Scrapped;
            created.Targeted = iwpSummary.Targeted;
            new IntermediateWorkPieceByMachineSummaryDAO ()
              .MakePersistent (created);
          }
        }
      }

    }
  }
}

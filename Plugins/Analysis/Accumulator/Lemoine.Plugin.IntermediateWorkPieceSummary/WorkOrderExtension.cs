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
  public class WorkOrderExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IWorkOrderExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (WorkOrderExtension).FullName);

    public void Merge (IWorkOrder oldWorkOrder, IWorkOrder newWorkOrder)
    {
      { // - Update IntermediateWorkPieceByMachineSummary
        IList<IIntermediateWorkPieceByMachineSummary> iwpSummarys =
          new IntermediateWorkPieceByMachineSummaryDAO ()
          .FindByWorkOrder (oldWorkOrder);
        foreach (IIntermediateWorkPieceByMachineSummary iwpSummary in iwpSummarys) {
          new IntermediateWorkPieceByMachineSummaryDAO ()
            .MakeTransient (iwpSummary);
          IIntermediateWorkPieceByMachineSummary existing =
            new IntermediateWorkPieceByMachineSummaryDAO ()
            .FindByKey (iwpSummary.Machine, iwpSummary.IntermediateWorkPiece, iwpSummary.Component, newWorkOrder,
                        iwpSummary.Line, iwpSummary.ManufacturingOrder,
                        iwpSummary.Day, iwpSummary.Shift);
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
                                                                                       iwpSummary.Component,
                                                                                       newWorkOrder,
                                                                                       iwpSummary.Line,
                                                                                       iwpSummary.ManufacturingOrder,
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

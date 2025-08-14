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

namespace Lemoine.Plugin.IntermediateWorkPieceSummary
{
  public class IntermediateWorkPieceExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IIntermediateWorkPieceExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (IntermediateWorkPieceExtension).FullName);

    public void Merge (IIntermediateWorkPiece oldIntermediateWorkPiece, IIntermediateWorkPiece newIntermediateWorkPiece)
    {
      { // - Update IntermediateWorkPieceByMachineSummary
        IList<IIntermediateWorkPieceByMachineSummary> iwpSummarys =
          new IntermediateWorkPieceByMachineSummaryDAO ()
          .FindByIntermediateWorkPiece (oldIntermediateWorkPiece);
        foreach (IIntermediateWorkPieceByMachineSummary iwpSummary in iwpSummarys) {
          new IntermediateWorkPieceByMachineSummaryDAO ()
            .MakeTransient (iwpSummary);
          IIntermediateWorkPieceByMachineSummary existing =
            new IntermediateWorkPieceByMachineSummaryDAO ()
            .FindByKey (iwpSummary.Machine, newIntermediateWorkPiece, iwpSummary.Component, iwpSummary.WorkOrder,
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
                                                                                       newIntermediateWorkPiece,
                                                                                       iwpSummary.Component,
                                                                                       iwpSummary.WorkOrder,
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

    public void UpdateQuantity (IIntermediateWorkPiece intermediateWorkPiece, int oldQuantity, int newQuantity)
    {
      // For the moment, do not retrofit any update of quantity in the past

      // If this needs to be done one day, check the following code:
      /*
      var summaries = new IntermediateWorkPieceByMachineSummaryDAO ()
        .FindByIntermediateWorkPiece (intermediateWorkPiece);
      foreach (IIntermediateWorkPieceByMachineSummary summary in summaries) {
        // UNDONE: note this computation below is not correct when some quantities are adjusted in a cycle
        summary.Counted = (summary.Counted / oldQuantity) * newQuantity;
        // UNDONE: this is hard with the information we currently have to update very easily summary.Corrected
        // because it is hard to distinguish the two different ways to correct the data
        // So let's to it later, when the IntermediateWorkPieceInformation table will be available
      }
      */
    }
  }
}

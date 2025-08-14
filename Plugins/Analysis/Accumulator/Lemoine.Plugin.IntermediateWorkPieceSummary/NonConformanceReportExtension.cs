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
  public class NonConformanceReportExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , INonConformanceReportExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (NonConformanceReportExtension).FullName);

    IMachine m_machine;

    public bool Initialize (IMachine machine)
    {
      m_machine = machine;
      return (null != machine);
    }

    public void ReportCycleNonConformance (IDeliverablePiece deliverablePiece, IOperationCycle operationCycle, INonConformanceReason reason, string details)
    {
      var intermediateWorkPieceSummary =
        new IntermediateWorkPieceByMachineSummaryDAO ()
        .FindByKey (m_machine,
                   deliverablePiece.Component.FinalWorkPiece,
                   deliverablePiece.Component,
                   operationCycle.OperationSlot.WorkOrder,
                   operationCycle.OperationSlot.Line,
                   operationCycle.OperationSlot.ManufacturingOrder,
                   operationCycle.OperationSlot.Day,
                   operationCycle.OperationSlot.Shift);

      if (null != intermediateWorkPieceSummary) {
        if (null != reason) {
          intermediateWorkPieceSummary.Scrapped++;
        }
        else {
          intermediateWorkPieceSummary.Corrected++;
          intermediateWorkPieceSummary.Scrapped--;
        }
        new IntermediateWorkPieceByMachineSummaryDAO ()
          .MakePersistent (intermediateWorkPieceSummary);
      }
    }
  }
}

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
  public class TaskNumberOfPartsExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , ITaskNumberOfPartsExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (TaskNumberOfPartsExtension).FullName);

    IMachine m_machine;

    public double Priority => 100.0;

    public bool GetNumberOfProducedParts (out double shiftPieces, out double globalPieces, ITask task, DateTime? day, IShift shift)
    {
      Debug.Assert (null != m_machine);
      Debug.Assert (null != task);

      shiftPieces = 0;
      globalPieces = 0;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IList<IIntermediateWorkPieceByMachineSummary> summarys = new IntermediateWorkPieceByMachineSummaryDAO ()
          .FindByTask (m_machine, task); // not summaries because it makes refactoring easier
        foreach (IIntermediateWorkPieceByMachineSummary summary in summarys) {
          if (day.HasValue
              && (null != shift)
              && object.Equals (day, summary.Day)
              && (null != summary.Shift)
              && (shift.Id == summary.Shift.Id)) {
            shiftPieces += summary.Counted;
          }
          globalPieces += summary.Counted;
        }
      }

      return true;
    }

    public bool Initialize (IMachine machine)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
      return (null != machine);
    }
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Analysis;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.DebugCycleEnd
{
  public class PendingModificationAnalysisExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IPendingModificationAnalysisExtension
  {
    ILog log = LogManager.GetLogger (typeof (PendingModificationAnalysisExtension).FullName);

    IMachine m_machine;
    UtcDateTimeRange m_lastRange;
    IDynamicTimeResponse m_lastResponse;

    public void AfterMakeAnalysis (IModification modification, bool completed)
    {
      if (null == m_machine) {
        return; // Not initialized
      }

      if (!(modification is IReasonMachineAssociation)) {
        return;
      }

      var reasonMachineAssociation = (IReasonMachineAssociation)modification;
      if (null == reasonMachineAssociation) {
        log.ErrorFormat ("Could not convert the reason machine association");
        return;
      }
      if (string.IsNullOrEmpty (reasonMachineAssociation.DynamicEnd)) {
        return;
      }
      if (!reasonMachineAssociation.DynamicEnd.Contains ("CycleEnd")) {
        return;
      }

      if ( (null != m_lastResponse)
        && !m_lastResponse.NoData
        && m_lastResponse.Final.HasValue) { // Check previous
        var newResponse = Lemoine.Business.DynamicTimes.DynamicTime
        .GetDynamicTime (reasonMachineAssociation.DynamicEnd, m_machine, m_lastRange);
        if (!object.Equals (m_lastResponse.Final, newResponse.Final)) {
          log.ErrorFormat ("Final {0}=>{1} for {2}", m_lastResponse.Final, newResponse.Final, m_lastRange);
          LogAdditionalInformation ();
        }
      }

      m_lastRange = reasonMachineAssociation.Range;
      m_lastResponse = Lemoine.Business.DynamicTimes.DynamicTime
        .GetDynamicTime (reasonMachineAssociation.DynamicEnd, m_machine, reasonMachineAssociation.Range);
      if (log.IsDebugEnabled) {
        log.DebugFormat ("CycleEnd({0}): Final={1} Hint={2} NoData={3} NotApplicable={4}",
          reasonMachineAssociation.Range, m_lastResponse.Final, m_lastResponse.Hint, m_lastResponse.NoData, m_lastResponse.NotApplicable);
      }
      LogAdditionalInformation ();
    }

    void LogAdditionalInformation ()
    {
      try {
        var operationDetectionStatus = Lemoine.Business.ServiceProvider
          .Get (new Lemoine.Business.Operation.OperationDetectionStatus (m_machine));
        log.DebugFormat ("OperationDetectionStatus: {0}", operationDetectionStatus);

        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("DebugCycleEnd")) {
            var lastCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
              .GetLast (m_machine);
            if (null != lastCycle) {
              log.DebugFormat ("LastCycle: {0}-{1} Status={2} Full={3}", lastCycle.Begin, lastCycle.End, lastCycle.Status, lastCycle.Full);
            }
          }
        }
      }
      catch (Exception ex) {
        log.Error ("LogAdditionalInformation", ex);
      }
    }

    public void BeforeMakeAnalysis (IModification modification)
    {
    }

    public void Initialize (IMachine machine)
    {
      if (null != machine) {
        m_machine = machine;
        log = LogManager.GetLogger (typeof (PendingModificationAnalysisExtension).FullName + "." + machine.Id);
      }
    }

    public void MakeAnalysisException (IModification modification, Exception ex)
    {
    }

    public void NotifyAllSubModificationsCompleted (IModification modification)
    {
    }
  }
}

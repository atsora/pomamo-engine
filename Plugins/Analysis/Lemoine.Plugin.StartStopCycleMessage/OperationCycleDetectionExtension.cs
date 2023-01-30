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

namespace Lemoine.Plugin.StartStopCycleMessage
{
  public class OperationCycleDetectionExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , Lemoine.Extensions.Analysis.IOperationCycleDetectionExtension
  {
    ILog log = LogManager.GetLogger (typeof (OperationCycleDetectionExtension).FullName);

    IMonitoredMachine m_machine;

    public void CreateBetweenCycle (Lemoine.Model.IBetweenCycles betweenCycles)
    {
    }

    public void DetectionProcessComplete ()
    {
    }

    public void DetectionProcessError (Lemoine.Model.IMachineModule machineModule, Exception ex)
    {
    }

    public void DetectionProcessStart ()
    {
    }

    public bool Initialize (Lemoine.Model.IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);

      log = LogManager.GetLogger (typeof (OperationCycleDetectionExtension).FullName + "." + machine.Id);

      m_machine = machine;

      return true;
    }

    public void StartCycle (Lemoine.Model.IOperationCycle operationCycle)
    {
      PushClearDomainMessage ("StartCycle");
    }

    public void StopCycle (Lemoine.Model.IOperationCycle operationCycle)
    {
      PushClearDomainMessage ("StopCycle");
    }

    void PushClearDomainMessage (string domain)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"PushClearDomainMessage: domain={domain}");
      }
      string message = $"Cache/ClearDomainByMachine/{domain}/{m_machine.Id}?Broadcast=true";
      //Debug.Assert (!ModelDAOHelper.DAOFactory.IsTransactionActive ()); // Not active because of the unit tests
      ModelDAOHelper.DAOFactory.PushMessage (message);
    }
  }
}

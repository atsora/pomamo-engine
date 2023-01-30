// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Analysis;
using Lemoine.Model;

namespace Lemoine.Plugin.AnalysisDetectionStatus
{
  public class AfterSequenceDetectionExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IAfterSequenceDetectionExtension
  {
    ILog log = LogManager.GetLogger (typeof (AfterSequenceDetectionExtension).FullName);

    Configuration m_configuration;
    IMachineModule m_machineModule;

    public IMachineModule MachineModule
    {
      get { return m_machineModule; }
    }

    public bool Initialize (IMachineModule machineModule)
    {
      log = LogManager.GetLogger (typeof (AfterSequenceDetectionExtension).FullName + "." + machineModule.MonitoredMachine.Id + "." + machineModule.Id);

      m_machineModule = machineModule;

      return LoadConfiguration (out m_configuration);
    }

    public void StartAutoOnlyOperation (IOperation operation, DateTime dateTime)
    {
      OperationDetectionNotifier.NotifyOperationDetection (m_machineModule.MonitoredMachine, dateTime);
    }

    public void StartSequence (ISequence sequence, DateTime dateTime)
    {
      OperationDetectionNotifier.NotifyOperationDetection (m_machineModule.MonitoredMachine, dateTime);
    }

    public void StopSequence (DateTime dateTime)
    {
      OperationDetectionNotifier.NotifyOperationDetection (m_machineModule.MonitoredMachine, dateTime);
    }
  }
}

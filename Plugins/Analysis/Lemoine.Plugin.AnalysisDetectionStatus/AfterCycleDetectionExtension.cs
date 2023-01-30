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
  public class AfterCycleDetectionExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IAfterCycleDetectionExtension
  {
    ILog log = LogManager.GetLogger (typeof (AfterCycleDetectionExtension).FullName);

    Configuration m_configuration;
    IMonitoredMachine m_machine;

    public IMonitoredMachine Machine
    {
      get { return m_machine; }
    }

    public void DetectionProcessError (IMachineModule machineModule, Exception ex)
    {
    }

    public bool Initialize (IMonitoredMachine machine)
    {
      log = LogManager.GetLogger (typeof (AfterCycleDetectionExtension).FullName + "." + machine.Id);

      m_machine = machine;

      return LoadConfiguration (out m_configuration);
    }

    public void StartCycle (DateTime dateTime)
    {
      CycleDetectionNotifier.NotifyCycleDetection (m_machine, dateTime);
    }

    public void StartStopCycle (DateTime startDateTime, DateTime stopDateTime)
    {
      CycleDetectionNotifier.NotifyCycleDetection (m_machine, stopDateTime);
    }

    public void StopCycle (int? quantity, DateTime dateTime)
    {
      CycleDetectionNotifier.NotifyCycleDetection (m_machine, dateTime);
    }
  }
}

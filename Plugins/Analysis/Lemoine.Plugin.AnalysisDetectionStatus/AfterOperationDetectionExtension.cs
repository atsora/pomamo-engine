// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
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
  public class AfterOperationDetectionExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IAfterOperationDetectionExtension
  {
    ILog log = LogManager.GetLogger (typeof (AfterOperationDetectionExtension).FullName);

    Configuration m_configuration;
    IMonitoredMachine m_machine;

    public IMonitoredMachine Machine
    {
      get { return m_machine; }
    }

    public void AddOperation (IOperation operation, UtcDateTimeRange range, bool autoOperation = true)
    {
      if (range.Lower.HasValue) {
        OperationDetectionNotifier.NotifyOperationDetection (m_machine, range.Lower.Value);
      }
    }

    public void ExtendOperation (IOperation operation, DateTime dateTime)
    {
      OperationDetectionNotifier.NotifyOperationDetection (m_machine, dateTime);
    }

    public bool Initialize (IMonitoredMachine machine)
    {
      log = LogManager.GetLogger (typeof (AfterOperationDetectionExtension).FullName + "." + machine.Id);

      m_machine = machine;

      return LoadConfiguration (out m_configuration);
    }

    public void StartOperation (IOperation operation, DateTime dateTime)
    {
      OperationDetectionNotifier.NotifyOperationDetection (m_machine, dateTime);
    }

    public void StopOperation (DateTime dateTime)
    {
      OperationDetectionNotifier.NotifyOperationDetection (m_machine, dateTime);
    }

    /// <summary>
    /// Add an operation (without calling the extension point afterwards)
    /// 
    /// Note:
    /// - for multi-machine module machines, the call of AddOperation is not always chronological
    /// - because 'Auto-Only' and 'Not auto-only' sequences are processed separately, the call to AddOperation
    ///   could not be chronological
    /// </summary>
    /// <param name="operation">Not null</param>
    /// <param name="range"></param>
    /// <param name="autoOperation"></param>
    public void AddOperationOnly (IOperation operation, UtcDateTimeRange range, bool autoOperation)
    {
      if (range.Lower.HasValue) {
        OperationDetectionNotifier.NotifyOperationDetection (m_machine, range.Lower.Value);
      }
    }
  }
}

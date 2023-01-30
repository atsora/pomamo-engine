// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Model;
using Pulse.Extensions.Database;

namespace Lemoine.Plugin.AnalysisDetectionStatus
{
  public class OperationDetectionStatusExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IOperationDetectionStatusExtension
    , IOperationDetectionListener
  {
    ILog log = LogManager.GetLogger (typeof (OperationDetectionStatusExtension).FullName);

    Configuration m_configuration;
    DateTime? m_dateTime;
    IMachine m_machine;

    #region IOperationDetectionStatusExtension
    public int OperationDetectionStatusPriority
    {
      get
      {
        return m_configuration.OperationDetectionStatusPriority;
      }
    }

    public DateTime? GetOperationDetectionDateTime ()
    {
      return m_dateTime;
    }

    public bool Initialize (IMachine machine)
    {
      Debug.Assert (null != machine);

      log = LogManager.GetLogger (typeof (OperationDetectionStatusExtension).FullName + "." + machine.Id);

      m_machine = machine;

      if (!LoadConfiguration (out m_configuration)) {
        log.ErrorFormat ("Initialize: load configuration was not ok");
        return false;
      }

      OperationDetectionNotifier.AddListener (this);

      return true;
    }
    #endregion // IOperationDetectionStatusExtension

    #region IOperationDetectionListener
    public IMachine Machine { get { return m_machine; } }

    public void NotifyOperationDetection (IMachine machine, DateTime dateTime)
    {
      if (machine.Id != m_machine.Id) {
        return;
      }

      if (!m_dateTime.HasValue || (m_dateTime.Value < dateTime)) {
        m_dateTime = dateTime;
      }
    }
    #endregion // IOperationDetectionListener
  }
}

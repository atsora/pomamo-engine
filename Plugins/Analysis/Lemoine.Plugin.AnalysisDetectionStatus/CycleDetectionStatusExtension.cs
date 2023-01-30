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
  public class CycleDetectionStatusExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , ICycleDetectionStatusExtension
    , ICycleDetectionListener
  {
    ILog log = LogManager.GetLogger (typeof (CycleDetectionStatusExtension).FullName);

    Configuration m_configuration;
    DateTime? m_dateTime;
    IMachine m_machine;

    public IMachine Machine
    {
      get
      {
        return m_machine;
      }
    }

    #region ICycleDetectionStatusExtension
    public int CycleDetectionStatusPriority
    {
      get
      {
        return m_configuration.CycleDetectionStatusPriority;
      }
    }

    public DateTime? GetCycleDetectionDateTime ()
    {
      return m_dateTime;
    }

    public bool Initialize (IMachine machine)
    {
      Debug.Assert (null != machine);

      log = LogManager.GetLogger (typeof (CycleDetectionStatusExtension).FullName + "." + machine.Id);

      m_machine = machine;

      if (!LoadConfiguration (out m_configuration)) {
        log.ErrorFormat ("Initialize: load configuration error");
        return false;
      }

      CycleDetectionNotifier.AddListener (this);

      return true;
    }
    #endregion // ICycleDetectionStatusExtension

    #region ICycleDetectionListener
    public void NotifyCycleDetection (IMachine machine, DateTime dateTime)
    {
      if (machine.Id != m_machine.Id) {
        return;
      }

      if (!m_dateTime.HasValue || (m_dateTime.Value < dateTime)) {
        m_dateTime = dateTime;
      }
    }
    #endregion // ICycleDetectionListener
  }
}

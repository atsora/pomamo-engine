// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Database;
using Lemoine.Model;
using Pulse.Extensions.Database;

namespace Lemoine.Plugin.DetectionStatusFromDatabase
{
  public class CycleDetectionStatusExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , ICycleDetectionStatusExtension
  {
    ILog log = LogManager.GetLogger (typeof (CycleDetectionStatusExtension).FullName);

    Configuration m_configuration;
    IMachine m_machine;

    public int CycleDetectionStatusPriority
    {
      get
      {
        return m_configuration.CycleDetectionStatusPriority;
      }
    }

    public DateTime? GetCycleDetectionDateTime ()
    {
      if (m_configuration.CycleDetectionStatusPriority < 0) {
        return null;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var lastCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
          .GetLast (m_machine);
        if (null == lastCycle) {
          log.ErrorFormat ("GetFromOperationCycles: no cycle");
          return null;
        }
        else { // null != lastCycle
          if (lastCycle.HasRealEnd ()) {
            Debug.Assert (lastCycle.End.HasValue);
            return lastCycle.End.Value;
          }
          else {
            return lastCycle.DateTime;
          }
        }
      }
    }

    public bool Initialize (IMachine machine)
    {
      Debug.Assert (null != machine);

      log = LogManager.GetLogger (typeof (CycleDetectionStatusExtension).FullName + "." + machine.Id);
      m_machine = machine;

      return LoadConfiguration (out m_configuration);
    }
  }
}

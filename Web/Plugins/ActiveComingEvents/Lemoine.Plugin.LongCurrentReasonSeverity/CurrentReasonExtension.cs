// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Reason;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Extensions.Web;
using Pulse.Extensions.Web.Reason;

namespace Lemoine.Plugin.LongCurrentReasonSeverity
{
  public class CurrentReasonExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , ICurrentReasonExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (CurrentReasonExtension).FullName);

    Configuration m_configuration;
    IMonitoredMachine m_machine;

    public EventSeverity GetSeverity (ICurrentReasonData currentReasonData)
    {
      if (!currentReasonData.PeriodStart.HasValue) {
        return null;
      }

      if (currentReasonData.MachineMode?.Running ?? !m_configuration.IncludeUnknown) {
        return null;
      }

      var duration = currentReasonData.CurrentDateTime.Subtract (currentReasonData.PeriodStart.Value);
      if (m_configuration.ErrorDelay.HasValue && (m_configuration.ErrorDelay.Value <= duration)) {
        return new EventSeverity (EventSeverityLevel.Error);
      }
      if (m_configuration.Warning2Delay.HasValue && (m_configuration.Warning2Delay.Value <= duration)) {
        return new EventSeverity (EventSeverityLevel.Warning2);
      }
      return null;
    }

    public bool Initialize (IMonitoredMachine machine)
    {
      m_machine = machine;

      if (!LoadConfiguration (out m_configuration)) {
        log.Warn ("Initialize: the configuration is not valid");
        return false;
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int machineFilterId = m_configuration.MachineFilterId;
        if (0 != machineFilterId) {
          var machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
            .FindById (machineFilterId);
          if (null == machineFilter) {
            log.Error ($"Initialize: machine filter id {machineFilterId} does not exist");
            return false;
          }
          // Note: machineFilter.IsMatch requires it is done in the same session
          if (!machineFilter.IsMatch (machine)) {
            return false;
          }
        }
      }

      return true;
    }
  }
}

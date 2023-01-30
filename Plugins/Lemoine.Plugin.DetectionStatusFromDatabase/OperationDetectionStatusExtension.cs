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
  public class OperationDetectionStatusExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IOperationDetectionStatusExtension
  {
    ILog log = LogManager.GetLogger (typeof (OperationDetectionStatusExtension).FullName);

    Configuration m_configuration;
    IMonitoredMachine m_machine;

    public int OperationDetectionStatusPriority
    {
      get
      {
        return m_configuration.OperationDetectionStatusPriority;
      }
    }

    public DateTime? GetOperationDetectionDateTime ()
    {
      if (m_configuration.OperationDetectionStatusPriority < 0) {
        return null;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Plugin.DetectionStatusFromDatabase.GetOperationDectionDateTime")) {
          var sequenceSlots = m_machine.MachineModules
            .Select (m => GetLastSequenceSlot (m))
            .Where (s => null != s);
          Debug.Assert (sequenceSlots.All (s => null != s));
          Debug.Assert (sequenceSlots.All (s => null != s.Sequence));
          Debug.Assert (sequenceSlots.All (s => s.BeginDateTime.HasValue));
          if (!sequenceSlots.Any ()) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("GetOperationDetectionDateTime: no sequence slot");
            }
            return null;
          }
          else { // Any ()
            return sequenceSlots
              .Min (s => s.BeginDateTime.Value);
          }
        }
      }
    }

    ISequenceSlot GetLastSequenceSlot (IMachineModule machineModule)
    {
      return ModelDAOHelper.DAOFactory.SequenceSlotDAO
        .FindLastWithSequence (machineModule,
        new UtcDateTimeRange (new LowerBound<DateTime> (null)));
    }

    public bool Initialize (IMachine machine)
    {
      Debug.Assert (null != machine);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Plugin.DetectionStatusFromDatabase.Initialize")) {
          m_machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindByIdWithMachineModules (machine.Id);
        }
      }
      if (null == m_machine) {
        log.ErrorFormat ("Initialize: no monitored machine with id {0}", machine.Id);
        return false;
      }

      return LoadConfiguration (out m_configuration);
    }
  }
}

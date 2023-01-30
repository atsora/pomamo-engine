// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Threading;
using Pulse.Extensions.Database;

namespace Lemoine.Plugin.AutoMachineStateTemplateConfig
{
  public class CncActivityExtension
    : Lemoine.Extensions.UniqueInstanceConfigurableExtension<Configuration>
    , ICncActivityExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncActivityExtension).FullName);

    double m_priority;
    IMonitoredMachine m_machine;

    public double Priority => m_priority;

    public bool Initialize (IMonitoredMachine machine)
    {
      m_machine = machine;

      var configurations = LoadConfigurations ();
      if (!configurations.Any ()) {
        log.ErrorFormat ("Initialize: no valid configuration, skip this instance");
        return false;
      }

      m_priority = configurations.Min (c => c.Priority);
      return true;
    }

    public bool ProcessAssociation (IChecked checkedThread, UtcDateTimeRange range, IMachineMode machineMode, IMachineStateTemplate machineStateTemplate, IMachineObservationState machineObservationState, IShift shift, IMachineStatus machineStatus, CancellationToken cancellationToken = default)
    {
      Debug.Assert (range.Lower.HasValue);
      Debug.Assert (range.Upper.HasValue);
      Debug.Assert (null != machineMode);

      if (machineStatus is null) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ProcessAssociation: do nothing if machine status is not set yet");
        }
        return true;
      }

      if (!machineStatus.CncMachineMode.Equals (machineMode)) {
        bool autoMachineStateTemplateApplied =
          CheckAutoMachineStateTemplate (checkedThread, range, machineMode,
                                         machineStateTemplate);
        if (autoMachineStateTemplateApplied) {
          // The observation state slots may have changed
          log.Info ("ProcessAssociation: CheckAutoMachineStateTemplate returned false, the observation state slots must be reloaded");
          return false;
        }
      }


      return true;
    }

    /// <summary>
    /// Check if an auto machine state template must be processed
    /// </summary>
    /// <returns>an auto machine state template was processed, and the observation state slots were updated</returns>
    bool CheckAutoMachineStateTemplate (IChecked checkedThread, UtcDateTimeRange range, IMachineMode newMachineMode,
                                        IMachineStateTemplate currentMachineStateTemplate)
    {
      Debug.Assert (null != newMachineMode);
      IMachineStateTemplate newMachineStateTemplate = null;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ("CncActivityMachineAssociation.CheckAutoMachineStateTemplate")) {
        if (null != currentMachineStateTemplate) {
          // Check first a AutoMachineStateTemplate that applies only for currentMachineStateTemplate
          IAutoMachineStateTemplate automst = ModelDAOHelper.DAOFactory.AutoMachineStateTemplateDAO
            .Find (newMachineMode, currentMachineStateTemplate);
          if (null != automst) {
            Debug.Assert (object.Equals (automst.MachineMode, newMachineMode));
            Debug.Assert (object.Equals (automst.Current, currentMachineStateTemplate));
            newMachineStateTemplate = automst.New;
          }
        }
        checkedThread.SetActive ();
        if (null == newMachineStateTemplate) {
          // If newMachineStateTemplate is not known yet,
          // try with no currentMachineStateTemplate criteria
          IAutoMachineStateTemplate automst = ModelDAOHelper.DAOFactory.AutoMachineStateTemplateDAO
            .Find (newMachineMode);
          if (null != automst) {
            Debug.Assert (object.Equals (automst.MachineMode, newMachineMode));
            Debug.Assert (null == automst.Current);
            newMachineStateTemplate = automst.New;
          }
        }

        if ((null == newMachineStateTemplate)
            || (object.Equals (newMachineStateTemplate, currentMachineStateTemplate))) {
          transaction.Commit ();
          return false;
        }
        else { // null != newMachineStateTemplate
          // Apply the new machine state template
          IMachineStateTemplateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (m_machine, newMachineStateTemplate,
                                                    new UtcDateTimeRange (range.Lower, new UpperBound<DateTime> (null)));
          association.Apply ();
          transaction.Commit ();
          return true;
        }
      }
    }
  }
}

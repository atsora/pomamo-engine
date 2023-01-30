// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Group;
using Lemoine.Extensions.Web;
using Lemoine.ModelDAO;
using Pulse.Extensions.Web;

namespace Lemoine.Plugin.PieOperationProgress
{
  public class PieExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IPieExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (PieExtension).FullName);

    Configuration m_configuration;

    public double Score
    {
      get { return m_configuration.Score; }
    }

    public string PieType
    {
      get { return "operationprogresspie"; }
    }

    public bool Permanent
    {
      get { return true; }
    }

    public bool Initialize (IGroup group)
    {
      if (null == group) {
        log.Fatal ("Initialize: empty group");
        Debug.Assert (false);
        return false;
      }

      if (!LoadConfiguration (out m_configuration)) {
        log.Error ("Initialize: configuration error");
        return false;
      }

      if (!group.SingleMachine) { // Applicable only on single machines
        return false;
      }

      if (0 < m_configuration.MachineFilterId) {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
            .FindById (m_configuration.MachineFilterId);
          if (null == machineFilter) {
            if (log.IsErrorEnabled) {
              log.ErrorFormat ("Initialize: machine filter with id {0} does not exist", m_configuration.MachineFilterId);
            }
            return false;
          }
          var machine = group.GetMachines ().First ();
          if (!machineFilter.IsMatch (machine)) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("Initialize: no match between machine id={0} and machine filter id={1}", machine.Id, machineFilter.Id);
            }
            return false;
          }
        }
      }

      return true;
    }
  }
}

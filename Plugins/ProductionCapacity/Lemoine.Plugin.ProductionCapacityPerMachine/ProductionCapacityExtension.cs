// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Operation;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.ProductionCapacityPerMachine
{
  public class ProductionCapacityExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IProductionCapacityExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (ProductionCapacityExtension).FullName);

    IMachine m_machine;
    Configuration m_configuration;

    public double Score => m_configuration.Score;

    public bool Initialize (IMachine machine)
    {
      m_machine = machine;

      if (!LoadConfiguration (out m_configuration)) {
        log.Warn ("Initialize: the configuration is not valid");
        return false;
      }

      if (!(m_configuration.MachineIds is null) && m_configuration.MachineIds.Any ()) {
        if (!m_configuration.MachineIds.Contains (machine.Id)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Initialize: the machine does not match => return false");
          }
          return false;
        }
      }

      if (0 != m_configuration.MachineFilterId) {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (var transaction = session.BeginReadOnlyTransaction ("Lemoine.Plugin.ProductionCapacityPerMachine.Initialize")) {
          var machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
            .FindById (m_configuration.MachineFilterId);
          if (machineFilter is null) {
            log.Error ($"Initialize: machine filter with id={m_configuration.MachineFilterId} does not exist => return false");
            return false;
          }
          if (!machineFilter.IsMatch (machine)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Initialize: machine filter does not match");
            }
            return false;
          }
        }
      }

      return true;
    }

    public Task<double> GetCapacityPerHourAsync (IIntermediateWorkPiece intermediateWorkPiece)
    {
      var result = m_configuration.CapacityPerHour;
      return Task.FromResult (result);
    }

  }
}

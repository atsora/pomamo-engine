// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Business.Operation;
using Lemoine.Collections;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Operation;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.ProductionCapacityFromStandardCycleDuration
{
  public class ProductionCapacityExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IProductionCapacityExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (ProductionCapacityExtension).FullName);

    IMachine m_machine;

    public double Score => 10.0;

    public bool Initialize (IMachine machine)
    {
      m_machine = machine;
      return !(m_machine is null);
    }

    public async System.Threading.Tasks.Task<double> GetCapacityPerHourAsync (IIntermediateWorkPiece intermediateWorkPiece)
    {
      var monitoredMachineBusinessRequest = new Lemoine.Business.Machine.MonitoredMachineFromId (m_machine.Id);
      var monitoredMachine = await Lemoine.Business.ServiceProvider.GetAsync (monitoredMachineBusinessRequest);
      if (null == monitoredMachine) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetCapacityPerHourAsync: machine {m_machine.Id} is not monitored");
        }
        throw new Exception ("Machine is not monitored");
      }

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var iwp = await ModelDAO.ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO
          .FindByIdAsync (((IDataWithId)intermediateWorkPiece).Id);
        var operation = iwp.Operation;
        var quantity = iwp.OperationQuantity;
        var standardCycleDurationRequest = new StandardCycleDuration (monitoredMachine, operation);
        var standardCycleDuration = await Lemoine.Business.ServiceProvider
          .GetAsync (standardCycleDurationRequest);
        if (standardCycleDuration.HasValue) {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetCapacityPerHourAsync: for intermediateWorkPiece={((IDataWithId)iwp).Id} quantity={quantity} standardCycleDuration={standardCycleDuration}");
          }
          return quantity / standardCycleDuration.Value.TotalHours;
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetCapacityPerHourAsync: no standard cycle duration for intermediate work piece={((IDataWithId)iwp).Id}");
          }
          throw new Exception ($"No standard cycle duration");
        }
      }
    }
  }
}

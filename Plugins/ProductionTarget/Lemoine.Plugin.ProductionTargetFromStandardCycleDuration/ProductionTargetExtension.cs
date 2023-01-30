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

namespace Lemoine.Plugin.ProductionTargetFromStandardCycleDuration
{
  public class ProductionTargetExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IProductionTargetExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (ProductionTargetExtension).FullName);

    IMachine m_machine;

    public double Score => 10.0;

    public bool Initialize (IMachine machine)
    {
      m_machine = machine;
      return !(m_machine is null);
    }

    public double GetTargetPerHour (IIntermediateWorkPiece intermediateWorkPiece)
    {
      Debug.Assert (null != m_machine);

      var monitoredMachineBusinessRequest = new Lemoine.Business.Machine.MonitoredMachineFromId (m_machine.Id);
      var monitoredMachine = Lemoine.Business.ServiceProvider.Get (monitoredMachineBusinessRequest);
      if (null == monitoredMachine) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetTargetPerHour: machine {m_machine.Id} is not monitored");
        }
        throw new Exception ("Machine is not monitored");
      }

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var iwp = ModelDAO.ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO
          .FindById (((IDataWithId)intermediateWorkPiece).Id);
        var operation = iwp.Operation;
        var quantity = iwp.OperationQuantity;
        var standardCycleDurationRequest = new StandardCycleDuration (monitoredMachine, operation);
        var standardCycleDuration = Lemoine.Business.ServiceProvider
          .Get (standardCycleDurationRequest);
        if (standardCycleDuration.HasValue) {
          var partProductionTargetFactorRequest = new PartProductionTargetFactor (m_machine);
          var partProductionTargetFactor = Lemoine.Business.ServiceProvider
            .Get (partProductionTargetFactorRequest);
          if (log.IsDebugEnabled) {
            log.Debug ($"GetTargetPerHour: for intermediateWorkPiece={((IDataWithId)iwp).Id} quantity={quantity} factor={partProductionTargetFactor} standardCycleDuration={standardCycleDuration}");
          }
          return partProductionTargetFactor * quantity / standardCycleDuration.Value.TotalHours;
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetTargetPerHour: no standard cycle duration for intermediate work piece={((IDataWithId)iwp).Id}");
          }
          throw new Exception ($"No standard cycle duration");
        }
      }
    }

    public async System.Threading.Tasks.Task<double> GetTargetPerHourAsync (IIntermediateWorkPiece intermediateWorkPiece)
    {
      Debug.Assert (null != m_machine);

      var monitoredMachineBusinessRequest = new Lemoine.Business.Machine.MonitoredMachineFromId (m_machine.Id);
      var monitoredMachine = await Lemoine.Business.ServiceProvider.GetAsync (monitoredMachineBusinessRequest);
      if (null == monitoredMachine) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetTargetPerHourAsync: machine {m_machine.Id} is not monitored");
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
          var partProductionTargetFactorRequest = new PartProductionTargetFactor (m_machine);
          var partProductionTargetFactor = await Lemoine.Business.ServiceProvider
            .GetAsync (partProductionTargetFactorRequest);
          if (log.IsDebugEnabled) {
            log.Debug ($"GetTargetPerHourAsync: for intermediateWorkPiece={((IDataWithId)iwp).Id} quantity={quantity} factor={partProductionTargetFactor} standardCycleDuration={standardCycleDuration}");
          }
          return partProductionTargetFactor * quantity / standardCycleDuration.Value.TotalHours;
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetTargetPerHourAsync: no standard cycle duration for intermediate work piece={((IDataWithId)iwp).Id}");
          }
          throw new Exception ($"No standard cycle duration");
        }
      }
    }
  }
}

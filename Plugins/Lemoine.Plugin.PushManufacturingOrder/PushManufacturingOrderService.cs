// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Services;
using Lemoine.Collections;

namespace Lemoine.Plugin.PushManufacturingOrder
{
  class PushManufacturingOrderService : GenericSyncSaveService<PushManufacturingOrderRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PushManufacturingOrderService).FullName);

    public override object GetSync (PushManufacturingOrderRequestDTO request)
    {
      IRevision revision;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("Web.Addon.PushManufacturingOrder")) {
          // - Machine
          int machineId = request.MachineId;
          IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
            .FindById (machineId);
          if (null == machine) {
            log.Error ($"GetWithoutCache: unknown machine with ID {machineId}");
            transaction.Commit ();
            return new ErrorDTO ("No monitored machine with the specified ID",
                                 ErrorStatus.WrongRequestParameter);
          }

          // - Range
          UtcDateTimeRange range = new UtcDateTimeRange (DateTime.UtcNow, new Model.UpperBound<DateTime> (null));
          if (!string.IsNullOrEmpty (request.Range)) {
            range = new UtcDateTimeRange (request.Range);
          }

          revision = ModelDAOHelper.ModelFactory.CreateRevision ();
          revision.Application = "AspService";
          revision.IPAddress = GetRequestRemoteIp ();
          ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent (revision);

          // - WorkOrder
          IWorkOrder workOrder = null;
          if (!string.IsNullOrEmpty (request.WorkorderName)) {
            workOrder = ModelDAOHelper.DAOFactory.WorkOrderDAO
              .FindByCode (request.WorkorderName);
            if (null == workOrder) { // Create one
              IWorkOrderStatus status = ModelDAOHelper.DAOFactory.WorkOrderStatusDAO.FindById (1);
              workOrder = ModelDAOHelper.ModelFactory.CreateWorkOrder (status, request.WorkorderName);
              ModelDAOHelper.DAOFactory.WorkOrderDAO.MakePersistent (workOrder);
            }
            var workOrderAssociation = ModelDAOHelper.ModelFactory.CreateWorkOrderMachineAssociation (machine, workOrder, range);
            workOrderAssociation.ResetManufacturingOrder = true;
            workOrderAssociation.Revision = revision;
            ModelDAOHelper.DAOFactory.WorkOrderMachineAssociationDAO.MakePersistent (workOrderAssociation);
          }

          // - Manufacturing order
          IManufacturingOrder manufacturingOrder = null;
          if (0 < request.ManufacturingOrderId) { // Normally always
            manufacturingOrder = ModelDAOHelper.DAOFactory.ManufacturingOrderDAO.FindById (request.ManufacturingOrderId);
            if (null == manufacturingOrder) { // Create it
              if (request.CreateManufacturingOrder) {
                manufacturingOrder = ModelDAOHelper.ModelFactory.CreateManufacturingOrder (request.ManufacturingOrderId);
                manufacturingOrder.Machine = machine;
                manufacturingOrder.WorkOrder = workOrder;
                ModelDAOHelper.DAOFactory.ManufacturingOrderDAO.MakePersistent (manufacturingOrder);
              }
              else {
                log.Error ($"PushManufacturingOrderService: manufacturing order id {request.ManufacturingOrderId} does not exist and is not required to be created");
                transaction.Rollback ();
                return new ErrorDTO ("Missing manufacturing order", ErrorStatus.UnexpectedError);
              }
            }
            else { // the manufacturing order already exists
                   // Check the existing values of manufacturing order
              if ((null != manufacturingOrder.Machine) && (manufacturingOrder.Machine.Id != machine.Id)) { // Check machine
                log.Error ($"PushManufacturingOrderService: manufacturing order machine id {manufacturingOrder.Machine.Id} differs from machine id {machine.Id}");
                transaction.Rollback ();
                return new ErrorDTO ("Wrong manufacturing order machine", ErrorStatus.UnexpectedError);
              }
              if ((null != workOrder) && (null != manufacturingOrder.WorkOrder)
                && ((Lemoine.Collections.IDataWithId<int>)manufacturingOrder.WorkOrder).Id != ((Lemoine.Collections.IDataWithId<int>)workOrder).Id) {
                log.Error ($"PushManufacturingOrderService: manufacturing order work order id {((Lemoine.Collections.IDataWithId<int>)manufacturingOrder.WorkOrder).Id} differs from work order id {((Lemoine.Collections.IDataWithId<int>)workOrder).Id}");
                transaction.Rollback ();
                return new ErrorDTO ("Wrong manufacturing order work order", ErrorStatus.UnexpectedError);
              }
              if (!string.IsNullOrEmpty (request.ManufacturingOrderExternalCode) && !string.IsNullOrEmpty (manufacturingOrder.ExternalCode)
                && !string.Equals (request.ManufacturingOrderExternalCode, manufacturingOrder.ExternalCode, StringComparison.InvariantCultureIgnoreCase)) {
                log.Error ($"PushManufacturingOrderService: manufacturing order external code {manufacturingOrder.ExternalCode} differs from request manufacturing order extenal code {request.ManufacturingOrderExternalCode}");
                transaction.Rollback ();
                return new ErrorDTO ("Wrong manufacturing order external code", ErrorStatus.UnexpectedError);
              }

              // Update manufacturing order ?
              if (request.UpdateManufacturingOrder) {
                if (null == manufacturingOrder.Machine) {
                  manufacturingOrder.Machine = machine;
                  if (log.IsDebugEnabled) {
                    log.Debug ($"PushManufacturingOrderService: set the machine in manufacturing order id {((IDataWithId<int>)manufacturingOrder).Id}");
                  }
                  ModelDAOHelper.DAOFactory.ManufacturingOrderDAO.MakePersistent (manufacturingOrder);
                }
                if ((null != workOrder) && (null == manufacturingOrder.WorkOrder)) {
                  manufacturingOrder.WorkOrder = workOrder;
                  if (log.IsDebugEnabled) {
                    log.Debug ($"PushManufacturingOrderService: set the work order in manufacturing order id {((IDataWithId<int>)manufacturingOrder).Id}");
                  }
                  ModelDAOHelper.DAOFactory.ManufacturingOrderDAO.MakePersistent (manufacturingOrder);
                }
                if (!string.IsNullOrEmpty (request.ManufacturingOrderExternalCode) && string.IsNullOrEmpty (manufacturingOrder.ExternalCode)) {
                  manufacturingOrder.ExternalCode = request.ManufacturingOrderExternalCode;
                  if (log.IsDebugEnabled) {
                    log.Debug ($"PushManufacturingOrderService: set the external code {request.ManufacturingOrderExternalCode} in manufacturing order id {((IDataWithId<int>)manufacturingOrder).Id}");
                  }
                  ModelDAOHelper.DAOFactory.ManufacturingOrderDAO.MakePersistent (manufacturingOrder);
                }
                if (0 < request.ManufacturingOrderQuantity) {
                  manufacturingOrder.Quantity = request.ManufacturingOrderQuantity;
                  if (log.IsDebugEnabled) {
                    log.Debug ($"PushManufacturingOrderService: set the quantity {request.ManufacturingOrderQuantity} in manufacturing order id {((IDataWithId<int>)manufacturingOrder).Id}");
                  }
                  ModelDAOHelper.DAOFactory.ManufacturingOrderDAO.MakePersistent (manufacturingOrder);
                }
              }
            }
          }

          // - Manufacturing order machine association
          {
            var association = ModelDAOHelper.ModelFactory.CreateManufacturingOrderMachineAssociation (machine, manufacturingOrder, range);
            association.Revision = revision;
            ModelDAOHelper.DAOFactory.ManufacturingOrderMachineAssociationDAO.MakePersistent (association);
          }

          transaction.Commit ();
        } // transaction
      } // session

      return new PushManufacturingOrderResponseDTO (revision);
    }

  }
}

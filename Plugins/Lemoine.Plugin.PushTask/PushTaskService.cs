// Copyright (C) 2009-2023 Lemoine Automation Technologies
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

namespace Lemoine.Plugin.PushTask
{
  class PushTaskService : GenericSyncSaveService<PushTaskRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PushTaskService).FullName);

    public override object GetSync (PushTaskRequestDTO request)
    {
      IRevision revision;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("Web.Addon.PushTask")) {
          // - Machine
          int machineId = request.MachineId;
          IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
            .FindById (machineId);
          if (null == machine) {
            log.ErrorFormat ("GetWithoutCache: " +
                             "unknown machine with ID {0}",
                             machineId);
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
          revision.Application = "Lem_AspService";
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
            workOrderAssociation.ResetTask = true;
            workOrderAssociation.Revision = revision;
            ModelDAOHelper.DAOFactory.WorkOrderMachineAssociationDAO.MakePersistent (workOrderAssociation);
          }

          // - Task
          ITask task = null;
          if (0 < request.TaskId) { // Normally always
            task = ModelDAOHelper.DAOFactory.TaskDAO.FindById (request.TaskId);
            if (null == task) { // Create it
              if (request.CreateTask) {
                task = ModelDAOHelper.ModelFactory.CreateTask (request.TaskId);
                task.Machine = machine;
                task.WorkOrder = workOrder;
                ModelDAOHelper.DAOFactory.TaskDAO.MakePersistent (task);
              }
              else {
                log.ErrorFormat ("PushTaskService: task id {0} does not exist and is not required to be created",
                  request.TaskId);
                transaction.Rollback ();
                return new ErrorDTO ("Missing task", ErrorStatus.UnexpectedError);
              }
            }
            else { // the task already exists
                   // Check the existing values of task
              if ((null != task.Machine) && (task.Machine.Id != machine.Id)) { // Check machine
                log.ErrorFormat ("PushTaskService: task machine id {0} differs from machine id {1}",
                  task.Machine.Id, machine.Id);
                transaction.Rollback ();
                return new ErrorDTO ("Wrong task machine", ErrorStatus.UnexpectedError);
              }
              if ((null != workOrder) && (null != task.WorkOrder)
                && ((Lemoine.Collections.IDataWithId<int>)task.WorkOrder).Id != ((Lemoine.Collections.IDataWithId<int>)workOrder).Id) {
                log.ErrorFormat ("PushTaskService: task work order id {0} differs from work order id {1}",
                  ((Lemoine.Collections.IDataWithId<int>)task.WorkOrder).Id, ((Lemoine.Collections.IDataWithId<int>)workOrder).Id);
                transaction.Rollback ();
                return new ErrorDTO ("Wrong task work order", ErrorStatus.UnexpectedError);
              }
              if (!string.IsNullOrEmpty (request.TaskExternalCode) && !string.IsNullOrEmpty (task.ExternalCode)
                && !string.Equals (request.TaskExternalCode, task.ExternalCode, StringComparison.InvariantCultureIgnoreCase)) {
                log.ErrorFormat ("PushTaskService: task external code {0} differs from request task extenal code {1}",
                  task.ExternalCode, request.TaskExternalCode);
                transaction.Rollback ();
                return new ErrorDTO ("Wrong task external code", ErrorStatus.UnexpectedError);
              }

              // Update task ?
              if (request.UpdateTask) {
                if (null == task.Machine) {
                  task.Machine = machine;
                  log.DebugFormat ("PushTaskService: set the machine in task id {0}",
                    ((IDataWithId<int>)task).Id);
                  ModelDAOHelper.DAOFactory.TaskDAO.MakePersistent (task);
                }
                if ((null != workOrder) && (null == task.WorkOrder)) {
                  task.WorkOrder = workOrder;
                  log.DebugFormat ("PushTaskService: set the work order in task id {0}",
                    ((IDataWithId<int>)task).Id);
                  ModelDAOHelper.DAOFactory.TaskDAO.MakePersistent (task);
                }
                if (!string.IsNullOrEmpty (request.TaskExternalCode) && string.IsNullOrEmpty (task.ExternalCode)) {
                  task.ExternalCode = request.TaskExternalCode;
                  log.DebugFormat ("PushTaskService: set the external code {1} in task id {0}",
                    ((IDataWithId<int>)task).Id, request.TaskExternalCode);
                  ModelDAOHelper.DAOFactory.TaskDAO.MakePersistent (task);
                }
                if (0 < request.TaskQuantity) {
                  task.Quantity = request.TaskQuantity;
                  log.DebugFormat ("PushTaskService: set the quantity {1} in task id {0}",
                    ((IDataWithId<int>)task).Id, request.TaskQuantity);
                  ModelDAOHelper.DAOFactory.TaskDAO.MakePersistent (task);
                }
              }
            }
          }

          // - Task machine association
          {
            var association = ModelDAOHelper.ModelFactory.CreateTaskMachineAssociation (machine, task, range);
            association.Revision = revision;
            ModelDAOHelper.DAOFactory.TaskMachineAssociationDAO.MakePersistent (association);
          }

          transaction.Commit ();
        } // transaction
      } // session

      return new PushTaskResponseDTO (revision);
    }

  }
}

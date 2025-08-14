// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;
using Pulse.Web.WebDataAccess.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// WorkOrderMachineAssociationSave Service.
  /// </summary>
  public class WorkOrderMachineAssociationSaveService : GenericSaveService<Pulse.Web.WebDataAccess.WorkOrderMachineAssociationSave>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (WorkOrderMachineAssociationSaveService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public WorkOrderMachineAssociationSaveService ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetSync(Pulse.Web.WebDataAccess.WorkOrderMachineAssociationSave request)
    {
      IWorkOrderMachineAssociation workOrderMachineAssociation;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginTransaction ("WorkOrderMachineAssociationSaveService"))
        {
          IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
            .FindById (request.MachineId);
          if (null == machine) {
            log.ErrorFormat ("Get: " +
                             "Machine with {0} does not exist",
                             request.MachineId);
            transaction.Commit ();
            return new ErrorDTO ("No machine with id " + request.MachineId,
                                 ErrorStatus.WrongRequestParameter);
          }
          
          UtcDateTimeRange range = new UtcDateTimeRange (request.Range);
          
          if (request.WorkOrderId.HasValue) {
            IWorkOrder workOrder = ModelDAOHelper.DAOFactory.WorkOrderDAO
              .FindById (request.WorkOrderId.Value);
            if (null == workOrder) {
              log.ErrorFormat ("Get: " +
                               "No reason with ID {0}",
                               request.WorkOrderId.Value);
              transaction.Commit ();
              return new ErrorDTO ("No work order with id " + request.WorkOrderId.Value,
                                   ErrorStatus.WrongRequestParameter);
            }
            else {
              workOrderMachineAssociation = ModelDAOHelper.ModelFactory
                .CreateWorkOrderMachineAssociation (machine, workOrder, range);
            }
          }
          else {
            workOrderMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateWorkOrderMachineAssociation (machine, null, range);
          }
          if (request.ResetManufacturingOrder.HasValue) {
            workOrderMachineAssociation.ResetManufacturingOrder = true;
          }
          if (request.RevisionId.HasValue) {
            if (-1 == request.RevisionId.Value) { // auto-revision
              IRevision revision = ModelDAOHelper.ModelFactory.CreateRevision ();
              revision.Application = "Lem_AspService";
              revision.IPAddress = GetRequestRemoteIp ();
              ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent (revision);
              workOrderMachineAssociation.Revision = revision;
            }
            else {
              IRevision revision = ModelDAOHelper.DAOFactory.RevisionDAO
                .FindById (request.RevisionId.Value);
              if (null == revision) {
                log.WarnFormat ("Get: " +
                                "No revision with ID {0}",
                                request.RevisionId.Value);
              }
              else {
                workOrderMachineAssociation.Revision = revision;
              }
            }
          }
          
          ModelDAOHelper.DAOFactory.WorkOrderMachineAssociationDAO
            .MakePersistent (workOrderMachineAssociation);
          
          transaction.Commit ();
        }
      }
      
      Debug.Assert (null != workOrderMachineAssociation);
      return new SaveModificationResponseDTO (workOrderMachineAssociation);
    }
    #endregion // Methods
  }
}

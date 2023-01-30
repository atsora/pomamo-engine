// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
#endif // NSERVICEKIT

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// Response DTO for WorkOrder
  /// </summary>
  [Api ("WorkOrder Response DTO")]
  public class WorkOrderDTO
  {
    /// <summary>
    /// Id of machine
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display of machine
    /// </summary>
    public string Display { get; set; }

    /// <summary>
    /// Documentation path
    /// </summary>
    public string DocumentLink { get; set; }
  }

  /// <summary>
  /// Assembler for ComponentDTO
  /// </summary>
  public class WorkOrderDTOAssembler : IGenericDTOAssembler<WorkOrderDTO, Lemoine.Model.IWorkOrder>
  {
    readonly ILog log = LogManager.GetLogger<WorkOrderDTOAssembler> ();

    /// <summary>
    /// ComponentDTO assembler
    /// </summary>
    /// <param name="workOrder">nullable</param>
    /// <returns></returns>
    public WorkOrderDTO Assemble (Lemoine.Model.IWorkOrder workOrder)
    {
      if (null == workOrder) {
        return null;
      }
      if (!Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (workOrder)) {
        using (var session = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("Web.WorkOrderDTO.Assemble")) {
            var initializedWorkOrder = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.WorkOrderDAO
              .FindById (((Lemoine.Collections.IDataWithId<int>)workOrder).Id);
            if (null == initializedWorkOrder) {
              log.Error ($"Assemble: work order with id {((Lemoine.Collections.IDataWithId<int>)workOrder).Id} does not exist");
              return null;
            }
            else {
              return Assemble (initializedWorkOrder);
            }
          }
        }
      }
      WorkOrderDTO workOrderDto = new WorkOrderDTO ();
      workOrderDto.Id = ((Lemoine.Collections.IDataWithId<int>)workOrder).Id;
      workOrderDto.Display = workOrder.Display;
      workOrderDto.DocumentLink = workOrder.DocumentLink;
      return workOrderDto;
    }

    /// <summary>
    /// WorkOrderDTO list assembler (default display)
    /// </summary>
    /// <param name="workOrders"></param>
    /// <returns></returns>
    public IEnumerable<WorkOrderDTO> Assemble (IEnumerable<Lemoine.Model.IWorkOrder> workOrders)
    {
      Debug.Assert (null != workOrders);

      IList<WorkOrderDTO> result = new List<WorkOrderDTO> ();
      foreach (var workOrder in workOrders) {
        result.Add (Assemble (workOrder));
      }
      return result;
    }
  }
}

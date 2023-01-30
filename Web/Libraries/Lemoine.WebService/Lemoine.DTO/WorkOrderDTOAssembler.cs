// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for WorkOrderDTO
  /// </summary>
  public class WorkOrderDTOAssembler: IGenericDTOAssembler<WorkOrderDTO, IWorkOrder>
  {
    /// <summary>
    /// WorkOrderDTO assembler
    /// </summary>
    /// <param name="workorder"></param>
    /// <returns></returns>
    public WorkOrderDTO Assemble(Lemoine.Model.IWorkOrder workorder)
    {
      WorkOrderDTO dto = new WorkOrderDTO();
      dto.Id = ((Lemoine.Collections.IDataWithId)workorder).Id;
      dto.Name = workorder.Display;
      dto.DeliveryDate = workorder.DeliveryDate;
      dto.StatusId = workorder.Status.Id;
      return dto;
    }
    
    /// <summary>
    /// WorkOrderDTO list assembler
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public IEnumerable<WorkOrderDTO> Assemble(IEnumerable<Lemoine.Model.IWorkOrder> list)
    {
      IList<WorkOrderDTO> dtoList = new List<WorkOrderDTO>();
      foreach (Lemoine.Model.IWorkOrder item in list) {
        dtoList.Add(Assemble(item));
      }
      return dtoList;
    }
  }
}

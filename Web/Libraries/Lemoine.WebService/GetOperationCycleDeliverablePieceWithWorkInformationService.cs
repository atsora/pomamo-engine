// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Collections;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.DTO;

namespace Lemoine.WebService
{
  /// <summary>
  /// Xxx service
  /// </summary>
  public class GetOperationCycleDeliverablePieceWithWorkInformationService: GenericCachedService<Lemoine.DTO.GetOperationCycleDeliverablePieceWithWorkInformation>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GetOperationCycleDeliverablePieceWithWorkInformationService).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public GetOperationCycleDeliverablePieceWithWorkInformationService () : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (Lemoine.DTO.GetOperationCycleDeliverablePieceWithWorkInformation request)
    {
      int? departmentId = request.DepartmentId;
      int? machineId = request.MachineId;
      DateTime?  rangeEnd = (null != request.End)?Lemoine.DTO.ConvertDTO.IsoStringToDateTimeUtc(request.End):(DateTime?)null;
      DateTime?  rangeBegin = (null != request.Begin)?Lemoine.DTO.ConvertDTO.IsoStringToDateTimeUtc(request.Begin):(DateTime?)null;
      
      if(!rangeBegin.HasValue){
        // use current day BEGIN if Begin parameter of request is null
        rangeBegin = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetTodayBeginUtcDateTime();
      }

      if ( (rangeEnd.HasValue) && (rangeBegin > rangeEnd) ) {
        return ServiceHelper.BadDateTimeRange(rangeBegin.Value, rangeEnd.Value);
      }

      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IList<IMonitoredMachine> machines ;
        if(null == machineId){
          if(null == departmentId){
            machines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindAll();
          }
          else {
            machines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindAllByDepartment(departmentId.Value);
          }
        }
        else {
          machines = new List<IMonitoredMachine>();
          machines.Add(ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById(machineId.Value));
        }
                
        IList<IOperationCycleDeliverablePiece> operationCycleDeliverablePieceList = new List<IOperationCycleDeliverablePiece>();
        
        foreach(IMonitoredMachine machine in machines){
            IList<IOperationCycleDeliverablePiece> list = ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.FindAllInRangeByMachine(machine, rangeBegin, rangeEnd);

          if(null != list){
            foreach (IOperationCycleDeliverablePiece ocdp in list) {
              operationCycleDeliverablePieceList.Add(ocdp);
            }
          }          
        }
        
        //retrieve all Components and all workorders concerned with operationcycledeliverablepiece
        HashSet<IComponent> componentSet = new HashSet<IComponent>();
        HashSet<IWorkOrder> workOrderSet = new HashSet<IWorkOrder>();
        HashSet<IOperation> operationSet = new HashSet<IOperation>();
        
        foreach (IOperationCycleDeliverablePiece ocdp in operationCycleDeliverablePieceList) {
          if(!componentSet.Contains(ocdp.DeliverablePiece.Component)){
            componentSet.Add(ocdp.DeliverablePiece.Component);
          }
          
          if( (null != ocdp.OperationCycle.OperationSlot.WorkOrder) && (!workOrderSet.Contains(ocdp.OperationCycle.OperationSlot.WorkOrder))) {
            workOrderSet.Add(ocdp.OperationCycle.OperationSlot.WorkOrder);
          }

          if((null != ocdp.OperationCycle.OperationSlot.Operation) && (!operationSet.Contains(ocdp.OperationCycle.OperationSlot.Operation))){
            operationSet.Add(ocdp.OperationCycle.OperationSlot.Operation);
          }

        }

        //build response dto
        OperationCycleDeliverablePieceWithWorkInformationDTO dto = new OperationCycleDeliverablePieceWithWorkInformationDTO();        
        dto.Components = (new ComponentDTOAssembler()).Assemble(componentSet.ToList()).ToList();
        dto.WorkOrders = (new WorkOrderDTOAssembler()).Assemble(workOrderSet.ToList()).ToList();
        dto.Operations = (new OperationDTOAssembler()).Assemble(operationSet.ToList()).ToList();
        dto.OperationCycleDeliverablePieces = (new OperationCycleDeliverablePieceDTOAssembler()).Assemble(operationCycleDeliverablePieceList.ToList()).ToList();
        
        return dto;
      }
    }
  }
}

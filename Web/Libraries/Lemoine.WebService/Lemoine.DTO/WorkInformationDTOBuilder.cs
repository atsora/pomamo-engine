// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Collections.Generic;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using Lemoine.Model;

namespace Lemoine.DTO
{
  /// <summary>
  /// Factory for WorkInformationDTO
  /// </summary>
  public class WorkInformationDTOBuilder
  {
    /// <summary>
    /// No work order workpiece information
    /// </summary>
    /// <returns></returns>
    public static WorkInformationDTO BuildNoWorkOrder() {
      WorkInformationDTO wpInfoDTO = new WorkInformationDTO();
      wpInfoDTO.Kind = WorkInformationKind.WorkOrder;
      wpInfoDTO.Value = null;
      return wpInfoDTO;
    }

    /// <summary>
    /// No job workpiece information
    /// </summary>
    /// <returns></returns>
    public static WorkInformationDTO BuildNoJob() {
      WorkInformationDTO wpInfoDTO = new WorkInformationDTO();
      wpInfoDTO.Kind = WorkInformationKind.Job;
      wpInfoDTO.Value = null;
      return wpInfoDTO;
    }
    
    /// <summary>
    /// No part workpiece information
    /// </summary>
    /// <returns></returns>
    public static WorkInformationDTO BuildNoPart() {
      WorkInformationDTO wpInfoDTO = new WorkInformationDTO();
      wpInfoDTO.Kind = WorkInformationKind.Part;
      wpInfoDTO.Value = null;
      return wpInfoDTO;
    }

    /// <summary>
    /// No project workpiece information
    /// </summary>
    /// <returns></returns>
    public static WorkInformationDTO BuildNoProject() {
      WorkInformationDTO wpInfoDTO = new WorkInformationDTO();
      wpInfoDTO.Kind = WorkInformationKind.Project;
      wpInfoDTO.Value = null;
      return wpInfoDTO;
    }

    /// <summary>
    /// No component workpiece information
    /// </summary>
    /// <returns></returns>
    public static WorkInformationDTO BuildNoComponent() {
      WorkInformationDTO wpInfoDTO = new WorkInformationDTO();
      wpInfoDTO.Kind = WorkInformationKind.Component;
      wpInfoDTO.Value = null;
      return wpInfoDTO;
    }
    
    /// <summary>
    /// No operation workpiece information
    /// </summary>
    /// <returns></returns>
    public static WorkInformationDTO BuildNoOperation() {
      WorkInformationDTO wpInfoDTO = new WorkInformationDTO();
      wpInfoDTO.Kind = WorkInformationKind.Operation;
      wpInfoDTO.Value = null;
      return wpInfoDTO;
    }
    
    /// <summary>
    /// No information workpiece information
    /// </summary>
    /// <returns></returns>
    public static List<WorkInformationDTO> BuildNothing()
    {
      var wpInfoDTOList = new List<WorkInformationDTO>();
      
      if (Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderProjectIsJob))) {
        wpInfoDTOList.Add(BuildNoJob());
      } else {
        wpInfoDTOList.Add(BuildNoWorkOrder());
      }
      
      if (Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ProjectComponentIsPart))) {
        wpInfoDTOList.Add(BuildNoPart());
      } else {
        wpInfoDTOList.Add(BuildNoComponent());
      }
      
      if ((!Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderProjectIsJob)))
          && (!Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ProjectComponentIsPart))))
      {
        wpInfoDTOList.Add(BuildNoProject());
      }
      
      wpInfoDTOList.Add(BuildNoOperation());
      
      return wpInfoDTOList;
    }
    
    /// <summary>
    /// WorkOrder workpiece information
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    public static WorkInformationDTO BuildWorkOrder(Lemoine.Model.IWorkOrder workOrder) {
      WorkInformationDTO wpInfoDTO = new WorkInformationDTO();
      wpInfoDTO.Kind = WorkInformationKind.WorkOrder;
      wpInfoDTO.Value = workOrder.Display;
      return wpInfoDTO;
    }

    /// <summary>
    /// WorkOrder workpiece information
    /// </summary>
    /// <param name="part"></param>
    /// <returns></returns>
    public static WorkInformationDTO BuildPart(Lemoine.Model.IPart part) {
      WorkInformationDTO wpInfoDTO = new WorkInformationDTO();
      wpInfoDTO.Kind = WorkInformationKind.Part;
      wpInfoDTO.Value = part.Display;
      return wpInfoDTO;
    }

    /// <summary>
    /// Component workpiece information
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public static WorkInformationDTO BuildComponent(Lemoine.Model.IComponent component) {
      WorkInformationDTO wpInfoDTO = new WorkInformationDTO();
      wpInfoDTO.Kind = WorkInformationKind.Component;
      wpInfoDTO.Value = component.Display;
      return wpInfoDTO;
    }
    
    /// <summary>
    /// Operation workpiece information
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public static WorkInformationDTO BuildOperation(Lemoine.Model.IOperation operation) {
      WorkInformationDTO wpInfoDTO = new WorkInformationDTO();
      wpInfoDTO.Kind = WorkInformationKind.Operation;
      wpInfoDTO.Value = operation.Display;
      return wpInfoDTO;
    }

    /// <summary>
    /// Job workpiece information
    /// </summary>
    /// <param name="job"></param>
    /// <returns></returns>
    public static WorkInformationDTO BuildJob(Lemoine.Model.IJob job) {
      WorkInformationDTO wpInfoDTO = new WorkInformationDTO();
      wpInfoDTO.Kind = WorkInformationKind.Job;
      wpInfoDTO.Value = job.Display;
      return wpInfoDTO;
    }

    /// <summary>
    /// Project workpiece information
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public static WorkInformationDTO BuildProject(Lemoine.Model.IProject project) {
      WorkInformationDTO wpInfoDTO = new WorkInformationDTO();
      wpInfoDTO.Kind = WorkInformationKind.Project;
      wpInfoDTO.Value = project.Display;
      return wpInfoDTO;
    }
    
    /// <summary>
    /// Workpiece informations from a slot
    /// </summary>
    /// <param name="operationSlot"></param>
    /// <returns></returns>
    public static List<WorkInformationDTO> BuildFromOperationSlot(Lemoine.Model.IOperationSlot operationSlot)
    {
      if (operationSlot == null) {
        return WorkInformationDTOBuilder.BuildNothing();
      }
      else {
        List<WorkInformationDTO> workPieceInformationList = new List<WorkInformationDTO>();
        
        if (operationSlot.WorkOrder == null) {
          if (Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderProjectIsJob))) {
            workPieceInformationList.Add(WorkInformationDTOBuilder.BuildNoJob());
          } else {
            workPieceInformationList.Add(WorkInformationDTOBuilder.BuildNoWorkOrder());
          }
        } else {
          if ((Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderProjectIsJob)))
              && (operationSlot.WorkOrder.Job != null))
          {
            workPieceInformationList.Add(WorkInformationDTOBuilder.BuildJob(operationSlot.WorkOrder.Job));
          } else {
            workPieceInformationList.Add(WorkInformationDTOBuilder.BuildWorkOrder(operationSlot.WorkOrder));
          }
        }
        
        if (operationSlot.Component == null) {
          if (Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ProjectComponentIsPart))) {
            workPieceInformationList.Add(WorkInformationDTOBuilder.BuildNoPart());
          } else {
            workPieceInformationList.Add(WorkInformationDTOBuilder.BuildNoComponent());
          }
        } else {
          if ((Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ProjectComponentIsPart))) &&
              (operationSlot.Component.Part != null))
          {
            workPieceInformationList.Add(WorkInformationDTOBuilder.BuildPart(operationSlot.Component.Part));
          } else {
            workPieceInformationList.Add(WorkInformationDTOBuilder.BuildComponent(operationSlot.Component));
          }
        }
        
        if (operationSlot.Operation == null) {
          workPieceInformationList.Add(WorkInformationDTOBuilder.BuildNoOperation());
        } else {
          workPieceInformationList.Add(WorkInformationDTOBuilder.BuildOperation(operationSlot.Operation));
        }
        
        if (   (!Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderProjectIsJob)))
            && (!Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ProjectComponentIsPart)))) {
          
          // there are projects
          Lemoine.Model.IProject project = null;
          
          if (operationSlot.Component != null) {
            project = operationSlot.Component.Project;
          }
          if (project != null) {
            workPieceInformationList.Add(WorkInformationDTOBuilder.BuildProject(project));
          } else {
            workPieceInformationList.Add(WorkInformationDTOBuilder.BuildNoProject());
          }
        }
        
        return workPieceInformationList;
      }
    }
  }
}

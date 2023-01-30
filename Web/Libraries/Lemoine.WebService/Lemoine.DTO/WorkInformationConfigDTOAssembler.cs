// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using Lemoine.Model;

namespace Lemoine.DTO
{
  /// <summary>
  /// Description of WorkInformationConfigDTOAssembler.
  /// </summary>
  public class WorkInformationConfigDTOAssembler : IGenericDTOAssembler<Lemoine.DTO.WorkInformationConfigDTO, Lemoine.Model.IMonitoredMachine>
  {

    /// <summary>
    /// WorkInformationConfigDTO assembler
    /// </summary>
    /// <returns></returns>
    public WorkInformationConfigDTO Assemble(Lemoine.Model.IMonitoredMachine monitoredMachine){
      WorkInformationConfigDTO workInformationConfigDTO = new WorkInformationConfigDTO();
      workInformationConfigDTO.IsEditable = false;
      workInformationConfigDTO.OperationFromCnc = monitoredMachine.OperationFromCnc;
      workInformationConfigDTO.OnePartPerWorkOrder =
        Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueProjectOrPartFromWorkOrder));
      return workInformationConfigDTO;
    }
    
    /// <summary>
    /// WorkInformationConfigDTO list assembler
    /// </summary>
    /// <param name="monitoredMachineList"></param>
    /// <returns></returns>
    public IEnumerable<WorkInformationConfigDTO> Assemble(IEnumerable<Lemoine.Model.IMonitoredMachine> monitoredMachineList) {
      IList<WorkInformationConfigDTO> workInformationConfigDTOList = new List<WorkInformationConfigDTO>();
      foreach (Lemoine.Model.IMonitoredMachine monitoredMachine in monitoredMachineList) {
        workInformationConfigDTOList.Add(Assemble(monitoredMachine));
      }
      return workInformationConfigDTOList;
    }
  }
}

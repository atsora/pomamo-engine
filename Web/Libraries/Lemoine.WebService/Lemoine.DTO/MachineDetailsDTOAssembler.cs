// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Description of MachineDetailsDTOAssembler.
  /// </summary>
  public class MachineDetailsDTOAssembler
  {
    /// <summary>
    /// MachineDetailsDTO assembler
    /// </summary>
    /// <param name="monitoredMachine"></param>
    /// <returns></returns>
    public MachineDetailsDTO Assemble(Lemoine.Model.IMonitoredMachine monitoredMachine)
    {
      MachineDetailsDTO machineDetailsDTO = new MachineDetailsDTO();
      machineDetailsDTO.Id = monitoredMachine.Id;
      machineDetailsDTO.Name = monitoredMachine.Display;
      machineDetailsDTO.DisplayPriority = monitoredMachine.DisplayPriority;
      machineDetailsDTO.CompanyId = (monitoredMachine.Company != null) ? (int?)monitoredMachine.Company.Id : null;
      machineDetailsDTO.DepartmentId = (monitoredMachine.Department != null) ? (int?)monitoredMachine.Department.Id : null;
      machineDetailsDTO.MachineCategoryId = (monitoredMachine.Category != null) ? (int?)monitoredMachine.Category.Id : null;
      machineDetailsDTO.SubCategoryId = (monitoredMachine.SubCategory != null) ? (int?)monitoredMachine.SubCategory.Id : null;
      machineDetailsDTO.CellId = (monitoredMachine.Cell != null) ? (int?)monitoredMachine.Cell.Id : null;
      return machineDetailsDTO;
    }
    
    /// <summary>
    /// MachineCategoryDTO list assembler
    /// </summary>
    /// <param name="monitoredMachineList"></param>
    /// <returns></returns>
    public IEnumerable<MachineDetailsDTO> Assemble(IEnumerable<Lemoine.Model.IMonitoredMachine> monitoredMachineList)
    {
      IList<MachineDetailsDTO> machineDetailsDTOList = new List<MachineDetailsDTO>();
      foreach (Lemoine.Model.IMonitoredMachine monitoredMachine in monitoredMachineList
               .OrderBy (machine => machine.DisplayPriority.HasValue
                          ? machine.DisplayPriority.Value
                          : int.MaxValue)) {
        machineDetailsDTOList.Add(Assemble(monitoredMachine));
      }
      return machineDetailsDTOList;
    }
  }
}

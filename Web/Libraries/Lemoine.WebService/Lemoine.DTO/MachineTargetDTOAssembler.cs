// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for MachineTargetDTO
  /// </summary>
  public class MachineTargetDTOAssembler : IGenericDTOAssembler<Lemoine.DTO.MachineTargetDTO, Tuple<Lemoine.Model.IMachine, double>> 
  {
    /// <summary>
    /// MachineTargetDTO assembler
    /// </summary>
    /// <param name="machineTargetPercentage"></param>
    /// <returns></returns>
    public MachineTargetDTO Assemble(Tuple<Lemoine.Model.IMachine, double> machineTargetPercentage) {
      MachineTargetDTO machineTargetDTO = new MachineTargetDTO();
      Lemoine.Model.IMachine machine = machineTargetPercentage.Item1;
      double targetPercentage = machineTargetPercentage.Item2;
      machineTargetDTO.Id = machine.Id;
      machineTargetDTO.Name = machine.Display;
      machineTargetDTO.TargetPercentage = targetPercentage;
      return machineTargetDTO;
    }
    
    /// <summary>
    /// MachineTargetDTO list assembler
    /// </summary>
    /// <param name="machineTargetList"></param>
    /// <returns></returns>
    public IEnumerable<MachineTargetDTO> Assemble(IEnumerable<Tuple<Lemoine.Model.IMachine, double>> machineTargetList) {
      IList<MachineTargetDTO> machineTargetDTOList = new List<MachineTargetDTO>();
      foreach (Tuple<Lemoine.Model.IMachine, double> machineTargetPercentage in machineTargetList) {
        machineTargetDTOList.Add(Assemble(machineTargetPercentage));
      }
      return machineTargetDTOList;
    }
  }
}

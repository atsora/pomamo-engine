// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for MachineModuleCycleProgressV2DTO
  /// </summary>
  public class MachineModuleCycleProgressV2DTOAssembler :
    IGenericDTOAssembler<Lemoine.DTO.MachineModuleCycleProgressDTO, Tuple<IMachineModule, DateTime>>
  {

    static readonly ILog log = LogManager.GetLogger(typeof (MachineModuleCycleProgressV2DTOAssembler).FullName);

    /// <summary>
    /// MachineModuleCycleProgressV2DTO assembler
    /// </summary>
    /// <param name="machineModuleDateTimePair"></param>
    /// <returns></returns>
    public MachineModuleCycleProgressDTO Assemble(Tuple<IMachineModule, DateTime> machineModuleDateTimePair) {
      IMachineModule machineModule = machineModuleDateTimePair.Item1;
      DateTime dateOfRequest = machineModuleDateTimePair.Item2;
      
      // find last sequence slot on machine module whose begin is strictly before datetime of request
      ISequenceSlot sequenceSlot = ModelDAOHelper.DAOFactory.SequenceSlotDAO.FindLastBefore(machineModule, dateOfRequest);

      if ((sequenceSlot == null) ||
          (sequenceSlot.Sequence == null) ||
          (sequenceSlot.Sequence.Path == null) ||
          (sequenceSlot.Sequence.Operation == null))
      {
        return null;
      }
      
      MachineModuleCycleProgressDTO machineModuleCycleProgressDTO = new MachineModuleCycleProgressDTO();
      machineModuleCycleProgressDTO.Id = machineModule.Id;
      machineModuleCycleProgressDTO.Name = machineModule.Name;
      
      // operation/path selection
      IPath operationPath = sequenceSlot.Sequence.Path;
      IOperation operation = sequenceSlot.Sequence.Operation;
      
      // create list of sequence slots belonging to the "virtual operation cycle"
      IList<ISequenceSlot> sequenceSlotList = new List<ISequenceSlot>();
      sequenceSlotList.Add(sequenceSlot);
      
      // look for previous sequences
      DateTime currentDateForSearch = sequenceSlot.BeginDateTime.Value;
      int currentOrder = sequenceSlot.Sequence.Order;
      
      while(currentOrder >= 1) {
        // not efficient but cannot do better if there is no operation cycle
        ISequenceSlot previousSequenceSlot = ModelDAOHelper.DAOFactory.SequenceSlotDAO.FindLastBefore(machineModule, currentDateForSearch);
        if ((previousSequenceSlot != null) && (previousSequenceSlot.Sequence != null)) {
          if ((previousSequenceSlot.Sequence.Path == operationPath) &&
              (previousSequenceSlot.Sequence.Order < currentOrder))
          {
            // previous sequence slot seems to belong to same cycle
            sequenceSlotList.Add(previousSequenceSlot);
            currentOrder = previousSequenceSlot.Sequence.Order;
            currentDateForSearch = previousSequenceSlot.BeginDateTime.Value;
            continue;
          }
        }
        break; // not in "virtual operation cycle" anymore: stop
      }
      
      // now sequenceSlotList contains the sequence slots of the "virtual operation cycle" in reverse order
      
      return MachineModuleCycleProgressDTOAssembler.AssembleCommon(machineModule, sequenceSlotList.Reverse().ToList(), operationPath, dateOfRequest);
    }
        
    /// <summary>
    /// MachineModuleCycleProgressDTO list assembler
    /// </summary>
    /// <param name="machineModuleDateTimePairList"></param>
    /// <returns></returns>
    public IEnumerable<MachineModuleCycleProgressDTO> Assemble(IEnumerable<Tuple<IMachineModule, DateTime>> machineModuleDateTimePairList) {
      IList<MachineModuleCycleProgressDTO> list = new List<MachineModuleCycleProgressDTO>();
      foreach (Tuple<IMachineModule, DateTime> elt in machineModuleDateTimePairList) {
        list.Add((MachineModuleCycleProgressDTO)this.Assemble((Tuple<IMachineModule, DateTime>)elt));
      }
      return list;
    }
  }
}
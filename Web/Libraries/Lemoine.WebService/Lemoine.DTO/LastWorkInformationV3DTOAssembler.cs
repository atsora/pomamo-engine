// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Config;

namespace Lemoine.DTO
{


  /// <summary>
  /// Assembler for LastWorkInformationV3DTO
  /// </summary>
  public class LastWorkInformationV3DTOAssembler: IGenericDTOAssembler<LastWorkInformationV3DTO, Tuple<IMonitoredMachine, DateTime>>
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="operationSlot"></param>
    /// <param name="operationFromCnc"></param>
    /// <returns></returns>
    internal static bool CheckOperationSlotWorkInformationMissing (IOperationSlot operationSlot, bool operationFromCnc)
    {

      if (!operationFromCnc && (operationSlot.Operation == null))
        return true;

      if (operationSlot.Component == null) {
        if (!operationFromCnc) {
          return true;
        }
        else {
          return false;
        }
      }

      if (operationSlot.WorkOrder == null) {
        return true;
      }

      Debug.Assert ((operationSlot.WorkOrder != null)
                   && (operationSlot.Component != null));

      if (Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderProjectIsJob))) {
        if (operationSlot.WorkOrder.Job == null) {
          return true;
        }
      }
      else {
        if ((operationSlot.Component.Project == null) && (!operationFromCnc)) {
          return true;
        }
      }

      if (Lemoine.Info.ConfigSet.Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ProjectComponentIsPart))) {
        if ((operationSlot.Component.Part == null) && (!operationFromCnc)) {
          return true;
        }
      }

      return false;
    }


    /// <summary>
    /// LastWorkInformationV3DTO assembler
    /// </summary>
    /// <param name="machineDateTimePair"></param>
    /// <returns></returns>
    public LastWorkInformationV3DTO Assemble (Tuple<IMonitoredMachine, DateTime> machineDateTimePair)
    {
      LastWorkInformationV3DTO dto = new LastWorkInformationV3DTO ();
      IMonitoredMachine machine = machineDateTimePair.Item1;

      var lastEffectiveOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
        .GetLastEffective (machine, machineDateTimePair.Item2);

      dto.DataMissing = false; // to be updated

      WorkInformationConfigDTOAssembler workInformationConfigDTOAssembler = new WorkInformationConfigDTOAssembler ();
      dto.Config = workInformationConfigDTOAssembler.Assemble (machine);

      if (null == lastEffectiveOperationSlot) {
        dto.SlotMissing = true;
        return dto;
      }

      if (CheckOperationSlotWorkInformationMissing (lastEffectiveOperationSlot, dto.Config.OperationFromCnc)) {
        dto.DataMissing = true;
      }

      dto.SlotMissing = false;
      dto.Begin = ConvertDTO.DateTimeUtcToIsoString (lastEffectiveOperationSlot.BeginDateTime);
      dto.End = ConvertDTO.DateTimeUtcToIsoString (lastEffectiveOperationSlot.EndDateTime);

      dto.WorkInformations =
        WorkInformationDTOBuilder.BuildFromOperationSlot (lastEffectiveOperationSlot);

      return dto;
    }

    /// <summary>
    /// LastWorkInformationV3DTO list assembler
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public IEnumerable<LastWorkInformationV3DTO> Assemble (IEnumerable<Tuple<IMonitoredMachine, DateTime>> list)
    {
      IList<LastWorkInformationV3DTO> dtoList = new List<LastWorkInformationV3DTO> ();
      foreach (Tuple<IMonitoredMachine, DateTime> item in list) {
        dtoList.Add (Assemble (item));
      }
      return dtoList;
    }
  }
}

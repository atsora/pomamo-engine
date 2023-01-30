// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for OperationDTO
  /// </summary>
  public class OperationDTOAssembler: IGenericDTOAssembler<OperationDTO, Lemoine.Model.IOperation>
  {
    /// <summary>
    /// OperationDTO assembler
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public OperationDTO Assemble(Lemoine.Model.IOperation operation)
    {
      OperationDTO dto = new OperationDTO();
      dto.Id = ((Lemoine.Collections.IDataWithId)operation).Id;
      dto.Name = operation.Display;
      if(operation.MachiningDuration.HasValue){
        dto.MachiningDuration = (int) Math.Round(operation.MachiningDuration.Value.TotalSeconds);;
      }
      if(operation.SetUpDuration.HasValue){
        dto.SetUpDuration = (int) Math.Round(operation.SetUpDuration.Value.TotalSeconds);;
      }
      if(operation.TearDownDuration.HasValue){
        dto.TearDownDuration = (int) Math.Round(operation.TearDownDuration.Value.TotalSeconds);;
      }
      if(operation.LoadingDuration.HasValue){
        dto.LoadingDuration = (int) Math.Round(operation.LoadingDuration.Value.TotalSeconds);;
      }
      if(operation.UnloadingDuration.HasValue){
        dto.UnloadingDuration = (int) Math.Round(operation.UnloadingDuration.Value.TotalSeconds);;
      }
      return dto;
    }
    
    /// <summary>
    /// OperationDTO list assembler
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public IEnumerable<OperationDTO> Assemble(IEnumerable<Lemoine.Model.IOperation> list)
    {
      IList<OperationDTO> dtoList = new List<OperationDTO>();
      foreach (Lemoine.Model.IOperation item in list) {
        dtoList.Add(Assemble(item));
      }
      return dtoList;
    }
  }
}

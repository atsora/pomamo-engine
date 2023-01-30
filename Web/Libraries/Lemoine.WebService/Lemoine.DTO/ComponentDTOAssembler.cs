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
  /// Assembler for ComponentDTO
  /// </summary>
  public class ComponentDTOAssembler: IGenericDTOAssembler<ComponentDTO, IComponent>
  {
    /// <summary>
    /// ComponentDTO assembler
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public ComponentDTO Assemble(Lemoine.Model.IComponent component)
    {
      ComponentDTO dto = new ComponentDTO();
      dto.Id = ((Lemoine.Collections.IDataWithId)component).Id;
      dto.Name = component.Display;
      dto.TypeId = component.Type.Id;
      dto.ProjectId = ((Lemoine.Collections.IDataWithId)component.Project).Id;
      dto.FinalWorkPieceId = (null != component.FinalWorkPiece)?((Lemoine.Collections.IDataWithId)component.FinalWorkPiece).Id : (int?)null;
      dto.EstimatedHours = component.EstimatedHours;
      return dto;
    }
    
    /// <summary>
    /// ComponentDTO list assembler
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public IEnumerable<ComponentDTO> Assemble(IEnumerable<Lemoine.Model.IComponent> list)
    {
      IList<ComponentDTO> dtoList = new List<ComponentDTO>();
      foreach (Lemoine.Model.IComponent item in list) {
        dtoList.Add(Assemble(item));
      }
      return dtoList;
    }
  }
}

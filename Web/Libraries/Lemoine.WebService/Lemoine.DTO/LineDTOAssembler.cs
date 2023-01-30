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
  /// Assembler for LineDTO
  /// </summary>
  public class LineDTOAssembler: IGenericDTOAssembler<LineDTO, ILine>
  {
    /// <summary>
    /// LineDTO assembler
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public LineDTO Assemble(Lemoine.Model.ILine line)
    {
      LineDTO dto = new LineDTO();
      dto.Id = line.Id;
      dto.Display = line.Display;
      return dto;
    }
    
    /// <summary>
    /// LineDTO list assembler
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public IEnumerable<LineDTO> Assemble(IEnumerable<Lemoine.Model.ILine> list)
    {
      IList<LineDTO> dtoList = new List<LineDTO>();
      foreach (Lemoine.Model.ILine item in list) {
        dtoList.Add(Assemble(item));
      }
      return dtoList;
    }
  }
}

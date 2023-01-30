// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;



namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// Assembler for ComputerDTO.
  /// </summary>
  public class ComputerDTOAssembler
    : IGenericDTOAssembler<ComputerDTO, Lemoine.Model.IComputer>
  {
    /// <summary>
    /// ComputerDTO assembler
    /// </summary>
    /// <param name="computer"></param>
    /// <returns></returns>
    public ComputerDTO Assemble(Lemoine.Model.IComputer computer)
    {
      ComputerDTO computerDTO = new ComputerDTO();
      computerDTO.Id = computer.Id;
      computerDTO.Name = computer.Name;
      computerDTO.Address = computer.Address;
      return computerDTO;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="users"></param>
    /// <returns></returns>
    public IEnumerable<ComputerDTO> Assemble (IEnumerable<Lemoine.Model.IComputer> users)
    {
      return users.Select (user => Assemble(user));
    }
  }
}

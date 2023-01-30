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
  /// Assembler for UserDTO.
  /// </summary>
  public class UserDTOAssembler
    : IGenericDTOAssembler<UserDTO, Lemoine.Model.IUser>
  {
    /// <summary>
    /// UserDTO assembler
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public UserDTO Assemble(Lemoine.Model.IUser user)
    {
      UserDTO userDTO = new UserDTO();
      userDTO.Id = user.Id;
      userDTO.Name = user.Name;
      userDTO.Code = user.Code;
      userDTO.Login = user.Login;
      return userDTO;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="users"></param>
    /// <returns></returns>
    public IEnumerable<UserDTO> Assemble (IEnumerable<Lemoine.Model.IUser> users)
    {
      return users.Select (user => Assemble(user));
    }
  }
}

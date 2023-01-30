// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// DTO for UserDTO.
  /// </summary>
  public class UserDTO
  {
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Code
    /// </summary>
    public string Code { get; set; }
    
    /// <summary>
    /// Login
    /// </summary>
    public string Login { get; set; }

  }
}

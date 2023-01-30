// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table UserTable
  /// </summary>
  public interface IUser: IUpdater
  {
    /// <summary>
    /// User name
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// User code
    /// </summary>
    string Code { get; set; }
    
    /// <summary>
    /// User external code
    /// </summary>
    string ExternalCode { get; set; }
    
    /// <summary>
    /// User login
    /// </summary>
    string Login { get; set; }
    
    /// <summary>
    /// User password
    /// </summary>
    string Password { get; set; }
    
    /// <summary>
    /// Shift the user belongs to (optional)
    /// </summary>
    IShift Shift { get; set; }
    
    /// <summary>
    /// Mobile number
    /// </summary>
    string MobileNumber { get; set; }
    
    /// <summary>
    /// Associated role (optional)
    /// </summary>
    IRole Role { get; set; }
    
    /// <summary>
    /// E-mail address
    /// </summary>
    string EMail { get; set; }

    /// <summary>
    /// Associated company (if we want to restrict the rights of the user)
    /// Default is null: no restriction
    /// </summary>
    ICompany Company { get; set; }

    /// <summary>
    /// Specific disconnection time
    /// </summary>
    TimeSpan? DisconnectionTime { get; set; }
  }
}

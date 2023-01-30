// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Virtual right model
  /// </summary>
  public interface IRight: IDataWithVersion, ISerializableModel
  {
    // Note: it does not inherit from IVersionable
    //       else the corresponding properties can't be used in a DataGridView binding
    
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Associated role
    /// 
    /// null means it is applicable to all roles
    /// </summary>
    IRole Role { get; }
    
    /// <summary>
    /// Access privilege
    /// </summary>
    RightAccessPrivilege AccessPrivilege { get; set; }
  }
}

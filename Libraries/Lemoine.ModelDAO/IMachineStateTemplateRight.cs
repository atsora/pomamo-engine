// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Description of IMachineStateTemplateRight.
  /// </summary>
  public interface IMachineStateTemplateRight: IRight
  {
    /// <summary>
    /// Associated machine state template
    /// 
    /// null means it is applicable to all the machine state templates
    /// </summary>
    IMachineStateTemplate MachineStateTemplate { get; }
  }
  
  /// <summary>
  /// Description of IMachineStateTemplateRight for the data grid views (to bypass a limitation of the MS data grid views)
  /// </summary>
  public interface IDataGridViewMachineStateTemplateRight
  {
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }
    
    /// <summary>
    /// Version
    /// </summary>
    int Version { get; }

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
    
    /// <summary>
    /// Associated machine state template
    /// 
    /// null means it is applicable to all the machine state templates
    /// </summary>
    IMachineStateTemplate MachineStateTemplate { get; }
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachineStateTemplateRight.
  /// </summary>
  public interface IMachineStateTemplateRightDAO: IGenericUpdateDAO<IMachineStateTemplateRight, int>
  {
    /// <summary>
    /// Get the default access privilege for the machine state templates
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="role">not null</param>
    /// <returns></returns>
    RightAccessPrivilege GetDefault (IRole role);
    
    /// <summary>
    /// Get the granted machine state templates for the specified role
    /// </summary>
    /// <param name="role">not null</param>
    /// <returns></returns>
    IList<IMachineStateTemplate> GetGranted (IRole role);
    
    /// <summary>
    /// Find all the entities given one or more Role
    /// Entities are grouped with config. sameness
    /// with children that are Eager Fetched
    /// </summary>
    /// <param name="roles">IList&lt;IRole&gt;</param>
    /// <returns>IList&lt;IMachineStateTemplateRight&gt;</returns> 
    IList<IMachineStateTemplateRight> FindWithForConfig(IList<IRole> roles);
  }
}

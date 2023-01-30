// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachineStateTemplate.
  /// </summary>
  public interface IMachineStateTemplateDAO: IGenericUpdateDAO<IMachineStateTemplate, int>
  {
    /// <summary>
    /// List all MachineStateTemplate with Eager Mode
    /// </summary>
    /// <returns></returns>
    IList<IMachineStateTemplate> FindAllForConfig();
    
    /// <summary>
    /// Find all the machine state templates for the specified category
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    IList<IMachineStateTemplate> FindByCategory (MachineStateTemplateCategory category);
  }
}

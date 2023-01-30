// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachineStateTemplateFlow.
  /// </summary>
  public interface IMachineStateTemplateFlowDAO: IGenericUpdateDAO<IMachineStateTemplateFlow, int>
  {
    /// <summary>
    /// Find the possible next machine state templates to a specified machine state template 
    /// </summary>
    /// <param name="machineStateTemplate"></param>
    /// <returns></returns>
    IEnumerable<IMachineStateTemplate> FindNext (IMachineStateTemplate machineStateTemplate);
  }
}

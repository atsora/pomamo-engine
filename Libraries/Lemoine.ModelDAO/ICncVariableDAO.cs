// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for ICncVariable.
  /// </summary>
  public interface ICncVariableDAO: IGenericByMachineModuleUpdateDAO<ICncVariable, int>
  {
    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="key">not null and not empty</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    ICncVariable FindAt (IMachineModule machineModule, string key, DateTime dateTime);

    /// <summary>
    /// Find all the slots in the specified UTC date/time range
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="key">not null and not empty</param>
    /// <param name="range">in UTC</param>
    /// <returns></returns>
    IList<ICncVariable> FindOverlapsRange (IMachineModule machineModule, string key, UtcDateTimeRange range);
  }
}

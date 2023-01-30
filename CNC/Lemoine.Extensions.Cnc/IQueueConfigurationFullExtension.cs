// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using System.Collections.Generic;

namespace Lemoine.Extensions.Cnc
{
  /// <summary>
  /// Extension to load an alternative queue configuration file
  /// </summary>
  public interface IQueueConfigurationFullExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Distant directory of the queue configuration file
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <returns>if null, not applicable</returns>
    string GetDistantDirectory (int machineId, int machineModuleId);

    /// <summary>
    /// Distant file name of the queue configuration file
    /// </summary>
    /// <param name="machineId"></param>
    /// <param name="machineModuleId"></param>
    /// <returns>if null or empty, not applicable</returns>
    string GetDistantFileName (int machineId, int machineModuleId);
  }
}

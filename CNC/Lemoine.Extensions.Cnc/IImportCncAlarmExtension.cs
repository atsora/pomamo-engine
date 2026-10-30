// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.Extensions.Cnc
{
  /// <summary>
  /// Extension to add additional actions when a new CNC alarm is imported
  /// </summary>
  public interface IImportCncAlarmExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <returns>return true if this extension is applicable to this machine module</returns>
    bool Initialize (IMachineModule machineModule);

    /// <summary>
    /// Run after a new CNC alarm was stored, inside the transaction of the import: an
    /// extension must keep it short. It is not run when an existing alarm is extended
    /// </summary>
    /// <param name="cncAlarm">not null</param>
    void AfterNewCncAlarm (ICncAlarm cncAlarm);
  }
}

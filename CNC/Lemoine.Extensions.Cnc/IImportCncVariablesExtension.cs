// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using System.Collections.Generic;

namespace Lemoine.Extensions.Cnc
{
  /// <summary>
  /// Extension to add additional actions when new Cnc variables are imported
  /// </summary>
  public interface IImportCncVariablesExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns>return true if this extension is applicable to this machine module</returns>
    bool Initialize (IMachineModule machineModule);

    /// <summary>
    /// Run after the cnc variables have been imported into the cncvariable table (not in a transaction, a new transaction must be created if needed).
    /// Only the updated variables are returned here
    /// </summary>
    /// <param name="variableSet"></param>
    /// <param name="startDateTime">in UTC</param>
    void AfterImportCncVariables (IDictionary<string, object> variableSet, DateTime startDateTime);
  }
}

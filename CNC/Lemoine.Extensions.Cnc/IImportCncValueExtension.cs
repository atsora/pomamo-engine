// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using System.Collections.Generic;

namespace Lemoine.Extensions.Cnc
{
  /// <summary>
  /// New extension of the ImportCncValue method of ImportDataCncValues, Lemoine.Cnc.DataImport
  /// </summary>
  public interface IImportCncValueExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns>return true if this extension is applicable to this machine module</returns>
    bool Initialize (IMachineModule machineModule);

    /// <summary>
    /// Run after a new cnc value has been imported into the cncvalue table (not in a transaction, a new transaction must be created if needed).
    /// Only new values are processed here.
    /// </summary>
    /// <param name="field"></param>
    /// <param name="range"></param>
    /// <param name="newValue"></param>
    void AfterImportNewCncValue (IField field, UtcDateTimeRange range, object newValue);
  }
}

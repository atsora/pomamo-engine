// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.Extensions.Cnc
{
  /// <summary>
  /// Extension to import the cnc data of a field in table cncdataperiod with a custom aggregation,
  /// for example a new <see cref="CncDataAggregationType"/> that has no built-in behavior
  /// (<see cref="CncDataPeriodAggregationKind.Custom"/>).
  ///
  /// Entry point in ImportCncDataFromQueue, Lemoine.CncDataImport.
  /// The methods are called inside the session and the transaction of the import:
  /// the periods are written with ICncDataPeriodDAO, partitioned by machine module and by week
  /// (a period can't cross <see cref="ICncDataPeriod.MaxEnd"/>, its begin can't be changed)
  /// </summary>
  public interface IImportCncDataPeriodExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialization
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <returns>the extension is active for this machine module</returns>
    bool Initialize (IMachineModule machineModule);

    /// <summary>
    /// Does this extension process the cnc data periods of this field?
    /// </summary>
    /// <param name="field">not null</param>
    /// <returns></returns>
    bool IsSupported (IField field);

    /// <summary>
    /// Import some new values of a field, oldest first
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="values">UTC date/time and value, oldest first</param>
    void Import (IMachineModule machineModule, IField field, IList<KeyValuePair<DateTime, object>> values);

    /// <summary>
    /// Stop the current period of a field (it must not be continued by the next value)
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="dateTime">UTC date/time of the stop</param>
    void Stop (IMachineModule machineModule, IField field, DateTime dateTime);

    /// <summary>
    /// The transaction was rolled back: forget what is kept in memory for this field
    /// </summary>
    /// <param name="machineModuleId"></param>
    /// <param name="fieldId"></param>
    void Reset (int machineModuleId, int fieldId);
  }
}

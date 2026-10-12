// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for <see cref="ICncDataPeriod"/> (table cncdataperiod)
  ///
  /// The table is partitioned by machine module, then by week.
  /// The requests are restricted to a few partitions with a criterion on the machine module and the begin date/time.
  /// Prefer <see cref="FindById(long, IMachineModule, DateTime)"/> to FindById (id, machineModule)
  /// that visits all the weeks of the machine module.
  ///
  /// MakePersistent creates the missing partition if needed
  /// </summary>
  public interface ICncDataPeriodDAO : IGenericByMachineModuleUpdateDAO<ICncDataPeriod, long>
  {
    /// <summary>
    /// Find a period by its id in a single partition
    /// </summary>
    /// <param name="id"></param>
    /// <param name="machineModule">not null</param>
    /// <param name="begin">UTC begin date/time of the period</param>
    /// <returns></returns>
    ICncDataPeriod FindById (long id, IMachineModule machineModule, DateTime begin);

    /// <summary>
    /// Find the recent period of a field that ends at end: the one to extend
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="end">UTC date/time</param>
    /// <returns></returns>
    ICncDataPeriod FindWithEnd (IMachineModule machineModule, IField field, DateTime end);

    /// <summary>
    /// Find the last recent period of a field, for example to continue after a restart
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <returns></returns>
    ICncDataPeriod FindLast (IMachineModule machineModule, IField field);

    /// <summary>
    /// Find the recent period of a field at a given date/time
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="at">UTC date/time</param>
    /// <returns></returns>
    ICncDataPeriod FindAt (IMachineModule machineModule, IField field, DateTime at);

    /// <summary>
    /// Find the recent (modifiable) periods of a field that overlap a range, ordered by begin
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<ICncDataPeriod> FindOverlapsRange (IMachineModule machineModule, IField field, UtcDateTimeRange range);

    /// <summary>
    /// Read the recent and archived periods of a field that overlap a range, ordered by begin
    /// (function cncdataperiodrange)
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range">with a lower bound</param>
    /// <returns></returns>
    IList<ICncDataPeriodValue> FindValuesOverlapsRange (IMachineModule machineModule, IField field, UtcDateTimeRange range);
  }
}

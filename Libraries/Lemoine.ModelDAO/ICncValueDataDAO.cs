// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface to read the cnc values from cncvalue, cncdataperiod and cncdataperiodarchive
  /// (functions cncvalueat and cncvaluerange, view cncvalueall).
  ///
  /// A field stores its data in a single table at a time: the historical data may be in cncvalue and the new ones in cncdataperiod.
  /// The requests have the same semantics as the ones of <see cref="ICncValueDAO"/>
  /// </summary>
  public interface ICncValueDataDAO
  {
    /// <summary>
    /// Values of all the fields of a machine module at a given date/time
    /// (begin &lt;= at &lt; end, or begin = end = at)
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="at">UTC date/time</param>
    /// <returns></returns>
    IList<ICncValueData> FindAt (IMachineModule machineModule, DateTime at);

    /// <summary>
    /// Value of a field at a given date/time
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="at">UTC date/time</param>
    /// <returns></returns>
    ICncValueData FindAt (IMachineModule machineModule, IField field, DateTime at);

    /// <summary>
    /// Value of a field that ends at a given date/time (and begins before)
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="end">UTC date/time</param>
    /// <returns></returns>
    ICncValueData FindWithEnd (IMachineModule machineModule, IField field, DateTime end);

    /// <summary>
    /// Values of a field that overlap a range, ordered by begin
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<ICncValueData> FindOverlapsRange (IMachineModule machineModule, IField field, UtcDateTimeRange range);

    /// <summary>
    /// Next values of a field that begin after (including) a date/time, ordered by begin
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="dateTime">UTC date/time</param>
    /// <param name="maxNbFetched">maximum number of values</param>
    /// <returns></returns>
    IList<ICncValueData> FindNext (IMachineModule machineModule, IField field, DateTime dateTime, int maxNbFetched);

    /// <summary>
    /// First n values of a field that overlap a range, in the given order of their begin
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range"></param>
    /// <param name="n">maximum number of values</param>
    /// <param name="descending">false: ascending order, true: descending order</param>
    /// <returns></returns>
    IEnumerable<ICncValueData> FindFirstOverlapsRange (IMachineModule machineModule, IField field, UtcDateTimeRange range, int n, bool descending);

    /// <summary>
    /// Values of a field that overlap a range in the ascending order, read by step:
    /// the enumeration may be interrupted without reading the whole range
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    IEnumerable<ICncValueData> FindOverlapsRangeAscending (IMachineModule machineModule, IField field, UtcDateTimeRange range, TimeSpan step);

    /// <summary>
    /// Values of a field that overlap a range in the descending order, read by step:
    /// the enumeration may be interrupted without reading the whole range
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    IEnumerable<ICncValueData> FindOverlapsRangeDescending (IMachineModule machineModule, IField field, UtcDateTimeRange range, TimeSpan step);
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for ICncValue.
  /// </summary>
  public interface ICncValueDAO: IGenericByMachineModuleUpdateDAO<ICncValue, long>
  {
    /// <summary>
    /// Find the unique ICncValue that matches the specified parameters
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="begin"></param>
    /// <returns></returns>
    ICncValue FindByMachineModuleFieldBegin (IMachineModule machineModule,
                                             IField field,
                                             DateTime begin);
    
    /// <summary>
    /// Find the ICncValues for specified machineModule and field
    /// which cross the interval [begin,end]
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="utcbegin"></param>
    /// <param name="utcend"></param>
    /// <returns></returns>
    IList<ICncValue> FindByMachineFieldDateRange(IMachineModule machineModule,
                                                 IField field,
                                                 DateTime utcbegin,
                                                 DateTime utcend);
    
    /// <summary>
    /// Find the ICncValues for specified machineModule and field
    /// in the specified range
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<ICncValue> FindByMachineFieldDateRange(IMachineModule machineModule,
                                                 IField field,
                                                 UtcDateTimeRange range);

    /// <summary>
    /// Find the next ICncValues for specified machineModule and field
    /// 
    /// The start date/time must be after (including) dateTime
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="dateTime"></param>
    /// <param name="maxNbFetched"></param>
    /// <returns></returns>
    IList<ICncValue> FindNext (IMachineModule machineModule,
                               IField field,
                               DateTime dateTime,
                               int maxNbFetched);

    /// <summary>
    /// Find the ICncValue at a specified date/time for all the fields
    /// 
    /// This request is not cacheable, because it does not behave well with a partitioned table
    /// (there is a risk to check the validity of the item again with only the ID)
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    IList<ICncValue> FindAt (IMachineModule machineModule,
                             DateTime at);

    /// <summary>
    /// Find the ICncValues at a specified date/time for all the fields with an eager fetch of the field
    /// 
    /// This request is not cacheable, because it does not behave well with a partitioned table
    /// (there is a risk to check the validity of the item again with only the ID)
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    IList<ICncValue> FindAtWithField (IMachineModule machineModule,
                                      DateTime at);

    /// <summary>
    /// Find the ICncValues at a specified date/time for all the fields with an eager fetch of the field and the unit
    /// 
    /// This request is not cacheable, because it does not behave well with a partitioned table
    /// (there is a risk to check the validity of the item again with only the ID)
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    IList<ICncValue> FindAtWithFieldUnit (IMachineModule machineModule,
                                          DateTime at);

    /// <summary>
    /// Find the ICncValue at a specified date/time
    /// 
    /// This request is not cacheable, because it does not behave well with a partitioned table
    /// (there is a risk to check the validity of the item again with only the ID)
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    ICncValue FindAt (IMachineModule machineModule,
                      IField field,
                      DateTime at);

    /// <summary>
    /// Find the ICncValue with a specified end date/time
    /// 
    /// This request is cacheable
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    ICncValue FindWithEnd (IMachineModule machineModule,
                           IField field,
                           DateTime end);

    /// <summary>
    /// Find the first n elements in the range in the specified order
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range"></param>
    /// <param name="n">maximum number elements to retrieve</param>
    /// <param name="descending">False: ascending order, True: descending order</param>
    /// <returns></returns>
    IEnumerable<ICncValue> FindFirstOverlapsRange (IMachineModule machineModule, IField field, UtcDateTimeRange range, int n, bool descending);

    /// <summary>
    /// Find the ICncValues for specified machineModule and field
    /// in the specified range
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<ICncValue> FindOverlapsRange (IMachineModule machineModule,
                                        IField field,
                                        UtcDateTimeRange range);

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// in a ascending order
    /// and with the specified step between two requests
    /// 
    /// Then the request may be interrupted without reaching the begin of the range
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    IEnumerable<ICncValue> FindOverlapsRangeAscending (IMachineModule machineModule, IField field,
     UtcDateTimeRange range, TimeSpan step);

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// in a descending order
    /// and with the specified step between two requests
    /// 
    /// Then the request may be interrupted without reaching the begin of the range
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    IEnumerable<ICncValue> FindOverlapsRangeDescending (IMachineModule machineModule, IField field,
      UtcDateTimeRange range, TimeSpan step);
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Business.CncValue
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ICncValueColorDAO">ICncValueColorDAO</see>
  /// </summary>
  public class CncValueColorDAO
    : ICncValueColorDAO
  {
    static readonly string MAX_GAP_KEY = "Business.CncValue.CncValueColor.MaxMergeGap";
    static readonly TimeSpan MAX_GAP_DEFAULT = TimeSpan.FromMinutes (4);

    readonly ILog log = LogManager.GetLogger (typeof (CncValueColorDAO).FullName);

    #region ICncValueColorDAO implementation
    /// <summary>
    /// ICncValueColorDAO implementation
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="dateTime"></param>
    /// <param name="extend">Try to extend the slot</param>
    /// <returns></returns>
    public virtual ICncValueColor FindAt (IMachineModule machineModule, IField field, DateTime dateTime, bool extend)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      ICncValue cncValue = FindAt (machineModule, field, dateTime);
      if (null == cncValue) {
        return null;
      }

      ICncValueColor slot = CreateCncValueColor (field,
                                               cncValue);
      if (extend && (null != slot)) {
        slot = ExtendLeft (field, slot);
        slot = ExtendRight (field, slot);
      }
      return slot;
    }

    /// <summary>
    /// ICncValueColorDAO implementation
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="range"></param>
    /// <param name="extend">Try to extend the slot</param>
    /// <returns></returns>
    public virtual IList<ICncValueColor> FindOverlapsRange (IMachineModule machineModule, IField field, UtcDateTimeRange range, bool extend)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      IEnumerable<ICncValue> cncValues = FindByMachineFieldDateRange (machineModule, field, range);
      return Merge (field, cncValues, extend);
    }

    /// <summary>
    /// Check if two IMergeable object can be effectively be merged
    /// </summary>
    /// <param name="left">not null</param>
    /// <param name="right">not null</param>
    /// <returns></returns>
    bool IsMergeable (ICncValueColor left, ICncValueColor right)
    {
      if (left.DateTimeRange.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsMergeable: left range is empty => return true because it can be dismissed");
        }
        return true;
      }
      if (right.DateTimeRange.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.Debug ("IsMergeable: right range is empty => return true because it can be dismissed");
        }
        return true;
      }

      if (!left.ReferenceDataEquals (right)) {
        return false;
      }
      else {
        if (left.DateTimeRange.IsAdjacentTo (right.DateTimeRange)) {
          if (!left.DateTimeRange.IsStrictlyLeftOf (right.DateTimeRange)) {
            log.Fatal ($"IsMergeable: adjacent but {left} is not strictly left of {right}");
            Debug.Assert (left.DateTimeRange.IsStrictlyLeftOf (right.DateTimeRange));
            return false;
          }
          return true;
        }
        else if (Bound.Equals (left.DateTimeRange.Upper, right.DateTimeRange.Lower)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"IsMergeable: common bound between {left} and {right}");
          }
          return true;
        }
        else { // Check the gap between them
          if (!left.DateTimeRange.IsStrictlyLeftOf (right.DateTimeRange)) {
            log.Fatal ($"IsMergeable: {left} is not strictly left of {right}");
            Debug.Assert (left.DateTimeRange.IsStrictlyLeftOf (right.DateTimeRange));
            return false;
          }
          Debug.Assert (right.DateTimeRange.Lower.HasValue);
          Debug.Assert (left.DateTimeRange.Upper.HasValue);
          TimeSpan gap = right.DateTimeRange.Lower.Value.Subtract (left.DateTimeRange.Upper.Value);
          TimeSpan maxGap = GetMaxGap ();
          return gap <= maxGap;
        }
      }
    }

    /// <summary>
    /// Merge two mergeable items
    /// </summary>
    /// <param name="left">not null</param>
    /// <param name="right">not null</param>
    /// <returns></returns>
    ICncValueColor Merge (ICncValueColor left, ICncValueColor right)
    {
      if (!IsMergeable (left, right)) {
        log.Fatal ($"Merge: {left} and {right} are not mergeable");
        Debug.Assert (IsMergeable (left, right));
      }

      if (left.DateTimeRange.IsEmpty ()) {
        return right;
      }
      if (right.DateTimeRange.IsEmpty ()) {
        return left;
      }

      Debug.Assert (left.DateTimeRange.IsStrictlyLeftOf (right.DateTimeRange));

      // Note: the Union function supports empty ranges
      //       but it does not support any gap between left and right although it may happen
      //       because of the IsMergeable method
      UtcDateTimeRange newRange = new UtcDateTimeRange (left.DateTimeRange.Lower,
                                                        right.DateTimeRange.Upper);
      DayRange newDayRange =
        new DayRange (left.DayRange.Lower,
                      right.DayRange.Upper);

      TimeSpan? newDuration;
      if (!left.Duration.HasValue || !right.Duration.HasValue) {
        newDuration = null;
      }
      else {
        newDuration = left.Duration.Value.Add (right.Duration.Value);
      }
      ICncValueColor result = new CncValueColor (left.MachineModule,
                                                 left.Field,
                                                 left.Color,
                                                 newRange,
                                                 newDayRange,
                                                 newDuration);
      return result;
    }

    IList<ICncValueColor> Merge (IField field, IEnumerable<ICncValue> cncValues, bool extend)
    {
      IList<ICncValueColor> result = new List<ICncValueColor> ();

      if (!cncValues.Any ()) {
        return result;
      }

      foreach (var cncValue in cncValues.Where (x => !x.DateTimeRange.IsEmpty ())) {
        ICncValueColor newSlot = CreateCncValueColor (field, cncValue);
        if ((1 <= result.Count)
            && IsMergeable (result[result.Count - 1], newSlot)) {
          result[result.Count - 1] =
            Merge (result[result.Count - 1], newSlot);
        }
        else {
          if (!newSlot.IsEmpty ()) {
            result.Add (newSlot);
          }
        }
      }
      if (extend && (0 < result.Count)) {
        result[0] = ExtendLeft (field, result[0]);
        result[result.Count - 1] = ExtendRight (field, result[result.Count - 1]);
      }
      return result;
    }

    /// <summary>
    /// Extend a slot to the left
    /// </summary>
    /// <param name="field"></param>
    /// <param name="slot"></param>
    /// <returns></returns>
    ICncValueColor ExtendLeft (IField field, ICncValueColor slot)
    {
      if (null == slot) {
        return null;
      }
      if (!slot.DateTimeRange.Lower.HasValue) {
        return slot;
      }
      else {
        ICncValue leftCncValue = FindWithEnd (slot.MachineModule, field, slot.DateTimeRange.Lower.Value);
        if (null == leftCncValue) {
          return slot;
        }
        else { // null != leftReasonSlot
          if (leftCncValue.IsEmpty ()) {
            Debug.Assert (false);
            log.ErrorFormat ("ExtendLeft: " +
                             "empty left reason slot {0}",
                             leftCncValue);
            return slot;
          }
          ICncValueColor leftCncValueColor = CreateCncValueColor (field, leftCncValue);
          if (IsMergeable (leftCncValueColor, slot)) { // Extend it
            ICncValueColor merged =
              Merge (leftCncValueColor, slot);
            return this.ExtendLeft (field, merged);
          }
          else {
            return slot;
          }
        }
      }
    }

    /// <summary>
    /// Extend a slot to the right
    /// </summary>
    /// <param name="field"></param>
    /// <param name="slot"></param>
    /// <returns></returns>
    ICncValueColor ExtendRight (IField field, ICncValueColor slot)
    {
      if (null == slot) {
        return null;
      }
      if (!slot.DateTimeRange.Upper.HasValue) {
        return slot;
      }
      else {
        ICncValue rightCncValue = FindAt (slot.MachineModule, field, slot.DateTimeRange.Upper.Value);
        if (null == rightCncValue) {
          return slot;
        }
        else { // null != rightCncValue
          if (rightCncValue.IsEmpty ()) {
            Debug.Assert (false);
            log.ErrorFormat ("ExtendRight: " +
                             "empty right reason slot {0}",
                             rightCncValue);
            return slot;
          }
          ICncValueColor rightCncValueColor = CreateCncValueColor (field, rightCncValue);
          if (IsMergeable (slot, rightCncValueColor)) { // Extend it
            ICncValueColor merged = Merge (slot, rightCncValueColor);
            return this.ExtendRight (field, merged);
          }
          else {
            return slot;
          }
        }
      }
    }
    #endregion // ICncValueColorDAO implementation

    /// <summary>
    /// Create a ICncValueColor from a field and a cncValue
    /// </summary>
    /// <param name="field">not null</param>
    /// <param name="cncValue">not null</param>
    /// <returns></returns>
    protected virtual ICncValueColor CreateCncValueColor (IField field, ICncValue cncValue)
    {
      Debug.Assert (null != field);
      Debug.Assert (null != cncValue);

      return new CncValueColor (field, cncValue);
    }

    /// <summary>
    /// Find a ICncValue at a specific time
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    protected virtual ICncValue FindAt (IMachineModule machineModule, IField field, DateTime dateTime)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      return ModelDAOHelper.DAOFactory.CncValueDAO
        .FindAt (machineModule, field, dateTime);
    }

    /// <summary>
    /// Find the ICncValue in a specific date/time range
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    protected virtual IEnumerable<ICncValue> FindByMachineFieldDateRange (IMachineModule machineModule, IField field, UtcDateTimeRange range)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      return ModelDAOHelper.DAOFactory.CncValueDAO
        .FindByMachineFieldDateRange (machineModule, field, range);
    }

    /// <summary>
    /// Find the ICncValue with a specific end
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    protected virtual ICncValue FindWithEnd (IMachineModule machineModule, IField field, DateTime dateTime)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      return ModelDAOHelper.DAOFactory.CncValueDAO
        .FindWithEnd (machineModule, field, dateTime);
    }

    /// <summary>
    /// Get the maximum gap
    /// </summary>
    /// <returns></returns>
    protected virtual TimeSpan GetMaxGap ()
    {
      return Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (MAX_GAP_KEY,
                                                          MAX_GAP_DEFAULT);
    }
  }
}

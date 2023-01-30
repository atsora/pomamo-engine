// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// Class with both a DateTimeRange
  /// </summary>
  public interface IWithDateTimeRange
  {
    /// <summary>
    /// Date/time range of the slot
    /// </summary>
    UtcDateTimeRange DateTimeRange { get; }
  }

  /// <summary>
  /// Class with both a DateTimeRange and a DayRange
  /// </summary>
  public interface IWithRange: IWithDateTimeRange
  {
    /// <summary>
    /// Day range (from cut-off time) of the slot
    /// </summary>
    DayRange DayRange { get; }
  }

  /// <summary>
  /// Typed
  /// </summary>
  public interface IMergeableItem<T>: IWithRange
  {
    /// <summary>
    /// Clone the object with a new range
    /// </summary>
    /// <param name="newRange">new range</param>
    /// <param name="newDayRange">new day range</param>
    /// <returns></returns>
    T Clone (UtcDateTimeRange newRange, DayRange newDayRange);
    
    /// <summary>
    /// Check the reference data of the other item matches
    /// the data of other without considering the date/time
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    bool ReferenceDataEquals (T other);
  }
  
  /// <summary>
  /// Utility class for the IMergeableItem objects
  /// </summary>
  public static class MergeableItem
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MergeableItem).FullName);

    /// <summary>
    /// Check if two IMergeable object can be effectively be merged
    /// </summary>
    /// <param name="left">not null</param>
    /// <param name="right">not null</param>
    /// <returns></returns>
    public static bool IsMergeable<T> (T left, T right)
      where T: IMergeableItem<T>
    {
      Debug.Assert (!object.Equals (default(T), left));
      Debug.Assert (!object.Equals (default(T), right));
      
      if (left.DateTimeRange.IsEmpty ()) {
        Debug.Assert (!left.DateTimeRange.IsEmpty ());
        log.ErrorFormat ("IsMergeable: " +
                         "left range is empty, which is unexpected " +
                         "=> return true because it can be dismissed");
        return true;
      }
      if (right.DateTimeRange.IsEmpty ()) {
        Debug.Assert (!right.DateTimeRange.IsEmpty ());
        log.ErrorFormat ("IsMergeable: " +
                         "right range is empty, which is unexpected " +
                         "=> return true because it can be dismissed");
        return true;
      }
      
      return left.ReferenceDataEquals (right)
        && left.DateTimeRange.IsAdjacentTo (right.DateTimeRange);
    }
    
    /// <summary>
    /// Merge two mergeable items
    /// </summary>
    /// <param name="left">not null</param>
    /// <param name="right">not null</param>
    /// <returns></returns>
    public static T Merge<T> (T left, T right)
      where T: IMergeableItem<T>
    {
      Debug.Assert (IsMergeable (left, right));
      
      // Note: the Union function supports empty ranges
      UtcDateTimeRange newRange =
        new UtcDateTimeRange (left.DateTimeRange.Union (right.DateTimeRange));
      DayRange newDayRange =
        new DayRange (left.DayRange.Union (right.DayRange));
      return left.Clone (newRange, newDayRange);
    }
  }
}

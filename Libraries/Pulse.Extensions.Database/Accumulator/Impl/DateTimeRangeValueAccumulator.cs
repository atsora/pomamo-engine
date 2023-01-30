// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Pulse.Extensions.Database.Accumulator.Impl
{
  /// <summary>
  /// Value that is associated to a date/time range
  /// 
  /// TODO: replace it by a pair
  /// </summary>
  /// <typeparam name="U"></typeparam>
  public class DateTimeRangeValue<U>
  {
    /// <summary>
    /// Range
    /// </summary>
    public UtcDateTimeRange Range { get; set; }
    /// <summary>
    /// Value
    /// </summary>
    public U Value { get; set; }
  }

  /// <summary>
  /// Accumulator where you store a value between two date/time range
  /// 
  /// T: type of the tracked value
  /// U: type that is stored in the accumulator
  /// </summary>
  public abstract class DateTimeRangeValueAccumulator<T, U> : Accumulator, IAccumulator
  {

    #region Members
    IList<DateTimeRangeValue<U>> m_dateTimeRangeValues = new List<DateTimeRangeValue<U>> ();
    readonly Func<T, U> m_insert;
    readonly Func<U, T, U> m_add;
    readonly Func<T, U> m_delete;
    readonly Func<U, T, U> m_remove;
    readonly Func<U, bool> m_purge;
    #endregion // Members

    // disable once StaticFieldInGenericType
    static readonly ILog log = LogManager.GetLogger (typeof (DateTimeRangeValueAccumulator<T, U>).FullName);

    #region Getters / Setters
    /// <summary>
    /// Accumulator values
    /// </summary>
    public IList<DateTimeRangeValue<U>> DateTimeRangeValues
    {
      get { return m_dateTimeRangeValues; }
    }

    /// <summary>
    /// Global range
    /// </summary>
    public UtcDateTimeRange GlobalRange
    {
      get
      {
        if (m_dateTimeRangeValues.Any ()) {
          return new UtcDateTimeRange (m_dateTimeRangeValues.First ().Range.Lower,
                                       m_dateTimeRangeValues.Last ().Range.Upper);
        }
        else {
          return new UtcDateTimeRange ();
        }
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    /// <param name="insertMethod"></param>
    /// <param name="addMethod"></param>
    /// <param name="deleteMethod"></param>
    /// <param name="removeMethod"></param>
    /// <param name="purgeCondition"></param>
    protected DateTimeRangeValueAccumulator (Func<T, U> insertMethod,
                                             Func<U, T, U> addMethod,
                                             Func<T, U> deleteMethod,
                                             Func<U, T, U> removeMethod,
                                             Func<U, bool> purgeCondition)
    {
      m_insert = insertMethod;
      m_add = addMethod;
      m_delete = deleteMethod;
      m_remove = removeMethod;
      m_purge = purgeCondition;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a value in the accumulator
    /// </summary>
    /// <param name="range"></param>
    /// <param name="v"></param>
    /// <param name="insert"></param>
    /// <param name="merge"></param>
    void Process (UtcDateTimeRange range, T v, Func<T, U> insert, Func<U, T, U> merge)
    {
      Bound<DateTime> t = new LowerBound<DateTime> (null);
      for (int i = 0; i < m_dateTimeRangeValues.Count; ++i) {
        var dateTimeRangeValue = m_dateTimeRangeValues[i];
        if (dateTimeRangeValue.Range.IsStrictlyLeftOf (range)) {
          t = dateTimeRangeValue.Range.Upper;
          continue;
        }
        if (Bound.Compare<DateTime> (t, dateTimeRangeValue.Range.Lower) < 0) { // Gap
          Debug.Assert (dateTimeRangeValue.Range.Lower.HasValue);
          var gapRange = new UtcDateTimeRange ((LowerBound<DateTime>)t, dateTimeRangeValue.Range.Lower.Value)
            .Intersects (range);
          if (!gapRange.IsEmpty ()) {
            var newDateTimeRangeValue = new DateTimeRangeValue<U> ();
            newDateTimeRangeValue.Range = new UtcDateTimeRange (gapRange);
            newDateTimeRangeValue.Value = insert (v);
            m_dateTimeRangeValues.Insert (i++, newDateTimeRangeValue);
            t = gapRange.Upper;
          }
        }
        if (dateTimeRangeValue.Range.IsStrictlyRightOf (range)) {
          break;
        }
        Debug.Assert (dateTimeRangeValue.Range.Overlaps (range));
        UtcDateTimeRange intersection = new UtcDateTimeRange (range.Intersects (dateTimeRangeValue.Range));
        Debug.Assert (!intersection.IsEmpty ());
        if (Bound.Compare<DateTime> (dateTimeRangeValue.Range.Lower, intersection.Lower) < 0) { // Left
          var newDateTimeRangeValue = new DateTimeRangeValue<U> ();
          newDateTimeRangeValue.Range = new UtcDateTimeRange (dateTimeRangeValue.Range.Lower, (UpperBound<DateTime>)intersection.Lower);
          newDateTimeRangeValue.Value = dateTimeRangeValue.Value;
          m_dateTimeRangeValues.Insert (i++, newDateTimeRangeValue);
          dateTimeRangeValue.Range = new UtcDateTimeRange (intersection.Lower, dateTimeRangeValue.Range.Upper);
        }
        if (Bound.Compare<DateTime> (intersection.Upper, dateTimeRangeValue.Range.Upper) < 0) { // Right
          var newDateTimeRangeValue = new DateTimeRangeValue<U> ();
          newDateTimeRangeValue.Range = new UtcDateTimeRange ((LowerBound<DateTime>)intersection.Upper, dateTimeRangeValue.Range.Upper);
          newDateTimeRangeValue.Value = dateTimeRangeValue.Value;
          m_dateTimeRangeValues.Insert (i + 1, newDateTimeRangeValue);
        }
        { // Intersection
          dateTimeRangeValue.Range = intersection;
          dateTimeRangeValue.Value = merge (dateTimeRangeValue.Value, v);
        }
        t = intersection.Upper;
      }
      if (Bound.Compare<DateTime> (t, range.Upper) < 0) {
        var intersection = new UtcDateTimeRange ((LowerBound<DateTime>)t, new UpperBound<DateTime> (null))
          .Intersects (range);
        Debug.Assert (!intersection.IsEmpty ());
        var newDateTimeRangeValue = new DateTimeRangeValue<U> ();
        newDateTimeRangeValue.Range = new UtcDateTimeRange (intersection);
        newDateTimeRangeValue.Value = insert (v);
        m_dateTimeRangeValues.Add (newDateTimeRangeValue);
      }
    }

    /// <summary>
    /// Add a value into the accumulator
    /// </summary>
    /// <param name="range"></param>
    /// <param name="v"></param>
    public void Add (UtcDateTimeRange range, T v)
    {
      Process (range, v, m_insert, m_add);
    }

    /// <summary>
    /// Remove a value from the accumulator
    /// </summary>
    /// <param name="range"></param>
    /// <param name="v"></param>
    public void Remove (UtcDateTimeRange range, T v)
    {
      Process (range, v, m_delete, m_remove);
    }

    /// <summary>
    /// Purge the accumulator
    /// </summary>
    public void Purge ()
    {
      m_dateTimeRangeValues = m_dateTimeRangeValues.Where (t => !m_purge (t.Value)).ToList ();
    }
    #endregion // Methods
  }

  /// <summary>
  /// Class to track the changes on a value
  /// </summary>
  public class ValueChangeTracker<T>
  {
    /// <summary>
    /// Old value
    /// </summary>
    public T Old { get; set; }

    /// <summary>
    /// Is the old value valid ?
    /// </summary>
    public bool OldValid { get; set; }

    /// <summary>
    /// New value
    /// </summary>
    public T New { get; set; }

    /// <summary>
    /// Is the new value valid
    /// </summary>
    public bool NewValid { get; set; }

    /// <summary>
    /// Factory method to create a new ValueChangeTracker that corresponds to a new inserted value
    /// </summary>
    /// <param name="newValue"></param>
    /// <returns></returns>
    public static ValueChangeTracker<T> Insert (T newValue)
    {
      var valueChangeTracker = new ValueChangeTracker<T> ();
      valueChangeTracker.New = newValue;
      valueChangeTracker.NewValid = true;
      return valueChangeTracker;
    }

    /// <summary>
    /// Factory method to create a new ValueChangeTracker that corresponds to a deleted value
    /// </summary>
    /// <param name="oldValue"></param>
    /// <returns></returns>
    public static ValueChangeTracker<T> Delete (T oldValue)
    {
      var valueChangeTracker = new ValueChangeTracker<T> ();
      valueChangeTracker.Old = oldValue;
      valueChangeTracker.OldValid = true;
      return valueChangeTracker;
    }

    /// <summary>
    /// Default constructor (no old or new value)
    /// </summary>
    public ValueChangeTracker ()
    {
      this.OldValid = false;
      this.NewValid = false;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    public ValueChangeTracker (T oldValue, T newValue)
    {
      this.Old = oldValue;
      this.OldValid = true;
      this.New = newValue;
      this.NewValid = true;
    }
  }

  /// <summary>
  /// Accumulator where you store a value between two date/time range
  /// in case the ValueChangeTracker is used to track the changes in the accumulator
  /// </summary>
  public abstract class DateTimeRangeValueChangeTrackerAccumulator<T>
    : DateTimeRangeValueAccumulator<T, ValueChangeTracker<T>>
  {
    /// <summary>
    /// Constructor
    /// </summary>
    protected DateTimeRangeValueChangeTrackerAccumulator ()
      : base (ValueChangeTracker<T>.Insert,
              DateTimeRangeValueChangeTrackerAccumulator<T>.Add,
              ValueChangeTracker<T>.Delete,
              DateTimeRangeValueChangeTrackerAccumulator<T>.Remove,
              a => object.Equals (a.Old, a.New))
    { }

    static ValueChangeTracker<T> Add (ValueChangeTracker<T> previous, T newValue)
    {
      Debug.Assert (!previous.NewValid);

      if (previous.OldValid) {
        return new ValueChangeTracker<T> (previous.Old, newValue);
      }
      else {
        return ValueChangeTracker<T>.Insert (newValue);
      }
    }

    static ValueChangeTracker<T> Remove (ValueChangeTracker<T> previous, T removed)
    {
      Debug.Assert (previous.NewValid);
      Debug.Assert (object.Equals (removed, previous.New));

      if (previous.OldValid) {
        return ValueChangeTracker<T>.Delete (previous.Old);
      }
      else {
        return new ValueChangeTracker<T> (); // Empty
      }
    }
  }
}

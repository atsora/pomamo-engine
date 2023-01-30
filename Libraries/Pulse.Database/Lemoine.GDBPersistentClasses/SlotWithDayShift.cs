// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Basic implementation of ISlotWithDayShift
  /// </summary>
  [Serializable]
  internal sealed class SlotWithDayShift: GenericRangeSlot, ISlot, ISlotWithDayShift
  {
    #region Members
    DateTime? m_day;
    IShift m_shift;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (SlotWithDayShift).FullName);

    #region Constructors and factory methods
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="range"></param>
    public SlotWithDayShift  (UtcDateTimeRange range)
      : base (false, range)
    {
    }
    #endregion // Constructors and factory methods
    
    #region Getters / Setters
    /// <summary>
    /// Reference to a shift
    /// 
    /// Always null
    /// </summary>
    public IShift Shift {
      get { return m_shift; }
      set { m_shift = value; }
    }

    /// <summary>
    /// Reference to the day
    /// </summary>
    public DateTime? Day {
      get { return m_day; }
      set
      {
        Debug.Assert (!value.HasValue || (DateTimeKind.Utc != value.Value.Kind));
        Debug.Assert (!value.HasValue || (0 == value.Value.TimeOfDay.Ticks));
        
        m_day = value;
      }
    }
    #endregion // Getters / Setters
    
    /// <summary>
    /// Combine two ISlotWithDayShift collections
    /// </summary>
    /// <param name="withShift"></param>
    /// <param name="withDay"></param>
    /// <returns></returns>
    public static IList<ISlotWithDayShift> Combine (IEnumerable<ISlotWithDayShift> withShift, IEnumerable<ISlotWithDayShift> withDay)
    {
      IList<ISlotWithDayShift> result = new List<ISlotWithDayShift> ();
      var withShiftIt = withShift.GetEnumerator ();
      var withDayIt = withDay.GetEnumerator ();
      ISlotWithDayShift lastInsertedSlot = null;
      
      if (!withShiftIt.MoveNext () || !withDayIt.MoveNext ()) {
        return result;
      }
      
      while (true) {
        Debug.Assert (null != withShiftIt.Current);
        if (withShiftIt.Current.DateTimeRange.Overlaps (withDayIt.Current.DateTimeRange)) {
          UtcDateTimeRange intersection =
            new UtcDateTimeRange (withShiftIt.Current.DateTimeRange.Intersects (withDayIt.Current.DateTimeRange));
          // Check if the previous inserted slot can be extended instead (for performance reasons)
          if ( (null != lastInsertedSlot)
              && object.Equals (lastInsertedSlot.Day, withDayIt.Current.Day)
              && object.Equals (lastInsertedSlot.Shift, withShiftIt.Current.Shift)
              && lastInsertedSlot.DateTimeRange.IsAdjacentTo (intersection)) { // Ok !
            // Update just lastInsertedSlot
            Debug.Assert (lastInsertedSlot.DateTimeRange.IsStrictlyLeftOf (intersection));
            lastInsertedSlot.EndDateTime = intersection.Upper; // Extend the last inserted slot
          }
          else { // Insert a new SlotWithDayShift
            lastInsertedSlot = new SlotWithDayShift (intersection);
            lastInsertedSlot.Shift = withShiftIt.Current.Shift;
            lastInsertedSlot.Day = withDayIt.Current.Day;
            result.Add (lastInsertedSlot);
          }
        }
        if (Bound.Compare<DateTime> (withShiftIt.Current.EndDateTime, withDayIt.Current.EndDateTime) < 0) {
          if (!withShiftIt.MoveNext ()) {
            return result;
          }
        }
        else {
          if (!withDayIt.MoveNext ()) {
            return result;
          }
        }
      }
    }
    
    #region Slot implementation
    /// <summary>
    /// <see cref="Slot.GetLogger" />
    /// </summary>
    /// <returns></returns>
    protected override ILog GetLogger()
    {
      return log;
    }

    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override int CompareTo(object obj)
    {
      if (obj is ISlotWithDayShift) {
        var other = (ISlotWithDayShift) obj;
        return this.BeginDateTime.CompareTo (other.BeginDateTime);
      }
      
      GetLogger ().ErrorFormat ("CompareTo: " +
                                "object {0} of invalid type",
                                obj);
      throw new ArgumentException ("object is not a DaySlot");
    }

    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(ISlotWithDayShift other)
    {
      return this.BeginDateTime.CompareTo (other.BeginDateTime);
    }
    
    
    /// <summary>
    /// <see cref="Slot.ReferenceDataEquals" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool ReferenceDataEquals (ISlot obj)
    {
      ISlotWithDayShift other = obj as ISlotWithDayShift;
      if (other == null) {
        return false;
      }

      return object.Equals (this.Day, other.Day)
        && NHibernateHelper.EqualsNullable (this.Shift, other.Shift, (a, b) => a.Id == b.Id);
    }

    /// <summary>
    /// <see cref="Slot.IsEmpty" />
    /// </summary>
    /// <returns></returns>
    public override bool IsEmpty ()
    {
      // No gap is wished
      // Return false even when the day is null
      return false;
    }
    
    /// <summary>
    /// <see cref="Slot.HandleAddedSlot" />
    /// </summary>
    public override void HandleAddedSlot ()
    {
      // There is nothing to do for this slot
    }
    
    /// <summary>
    /// <see cref="Slot.HandleRemovedSlot" />
    /// </summary>
    public override void HandleRemovedSlot ()
    {
      // There is nothing to do for this slot
    }
    
    /// <summary>
    /// <see cref="Slot.HandleModifiedSlot" />
    /// </summary>
    /// <param name="oldSlot"></param>
    public override void HandleModifiedSlot (ISlot oldSlot)
    {
      // There is nothing to do for this slot
    }
    #endregion // Slot implementation
    
    /// <summary>
    /// <see cref="Object.ToString" />
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return $"[SlotWithDayShift Range={this.DateTimeRange}]";
    }
  }
}

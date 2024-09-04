// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace WizardCreateProduction
{
  /// <summary>
  /// List of shifts
  /// </summary>
  public class ItemDataListShift
  {
    #region Members
    IList<ItemDataShift> m_shifts = new List<ItemDataShift>();
    IList<ItemDataShift> m_shiftsTmp = null;
    #endregion // Members

    #region Getters / Setters
    
    /// <summary>
    /// Get ShiftSlot
    /// </summary>
    /// <returns></returns>
    public IList<ItemDataShift> Shifts { get { return m_shifts; } }
    
    /// <summary>
    /// End datetime of the first recurrence, used to determine the beginning of
    /// each set of shiftslots in a recurrence
    /// </summary>
    public DateTime EndDateTimeFirstRecurrence { get; set; }
    
    /// <summary>
    /// Start datetime of the first recurrence, used to determine the end of 
    /// each set of shiftslots in a recurrence
    /// </summary>
    public DateTime StartDateTimeFirstRecurrence { get; set; }
    
    /// <summary>
    /// Return true if at least one shift comprises quantities > 0
    /// </summary>
    public bool HasQuantities {
      get {
        foreach (ItemDataShift shift in m_shifts) {
          if (shift.m_enabled && shift.Count > 0) {
            return true;
          }
        }
        return false;
      }
    }
    
    /// <summary>
    /// Just a way to store a value
    /// </summary>
    public int ConflictNumberWithRecurrences { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ItemDataListShift()
    {
      ConflictNumberWithRecurrences = 0;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get the different shift after having taken into account the recurrence
    /// </summary>
    /// <param name="recurrenceType">0: no recurrence, 1: daily, 2: weekly</param>
    /// <param name="endDate">End of recurrence, if any</param>
    /// <param name="days">Working days in case of a daily recurrence</param>
    /// <returns>list of list of shifts</returns>
    public IList<IList<ItemDataShift>> GetRecurrences(int recurrenceType, DateTime endDate, int days)
    {
      // Specify the start dateTime and end dateTime
      foreach (ItemDataShift shift in m_shifts) {
        shift.m_startPeriod = StartDateTimeFirstRecurrence;
        shift.m_endPeriod = EndDateTimeFirstRecurrence;
      }
      
      IList<IList<ItemDataShift>> recurrences;
      
      switch (recurrenceType) {
        case 1:
          recurrences = GetDailyRecurrences(endDate, days);
          break;
        case 2:
          recurrences = GetWeeklyRecurrences(endDate);
          break;
        default:
          recurrences = GetNoRecurrence();
          break;
      }
      
      return recurrences;
    }
    
    /// <summary>
    /// Clear all shifts
    /// </summary>
    public void PrepareNewInsertion()
    {
      m_shiftsTmp = m_shifts;
      m_shifts = new List<ItemDataShift>();
    }
    
    /// <summary>
    /// Add a shift in the list
    /// </summary>
    /// <param name="shift">Shift to add</param>
    public void Add(ItemDataShift shift)
    {
      // Try to find a previous shift already configured
      ItemDataShift shiftTmp = FindOldShift(shift);
      if (shiftTmp == null) {
        m_shifts.Add(shift);
      }
      else {
        m_shifts.Add(shiftTmp);
      }
    }
    
    /// <summary>
    /// Modify the number of operation for each shift
    /// </summary>
    /// <param name="numberOfOperation">number of operations</param>
    /// <param name="defaultQuantity">default quantity if operations are added</param>
    public void SetOperationCount(int numberOfOperation, int defaultQuantity)
    {
      int enabledShiftNumber = 0;
      foreach (ItemDataShift dataShift in m_shifts) {
        if (dataShift.m_enabled) {
          enabledShiftNumber++;
        }
      }

      if (enabledShiftNumber > 0) {
        defaultQuantity = (int)(((double)defaultQuantity / (double)enabledShiftNumber) + 0.5);
      }

      foreach (ItemDataShift shift in m_shifts) {
        while (shift.m_quantities.Count > numberOfOperation) {
          shift.m_quantities.RemoveAt(shift.m_quantities.Count - 1);
        }

        while (shift.m_quantities.Count < numberOfOperation) {
          shift.m_quantities.Add(defaultQuantity);
        }
      }
    }
    
    IList<IList<ItemDataShift>> GetNoRecurrence()
    {
      IList<IList<ItemDataShift>> recurrences = new List<IList<ItemDataShift>>();
      
      // Only one series of Shifts
      IList<ItemDataShift> shifts = new List<ItemDataShift>();
      foreach (ItemDataShift shift in m_shifts) {
        if (shift.m_enabled) {
          shifts.Add(shift);
        }
      }

      recurrences.Add(shifts);
      
      return recurrences;
    }
    
    IList<IList<ItemDataShift>> GetDailyRecurrences(DateTime endDate, int days)
    {
      IList<IList<ItemDataShift>> recurrences = new List<IList<ItemDataShift>>();
      if (m_shifts.Count == 0) {
        return recurrences;
      }

      IList<ItemDataShift> currentRecurrence = new List<ItemDataShift>();
      int indexShift = 0;
      DateTime currentDateTime = m_shifts[indexShift].m_startPeriod;
      int offsetDay = 0;
      while (currentDateTime < endDate)
      {
        if (m_shifts[indexShift].m_enabled && IsDayEnabled(currentDateTime, days)) {
          currentRecurrence.Add(m_shifts[indexShift].AddDays(offsetDay));
        }

        // Go on
        indexShift++;
        if (indexShift >= m_shifts.Count) {
          indexShift = 0;
          if (currentRecurrence.Count > 0) {
            recurrences.Add(currentRecurrence);
          }

          currentRecurrence = new List<ItemDataShift>();
          offsetDay++;
        }
        currentDateTime = m_shifts[indexShift].m_startPeriod.AddDays(offsetDay);
      }
      if (currentRecurrence.Count > 0) {
        recurrences.Add(currentRecurrence);
      }

      return recurrences;
    }
    
    IList<IList<ItemDataShift>> GetWeeklyRecurrences(DateTime endDate)
    {
      IList<IList<ItemDataShift>> recurrences = new List<IList<ItemDataShift>>();
      if (m_shifts.Count == 0) {
        return recurrences;
      }

      IList<ItemDataShift> currentRecurrence = new List<ItemDataShift>();
      int indexShift = 0;
      DateTime currentDateTime = m_shifts[indexShift].m_startPeriod;
      int offsetWeek = 0;
      while (currentDateTime < endDate)
      {
        currentRecurrence.Add(m_shifts[indexShift].AddDays(7 * offsetWeek));
        
        // Go on
        indexShift++;
        if (indexShift >= m_shifts.Count) {
          indexShift = 0;
          if (currentRecurrence.Count > 0) {
            recurrences.Add(currentRecurrence);
          }

          currentRecurrence = new List<ItemDataShift>();
          offsetWeek++;
        }
        currentDateTime = m_shifts[indexShift].m_startPeriod.AddDays(7 * offsetWeek);
      }
      if (currentRecurrence.Count > 0) {
        recurrences.Add(currentRecurrence);
      }

      return recurrences;
    }
    
    bool IsDayEnabled(DateTime date, int dayFlags)
    {
      // Initially 0 => sunday, 6 => saturday.
      // We want 1 => monday, 7 => sunday
      // Then 1, 2, 4, ...
      int day = (int)date.DayOfWeek;
      if (day == 0) {
        day += 7;
      }

      day = 0x01 << day;
      
      return (day & dayFlags) != 0;
    }
    
    ItemDataShift FindOldShift(ItemDataShift shift)
    {
      foreach (ItemDataShift shiftTmp in m_shiftsTmp) {
        if (shiftTmp.m_shiftID == shift.m_shiftID && shiftTmp.m_day == shift.m_day) {
          return shiftTmp;
        }
      }
      return null;
    }
    
    /// <summary>
    /// Textual description of data
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      string text = "\nStart datetime first recurrence = " + StartDateTimeFirstRecurrence + "\n"
         + "End datetime first recurrence = " + EndDateTimeFirstRecurrence;
      
      foreach (ItemDataShift datashift in m_shifts) {
        if (datashift.m_enabled) {
          text += "\n" + datashift.ToString();
        }
      }

      return text;
    }
    #endregion // Methods
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace WizardCreateProduction
{
  /// <summary>
  /// Description of StructShift.
  /// </summary>
  public class ItemDataShift
  {
    #region Members
    public IList<int> m_quantities = new List<int>();
    public int m_shiftID = 0;
    public string m_shiftDisplay = "";
    public DateTime m_day;
    public DateTime m_startPeriod;
    public DateTime m_endPeriod;
    public bool m_enabled = true;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Information regarding a potential conflict with another production
    /// </summary>
    public bool Conflict { get; set; }
    
    /// <summary>
    /// Return the sum of the parts to produce
    /// </summary>
    public int Count {
      get {
        int count = 0;
        
        foreach (int n in m_quantities) {
          count += n;
        }

        return count;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ItemDataShift(int shiftID, string shiftDisplay, DateTime day)
    {
      m_shiftID = shiftID;
      m_shiftDisplay = shiftDisplay;
      m_day = day;
      Conflict = false;
    }
    
    /// <summary>
    /// Return a new DataShift after having added a number of days
    /// </summary>
    /// <param name="days">number of days to add</param>
    /// <returns></returns>
    public ItemDataShift AddDays(int days)
    {
      ItemDataShift newItem = new ItemDataShift(m_shiftID, m_shiftDisplay, m_day.AddDays(days));
      foreach (int quantity in m_quantities) {
        newItem.m_quantities.Add(quantity);
      }

      newItem.m_endPeriod = m_endPeriod.AddDays(days);
      newItem.m_startPeriod = m_startPeriod.AddDays(days);
      newItem.m_enabled = m_enabled;
      
      return newItem;
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// Textual description of data
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return string.Format("  Day={0}, ShiftId = {1}\n  Quantities={2}\n  StartPeriod={3}, EndPeriod={4}",
                           m_day, m_shiftID, m_quantities, m_startPeriod, m_endPeriod);
    }
    #endregion // Methods
  }
}

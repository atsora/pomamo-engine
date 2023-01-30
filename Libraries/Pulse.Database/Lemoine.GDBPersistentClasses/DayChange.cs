// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Modification class to easily update the day range
  /// 
  /// This is not a persistent class, no database table is associated to it !
  /// </summary>
  public class DayChange: PeriodAssociation
  {
    IDayTemplate m_dayTemplate;
    DateTime m_day;

    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected DayChange ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dayTemplate"></param>
    /// <param name = "day"></param>
    /// <param name = "range">range</param>
    /// <param name = "mainModification"></param>
    internal protected DayChange (IDayTemplate dayTemplate,
                                  DateTime day,
                                  UtcDateTimeRange range,
                                  IModification mainModification)
      : base (range, mainModification)
    {
      m_dayTemplate = dayTemplate;
      m_day = day;
    }
    
    /// <summary>
    /// Reference to the Day Template
    /// </summary>
    public virtual IDayTemplate DayTemplate {
      get { return m_dayTemplate; }
    }
    
    /// <summary>
    /// Reference to the Day
    /// </summary>
    public virtual DateTime Day {
      get { return m_day; }
    }

    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    public override string ModificationType
    {
      get { return "DayChange"; }
    }

    /// <summary>
    /// Get the impacted activity analysis
    /// so that the activity analysis makes a pause
    /// </summary>
    /// <returns></returns>
    public override IList<IMachine> GetImpactedActivityAnalysis ()
    {
      return null;
    }
    
    /// <summary>
    /// <see cref="PeriodAssociation.ConvertToSlot" />
    /// </summary>
    /// <returns></returns>
    public override TSlot ConvertToSlot<TSlot> ()
    {
      if (typeof (TSlot).Equals (typeof (DaySlot))) {
        IDaySlot daySlot = ModelDAOHelper.ModelFactory
          .CreateDaySlot (this.DayTemplate, this.Range);
        daySlot.Day = this.Day;
        return (TSlot) daySlot;
      }
      else if (typeof (TSlot).Equals (typeof (ShiftSlot))) {
        IShiftSlot shiftSlot = ModelDAOHelper.ModelFactory
          .CreateShiftSlot (null,
                            this.Range);
        shiftSlot.Day = this.Day;
        shiftSlot.Shift = null;
        return (TSlot) shiftSlot;
      }
      else {
        System.Diagnostics.Debug.Assert (false);
        throw new NotImplementedException ("Slot type not implemented");
      }
    }

    /// <summary>
    /// <see cref="PeriodAssociation.MergeDataWithOldSlot" />
    /// </summary>
    /// <param name="oldSlot"></param>
    /// <param name="range">merge period range</param>
    /// <returns></returns>
    public override TSlot MergeDataWithOldSlot<TSlot>(TSlot oldSlot,
                                                      UtcDateTimeRange range)
    {
      Debug.Assert (null != oldSlot);
      Debug.Assert (oldSlot is GenericSlot || oldSlot is GenericRangeSlot);
      
      if (oldSlot is DaySlot) {
        IDaySlot oldDaySlot = oldSlot as DaySlot;
        
        IDaySlot newDaySlot = (DaySlot) oldDaySlot.Clone ();
        newDaySlot.Day = this.Day;
        newDaySlot.DayTemplate = this.DayTemplate;
        
        return (TSlot) newDaySlot;
      }
      else if (oldSlot is ShiftSlot) {
        IShiftSlot oldShiftSlot = oldSlot as ShiftSlot;
        
        IShiftSlot newShiftSlot = (ShiftSlot) oldShiftSlot.Clone ();
        if (!object.Equals (newShiftSlot.Day, this.Day)) {
          newShiftSlot.Day = this.Day;
          newShiftSlot.Shift = null;
          newShiftSlot.TemplateProcessed = false;
        }
        
        return (TSlot) newShiftSlot;
      }
      else {
        System.Diagnostics.Debug.Assert (false);
        log.FatalFormat ("MergeData: " +
                         "trying to merge the association with a not supported slot {0}",
                         typeof (TSlot));
        throw new ArgumentException ("Not supported slot");
      }
    }
    
    /// <summary>
    /// MakeAnalysis
    /// 
    /// Not valid because this modification is always transient
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (false);
      log.FatalFormat ("MakeAnalysis: not valid");
      throw new NotImplementedException ("DayChange.MakeAnalysis");
    }
    
    /// <summary>
    /// Apply the modification while keeping it transient
    /// </summary>
    public override void Apply ()
    {
      {
        DaySlotDAO daySlotDAO = new DaySlotDAO ();
        daySlotDAO.Caller = this;
        Insert<DaySlot, IDaySlot, DaySlotDAO> (daySlotDAO);
      }

      {
        ShiftSlotDAO shiftSlotDAO = new ShiftSlotDAO ();
        shiftSlotDAO.Caller = this;
        Insert<ShiftSlot, IShiftSlot, ShiftSlotDAO> (shiftSlotDAO);
      }
    }
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      string modificationType = this.ModificationType;
    }
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Modification class to easily update the shift range.
  /// This must be used only to process a shift template
  /// 
  /// This is not a persistent class, no database table is associated to it !
  /// </summary>
  public class ShiftChange: PeriodAssociation
  {
    #region Members
    DateTime m_day;
    IShift m_shift;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected ShiftChange ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name = "day"></param>
    /// <param name = "shift"></param>
    /// <param name = "range">range</param>
    /// <param name = "mainModification"></param>
    internal protected ShiftChange (DateTime day,
                                    IShift shift,
                                    UtcDateTimeRange range,
                                    IModification mainModification)
      : base (range, mainModification)
    {
      Debug.Assert (object.Equals (day.Date, day));
      Debug.Assert (DateTimeKind.Utc != day.Kind);
      
      m_day = day;
      m_shift = shift;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    public override string ModificationType
    {
      get { return "ShiftChange"; }
    }

    /// <summary>
    /// Reference to the day
    /// </summary>
    public virtual DateTime Day {
      get { return m_day; }
    }
    
    /// <summary>
    /// Reference to the Shift
    /// </summary>
    public virtual IShift Shift {
      get { return m_shift; }
    }
    #endregion // Getters / Setters

    #region Modification implementation
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
      if (typeof (TSlot).Equals (typeof (ShiftSlot))) {
        IShiftSlot shiftSlot = ModelDAOHelper.ModelFactory
          .CreateShiftSlot (null,
                            this.Range);
        shiftSlot.Day = this.Day;
        shiftSlot.Shift = this.Shift;
        shiftSlot.TemplateProcessed = true;
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
      
      if (oldSlot is ShiftSlot) {
        IShiftSlot oldShiftSlot = oldSlot as ShiftSlot;
        IShiftSlot newShiftSlot = (IShiftSlot) oldShiftSlot.Clone ();
        newShiftSlot.Day = this.Day;
        newShiftSlot.Shift = this.Shift;
        newShiftSlot.TemplateProcessed = true;
        
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
        ShiftSlotDAO shiftSlotDAO = new ShiftSlotDAO ();
        shiftSlotDAO.Caller = this;
        Insert<ShiftSlot, IShiftSlot, ShiftSlotDAO> (shiftSlotDAO);
      }
    }
    #endregion // Modification implementation
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      NHibernateHelper.Unproxy<IShift> (ref m_shift);
    }
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;

using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Abstract base class for all the analysis slot persistent classes that is using the new DateTimeRange type
  /// </summary>
  public abstract class RangeSlot: Slot, ISlot, Lemoine.Threading.IChecked
  {
    readonly ILog log = LogManager.GetLogger(typeof (RangeSlot).FullName);

    #region Members
    bool m_dayColumns;
    /// <summary>
    /// Associated UTC date/time range
    /// </summary>
    protected UtcDateTimeRange m_dateTimeRange = new UtcDateTimeRange (new LowerBound<DateTime> (null), new UpperBound<DateTime> (null));
    /// <summary>
    /// Associated day range, only if there is a day column
    /// </summary>
    protected DayRange m_dayRange; // If m_dayColumns == true
    #endregion // Members
    
    #region Constructors
    /// <summary>
    /// Default constructor (forbidden outside this library)
    /// </summary>
    /// <param name="dayColumns"></param>
    protected RangeSlot (bool dayColumns)
    {
      m_dayColumns = dayColumns;
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dayColumns"></param>
    /// <param name="range"></param>
    protected RangeSlot(bool dayColumns, UtcDateTimeRange range)
    {
      m_dayColumns = dayColumns;
      SetDateTimeDayRange (range);
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Are day columns used ?
    /// </summary>
    public virtual bool DayColumns {
      get { return m_dayColumns; }
    }
    
    /// <summary>
    /// Date/time range of the slot
    /// </summary>
    [XmlIgnore]
    public override UtcDateTimeRange DateTimeRange {
      get { return m_dateTimeRange; }
      set { UpdateDateTimeRange (value); }
    }
    
    /// <summary>
    /// Begin date/time of the slot
    /// </summary>
    [XmlIgnore]
    public override LowerBound<DateTime> BeginDateTime {
      get { return m_dateTimeRange.Lower; }
      protected set { throw new NotImplementedException (); } // TODO: remove it once BeginEndSlot is removed
    }
    
    /// <summary>
    /// Optionally end date/time of the slot
    /// </summary>
    [XmlIgnore]
    public override UpperBound<DateTime> EndDateTime {
      get { return m_dateTimeRange.Upper; }
      set
      {
        if (object.Equals (value, this.EndDateTime)) {
          // No change
          return;
        }
        
        this.DateTimeRange = new UtcDateTimeRange (m_dateTimeRange.Lower, value);
      }
    }
    
    /// <summary>
    /// Day range of the slot
    /// </summary>
    public override DayRange DayRange {
      get
      {
        if (m_dayColumns) {
          return m_dayRange;
        }
        else {
          return ModelDAOHelper.DAOFactory.DaySlotDAO
            .ConvertToDayRange (this.DateTimeRange);
        }
      }
    }
    
    /// <summary>
    /// Begin day (from cut-off time) of the slot
    /// </summary>
    public override LowerBound<DateTime> BeginDay {
      get { return this.DayRange.Lower; }
    }
    
    /// <summary>
    /// Optionally end day of the slot
    /// </summary>
    public override UpperBound<DateTime> EndDay {
      get { return this.DayRange.Upper; }
    }
    
    /// <summary>
    /// Duration of the slot
    /// </summary>
    [XmlIgnore]
    public override TimeSpan? Duration {
      get { return m_dateTimeRange.Duration; }
      // disable once ValueParameterNotUsed
      protected set { } // For NHibernate
    }
    #endregion // Getters / Setters

    #region Methods
    /// <summary>
    /// Set only m_dateTimeRange
    /// </summary>
    /// <param name="newRange"></param>
    protected void SetDateTimeRange (UtcDateTimeRange newRange)
    {
      m_dateTimeRange = newRange;
    }
    
    /// <summary>
    /// Set both m_dateTimeRange and m_dayRange in the same time
    /// </summary>
    /// <param name="newRange"></param>
    protected void SetDateTimeDayRange (UtcDateTimeRange newRange)
    {
      m_dateTimeRange = newRange;
      
      if (m_dayColumns) {
        m_dayRange = ModelDAOHelper.DAOFactory.DaySlotDAO
          .ConvertToDayRange (m_dateTimeRange);
      }
    }
    
    /// <summary>
    /// Set a new date/time range
    /// </summary>
    /// <param name="newRange">not empty</param>
    public override void UpdateDateTimeRange (UtcDateTimeRange newRange)
    {
      Debug.Assert (!newRange.IsEmpty ()); // For the moment, this case is not coded
      if (newRange.IsEmpty ()) {
        log.FatalFormat ("UpdateDateTimeRange: newRange is empty. StackTrace:{0}",
          System.Environment.StackTrace);
      }
      
      if (object.Equals (newRange, m_dateTimeRange)) {
        // No change
        return;
      }
      
      SetDateTimeDayRange (newRange);
    }
    
    /// <summary>
    /// Merge the next slot with this when:
    /// <item>it comes right after this</item>
    /// <item>the reference data are the same</item>
    /// </summary>
    /// <param name="nextSlot"></param>
    public override void Merge (ISlot nextSlot)
    {
      Debug.Assert (null != nextSlot);
      Debug.Assert (nextSlot.ReferenceDataEquals (this));
      Debug.Assert (this.DateTimeRange.IsAdjacentTo (nextSlot.DateTimeRange));
      this.DateTimeRange = new UtcDateTimeRange (this.DateTimeRange.Union (nextSlot.DateTimeRange));
    }    
    #endregion // Methods
  }
}

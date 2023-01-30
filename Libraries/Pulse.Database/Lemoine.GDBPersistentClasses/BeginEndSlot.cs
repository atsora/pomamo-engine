// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Abstract base class for all the analysis slot persistent classes
  /// 
  /// A slot is made of the following properties:
  /// <item>UTC begin date/time</item>
  /// <item>Begin day (from cut-off time)</item>
  /// <item>Optionally a UTC end date/time</item>
  /// <item>Optionally an end day (from cut-off time)</item>
  /// </summary>
  public abstract class BeginEndSlot: Slot, ISlot, Lemoine.Threading.IChecked
  {
    static readonly ILog log = LogManager.GetLogger(typeof (BeginEndSlot).FullName);

    #region Members
    /// <summary>
    /// Start date/time
    /// </summary>
    protected LowerBound<DateTime> m_beginDateTime;
    /// <summary>
    /// Start day
    /// </summary>
    protected LowerBound<DateTime> m_beginDay;
    /// <summary>
    /// End date/time
    /// </summary>
    protected UpperBound<DateTime> m_endDateTime;
    /// <summary>
    /// End day
    /// </summary>
    protected UpperBound<DateTime> m_endDay;
    #endregion // Members
    
    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected BeginEndSlot()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="range"></param>
    protected BeginEndSlot (UtcDateTimeRange range)
    {
      SetInitialDateTimeRange (range);
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Date/time range of the slot
    /// </summary>
    public override UtcDateTimeRange DateTimeRange {
      get { return new UtcDateTimeRange (this.BeginDateTime, this.EndDateTime); }
      set
      {
        if (value.IsEmpty ()) {
          log.ErrorFormat ("DateTimeRange.set: " +
                           "value is empty. " +
                           "StackTrace: {0}",
                           System.Environment.StackTrace);
        }
        
        if (!Bound.Equals<DateTime> (value.Lower, this.BeginDateTime)) {
          this.BeginDateTime = value.Lower;
        }
        if (!Bound.Equals<DateTime> (value.Upper, this.EndDateTime)) {
          this.EndDateTime = value.Upper;
        }
      }
    }
    
    /// <summary>
    /// Begin date/time of the slot
    /// </summary>
    public override LowerBound<DateTime> BeginDateTime {
      get { return m_beginDateTime; }
      protected set { SetBeginDateTime (value); }
    }
    
    /// <summary>
    /// Optionally end date/time of the slot
    /// </summary>
    public override UpperBound<DateTime> EndDateTime {
      get { return m_endDateTime; }
      set
      {
        if (Bound.Compare<DateTime> (value, m_beginDateTime) <= 0) {
          log.ErrorFormat ("EndDateTime.set: " +
                           "value {0} is before begin {1}",
                           value, m_beginDateTime);
        }
        
        if (object.Equals (value, m_endDateTime)) {
          // No change
          return;
        }
        
        SetEndDateTime (value);
      }
    }
    
    /// <summary>
    /// Day range of the slot
    /// </summary>
    public override DayRange DayRange {
      get { return new DayRange (this.BeginDay, this.EndDay); }
    }
    
    /// <summary>
    /// Begin day (from cut-off time) of the slot
    /// </summary>
    public override LowerBound<DateTime> BeginDay {
      get { return m_beginDay; }
    }
    
    /// <summary>
    /// Optionally end day of the slot
    /// </summary>
    public override UpperBound<DateTime> EndDay {
      get { return m_endDay; }
    }
    
    /// <summary>
    /// Duration of the slot
    /// </summary>
    public override TimeSpan? Duration {
      get
      {
        return this.DateTimeRange.Duration;
      }
      // disable once ValueParameterNotUsed
      protected set { } // For NHibernate
    }
    #endregion // Getters / Setters

    #region Methods
    /// <summary>
    /// Set m_beginDateTime and m_beginDay in the same time
    /// </summary>
    protected void SetBeginDateTime (LowerBound<DateTime> beginDateTime)
    {
      m_beginDateTime = beginDateTime;
      m_beginDay = new LowerBound<DateTime> (beginDateTime.HasValue
                                             ? ModelDAOHelper.DAOFactory.TimeConfigDAO.GetDay (m_beginDateTime.Value)
                                             : (DateTime?) null);
    }
    
    /// <summary>
    /// Set m_endDateTime and m_endDay in the same time
    /// </summary>
    /// <param name="endDateTime"></param>
    protected void SetEndDateTime (UpperBound<DateTime> endDateTime)
    {
      if (Bound.Compare<DateTime> (endDateTime, m_beginDateTime) <= 0) {
        log.ErrorFormat ("SetEndDateTime: " +
                         "new end date/time {0} is before begin {1}",
                         endDateTime, m_beginDateTime);
      }
      
      m_endDateTime = endDateTime;
      m_endDay = new UpperBound<DateTime> (endDateTime.HasValue
                                           ? ModelDAOHelper.DAOFactory.TimeConfigDAO.GetEndDay (endDateTime.Value)
                                           : (DateTime?) null);
    }
    
    /// <summary>
    /// Set the initial date/time range
    /// </summary>
    /// <param name="initialRange"></param>
    void SetInitialDateTimeRange (Range<DateTime> initialRange)
    {
      if (initialRange.IsEmpty ()) {
        log.ErrorFormat ("SetInitialDateTimeRange: " +
                         "empty initialRange. " +
                         "StackTrace: {0}",
                         System.Environment.StackTrace);
      }

      SetBeginDateTime (initialRange.Lower);
      SetEndDateTime (initialRange.Upper);
    }
    
    /// <summary>
    /// Set a new date/time range
    /// </summary>
    /// <param name="newRange"></param>
    public override void UpdateDateTimeRange (UtcDateTimeRange newRange)
    {
      if (newRange.IsEmpty ()) {
        log.ErrorFormat ("UpdateDateTimeRange: " +
                         "empty newRange. " +
                         "StackTrace: {0}",
                         System.Environment.StackTrace);
      }
      Debug.Assert (!newRange.IsEmpty ());
      
      if (!this.DateTimeRange.Equals (newRange)) {
        SetBeginDateTime (newRange.Lower);
        SetEndDateTime (newRange.Upper);
      }
    }
    #endregion // Methods
  }
}

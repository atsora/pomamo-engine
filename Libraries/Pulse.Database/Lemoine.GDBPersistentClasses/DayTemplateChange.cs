// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table DayTemplateChange
  /// 
  /// This new table is designed to add any DayTemplate change
  /// 
  /// It does not represent the current DayTemplate ,
  /// but all the manual or automatic DayTemplate changes that have been made.
  /// 
  /// To know the current DayTemplates, the table DayTemplateSlot
  /// that is filled in by the Analyzer must be used.
  /// </summary>
  [Serializable]
  public class DayTemplateChange: PeriodAssociation, IDayTemplateChange
  {
    #region Members
    IDayTemplate m_dayTemplate;
    bool m_force;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected DayTemplateChange ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dayTemplate">not null</param>
    /// <param name="beginDateTime">UTC begin date/time</param>
    internal protected DayTemplateChange (IDayTemplate dayTemplate,
                                          DateTime beginDateTime)
      : base (beginDateTime)
    {
      Debug.Assert (null != dayTemplate);
      if (null == dayTemplate) {
        log.FatalFormat ("DayTemplateChange: " +
                         "null dayTemplate");
        throw new ArgumentNullException ("dayTemplate");
      }
      
      m_dayTemplate = dayTemplate;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "DayTemplateChange"; }
    }

    /// <summary>
    /// Reference to the DayTemplate
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual IDayTemplate DayTemplate {
      get
      {
        Debug.Assert (null != m_dayTemplate);
        return m_dayTemplate;
      }
    }
    
    /// <summary>
    /// Reference to the DayTemplate for Xml Serialization
    /// </summary>
    [XmlElement("DayTemplate")]
    public virtual DayTemplate XmlSerializationDayTemplate {
      get { return this.DayTemplate as DayTemplate; }
      set { m_dayTemplate = value; }
    }
    
    /// <summary>
    /// Force re-building the day in case there is no day template change
    /// 
    /// Default is False
    /// </summary>
    [XmlIgnore]
    public virtual bool Force {
      get { return m_force; }
      set { m_force = value; }
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
      Debug.Assert (null != m_dayTemplate);
      
      if (typeof (TSlot).Equals (typeof (DayTemplateSlot))) {
        IDayTemplateSlot dayTemplateSlot = ModelDAO.ModelDAOHelper.ModelFactory
          .CreateDayTemplateSlot (this.DayTemplate,
                                  new UtcDateTimeRange (this.Begin, this.End));
        return (TSlot) dayTemplateSlot;
      }
      else if (typeof (TSlot).Equals (typeof (DaySlot))) {
        IDaySlot daySlot = ModelDAO.ModelDAOHelper.ModelFactory
          .CreateDaySlot (this.DayTemplate,
                          new UtcDateTimeRange (this.Begin, this.End));
        return (TSlot) daySlot;
      }
      else {
        Debug.Assert (false);
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
      Debug.Assert (null != this.DayTemplate);
      
      if (oldSlot is IDayTemplateSlot) {
        var oldDayTemplateSlot = oldSlot as IDayTemplateSlot;
        
        var newDayTemplateSlot = (IDayTemplateSlot) oldDayTemplateSlot.Clone ();
        newDayTemplateSlot.DayTemplate = this.DayTemplate;
        
        return (TSlot) newDayTemplateSlot;
      }
      else if (oldSlot is IDaySlot) {
        var oldDaySlot = oldSlot as IDaySlot;
        
        if (object.Equals (oldDaySlot.DayTemplate, this.DayTemplate) && !Force) { // Keep the existing day
          var newDaySlot = (IDaySlot) oldDaySlot.Clone ();
          return (TSlot) newDaySlot;
        }
        else {
          IDaySlot daySlot = ModelDAO.ModelDAOHelper.ModelFactory
            .CreateDaySlot (this.DayTemplate,
                            range);
          return (TSlot) daySlot;
        }
      }
      else {
        Debug.Assert (false);
        log.FatalFormat ("MergeData: " +
                         "trying to merge the association with a not supported slot {0}",
                         typeof (TSlot));
        throw new ArgumentException ("Not supported slot");
      }
    }
    
    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());
      
      if (Bound.Compare<DateTime> (this.End, this.Begin) < 0) { // Empty period: error
        string message = string.Format ("End={0} before Begin={1}",
                                        this.End, this.Begin);
        log.ErrorFormat ("MakeAnalysis: " +
                         "{0} " +
                         "=> finish in error",
                         message);
        AddAnalysisLog(LogLevel.ERROR, message);
        MarkAsError ();
        return;
      }

      // 1. Adjust the time begin to a day change
      if (this.Range.Lower.HasValue) {
        DateTime adjustedBegin = ((DayTemplate)this.DayTemplate).AdjustToDayBegin (this.Range.Lower.Value);
        if (!this.Range.Lower.Value.Equals (adjustedBegin)) { // Adjust the begin date/time
          // Check if the previous day must be extended
          DateTime adjustedLocalBegin = adjustedBegin.ToLocalTime ();
          DateTime firstNewDay;
          if (adjustedLocalBegin.TimeOfDay <= TimeSpan.FromHours (12)) { // Positive cut-off
            firstNewDay = adjustedLocalBegin.Date;
          }
          else { // Negative cut-off
            firstNewDay = adjustedLocalBegin.Date.AddDays (1);
          }
          DateTime dayToExtend = firstNewDay.AddDays (-1).Date;
          IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindProcessedByDay (dayToExtend);
          if ((null != daySlot)
              && (Bound.Compare<DateTime> (daySlot.DateTimeRange.Upper, adjustedBegin) < 0)) { // Extend the day
            var range = new UtcDateTimeRange (daySlot.BeginDateTime, adjustedBegin);
            var dayChange = new DayChange (daySlot.DayTemplate, dayToExtend, range, this);
            dayChange.Apply ();
            AnalysisAccumulator.PushMessage ("Cache/ClearDomain/DayChange?Broadcast=true");
          }
          
          // Make the change from adjustedBegin
          var newDayTemplateChange = ModelDAOHelper.ModelFactory
            .CreateDayTemplateChange (this.DayTemplate, adjustedBegin);
          newDayTemplateChange.End = this.End;
          newDayTemplateChange.DateTime = this.DateTime;
          ModelDAOHelper.DAOFactory.DayTemplateChangeDAO.MakePersistent (newDayTemplateChange);
          newDayTemplateChange.Parent = this.MainModification ?? this;
          newDayTemplateChange.Priority = this.StatusPriority;
          MarkAsCompleted("");
          return;
        }
      }
      
      // 2. Adjust the time end to a day change
      if (this.Range.Upper.HasValue) {
        DateTime adjustedEnd = ((DayTemplate)this.DayTemplate).AdjustToDayBegin (this.Range.Upper.Value);
        if (!this.Range.Upper.Value.Equals (adjustedEnd)) { // Adjust the end date/time
          // Check if the next day must be extended
          DateTime adjustedLocalEnd = adjustedEnd.ToLocalTime ();
          DateTime firstNextDay;
          if (adjustedLocalEnd.TimeOfDay <= TimeSpan.FromHours (12)) { // Positive cut-off
            firstNextDay = adjustedLocalEnd.Date;
          }
          else { // Negative cut-off
            firstNextDay = adjustedLocalEnd.Date.AddDays (1);
          }
          DateTime dayToExtend = firstNextDay.Date;
          IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindProcessedByDay (dayToExtend);
          if ((null != daySlot)
            && (Bound.Compare<DateTime> (adjustedEnd, daySlot.DateTimeRange.Lower) < 0)) { // Extend the day
            var range = new UtcDateTimeRange (adjustedEnd, daySlot.EndDateTime);
            var dayChange = new DayChange (daySlot.DayTemplate, dayToExtend, range, this);
            dayChange.Apply ();
            AnalysisAccumulator.PushMessage ("Cache/ClearDomain/DayChange?Broadcast=true");
          }

          // Make the change to adjustedEnd
          Debug.Assert (this.Begin.HasValue);
          var newDayTemplateChange = ModelDAOHelper.ModelFactory
            .CreateDayTemplateChange (this.DayTemplate, this.Begin.Value);
          newDayTemplateChange.End = adjustedEnd;
          newDayTemplateChange.DateTime = this.DateTime;
          ModelDAOHelper.DAOFactory.DayTemplateChangeDAO.MakePersistent (newDayTemplateChange);
          newDayTemplateChange.Parent = this.MainModification ?? this;
          newDayTemplateChange.Priority = this.StatusPriority;
          MarkAsCompleted ("");
          return;
        }
      }
      
      {
        var dayTemplateSlotDAO = new DayTemplateSlotDAO ();
        dayTemplateSlotDAO.Caller = this;
        this.Insert<DayTemplateSlot, IDayTemplateSlot, DayTemplateSlotDAO> (dayTemplateSlotDAO);
      }
      SetActive ();
      using (var daySlotCacheSuspend = new DaySlotCacheSuspend ())
      {
        var daySlotDAO = new DaySlotDAO ();
        daySlotDAO.Caller = this;
        this.Insert<DaySlot, IDaySlot, DaySlotDAO> (daySlotDAO);
      }
      
      // Analysis is done
      MarkAsCompleted ("Cache/ClearDomain/DayChange?Broadcast=true");
    }
    
    /// <summary>
    /// Apply the modification while keeping it transient
    /// 
    /// It should be never called, because there is no transient DayTemplateChange to process.
    /// Use a persistent entity instead and MakeAnalysis.
    /// </summary>
    public override void Apply ()
    {
      Debug.Assert (false);
      log.FatalFormat ("Apply: not implemented/supported");
      throw new NotImplementedException ();
    }

    /// <summary>
    /// Check if the step process should be active or not for the specified range
    /// 
    /// By default, it is not active in the future or when the main modification is transient
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    protected override bool IsStepActive (UtcDateTimeRange range)
    {
      // Just return false for the moment because there are two steps in the analysis
      return false;
    }
    #endregion // Modification implementation
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      string modificationType = this.ModificationType;
      NHibernateHelper.Unproxy<IDayTemplate> (ref m_dayTemplate);
    }
  }
}

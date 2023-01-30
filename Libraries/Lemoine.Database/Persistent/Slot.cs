// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;

using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.Database.Persistent
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
  public abstract class Slot : ISlot, Lemoine.Threading.IChecked
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Slot).FullName);

    /// <summary>
    /// Get the right logger for this class
    /// </summary>
    /// <returns></returns>
    protected virtual ILog GetLogger () {
      return log;
    }

    #region Members
    int m_id = 0;
    int m_version = 0;
    Lemoine.Threading.IChecked m_caller = null;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected Slot ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="range"></param>
    protected Slot (UtcDateTimeRange range)
    {
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// Modification tracker level
    /// </summary>
    [XmlIgnore]
    public virtual int ModificationTrackerLevel
    {
      get; set;
    } = 0;

    /// <summary>
    /// Slot Id
    /// </summary>
    [XmlIgnore]
    public virtual int Id
    {
      get { return this.m_id; }
      protected internal set { m_id = value; }
    }
    
    /// <summary>
    /// Slot Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
      protected internal set { m_version = value; }
    }

    /// <summary>
    /// Date/time range of the slot
    /// </summary>
    [XmlIgnore]
    public abstract UtcDateTimeRange DateTimeRange { get; set; }
    
    /// <summary>
    /// Range for Xml serialization
    /// </summary>
    [XmlElement("DateTimeRange")]
    public virtual string XmlSerializationDateTimeRange
    {
      get
      {
        return DateTimeRange == null ? "" :
          DateTimeRange.ToString(dt => dt.ToString("yyyy-MM-dd HH:mm:ss"));
      }
      set { this.UpdateDateTimeRange(new UtcDateTimeRange(value)); }
    }
    
    /// <summary>
    /// Begin date/time of the slot
    /// </summary>
    [XmlIgnore]
    public abstract LowerBound<DateTime> BeginDateTime { get; protected set; }
    
    /// <summary>
    /// Begin date/time for XML serialization
    /// </summary>
    [XmlElement("BeginDateTime")]
    public virtual string XmlSerializationBeginDateTime {
      get { return BeginDateTime.Value.ToString("yyyy-MM-dd HH:mm:ss"); }
      set { throw new InvalidOperationException ("BeginDateTime - deserialization not supported"); }
    }
    
    /// <summary>
    /// Local begin date/time for XML serialization
    /// </summary>
    [XmlElement("LocalBeginDateTime")]
    public virtual string XmlSerializationLocalBeginDateTime {
      get { return BeginDateTime.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"); }
      set { throw new InvalidOperationException ("LocalBeginDateTime - deserialization not supported"); }
    }
    
    /// <summary>
    /// Local begin date/time for XML serialization in G format
    /// 8/18/2015 1:31:17 PM
    /// </summary>
    [XmlElement ("LocalBeginDateTimeG")]
    public virtual string XmlSerializationLocalBeginDateTimeG
    {
      get { return BeginDateTime.Value.ToLocalTime ().ToString ("G"); }
      set { throw new InvalidOperationException ("LocalBeginDateTimeG - deserialization not supported"); }
    }

    /// <summary>
    /// Optionally end date/time of the slot
    /// </summary>
    [XmlIgnore]
    public abstract UpperBound<DateTime> EndDateTime { get; set; }
    
    /// <summary>
    /// Begin date/time for XML serialization
    /// </summary>
    [XmlElement("EndDateTime")]
    public virtual string XmlSerializationEndDateTime {
      get { return EndDateTime.Value.ToString("yyyy-MM-dd HH:mm:ss"); }
      set { throw new InvalidOperationException ("EndDateTime - deserialization not supported"); }
    }
    
    /// <summary>
    /// Local end date/time for XML serialization
    /// </summary>
    [XmlElement("LocalEndDateTime")]
    public virtual string XmlSerializationLocalEndDateTime {
      get { return EndDateTime.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"); }
      set { throw new InvalidOperationException ("LocalEndDateTime - deserialization not supported"); }
    }
    
    /// <summary>
    /// Local end date/time for XML serialization in G format
    /// 8/18/2015 1:31:17 PM
    /// </summary>
    [XmlElement ("LocalEndDateTimeG")]
    public virtual string XmlSerializationLocalEndDateTimeG
    {
      get { return EndDateTime.Value.ToLocalTime ().ToString ("G"); }
      set { throw new InvalidOperationException ("LocalEndDateTimeG - deserialization not supported"); }
    }

    /// <summary>
    /// Day range of the slot
    /// </summary>
    public abstract DayRange DayRange { get; }
    
    /// <summary>
    /// Begin day (from cut-off time) of the slot
    /// </summary>
    public abstract LowerBound<DateTime> BeginDay { get; }
    
    /// <summary>
    /// Optionally end day of the slot
    /// </summary>
    public abstract UpperBound<DateTime> EndDay { get; }
    
    /// <summary>
    /// Duration of the slot
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan? Duration {
      get { return DateTimeRange.Duration; }
      // disable once ValueParameterNotUsed
      protected set { } // For NHibernate
    }
    
    /// <summary>
    /// Duration of the slot for xml serialization
    /// </summary>
    [XmlElement("Duration")]
    public virtual string XmlSerializationDuration {
      get { return DateTimeRange.Duration == null ? "" : DateTimeRange.Duration.ToString(); }
      set { throw new InvalidOperationException ("LocalEndDateTime - deserialization not supported"); }
    }
    
    /// <summary>
    /// Is the slot consolidated ?
    /// </summary>
    [XmlIgnore]
    public virtual bool Consolidated {
      get { return true; }
      // disable once ValueParameterNotUsed
      set { }
    }
    #endregion // Getters / Setters

    #region IChecked implementation
    /// <summary>
    /// Add a caller to this class to correctly redirect the SetActive calls
    /// </summary>
    [XmlIgnore]
    public virtual Lemoine.Threading.IChecked Caller {
      get { return m_caller; }
      set { m_caller = value; }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked">IChecked implementation</see>
    /// </summary>
    public virtual void SetActive ()
    {
      if (null != m_caller) {
        m_caller.SetActive ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public virtual void PauseCheck()
    {
      if (null != m_caller) {
        m_caller.PauseCheck ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public virtual void ResumeCheck()
    {
      if (null != m_caller) {
        m_caller.ResumeCheck ();
      }
    }
    #endregion
    
    #region IMergeableItem implementation
    /// <summary>
    /// IMergeableItem implementation
    /// 
    /// Clone the object with a new range
    /// </summary>
    /// <param name="newRange"></param>
    /// <param name="newDayRange">unused</param>
    /// <returns></returns>
    public virtual ISlot Clone (UtcDateTimeRange newRange, DayRange newDayRange)
    {
      return (ISlot) Clone (newRange);
    }
    
    /// <summary>
    /// IMergeable implementation
    /// 
    /// Check the reference data of the other item matches
    /// the data of other without considering the date/time
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public abstract bool ReferenceDataEquals (ISlot obj);
    #endregion // IMergeble implementation

    #region Methods
    /// <summary>
    /// Merge two slots for the same period when ReferenceDataEquals returns true
    /// </summary>
    /// <typeparam name="TSlot"></typeparam>
    /// <param name="slot"></param>
    public virtual void MergeSamePeriodAdditionalProperties<TSlot> (TSlot slot)
    {
      // By default, do nothing
    }

    /// <summary>
    /// Merge a slot with the next one (extend at least the range)
    /// </summary>
    /// <typeparam name="TSlot"></typeparam>
    /// <param name="slot"></param>
    public virtual void MergeWithNextSlot<TSlot> (TSlot slot)
      where TSlot: Slot
    {
      // Note: slot does not need to be only adjacent to this, it can overlap it
      Debug.Assert (Bound.Compare<DateTime> (this.BeginDateTime, slot.BeginDateTime) <= 0);
      Debug.Assert (Bound.Compare<DateTime> (this.EndDateTime, slot.EndDateTime) <= 0);

      var extendedRange = new UtcDateTimeRange (this.DateTimeRange.Lower, slot.DateTimeRange.Upper);
      UpdateDateTimeRange (extendedRange);
    }

    /// <summary>
    /// Set a new date/time range
    /// </summary>
    /// <param name="newRange"></param>
    public abstract void UpdateDateTimeRange (UtcDateTimeRange newRange);

    /// <summary>
    /// Clone the object with a new begin date/time
    /// </summary>
    /// <param name="newRange">new range (not empty)</param>
    /// <returns></returns>
    public virtual ISlot Clone (UtcDateTimeRange newRange)
    {
      if (newRange.IsEmpty ()) {
        log.ErrorFormat ("Clone: " +
                         "empty newRange. " +
                         "StackTrace: {0}",
                         System.Environment.StackTrace);
      }
      
      var newSlot = (Slot) Clone ();
      newSlot.UpdateDateTimeRange (newRange);
      return newSlot;
    }
    
    /// <summary>
    /// Make a shallow copy
    /// <see cref="ICloneable.Clone" />
    /// </summary>
    /// <returns></returns>
    public virtual object Clone()
    {
      object clone = this.MemberwiseClone ();
      var cloneSlot = clone as Slot;
      cloneSlot.m_id = 0;
      cloneSlot.m_version = 0;
      return clone;
    }
    
    /// <summary>
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public abstract int CompareTo (object obj);
    
    /// <summary>
    /// Is the machine slot empty ?
    /// 
    /// If the slot is empty, it will not be inserted in the database.
    /// </summary>
    /// <returns></returns>
    public abstract bool IsEmpty ();
    
    /// <summary>
    /// Consolidate the machine slot from other analysis tables.
    /// 
    /// This is mainly useful to update the runtime values
    /// from the activity slot when the period of the machine slot changes.
    /// </summary>
    protected virtual void Consolidate ()
    { }

    /// <summary>
    /// Consolidate the machine slot from other analysis tables.
    /// 
    /// This is mainly useful to update the runtime values
    /// from the activity slot when the period of the machine slot changes.
    /// </summary>
    /// <param name="oldSlot">previous slot, null if not applicable</param>
    public virtual void Consolidate (ISlot oldSlot)
    {
      Consolidate ();
    }

    /// <summary>
    /// Consolidate the machine slot from other analysis tables.
    /// 
    /// This is mainly useful to update the runtime values
    /// from the activity slot when the period of the machine slot changes.
    /// </summary>
    /// <param name="oldSlot">previous slot, null if not applicable</param>
    /// <param name="association"></param>
    public virtual void Consolidate (ISlot oldSlot, IPeriodAssociation association)
    {
      Consolidate (oldSlot);
    }

    /// <summary>
    /// Check if an early consolidate() call is required by SlotDAO
    /// to check the equality of the data reference
    /// </summary>
    /// <returns></returns>
    public virtual bool IsEarlyConsolidateRequiredForDataReference ()
    {
      return false;
    }

    /// <summary>
    /// Handle here the specific tasks when a slot is added
    /// 
    /// This can be for example an update of a summary analysis table
    /// </summary>
    public abstract void HandleAddedSlot ();
    
    /// <summary>
    /// Handle here the specific tasks for a removed slot
    /// 
    /// This can be for example an update of a summary analysis table
    /// </summary>
    public abstract void HandleRemovedSlot ();
    
    /// <summary>
    /// Handle here the specific tasks for a modified slot
    /// 
    /// This can be for example an update of a summary analysis table
    /// </summary>
    /// <param name="oldSlot"></param>
    public abstract void HandleModifiedSlot (ISlot oldSlot);
    
    /// <summary>
    /// Merge the next slot with this when:
    /// <item>it comes right after this</item>
    /// <item>the reference data are the same</item>
    /// </summary>
    /// <param name="nextSlot"></param>
    public virtual void Merge (ISlot nextSlot)
    {
      Debug.Assert (null != nextSlot);
      Debug.Assert (nextSlot.ReferenceDataEquals (this));
      Debug.Assert (this.EndDateTime.HasValue);
      Debug.Assert (nextSlot.BeginDateTime.HasValue);
      Debug.Assert (this.EndDateTime.Value.Equals (nextSlot.BeginDateTime.Value));
      this.EndDateTime = nextSlot.EndDateTime;
    }
    #endregion // Methods
  }
}

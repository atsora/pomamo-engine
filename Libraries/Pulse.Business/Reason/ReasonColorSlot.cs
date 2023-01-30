// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Database;
using System.Linq;
using Pulse.Extensions.Database;

namespace Lemoine.Business.Reason
{
  /// <summary>
  /// Reason color slot
  /// </summary>
  [Serializable]
  public class ReasonColorSlot
    : IReasonColorSlot
    , IWithRange
    , IPartitionedByMachine
  {
    ILog log = LogManager.GetLogger (typeof (ReasonColorSlot).FullName);

    static readonly string PROCESSING_DEFAULT_COLOR = "transparent";

    static readonly string GUESS_COLOR_FOR_PROCESSING_KEY = "ReasonColorSlot.Processing.GuessColor";
    static readonly bool GUESS_COLOR_FOR_PROCESSING_DEFAULT = true;

    #region Members
    IMachine m_machine;
    bool m_processing = false;
    string m_color;
    bool m_overwriteRequired;
    bool m_auto;
    bool m_running;
    bool m_notRunning;
    UtcDateTimeRange m_dateTimeRange;
    DayRange m_dayRange;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected ReasonColorSlot ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="processing"></param>
    /// <param name="color"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="auto"></param>
    /// <param name="running"></param>
    /// <param name="notRunning"></param>
    /// <param name="range"></param>
    /// <param name="dayRange"></param>
    protected internal ReasonColorSlot (IMachine machine,
                                        bool processing,
                                        string color,
                                        bool overwriteRequired,
                                        bool auto,
                                        bool running,
                                        bool notRunning,
                                        UtcDateTimeRange range,
                                        DayRange dayRange)
    {
      Debug.Assert (null != machine);
      if (machine is null) {
        log.Fatal ("ReasonColorSlot: null value");
        throw new ArgumentNullException ();
      }
      m_machine = machine;

      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{machine.Id}");

      m_processing = processing;
      m_color = color;
      m_overwriteRequired = overwriteRequired;
      m_auto = auto;
      m_running = running;
      m_notRunning = notRunning;
      m_dateTimeRange = range;
      m_dayRange = dayRange;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="processing"></param>
    /// <param name="color"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="auto"></param>
    /// <param name="running"></param>
    /// <param name="notRunning"></param>
    /// <param name="range"></param>
    protected internal ReasonColorSlot (IMachine machine,
                                        bool processing,
                                        string color,
                                        bool overwriteRequired,
                                        bool auto,
                                        bool running,
                                        bool notRunning,
                                        UtcDateTimeRange range)
      : this (machine, processing, color, overwriteRequired, auto, running, notRunning, range,
              ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (range)))
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="reasonSlot">not null</param>
    protected internal ReasonColorSlot (IReasonSlot reasonSlot)
    {
      Debug.Assert (null != reasonSlot);
      if (reasonSlot is null) {
        log.Fatal ("ReasonColorSlot: null value");
        throw new ArgumentNullException ();
      }

      m_machine = reasonSlot.Machine;

      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{m_machine.Id}");

      m_processing = reasonSlot.IsProcessing ();
      if (m_processing) {
        if (Lemoine.Info.ConfigSet.LoadAndGet (GUESS_COLOR_FOR_PROCESSING_KEY, GUESS_COLOR_FOR_PROCESSING_DEFAULT)) {
          var possibleReason = GuessPossibleReasonFromProcessingReasonSlot (reasonSlot);
          if (null != possibleReason) {
            m_color = possibleReason.Reason is null ? PROCESSING_DEFAULT_COLOR : possibleReason.Reason.Color;
            m_overwriteRequired = possibleReason.OverwriteRequired;
            m_auto = possibleReason.ReasonSource.IsAuto ();
          }
          else {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetProcessingColor: no reason could be guessed from the machine mode");
            }
            m_color = PROCESSING_DEFAULT_COLOR;
            m_overwriteRequired = reasonSlot.OverwriteRequired;
            m_auto = reasonSlot.ReasonSource.IsAuto ();
          }
        }
        else {
          m_color = PROCESSING_DEFAULT_COLOR;
          m_overwriteRequired = reasonSlot.OverwriteRequired;
          m_auto = reasonSlot.ReasonSource.IsAuto ();
        }
      }
      else {
        m_color = reasonSlot.Reason.Color;
        m_overwriteRequired = reasonSlot.OverwriteRequired;
        m_auto = reasonSlot.ReasonSource.IsAuto ();
      }
      m_running = reasonSlot.Running;
      m_notRunning = reasonSlot.NotRunning;
      m_dateTimeRange = reasonSlot.DateTimeRange;
      m_dayRange = reasonSlot.DayRange;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// Reference to the machine
    /// </summary>
    public virtual IMachine Machine
    {
      get { return m_machine; }
    }

    /// <summary>
    /// Does this period correspond to a processing period ?
    /// </summary>
    public virtual bool Processing => m_processing;

    /// <summary>
    /// Reference to the reason color
    /// 
    /// An empty string may be returned
    /// </summary>
    public virtual string Color
    {
      get { return m_color; }
    }

    /// <summary>
    /// Overwrite required ?
    /// </summary>
    public virtual bool OverwriteRequired
    {
      get { return m_overwriteRequired; }
    }

    /// <summary>
    /// Auto-reason
    /// </summary>
    public virtual bool Auto
    {
      get { return m_auto; }
    }

    /// <summary>
    /// True if the associated machine mode has a Running value and it is true
    /// </summary>
    public virtual bool Running
    {
      get { return m_running; }
    }

    /// <summary>
    /// True if the associated machine mode has a NotRunning value and it is true
    /// </summary>
    public virtual bool NotRunning
    {
      get { return m_notRunning; }
    }

    /// <summary>
    /// Date/time range of the slot
    /// </summary>
    public virtual UtcDateTimeRange DateTimeRange
    {
      get { return m_dateTimeRange; }
      protected set {
        m_dateTimeRange = value;
        m_dayRange = ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (m_dateTimeRange));
      }
    }

    /// <summary>
    /// Day range of the slot
    /// </summary>
    public virtual DayRange DayRange
    {
      get { return m_dayRange; }
    }

    /// <summary>
    /// Duration of the slot
    /// </summary>
    public virtual TimeSpan? Duration
    {
      get { return m_dateTimeRange.Duration; }
    }
    #endregion // Getters / Setters

    IPossibleReason GuessPossibleReasonFromProcessingReasonSlot (IReasonSlot reasonSlot)
    {
      return GuessReasonFromMachineMode (reasonSlot.Machine, reasonSlot.MachineMode, reasonSlot.DateTimeRange.Lower.Value);
    }

    IPossibleReason GuessReasonFromMachineMode (IMachine machine, IMachineMode machineMode, DateTime dateTime)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != machineMode);

      IObservationStateSlot observationStateSlot = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
        .FindAt (machine, dateTime);
      if (null != observationStateSlot?.MachineObservationState) {
        var monitoredMachineRequest = new Lemoine.Business.Machine.MonitoredMachineFromId (machine.Id);
        var monitoredMachine = Lemoine.Business.ServiceProvider.Get (monitoredMachineRequest);
        var reasonExtensionRequest = new Lemoine.Business.Extension
          .MonitoredMachineExtensions<IReasonExtension> (monitoredMachine, (ext, m) => ext.Initialize (m));
        var reasonExtensions = Lemoine.Business.ServiceProvider
          .Get (reasonExtensionRequest);
        var possibleReasons = reasonExtensions
          .SelectMany (ext => ext.TryGetActiveAt (dateTime, machineMode, observationStateSlot.MachineObservationState, false))
          .OrderByDescending (p => p.ReasonScore);
        if (!possibleReasons.Any ()) {
          if (log.IsWarnEnabled) {
            log.Warn ($"GuessReasonFromMachineMode: could not get a reason from reason extensions for machine mode {machineMode.Id}");
          }
        }
        var possibleReason = possibleReasons.FirstOrDefault ();
        if (log.IsDebugEnabled) {
          log.Debug ($"GuessReasonFromMachineMode: consider reason {possibleReason?.Reason?.Id} from reason extensions machinemode={machineMode.Id}");
        }
        return possibleReason;
      }
      else {
        log.Error ($"GuessReasonFromMachineMode: no machine observation state at {dateTime}");
        return null;
      }
    }

    /// <summary>
    /// IMergeable implementation
    /// 
    /// Check the reference data of the other item matches
    /// the data of other without considering the date/time
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool ReferenceDataEquals (IReasonColorSlot other)
    {
      if (other == null) {
        return false;
      }

      return object.Equals (this.Machine, other.Machine)
        && object.Equals (this.Processing, other.Processing)
        && string.Equals (this.Color, other.Color, StringComparison.InvariantCultureIgnoreCase)
        && object.Equals (this.OverwriteRequired, other.OverwriteRequired)
        && object.Equals (this.Auto, other.Auto)
        && object.Equals (this.Running, other.Running)
        && object.Equals (this.NotRunning, other.NotRunning);
    }

    #region IWithRange implementation
    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public virtual int CompareTo (object obj)
    {
      if (obj is ReasonColorSlot) {
        IReasonColorSlot other = (IReasonColorSlot)obj;
        if (other.Machine.Equals (this.Machine)) {
          return this.DateTimeRange.CompareTo (other.DateTimeRange);
        }
        else {
          log.ErrorFormat ("CompareTo: " +
                           "trying to compare slots " +
                           "for different machines {0} {1}",
                           this, other);
          throw new ArgumentException ("Comparison of slots from different machines");
        }
      }

      log.ErrorFormat ("CompareTo: " +
                       "object {0} of invalid type",
                       obj);
      throw new ArgumentException ("object is not the right slot");
    }

    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo (IReasonColorSlot other)
    {
      if (other.Machine.Equals (this.Machine)) {
        return this.DateTimeRange.CompareTo (other.DateTimeRange);
      }

      log.ErrorFormat ("CompareTo: " +
                       "trying to compare ReasonColorSlots " +
                       "for different machines {0} {1}",
                       this, other);
      throw new ArgumentException ("Comparison of ReasonColorSlots from different machines");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual bool IsEmpty ()
    {
      return false;
    }
    #endregion // IWithRange implementation

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IReasonColorSlot other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      ReasonColorSlot other = obj as ReasonColorSlot;
      if (null == other) {
        return false;
      }
      return object.Equals (this.Machine, other.Machine)
        && object.Equals (this.DateTimeRange, other.DateTimeRange);
    }


    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * Machine.GetHashCode ();
        hashCode += 1000000009 * DateTimeRange.GetHashCode ();
      }
      return hashCode;
    }

    /// <summary>
    /// <see cref="Object.ToString" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      return string.Format ("[ReasonColorSlot Machine={0} Range={1}]",
                           this.Machine.Id, this.DateTimeRange);
    }

    /// <summary>
    /// Clone the reason color slot but with a new date/time range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public IReasonColorSlot Clone (UtcDateTimeRange range)
    {
      if (UtcDateTimeRange.Equals (range, this.DateTimeRange)) {
        var clone = new ReasonColorSlot (this.Machine,
          this.Processing,
          this.Color,
          this.OverwriteRequired,
          this.Auto,
          this.Running,
          this.NotRunning,
          this.DateTimeRange,
          this.DayRange);
        return clone;
      }
      else {
        var clone = new ReasonColorSlot (this.Machine,
          this.Processing,
          this.Color,
          this.OverwriteRequired,
          this.Auto,
          this.Running,
          this.NotRunning,
          range);
        return clone;
      }
    }
  }
}

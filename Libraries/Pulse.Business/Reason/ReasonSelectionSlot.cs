// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Common;
using Pulse.Extensions.Database;

namespace Lemoine.Business.Reason
{
  /// <summary>
  /// Reason+details only slot
  /// </summary>
  [Serializable]
  public class ReasonSelectionSlot
    : IReasonSelectionSlot
    , IWithRange
    , IPartitionedByMachine
  {
    ILog log = LogManager.GetLogger(typeof (ReasonSelectionSlot).FullName);

    #region Members
    readonly IMachine m_machine;
    readonly IReason m_reason;
    readonly bool m_running;
    readonly bool m_overwriteRequired;
    readonly string m_reasonDetails;
    readonly bool m_defaultReason;
    readonly HashSet<IReason> m_selectableReasons = new HashSet<IReason> ();
    UtcDateTimeRange m_dateTimeRange;
    DayRange m_dayRange;
    readonly IList<IMachineModeSubSlot> m_machineModeSlots = new List<IMachineModeSubSlot> ();
    readonly IList<IMachineObservationStateSubSlot> m_machineObservationStateSlots = new List<IMachineObservationStateSubSlot> ();
    #endregion // Members

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected ReasonSelectionSlot ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="reason"></param>
    /// <param name="running"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="reasonDetails"></param>
    /// <param name="defaultReason"></param>
    /// <param name="selectableReasons"></param>
    /// <param name="range"></param>
    /// <param name="dayRange"></param>
    protected internal ReasonSelectionSlot (IMachine machine,
                                            IReason reason,
                                            bool running,
                                            bool overwriteRequired,
                                            string reasonDetails,
                                            bool defaultReason,
                                            HashSet<IReason> selectableReasons,
                                            UtcDateTimeRange range,
                                            DayRange dayRange)
    {
      Debug.Assert (null != machine);
      if (null == machine) {
        log.FatalFormat ("ReasonSelectionSlot: " +
                         "null value");
        throw new ArgumentNullException ();
      }
      m_machine = machine;
      log = LogManager.GetLogger(string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                machine.Id));
      m_reason = reason;
      m_running = running;
      m_overwriteRequired = overwriteRequired;
      m_reasonDetails = reasonDetails;
      m_defaultReason = defaultReason;
      m_selectableReasons = selectableReasons;
      m_dateTimeRange = range;
      m_dayRange = dayRange;
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="reason"></param>
    /// <param name="running"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="reasonDetails"></param>
    /// <param name="defaultReason"></param>
    /// <param name="selectableReasons"></param>
    /// <param name="range"></param>
    protected internal ReasonSelectionSlot (IMachine machine,
                                            IReason reason,
                                            bool running,
                                            bool overwriteRequired,
                                            string reasonDetails,
                                            bool defaultReason,
                                            HashSet<IReason> selectableReasons,
                                            UtcDateTimeRange range)
      : this (machine, reason, running, overwriteRequired, reasonDetails, defaultReason, selectableReasons,
              range,
              ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (range)))
    {
    }
    
    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="reasonSlot">not null</param>
    protected internal ReasonSelectionSlot (IReasonSlot reasonSlot)
      : this (reasonSlot.Machine, reasonSlot.Reason, reasonSlot.Running, reasonSlot.OverwriteRequired, reasonSlot.ReasonDetails,
              reasonSlot.DefaultReason, new HashSet<IReason> (),
              reasonSlot.DateTimeRange, reasonSlot.DayRange)
    {
      Debug.Assert (null != reasonSlot);

      var reasonSelectionExtensionsRequest = new Lemoine.Business.Extension.MachineExtensions<IReasonSelectionExtension> (m_machine, (ext, m) => ext.Initialize (m));
      var reasonSelectionExtensions = Lemoine.Business.ServiceProvider
        .Get (reasonSelectionExtensionsRequest);
      foreach (var reasonSelectionExtension in reasonSelectionExtensions) {
        var reasonSelections = reasonSelectionExtension.GetReasonSelections (reasonSlot.DateTimeRange, reasonSlot.MachineMode, reasonSlot.MachineObservationState, includeExtraAutoReasons: true);
        IEnumerable<IReason> reasons = reasonSelections.Select (reasonSelection => reasonSelection.Reason);
        m_selectableReasons.UnionWith (reasons);
      }
      
      {
        IMachineModeSubSlot subSlot = new MachineModeSubSlot (reasonSlot.MachineMode,
                                                              reasonSlot.DateTimeRange,
                                                              reasonSlot.DayRange);
        m_machineModeSlots.Add (subSlot);
      }
      {
        IMachineObservationStateSubSlot subSlot = new MachineObservationStateSubSlot (reasonSlot.MachineObservationState,
                                                                                      reasonSlot.DateTimeRange,
                                                                                      reasonSlot.DayRange);
        m_machineObservationStateSlots.Add (subSlot);
      }
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Reference to the machine
    /// </summary>
    public virtual IMachine Machine {
      get { return m_machine; }
    }

    /// <summary>
    /// Reference to the reason
    /// </summary>
    public virtual IReason Reason {
      get { return m_reason; }
    }
    
    /// <summary>
    /// Running
    /// </summary>
    public virtual bool Running {
      get { return m_running; }
    }
    
    /// <summary>
    /// Overwrite required ?
    /// </summary>
    public virtual bool OverwriteRequired {
      get { return m_overwriteRequired; }
    }
    
    /// <summary>
    /// Reason details
    /// </summary>
    public virtual string ReasonDetails {
      get { return m_reasonDetails; }
    }
    
    /// <summary>
    /// Default reason ?
    /// </summary>
    public virtual bool DefaultReason {
      get { return m_defaultReason; }
    }
    
    /// <summary>
    /// Selectable reasons
    /// </summary>
    public virtual HashSet<IReason> SelectableReasons {
      get { return m_selectableReasons; }
    }
    
    /// <summary>
    /// Date/time range of the slot
    /// </summary>
    public virtual UtcDateTimeRange DateTimeRange {
      get { return m_dateTimeRange; }
      protected set
      {
        m_dateTimeRange = value;
        m_dayRange = ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (m_dateTimeRange));
      }
    }
    
    /// <summary>
    /// Day range of the slot
    /// </summary>
    public virtual DayRange DayRange {
      get { return m_dayRange; }
    }
    
    /// <summary>
    /// Duration of the slot
    /// </summary>
    public virtual TimeSpan? Duration {
      get { return m_dateTimeRange.Duration; }
    }
    
    /// <summary>
    /// Sub-slots
    /// </summary>
    public virtual IList<IMachineModeSubSlot> MachineModeSlots {
      get { return m_machineModeSlots; }
    }

    /// <summary>
    /// Sub-slots
    /// </summary>
    public virtual IList<IMachineObservationStateSubSlot> MachineObservationStateSlots {
      get { return m_machineObservationStateSlots; }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// IMergeable implementation
    /// 
    /// Check the reference data of the other item matches
    /// the data of other without considering the date/time
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool ReferenceDataEquals (IReasonSelectionSlot other)
    {
      if (other == null) {
        return false;
      }

      bool test1 = object.Equals(this.Machine, other.Machine)
        && object.Equals(this.Reason, other.Reason)
        && object.Equals(this.Running, other.Running)
        && object.Equals(this.OverwriteRequired, other.OverwriteRequired)
        && object.Equals(this.ReasonDetails, other.ReasonDetails)
        && object.Equals(this.DefaultReason, other.DefaultReason);
      if (!test1) {
        return false;
      }
      else {
        // Note: object.Equals (this.SelectableReasons, other.SelectableReasons)
        //       does not work as expected
        Debug.Assert (null != this.SelectableReasons);
        Debug.Assert (null != other.SelectableReasons);
        if (this.SelectableReasons.Count != other.SelectableReasons.Count) {
          return false;
        }
        bool areSelectableReasonsEqual = HashSet<IReason>.CreateSetComparer ()
          .Equals (this.SelectableReasons, other.SelectableReasons);
        return areSelectableReasonsEqual;
      }
    }

    #region IWithRange implementation
    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public virtual int CompareTo(object obj)
    {
      if (obj is ReasonSelectionSlot) {
        IReasonSelectionSlot other = (IReasonSelectionSlot) obj;
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
    public virtual int CompareTo(IReasonSelectionSlot other)
    {
      if (other.Machine.Equals (this.Machine)) {
        return this.DateTimeRange.CompareTo (other.DateTimeRange);
      }

      log.ErrorFormat ("CompareTo: " +
                       "trying to compare ReasonSelectionSlots " +
                       "for different machines {0} {1}",
                       this, other);
      throw new ArgumentException ("Comparison of ReasonSelectionSlots from different machines");
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual bool IsEmpty ()
    {
      Debug.Assert (null != this.Reason);
      
      return false;
    }
    #endregion // IWithRange implementation
    
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(IReasonSelectionSlot other)
    {
      return this.Equals ((object) other);
    }
    
    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals(this,obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      ReasonSelectionSlot other = obj as ReasonSelectionSlot;
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
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * Machine.GetHashCode();
        hashCode += 1000000009 * DateTimeRange.GetHashCode();
      }
      return hashCode;
    }

    /// <summary>
    /// <see cref="Object.ToString" />
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return string.Format("[ReasonSelectionSlot Machine={0} Range={1}]",
                           this.Machine.Id, this.DateTimeRange);
    }
  }
}

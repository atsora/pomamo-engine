// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ReasonSelection
  /// 
  /// This table lists all the possible reasons
  /// for a given Machine Mode and Machine Observation State.
  /// 
  /// A specific column allows to list the reasons
  /// that can be effectively selected by the user. 
  /// </summary>
  [Serializable]
  public class ReasonSelection: IReasonSelection, IVersionable
  {
    /// <summary>
    /// Default manual reason score
    /// </summary>
    static readonly string DEFAULT_MANUAL_SCORE_KEY = "Reason.DefaultManualScore";
    static readonly double DEFAULT_MANUAL_SCORE_DEFAULT = 100.0;

    int m_id = 0;
    int m_version = 0;
    IMachineMode m_machineMode;
    IMachineObservationState m_machineObservationState;
    IReason m_reason;
    bool m_selectable = true;
    bool m_detailsRequired = false;
    IMachineFilter m_machineFilter = null;

    static readonly ILog log = LogManager.GetLogger(typeof (ReasonSelection).FullName);

    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected ReasonSelection ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineMode">not null</param>
    /// <param name="machineObservationState">not null</param>
    public ReasonSelection(IMachineMode machineMode, IMachineObservationState machineObservationState)
    {
      Debug.Assert (null != machineMode);
      Debug.Assert (null != machineObservationState);
      
      this.m_machineMode = machineMode;
      this.m_machineObservationState = machineObservationState;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineMode">not null</param>
    /// <param name="machineObservationState">not null</param>
    /// <param name="reason">not null</param>
    /// <param name="detailsRequired"></param>
    public ReasonSelection(IMachineMode machineMode, IMachineObservationState machineObservationState, IReason reason, bool detailsRequired)
    {
      Debug.Assert (null != machineMode);
      Debug.Assert (null != machineObservationState);
      Debug.Assert (null != reason);
      
      this.m_machineMode = machineMode;
      this.m_machineObservationState = machineObservationState;
      this.m_reason = reason;
      this.m_detailsRequired = detailsRequired;
    }

    /// <summary>
    /// MachineModeDefaultReason ID
    /// </summary>
    public virtual int Id => m_id;

    /// <summary>
    /// Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Reference to the Machine Mode
    /// </summary>
    public virtual IMachineMode MachineMode {
      get { return m_machineMode; }
    }
    
    /// <summary>
    /// Reference to the Machine Observation State
    /// </summary>
    public virtual IMachineObservationState MachineObservationState {
      get { return m_machineObservationState; }
    }
    
    /// <summary>
    /// Reference to the Reason
    /// </summary>
    public virtual IReason Reason {
      get { return m_reason; }
      set
      {
        if (null == value) {
          Debug.Assert (null != value);
          log.Error ("Reason.set: a null reason is not allowed");
          throw new ArgumentNullException ("ReasonSelection.Reason");
        }
        m_reason = value;
      }
    }

    /// <summary>
    /// Recommended reason score
    /// </summary>
    public virtual double ReasonScore {
      get { return Lemoine.Info.ConfigSet.LoadAndGet<double> (DEFAULT_MANUAL_SCORE_KEY, DEFAULT_MANUAL_SCORE_DEFAULT); }
    }

    /// <summary>
    /// Can this reason be selected by the user ? 
    /// </summary>
    public virtual bool Selectable {
      get { return m_selectable; }
      set { m_selectable = value; }
    }
    
    /// <summary>
    /// If TRUE, when this reason is selected, the operator must also enter a free detailed entry
    /// </summary>
    public virtual bool DetailsRequired {
      get { return m_detailsRequired; }
      set { m_detailsRequired = value; }
    }
    
    /// <summary>
    /// Associated machine filter
    /// 
    /// If null, all machines apply
    /// </summary>
    public virtual IMachineFilter MachineFilter {
      get { return m_machineFilter; }
      set { m_machineFilter = value; }
    }

    /// <summary>
    /// Additional data
    /// </summary>
    public virtual IDictionary<string, object> Data => null;

    /// <summary>
    /// <see cref="IReasonSelection"/>
    /// </summary>
    public virtual string AlternativeText => null;

    /// <summary>
    /// <see cref="IReasonSelection"/>
    /// </summary>
    public virtual string AlternativeLongText => null;

    /// <summary>
    /// <see cref="IReasonSelection"/>
    /// </summary>
    public virtual string AlternativeDescription => null;

    /// <summary>
    /// <see cref="IReasonSelection"/>
    /// </summary>
    public virtual bool TimeDependent => false;

    /// <summary>
    /// <see cref="IReasonSelection"/>
    /// </summary>
    public virtual bool AdditionalData => false;

    #region Equals and GetHashCode implementation
    /// <summary>
    /// <see cref="Object.Equals(object)" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      ReasonSelection other = obj as ReasonSelection;
      if (other == null) {
        return false;
      }

      return this.Id.Equals(other.Id)
        && this.Version.Equals(other.Version);
    }
    
    /// <summary>
    /// <see cref="Object.GetHashCode" />
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * m_id.GetHashCode();
        hashCode += 1000000009 * m_version.GetHashCode();
      }
      return hashCode;
    }
    #endregion
  }
}

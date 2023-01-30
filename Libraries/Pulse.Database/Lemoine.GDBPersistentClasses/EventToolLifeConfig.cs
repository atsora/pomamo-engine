// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table EventToolLifeConfig
  /// </summary>
  [Serializable]
  public class EventToolLifeConfig: IEventToolLifeConfig
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    EventToolLifeType m_eventToolLifeType = EventToolLifeType.Unknown;
    IMachineFilter m_machineFilter = null;
    IMachineObservationState m_machineObservationState = null;
    IEventLevel m_level;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof(EventToolLifeConfig).FullName);

    #region Getters / Setters
    /// <summary>
    /// EventToolLifeConfig Id
    /// </summary>
    public virtual int Id {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// EventToolLifeConfig Version
    /// </summary>
    public virtual int Version {
      get { return this.m_version; }
    }

    /// <summary>
    /// Type of tool life event
    /// </summary>
    public virtual EventToolLifeType Type {
      get { return this.m_eventToolLifeType; }
      set { this.m_eventToolLifeType = value; }
    }
    
    /// <summary>
    /// Level to associate to the event (can't be null)
    /// </summary>
    public virtual IEventLevel Level {
      get { return m_level; }
      set {
        if (null == value) {
          log.FatalFormat ("EventLevel can't be null");
          Debug.Assert (null != value);
          throw new ArgumentNullException("Level");
        } else {
          m_level = value;
        }
      }
    }
    
    /// <summary>
    /// Associated machine filter
    /// 
    /// This may be null (whichever machine)
    /// </summary>
    public virtual IMachineFilter MachineFilter {
      get { return m_machineFilter; }
      set { m_machineFilter = value; }
    }
    
    /// <summary>
    /// Associated machine observation state
    /// 
    /// It may be null (whichever machine observation state)
    /// </summary>
    public virtual IMachineObservationState MachineObservationState {
      get { return m_machineObservationState; }
      set { m_machineObservationState = value; }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected EventToolLifeConfig() {}
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="eventToolLifeType"></param>
    /// <param name="level"></param>
    public EventToolLifeConfig(EventToolLifeType eventToolLifeType, IEventLevel level)
    {
      this.Type = eventToolLifeType;
      this.Level = level;
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// Unproxy all the properties
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IMachineFilter> (ref m_machineFilter);
      NHibernateHelper.Unproxy<IMachineObservationState> (ref m_machineObservationState);
      NHibernateHelper.Unproxy<IEventLevel> (ref m_level);
    }
    #endregion // Methods
  }
}

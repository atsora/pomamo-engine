// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.GDBPersistentClasses;
using Lemoine.Core.Log;

namespace Lemoine.Alert.GDBListeners
{
  /// <summary>
  /// MachineModification delay direction
  /// </summary>
  public enum MachineModificationDelayDirection {
    /// <summary>
    /// UP
    /// </summary>
    UP,
    /// <summary>
    /// DOWN
    /// </summary>
    DOWN
  }
  
  /// <summary>
  /// Serializable class that lists the data of the MachineModificationDelayListener.
  /// </summary>
  [Serializable]
  public class MachineModificationDelayData
  {
    #region Members
    Machine m_machine;
    MachineModification m_modification;
    MachineModificationDelayDirection m_direction;
    string m_message;
    TimeSpan m_delay;
    TimeSpan m_threshold;
    DateTime m_timeStamp;
    bool m_flapping = false;
    bool m_beginFlapping = false;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineModificationDelayData).FullName);

    #region Getters / Setters
    /// <summary>
    /// Associated machine
    /// </summary>
    [XmlElement]
    public Machine Machine {
      get { return m_machine; }
      set { m_machine = value; }
    }

    /// <summary>
    /// Associated modification
    /// </summary>
    [XmlElement]
    public MachineModification Modification {
      get { return m_modification; }
      set { m_modification = value; }
    }
    
    /// <summary>
    /// Direction of the message: UP or DOWN
    /// </summary>
    [XmlAttribute("Direction")]
    public string Direction {
      get { return m_direction.ToString (); }
      set { throw new NotImplementedException (); }
    }
    
    /// <summary>
    /// Associated message
    /// </summary>
    [XmlAttribute("Message")]
    public string Message {
      get { return m_message; }
      set { m_message = value; }
    }
    
    /// <summary>
    /// Delay
    /// </summary>
    [XmlAttribute("Delay")]
    public string Delay {
      get { return m_delay.ToString (); }
      set { throw new NotImplementedException (); }
    }
    
    /// <summary>
    /// Threshold
    /// </summary>
    [XmlAttribute("Threshold")]
    public string Threshold {
      get { return m_threshold.ToString (); }
      set { throw new NotImplementedException (); }
    }

    /// <summary>
    /// UTC Timestamp
    /// </summary>
    [XmlAttribute("UTCTimeStamp")]
    public string UTCTimeStamp {
      get { return m_timeStamp.ToString (); }
      set { throw new NotImplementedException (); }
    }

    /// <summary>
    /// Local Timestamp
    /// </summary>
    [XmlAttribute("LocalTimeStamp")]
    public string LocalTimeStamp {
      get { return m_timeStamp.ToLocalTime ().ToString (); }
      set { throw new NotImplementedException (); }
    }
    
    /// <summary>
    /// Flapping ?
    /// </summary>
    [XmlAttribute("Flapping")]
    public bool Flapping {
      get { return m_flapping; }
      set { m_flapping = value; }
    }

    /// <summary>
    /// Begin Flapping ?
    /// </summary>
    [XmlAttribute("BeginFlapping")]
    public bool BeginFlapping {
      get { return m_beginFlapping; }
      set { m_beginFlapping = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    protected MachineModificationDelayData ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="modification">not null</param>
    /// <param name="direction"></param>
    /// <param name="message"></param>
    /// <param name="delay"></param>
    /// <param name="threshold"></param>
    /// <param name="flapping"></param>
    /// <param name="beginFlapping"></param>
    public MachineModificationDelayData (Lemoine.Model.IMachineModification modification, MachineModificationDelayDirection direction,
                                         string message, TimeSpan delay, TimeSpan threshold, bool flapping, bool beginFlapping)
    {
      Debug.Assert (null != modification);
      
      m_machine = modification.Machine as Machine;
      m_modification = modification as MachineModification;
      m_direction = direction;
      m_message = message;
      m_delay = delay;
      m_threshold = threshold;
      m_timeStamp = DateTime.UtcNow;
      m_flapping = flapping;
      m_beginFlapping = beginFlapping;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
  }
}

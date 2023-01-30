// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Log
  /// </summary>
  [Serializable
   , XmlInclude (typeof (CncDataImportLog))
   , XmlInclude(typeof(GlobalModificationLog))
   , XmlInclude(typeof(MachineModificationLog))
   , XmlInclude(typeof(DetectionAnalysisLog))
   , XmlInclude(typeof(MaintenanceLog))
   , XmlInclude(typeof(SynchronizationLog))]
  public abstract class Log: IBaseLog
  {
    #region Members
    int m_id = 0;
    DateTime m_dateTime = DateTime.UtcNow;
    LogLevel m_level;
    string m_message;
    string m_module;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Log).FullName);

    #region Getters / Setters
    /// <summary>
    /// Log ID
    /// </summary>
    [XmlIgnore]
    public virtual int Id
    {
      get { return m_id; }
      protected internal set { m_id = value; }
    }

    /// <summary>
    /// Log UTC date/time
    /// </summary>
    [XmlAttribute("DateTime")]
    public virtual DateTime DateTime
    {
      get { return m_dateTime; }
      set { m_dateTime = value; }
    }
    
    /// <summary>
    /// Log level
    /// </summary>
    [XmlAttribute("Level")]
    public virtual LogLevel Level
    {
      get { return m_level; }
      set { m_level = value; }
    }
    
    /// <summary>
    /// Log message
    /// </summary>
    [XmlAttribute("Message")]
    public virtual string Message
    {
      get { return m_message; }
      set { m_message = value; }
    }
    
    /// <summary>
    /// Module in which the log was recorded
    /// </summary>
    [XmlAttribute("Module")]
    public virtual string Module
    {
      get { return m_module; }
      set { m_module = value; }
    }
    #endregion // Getters / Setters
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    public Log (LogLevel level,
                string message)
    {
      m_level = level;
      m_message = message;
    }
    
    /// <summary>
    /// Protected constructor for NHibernate
    /// </summary>
    internal protected Log ()
    {
    }
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public abstract void Unproxy ();
  }
  
  /// <summary>
  /// Convert a LogLevel enum to a string in database
  /// </summary>
  [Serializable]
  public class EnumLogLevel: NHibernate.Type.EnumStringType
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public EnumLogLevel(): base (typeof (LogLevel))
    {
    }
  }
}

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
  /// Persistent class of the OperationType table
  /// </summary>
  [Serializable]
  public class OperationType: DataWithTranslation, IOperationType
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_code;
    int? m_priority;
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (OperationType).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "Code", "Name", "TranslationKey"}; }
    }
    
    /// <summary>
    /// Operation type ID
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Operation type code
    /// </summary>
    [XmlAttribute("Code")]
    public virtual string Code {
      get { return m_code; }
      set { m_code = value; }
    }
    
    /// <summary>
    /// Operation type priority
    /// </summary>
    [XmlIgnore]
    public virtual int? Priority {
      get { return m_priority; }
      set { m_priority = value; }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// Constructor
    /// </summary>
    public OperationType () { }

    #region IReferenceData implementation
    /// <summary>
    /// <see cref="IReferenceData.IsUndefined" />
    /// </summary>
    /// <returns></returns>
    public virtual bool IsUndefined()
    {
      return (1 == this.Id);
    }
    #endregion // IReferenceData implementation
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      // Nothing to do here for the moment
    }
  }
}

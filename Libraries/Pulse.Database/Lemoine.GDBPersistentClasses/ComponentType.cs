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
  /// Persistent class of the ComponentType table
  /// </summary>
  [Serializable]
  public class ComponentType: DataWithTranslation, IComponentType
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_code;
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (ComponentType).FullName);

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
    /// Component type ID
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
    /// Component type code
    /// </summary>
    [XmlAttribute("Code")]
    public virtual string Code {
      get { return m_code; }
      set { m_code = value; }
    }
    #endregion
    
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
      // Do nothing here for the moment
    }
  }
}

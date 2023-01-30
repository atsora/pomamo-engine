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
  /// Persistent class of table Display
  /// </summary>
  [Serializable]
  public class Display: IDisplay
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_table;
    string m_pattern;
    string m_variant;
    string m_description;
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (Display).FullName);

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected Display ()
    {  
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="table"></param>
    public Display (string table)
    {
      m_table = table;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// ID
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
    /// Table name
    /// </summary>
    [XmlAttribute("Table")]
    public virtual string Table {
      get { return m_table; }
    }
    
    /// <summary>
    /// Pattern to use to give a display name to a row of the associated table
    /// </summary>
    [XmlAttribute("Pattern")]
    public virtual string Pattern {
      get { return m_pattern; }
      set { m_pattern = value; }
    }
    
    /// <summary>
    /// Variant
    /// </summary>
    [XmlAttribute("Variant")]
    public virtual string Variant {
      get { return m_variant; }
      set { m_variant = value; }
    }
    
    /// <summary>
    /// Description
    /// </summary>
    [XmlAttribute("Description")]
    public virtual string Description {
      get { return m_description; }
      set { m_description = value; }
    }
    #endregion
  }
}

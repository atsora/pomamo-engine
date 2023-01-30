// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table NamedConfig
  /// </summary>
  [Serializable]
  public class NamedConfig: INamedConfig
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name;
    string m_key;
    object m_value;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (NamedConfig).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected internal NamedConfig () { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="key"></param>
    public NamedConfig (string name, string key)
    {
      m_name = name;
      m_key = key;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Config Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// Config Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Named config name
    /// </summary>
    public virtual string Name
    {
      get { return this.m_name; }
    }
    
    /// <summary>
    /// Config key
    /// 
    /// Use the '.' separator to structure the key
    /// </summary>
    public virtual string Key
    {
      get { return this.m_key; }
    }
    
    /// <summary>
    /// Config value, a serializable object
    /// </summary>
    public virtual object Value
    {
      get { return this.m_value; }
      set { this.m_value = value; }
    }
    #endregion // Getters / Setters
  }
}

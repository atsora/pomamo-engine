// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Config
  /// </summary>
  [Serializable]
  public class Config : IConfig
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_key;
    string m_description;
    object m_value;
    bool m_active;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (Config).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected internal Config () { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="key"></param>
    public Config (string key)
    {
      m_key = key;
    }

    /// <summary>
    /// Constructor for default values
    /// </summary>
    /// <param name="key"></param>
    /// <param name="description"></param>
    /// <param name="v">default value</param>
    /// <param name="active"></param>
    internal Config (string key, string description, object v, bool active = true)
    {
      m_key = key;
      m_description = description;
      m_value = v;
      m_active = active;
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
    /// Config key
    /// 
    /// Use the '.' separator to structure the key
    /// </summary>
    public virtual string Key
    {
      get { return this.m_key; }
    }

    /// <summary>
    /// Config description
    /// </summary>
    public virtual string Description
    {
      get { return this.m_description; }
      set { this.m_description = value; }
    }

    /// <summary>
    /// Config value, a serializable object
    /// </summary>
    public virtual object Value
    {
      get { return this.m_value; }
      set { this.m_value = value; }
    }

    /// <summary>
    /// Active property
    /// </summary>
    public virtual bool Active
    {
      get { return this.m_active; }
      set { this.m_active = value; }
    }
    #endregion // Getters / Setters
  }
}

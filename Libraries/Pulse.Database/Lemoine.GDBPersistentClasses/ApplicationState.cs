// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ApplicationState
  /// </summary>
  [Serializable]
  public class ApplicationState: IApplicationState
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_key;
    object m_value;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ApplicationState).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected internal ApplicationState () { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="key"></param>
    public ApplicationState (string key)
    {
      m_key = key;
    }
    
    /// <summary>
    /// Constructor for default values
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    internal ApplicationState (string key, object v)
    {
      m_key = key;
      m_value = v;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// ApplicationState Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// ApplicationState Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// ApplicationState key
    /// 
    /// Use the '.' separator to structure the key
    /// </summary>
    public virtual string Key
    {
      get { return this.m_key; }
    }
    
    /// <summary>
    /// ApplicationState value, a serializable object
    /// </summary>
    public virtual object Value
    {
      get { return this.m_value; }
      set { this.m_value = value; }
    }
    #endregion // Getters / Setters
  }
}

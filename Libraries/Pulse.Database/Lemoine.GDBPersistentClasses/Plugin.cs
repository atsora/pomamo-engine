// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Plugin
  /// </summary>
  public class Plugin: IPlugin, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0; // Version for the database
    string m_identifyingName = "";
    bool m_activated = true;
    int m_numVersion = 0; // Version of the plugin
    PluginFlag? m_pluginFlag = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof(Plugin).FullName);

    #region Getters / Setters
    /// <summary>
    /// Id
    /// </summary>
    public virtual int Id {
      get { return m_id; }
    }
    
    /// <summary>
    /// Version (for the database)
    /// </summary>
    public virtual int Version {
      get { return m_version; }
    }

    /// <summary>
    /// Name
    /// </summary>
    public virtual string IdentifyingName {
      get { return m_identifyingName; }
      protected set { m_identifyingName = value; }
    }

    /// <summary>
    /// Activation
    /// </summary>
    public virtual bool Activated {
      get { return m_activated; }
      set { m_activated = value; }
    }

    /// <summary>
    /// Version (of the plugin)
    /// </summary>
    public virtual int NumVersion {
      get { return m_numVersion; }
      set { m_numVersion = value; }
    }

    /// <summary>
    /// <see cref="IPlugin"/>
    /// </summary>
    public virtual PluginFlag? Flag {
      get { return m_pluginFlag; }
      set { m_pluginFlag = value; }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected internal Plugin() {}
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="identifyingName"></param>
    public Plugin(string identifyingName)
    {
      IdentifyingName = identifyingName;
    }
    #endregion // Constructors
  }
}

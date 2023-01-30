// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Extensions.Interfaces;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table PackagePluginAssociation
  /// </summary>
  public class PackagePluginAssociation: IPackagePluginAssociation, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_parameters = "";
    IPlugin m_plugin = null;
    IPackage m_package = null;
    string m_name = null;
    bool m_active = true;
    bool m_custom = false;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (PackagePluginAssociation).FullName);

    #region Getters / Setters
    /// <summary>
    /// PackagePluginAssociation Id
    /// </summary>
    public virtual int Id {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// PackagePluginAssociation Version
    /// </summary>
    public virtual int Version {
      get { return this.m_version; }
    }

    /// <summary>
    /// Parameters
    /// </summary>
    public virtual string Parameters {
      get { return this.m_parameters; }
      set { this.m_parameters = value; }
    }
    
    /// <summary>
    /// Plugin
    /// </summary>
    public virtual IPlugin Plugin {
      get { return m_plugin; }
      protected set { m_plugin = value; }
    }
    
    /// <summary>
    /// Package
    /// </summary>
    public virtual IPackage Package {
      get { return m_package; }
      protected set { m_package = value; }
    }
    
    /// <summary>
    /// Name (nullable)
    /// </summary>
    public virtual string Name {
      get { return m_name; }
    }

    /// <summary>
    /// Active
    /// </summary>
    public virtual bool Active {
      get { return m_active; }
      set { m_active = value; }
    }

    /// <summary>
    /// Custom
    /// </summary>
    public virtual bool Custom
    {
      get { return m_custom; }
      set { m_custom = value; }
    }
    #endregion // Getters / Setters

    #region IPluginInstance
    /// <summary>
    /// <see cref="IPluginInstance"/>
    /// </summary>
    public virtual int InstanceId => this.Id;

    /// <summary>
    /// <see cref="IPluginInstance"/>
    /// </summary>
    public virtual string InstanceName => this.Name;

    /// <summary>
    /// <see cref="IPluginInstance"/>
    /// </summary>
    public virtual string InstanceParameters => this.Parameters;

    /// <summary>
    /// <see cref="IPluginInstance"/>
    /// </summary>
    public virtual bool InstanceActive => this.Active && this.Package.Activated;

    /// <summary>
    /// <see cref="IPluginInstance"/>
    /// </summary>
    public virtual string PackageIdentifyingName => this.Package.IdentifyingName;
    #endregion // IPluginInstance

    #region Constructors
    /// <summary>
    /// Protected default constructor
    /// </summary>
    protected internal PackagePluginAssociation() {}
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="package"></param>
    /// <param name="plugin"></param>
    /// <param name="name"></param>
    public PackagePluginAssociation(IPackage package, IPlugin plugin, string name)
    {
      Package = package;
      Plugin = plugin;
      m_name = name;
    }
    #endregion // Constructors
  }
}

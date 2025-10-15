// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.Extensions.Extension;
using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Extensions.Plugin
{
  /// <summary>
  /// Description of GenericPlugin.
  /// </summary>
  public abstract class GenericPluginDll : IPluginDll
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GenericPluginDll).FullName);

    IPluginContext m_pluginContext = null;
    IEnumerable<Type> m_activeExtensionTypes;
    IList<IPluginInstance> m_instances = new List<IPluginInstance> ();

    /// <summary>
    /// Identifying name, based on the assembly name
    /// 
    /// null is returned in case no identifying name could be deduced from the assembly name
    /// </summary>
    public string IdentifyingName
    {
      get
      {
        string className = this.GetType ().ToString ();

        const string prefix = "Lemoine.Plugin.";
        if (!className.StartsWith (prefix, StringComparison.InvariantCultureIgnoreCase)) {
          log.ErrorFormat ("IdentifyingName: plugin class name {0} does not start with Lemoine.Plugin.", className);
          return null;
        }

        const string suffix = ".Plugin";
        if (!className.EndsWith (suffix, StringComparison.InvariantCultureIgnoreCase)) {
          log.ErrorFormat ("IdentifyingName: plugin class name {0} does not end with .Plugin", className);
          return null;
        }
        var identifiyingName = className.Substring (prefix.Length, className.Length - prefix.Length - suffix.Length);
        return identifiyingName;
      }
    }

    /// <summary>
    /// Dll path
    /// Set by the dll loader
    /// </summary>
    public string DllPath { get; set; }

    /// <summary>
    /// Activated
    /// Set by the extension manager
    /// </summary>
    public bool Activated { get; set; }

    /// <summary>
    /// Associated extensions
    /// </summary>
    public IEnumerable<Type> ActiveExtensionTypes
    {
      get { return m_activeExtensionTypes; }
      set { m_activeExtensionTypes = value; }
    }

    /// <summary>
    /// Context of the plugin
    /// Set by the extension manager
    /// </summary>
    public IPluginContext Context
    {
      get
      {
        Debug.Assert (null != m_pluginContext);

        return m_pluginContext;
      }
      set
      {
        Debug.Assert (null != value);

        m_pluginContext = value;
      }
    }

    /// <summary>
    /// Can the plugin have multiple configurations ?
    /// </summary>
    public virtual bool MultipleConfigurations => false;

    /// <summary>
    /// Name of the plugin
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public abstract int Version { get; }

    /// <summary>
    /// Configuration interface of the plugin
    /// Modified after the installation
    /// May be null
    /// </summary>
    public virtual IPluginConfigurationControl ConfigurationControl
    {
      get
      {
        return null;
      }
    }

    /// <summary>
    /// List of custom actions, may be null
    /// </summary>
    public virtual IList<IPluginCustomActionControl> CustomActionControls
    {
      get
      {
        return null;
      }
    }

    /// <summary>
    /// Instances that are associated to the plugin
    /// </summary>
    public virtual IEnumerable<IPluginInstance> Instances
    {
      get
      {
        return m_instances;
      }
    }

    /// <summary>
    /// Add an instance
    /// </summary>
    /// <param name="instance">not null</param>
    public virtual void AddInstance (IPluginInstance instance)
    {
      m_instances.Add (instance);
      if (CustomActionControls != null) {
        foreach (var control in CustomActionControls) {
          if (control is IConfigurable) {
            ((IConfigurable)control).AddConfigurationContext (instance);
          }
        }
      }
    }

    /// <summary>
    /// Install the plugin before calling its extensions
    /// (create tables if necessary, ...)
    /// </summary>
    /// <param name="baseVersion">last version already installed</param>
    public void Install (int baseVersion)
    {
      for (int i = baseVersion + 1; i <= Version; i++) {
        InstallVersion (i);
      }
    }

    /// <summary>
    /// Install a specific version
    /// Create or update table if necessary
    /// 
    /// By default, do nothing
    /// </summary>
    /// <param name="version"></param>
    protected virtual void InstallVersion (int version)
    { }

    /// <summary>
    /// Uninstall the plugin
    /// 
    /// To be overriden if required
    /// </summary>
    public virtual void Uninstall ()
    { }

    /// <summary>
    /// Check the consistency of the properties for the plugin to run
    /// 
    /// This is the method to override
    /// </summary>
    /// <param name="configurationText"></param>
    /// <returns>Can be null or empty if there are no errors</returns>
    public abstract IEnumerable<string> GetConfigurationErrors (string configurationText);

    /// <summary>
    /// Get the configuration errors based on the loading instances
    /// </summary>
    /// <returns>Can be null or empty it there are no errors</returns>
    public virtual IEnumerable<string> GetConfigurationErrors ()
    {
      IEnumerable<string> errors = new List<string> ();
      foreach (var instance in m_instances) {
        IEnumerable<string> instanceErrors = GetConfigurationErrors (instance.InstanceParameters);
        if (null != instanceErrors) {
          errors = errors.Union (instanceErrors);
        }
      }
      return errors;
    }

    /// <summary>
    /// Get the configuration errors for a specific package based on the loading instances
    /// </summary>
    /// <param name="packageName"></param>
    /// <returns>Can be null or empty it there are no errors</returns>
    public virtual IEnumerable<string> GetConfigurationErrorsForSpecificPackage (string packageName)
    {
      IEnumerable<string> errors = new List<string> ();
      foreach (var instance in m_instances.Where (inst => inst.PackageIdentifyingName.Equals (packageName))) {
        IEnumerable<string> instanceErrors = GetConfigurationErrors (instance.InstanceParameters);
        if (null != instanceErrors) {
          errors = errors.Union (instanceErrors);
        }
      }
      return errors;
    }

    #region Equals and GetHashCode implementation
    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals (IPluginDll other)
    {
      return Equals (other as object);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals (object obj)
    {
      GenericPluginDll other = obj as GenericPluginDll;
      if (other == null) {
        return false;
      }

      return object.Equals (this.IdentifyingName, other.IdentifyingName)
        && object.Equals (this.DllPath, other.DllPath);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode ()
    {
      int hashCode = 0;
      unchecked {
        if (null != IdentifyingName) {
          hashCode += 1000000007 * IdentifyingName.GetHashCode ();
        }
        if (DllPath != null) {
          hashCode += 1000000021 * DllPath.GetHashCode ();
        }
      }
      return hashCode;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool operator == (GenericPluginDll lhs, GenericPluginDll rhs)
    {
      if (ReferenceEquals (lhs, rhs)) {
        return true;
      }

      if (ReferenceEquals (lhs, null) || ReferenceEquals (rhs, null)) {
        return false;
      }

      return lhs.Equals (rhs);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool operator != (GenericPluginDll lhs, GenericPluginDll rhs)
    {
      return !(lhs == rhs);
    }
    #endregion
  }
}

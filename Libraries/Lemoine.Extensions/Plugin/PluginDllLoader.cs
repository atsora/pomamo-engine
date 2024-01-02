// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Lemoine.Core.Log;
using System.Diagnostics;
using System.Threading.Tasks;
using Lemoine.Core.Plugin;
using System.Linq;
using System.Threading;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Extensions.Plugin
{
  /// <summary>
  /// Description of ExtensionLoader.
  /// </summary>
  public class PluginDllLoader: IPluginDllLoader
  {
    const string ASSEMBLY_LOAD_KEY = "Plugin.AssemblyLoad";
    const string ASSEMLBY_LOAD_DEFAULT = ""; // load, from, file, memory, default: load or from or file

    #region Members
    readonly PluginLoadStatus m_status = PluginLoadStatus.Unknown;
    readonly string m_pluginName;
    readonly Assembly m_assembly = null;
    bool m_filterMatch = true;
    IPluginDll m_pluginDll = null;
    readonly IList<IExtension> m_extensions = new List<IExtension> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (PluginDllLoader).FullName);
    static readonly ILog pluginStatusLog = LogManager.GetLogger ("Lemoine.Extensions.PluginStatus");

    #region Getters / Setters
    /// <summary>
    /// Plugin load status
    /// </summary>
    public PluginLoadStatus Status { get { return m_status; } }

    /// <summary>
    /// Plugin name
    /// </summary>
    public string PluginName { get { return m_pluginName; } }

    /// <summary>
    /// Return true if the dll is valid
    /// </summary>
    public bool IsValid
    {
      get
      {
        return m_status.Equals (PluginLoadStatus.Success)
          && (m_pluginDll != null) && m_filterMatch;
      }
    }

    /// <summary>
    /// Plugin loaded
    /// </summary>
    public IPluginDll Plugin { get { return m_pluginDll; } }

    /// <summary>
    /// Associated assembly
    /// </summary>
    public Assembly Assembly { get { return m_assembly; } }

    /// <summary>
    /// Did the filter match the plugin ?
    /// </summary>
    public bool FilterMatch => m_filterMatch;

    /// <summary>
    /// Associated extensions
    /// </summary>
    public IList<IExtension> Extensions
    {
      get { return m_extensions; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="status"></param>
    /// <param name="pluginName"></param>
    PluginDllLoader (PluginLoadStatus status, string pluginName)
    {
      m_status = status;
      m_pluginName = pluginName;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="status"></param>
    /// <param name="pluginName"></param>
    /// <param name="assembly"></param>
    PluginDllLoader (PluginLoadStatus status, string pluginName, Assembly assembly)
      : this (status, pluginName)
    {
      m_assembly = assembly;
    }

    static void LogType (string message, Type type)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"LogType: {message} is {type.FullName} in {type.Assembly.FullName}, location {type.Assembly.Location}");
      }
    }

    static PluginDllLoader LoadFromAssembly (Assembly assembly, IPluginFilter pluginFilter)
    {
      Debug.Assert (null != assembly);
      var path = assembly.Location;
      var pluginName = Path.GetFileNameWithoutExtension (path)
        .Replace ("Lemoine.Plugin.", "");

      try {
        // Create an instance of itemDll if possible

        Type[] exportedTypes = assembly.GetExportedTypes ();
        Type pluginType = null;
        IList<Type> activeExtensionTypes = new List<Type> ();
        if (log.IsTraceEnabled) {
          LogType ("IPluginDll", typeof (IPluginDll));
        }
        foreach (Type exportedType in exportedTypes) {
          if (log.IsTraceEnabled) {
            LogType ("exportedType", exportedType);
            foreach (var exportedTypeInterface in exportedType.GetTypeInfo ().GetInterfaces ()) {
              LogType ("exportedTypeInterface", exportedTypeInterface);
            }
          }
          if ((typeof (IPluginDll).IsAssignableFrom (exportedType)
            || typeof (IPluginDll).GetTypeInfo ().IsAssignableFrom (exportedType.GetTypeInfo ()))
              && !typeof (IPluginDll).Equals (exportedType)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"LoadFromAssembly: IPluginDll class found in {assembly}");
            }
            if (null == pluginType) {
              pluginType = exportedType;
            }
            else {
              log.Error ("LoadFromAssembly: a second plugin object was found which was not expected, skip it");
            }
          }
          if (typeof (IExtension).IsAssignableFrom (exportedType)) {
            Type activeExtensionType = exportedType;
            activeExtensionTypes.Add (activeExtensionType);
          }
        }

        if (null == pluginType) {
          var pluginDllLoader = new PluginDllLoader (PluginLoadStatus.MissingPluginClass, pluginName, assembly);
          log.Error ($"LoadFromAssembly: no IPluginDll class found in assembly {path}");
          pluginStatusLog.Error ($"{pluginName}: No IPluginDll class in {path}");
          return pluginDllLoader;
        }
        else { // pluginType != null
          // Create a plugin and initialize the context
          var pluginDllObject = Activator.CreateInstance (pluginType);
          if (log.IsTraceEnabled) {
            var pluginDllType = pluginDllObject.GetType ();
            LogType ("Created instance", pluginDllType);
            foreach (var pluginDllInterface in pluginDllType.GetTypeInfo ().GetInterfaces ()) {
              LogType ("Created instance interface", pluginDllInterface);
            }
          }
          var pluginDll = (IPluginDll)pluginDllObject;
          if (!pluginFilter.IsMatch (pluginDll)) {
            var pluginDllLoader = new PluginDllLoader (PluginLoadStatus.NotMatchingFlags, pluginName, assembly);
            if (log.IsDebugEnabled) {
              log.Debug ($"LoadFromAssembly: do not consider plugin {path} because it does not match filter {pluginFilter}");
            }
            if (pluginStatusLog.IsDebugEnabled) {
              pluginStatusLog.Debug ($"{pluginName}: not matching in {pluginFilter}, path={path}");
            }
            pluginDllLoader.m_filterMatch = false;
            pluginDllLoader.m_pluginDll = pluginDll;
            return pluginDllLoader;
          }
          else {
            var pluginDllLoader = new PluginDllLoader (PluginLoadStatus.Success, pluginName, assembly);
            pluginDllLoader.m_pluginDll = pluginDll;
            pluginDllLoader.m_pluginDll.Context = new PluginContext ();
            pluginDllLoader.m_pluginDll.Context.DllPath = path;
            pluginDllLoader.m_pluginDll.ActiveExtensionTypes = activeExtensionTypes;
            if (pluginStatusLog.IsInfoEnabled) {
              pluginStatusLog.Info ($"{pluginName}: successfully loaded with {activeExtensionTypes.Count} extensions, path={path}");
            }
            return pluginDllLoader;
          }
        }
      }
      catch (Exception ex) {
        var pluginDllLoader = new PluginDllLoader (PluginLoadStatus.AssemblyLoadException, pluginName, assembly);
        log.Error ($"LoadFromAssembly: error while loading {assembly}", ex);
        pluginStatusLog.Error ($"{pluginName}: assembly load error, path={path}", ex);
        pluginDllLoader.m_pluginDll = null;
        pluginDllLoader.m_extensions.Clear ();
        if (Lemoine.Core.ExceptionManagement.ExceptionTest.RequiresExit (ex)) {
          log.Error ("LoadFromAssembly: exception requires to exit, throw it", ex);
          throw;
        }
        return pluginDllLoader;
      }
    }

    /// <summary>
    /// Factory method to load a plugin from 
    /// </summary>
    /// <param name="assemblyLoader">not null</param>
    /// <param name="name"></param>
    /// <param name="pluginDirectories"></param>
    /// <param name="pluginFilter"></param>
    /// <returns></returns>
    public static PluginDllLoader LoadFromName (IAssemblyLoader assemblyLoader, string name, IEnumerable<string> pluginDirectories, IPluginFilter pluginFilter)
    {
      Debug.Assert (null != assemblyLoader);

      string fullName = name;
      if (!fullName.Contains ("Plugin")) {
        fullName = "Lemoine.Plugin." + name;
      }

      Assembly assembly;
      try {
        assembly = assemblyLoader.LoadFromName (fullName, pluginDirectories);
      }
      catch (Exception ex) {
        log.Error ($"LoadFromName: error while loading {name}", ex);
        if (Lemoine.Core.ExceptionManagement.ExceptionTest.RequiresExit (ex)) {
          log.Error ("LoadFromName: exception requires to exit, throw it", ex);
          throw;
        }
        pluginStatusLog.Error ($"{name}: assembly not found (LoadFromName)", ex);
        return new PluginDllLoader (PluginLoadStatus.AssemblyNotFound, name);
      }

      return LoadFromAssembly (assembly, pluginFilter);
    }

    /// <summary>
    /// Factory method to load a plugin from 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="assemblyLoader">not null</param>
    /// <param name="name"></param>
    /// <param name="pluginDirectories"></param>
    /// <param name="pluginFilter"></param>
    /// <returns></returns>
    public static async Task<PluginDllLoader> LoadFromNameAsync (CancellationToken cancellationToken, IAssemblyLoader assemblyLoader, string name, IEnumerable<string> pluginDirectories, IPluginFilter pluginFilter)
    {
      Debug.Assert (null != assemblyLoader);

      return await Task
        .Run<PluginDllLoader> (() => LoadFromName (assemblyLoader, name, pluginDirectories, pluginFilter), cancellationToken);
    }

    /// <summary>
    /// Factory method to load from a plugin path
    /// </summary>
    /// <param name="assemblyLoader">not null</param>
    /// <param name="dllPath"></param>
    /// <param name="pluginFilter"></param>
    /// <returns></returns>
    public static PluginDllLoader LoadDll (IAssemblyLoader assemblyLoader, string dllPath, IPluginFilter pluginFilter)
    {
      Debug.Assert (null != assemblyLoader);

      Assembly assembly;
      try {
        string assemblyLoadType = Lemoine.Info.ConfigSet.LoadAndGet<string> (ASSEMBLY_LOAD_KEY,
                                                                             ASSEMLBY_LOAD_DEFAULT);
        switch (assemblyLoadType.ToLowerInvariant ()) {
        case "load": // This is with context the only safe load type when NHibernateExtension is used
          {
          string fileNameWithoutExtension = Path.GetFileNameWithoutExtension (dllPath);
          assembly = Assembly.Load (fileNameWithoutExtension);
        }
        break;
        case "from":
          assembly = Assembly.LoadFrom (dllPath);
          break;
        case "file":
          assembly = Assembly.LoadFile (dllPath);
          break;
        case "memory": // assembly in memory: this allows to remove the DLL live
                       // But note the debugger and NHibernate do not work in this mode
          assembly = Assembly.Load (File.ReadAllBytes (dllPath));
          break;
        default: // try load then from then file
          assembly = assemblyLoader.LoadFromPath (dllPath);
          break;
        }
      }
      catch (Exception ex) {
        log.Error ($"LoadDll: error while loading {dllPath}", ex);
        if (Lemoine.Core.ExceptionManagement.ExceptionTest.RequiresExit (ex)) {
          log.Error ($"LoadDll: exception requires to exit, throw it", ex);
          throw;
        }
        var pluginName = Path.GetFileNameWithoutExtension (dllPath).Replace ("Lemoine.Plugin.", "");
        return new PluginDllLoader (PluginLoadStatus.AssemblyNotFound, pluginName);
      }

      return LoadFromAssembly (assembly, pluginFilter);
    }

    /// <summary>
    /// Factory method to load from a plugin path
    /// </summary>
    /// <param name="assemblyLoader">not null</param>
    /// <param name="dllPath"></param>
    /// <param name="pluginFilter"></param>
    /// <returns></returns>
    public static async System.Threading.Tasks.Task<PluginDllLoader> LoadDllAsync (IAssemblyLoader assemblyLoader, string dllPath, IPluginFilter pluginFilter)
    {
      Debug.Assert (null != assemblyLoader);

      Assembly assembly;
      try {
        string assemblyLoadType = Lemoine.Info.ConfigSet.LoadAndGet<string> (ASSEMBLY_LOAD_KEY,
                                                                             ASSEMLBY_LOAD_DEFAULT);
        switch (assemblyLoadType.ToLowerInvariant ()) {
        case "load": // This is with context the only safe load type when NHibernateExtension is used
          {
          string fileNameWithoutExtension = Path.GetFileNameWithoutExtension (dllPath);
          assembly = Assembly.Load (fileNameWithoutExtension);
        }
        break;
        case "from":
          assembly = Assembly.LoadFrom (dllPath);
          break;
        case "file":
          assembly = Assembly.LoadFile (dllPath);
          break;
        case "memory": // assembly in memory: this allows to remove the DLL live
                       // But note the debugger and NHibernate do not work in this mode
          byte[] bytes;
          using (var streamReader = new StreamReader (dllPath)) {
            using (var memoryStream = new MemoryStream ()) {
              await streamReader.BaseStream.CopyToAsync (memoryStream);
              bytes = memoryStream.ToArray ();
            }
          }
          assembly = Assembly.Load (bytes);
          break;
        default: // try load then from then file
          assembly = assemblyLoader.LoadFromPath (dllPath);
          break;
        }
      }
      catch (Exception ex) {
        log.Error ($"LoadDll: error while loading {dllPath}", ex);
        if (Lemoine.Core.ExceptionManagement.ExceptionTest.RequiresExit (ex)) {
          log.Error ("LoadDll: exception requires to exit, throw it", ex);
          throw;
        }
        var pluginName = Path.GetFileNameWithoutExtension (dllPath).Replace ("Lemoine.Plugin.", "");
        return new PluginDllLoader (PluginLoadStatus.AssemblyNotFound, pluginName);
      }

      return LoadFromAssembly (assembly, pluginFilter);
    }
    #endregion // Constructors
  }
}

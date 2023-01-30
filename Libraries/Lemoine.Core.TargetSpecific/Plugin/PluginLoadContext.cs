// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#if (NETCOREAPP && !NETCOREAPP2_2 && !NETCOREAPP2_1 && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0)
using System.Runtime.Loader;
#endif // NETCOREAPP >= 3.0
#if NETSTANDARD || NET48 || NETCOREAPP
using System.Windows.Markup;
#endif // NETSTANDARD || NET48 || NETCOREAPP

namespace Lemoine.Core.Plugin.TargetSpecific
{
  internal class PluginLoadContext
#if (NETCOREAPP && !NETCOREAPP2_2 && !NETCOREAPP2_1 && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0)
    : AssemblyLoadContext
#endif // NETCOREAPP >= 3.0
  {
#if (NETCOREAPP && !NETCOREAPP2_2 && !NETCOREAPP2_1 && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0)
    static readonly string TRY_CURRENT_DOMAIN_KEY = "PluginLoadContext.TryCurrentDomain";
    static readonly bool TRY_CURRENT_DOMAIN_DEFAULT = true;

    static readonly string TRY_PROGRAM_DIRECTORY_KEY = "PluginLoadContext.TryProgramDirectory";
    static readonly bool TRY_PROGRAM_DIRECTORY_DEFAULT = true;

    static readonly string TRY_RESOLVERS_KEY = "PluginLoadContext.TryResolvers";
    static readonly bool TRY_RESOLVERS_DEFAULT = true;

    static readonly string NET_STANDARD_PATH_KEY = "PluginLoadContext.NetStandardPath";
    static readonly string NET_STANDARD_PATH_DEFAULT = ""; // Empty: not set. For example: @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.3\netstandard.dll"

    static readonly string ADDITIONAL_PATHS_KEY = "PluginLoadContext.AdditionalPaths";
    static readonly string ADDITIONAL_PATHS_DEFAULT = ""; // Empty: not set. Use ; as a separator between two paths. For example: @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\3.1.3\netstandard.dll;C:\Program Files\dotnet\..."
#endif // NETCOREAPP >= 3.0

    /// <summary>
    /// Accept all the time a change of revision, which is the last number in the version
    /// </summary>
    static readonly string ACCEPT_REVISION_CHANGE_KEY = "PluginLoadContext.AcceptRevisionChange";
    static readonly bool ACCEPT_REVISION_CHANGE_DEFAULT = true;

    static readonly string ACCEPT_SYSTEM_VERSION_MISMATCH_KEY = "PluginLoadContext.AcceptSystemVersionMismatch";
    static readonly bool ACCEPT_SYSTEM_VERSION_MISMATCH_DEFAULT = false;

    static readonly string STRICT_VERSION_CHECK_KEY = "PluginLoadContext.StrictVersionCheck";
    static readonly bool STRICT_VERSION_CHECK_DEFAULT = false;

    /// <summary>
    /// ; separated list of assembly names where the version check should be disallowed
    /// </summary>
    static readonly string DISALLOW_VERSION_CHECK_KEY = "PluginLoadContext.DisallowVersionCheck";
    static readonly string DISALLOW_VERSION_CHECK_DEFAULT = "";

    /// <summary>
    /// Accept Lemoine assemblies with version 1.0.0.0
    /// </summary>
    static readonly string ACCEPT_LEMOINE_INITIAL_VERSION_KEY = "PluginLoadContext.AcceptLemoineInitialVersion";
    static readonly bool ACCEPT_LEMOINE_INITIAL_VERSION_DEFAULT = true;

    readonly ILog log = LogManager.GetLogger<PluginLoadContext> ();
    readonly ILog successLog = LogManager.GetLogger ($"{typeof (PluginLoadContext).FullName}.Success");

#if (NETCOREAPP && !NETCOREAPP2_2 && !NETCOREAPP2_1 && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0)
    readonly IEnumerable<AssemblyDependencyResolver> m_resolvers;
#endif // NETCOREAPP >= 3.0

    public PluginLoadContext (params string[] paths)
    {
#if (NETCOREAPP && !NETCOREAPP2_2 && !NETCOREAPP2_1 && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0)
      m_resolvers = paths
        .Where (p => (null != p))
        .Select (p => new AssemblyDependencyResolver (p));
#endif // NETCOREAPP >= 3.0
    }

#if (NETCOREAPP && !NETCOREAPP2_2 && !NETCOREAPP2_1 && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0)
    public PluginLoadContext (string pluginPath, params string[] additionalPaths)
    {
      var list = new List<AssemblyDependencyResolver>
      {
        new AssemblyDependencyResolver (pluginPath),
      };
      var netStandardPath = Lemoine.Info.ConfigSet
        .LoadAndGet (NET_STANDARD_PATH_KEY, NET_STANDARD_PATH_DEFAULT);
      if (!string.IsNullOrEmpty (netStandardPath)) {
        list.Add (new AssemblyDependencyResolver (netStandardPath));
      }
      var configAdditionalPaths = Lemoine.Info.ConfigSet
        .LoadAndGet (ADDITIONAL_PATHS_KEY, ADDITIONAL_PATHS_DEFAULT);
      if (!string.IsNullOrEmpty (configAdditionalPaths)) {
        list.AddRange (configAdditionalPaths.Split (';', StringSplitOptions.RemoveEmptyEntries).Select (x => new AssemblyDependencyResolver (x)));
      }
      m_resolvers = list
        .Concat (additionalPaths
          .Where (p => (null != p))
          .Select (p => new AssemblyDependencyResolver (p)));
      if (log.IsDebugEnabled) {
        log.Debug ($"PluginLoadContext: {m_resolvers.Count ()} resolvers from pluginPath {pluginPath} and additional paths {string.Join (";", additionalPaths)}");
      }
    }
#endif // NETCOREAPP >= 3.0

    public Assembly LoadFromCurrentDomain (AssemblyName assemblyName)
    {
      var assembly = AppDomain.CurrentDomain.GetAssemblies ()
        .FirstOrDefault (a => IsAssemblyMatch (assemblyName, a));
      if (log.IsInfoEnabled) {
        if (null == assembly) {
          if (log.IsDebugEnabled) {
            log.Debug ($"LoadFromCurrentDomain: {assemblyName} was not in the already loaded assemblies");
          }
        }
        else { // null != assembly
          log.Info ($"LoadFromCurrentDomain: {assemblyName} is already loaded in current domain, return it");
        }
      }
      if (successLog.IsDebugEnabled) {
        successLog.Debug ($"LoadFromCurrentDomain: {assemblyName} loaded from current domain");
      }
      return assembly;
    }

    bool IsAssemblyMatch (AssemblyName x, Assembly assembly)
    {
      var y = assembly.GetName ();
      if (!string.Equals (x.Name, y.Name)) {
        return false;
      }
      if (string.Equals (x.FullName, assembly.FullName)) {
        return true;
      }
#if NETSTANDARD || NET48 || NETCOREAPP
      if (!object.Equals (x.CultureName, y.CultureName)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsAssemblyMatch: culture does not match for {y.FullName}, {x.CultureName} VS {y.CultureName}");
        }
        return false;
      }
#endif // NETSTANDARD || NET48 || NETCOREAPP
      var tx = x.GetPublicKeyToken ();
      if ((null != tx) && (0 < tx.Length)) {
        var ty = y.GetPublicKeyToken ();
        if (tx.Length != ty.Length) {
          log.Error ($"IsAssemblyMatch: public key {ty} does not match {tx} for {y.FullName} (different length)");
          return false;
        }
        for (int i = 0; i < tx.Length; ++i) {
          if (tx[i] != ty[i]) {
            log.Error ($"IsAssemblyMatch: public key {ty} does not match {tx} for {y.FullName}");
            return false;
          }
        }
      }
      var vx = x.Version;
      if (null != vx) {
        var vy = y.Version;
        var compare = vx.CompareTo (vy);
        if (0 == compare) {
          return true;
        }
        if (IsDisallowVersionCheck (x)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"IsAssemblyMatch: version check disallowed for {x.FullName}");
          }
          return true;
        }
        if ((vx.Major == vy.Major) && (vx.Minor == vy.Minor) && (vx.Build == vy.Build)
          && Lemoine.Info.ConfigSet.LoadAndGet (ACCEPT_REVISION_CHANGE_KEY, ACCEPT_REVISION_CHANGE_DEFAULT)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"IsAssemblyMatch: only a revision change in {x.FullName}, {vx} VS {vy} => accept it");
          }
          return true;
        }
        if (x.Name.StartsWith ("Lemoine.")
          && vx.ToString ().Equals ("1.0.0.0")
          && Lemoine.Info.ConfigSet.LoadAndGet (ACCEPT_LEMOINE_INITIAL_VERSION_KEY, ACCEPT_LEMOINE_INITIAL_VERSION_DEFAULT)) {
          if (log.IsInfoEnabled) {
            log.Info ($"IsAssemblyMatch: version of {x.FullName} is 1.0.0.0 while tested assembly is {y.FullName}, accept it");
          }
          return true;
        }
        if (x.Name.StartsWith ("netstandard")) {
          if (log.IsDebugEnabled) {
            log.Debug ($"IsAssemblyMatch: version mismatch in {x.FullName}, {vx} VS {vy}");
          }
          return false;
        }
        if (x.Name.StartsWith ("System.")) {
          var result = Lemoine.Info.ConfigSet.LoadAndGet (ACCEPT_SYSTEM_VERSION_MISMATCH_KEY, ACCEPT_SYSTEM_VERSION_MISMATCH_DEFAULT);
          if (log.IsDebugEnabled) {
            log.Debug ($"IsAssemblyMatch: major version mismatch in System. assembly {x.FullName}, {vx} VS {vy} => return {result}");
          }
          return result;
        }
        if (!x.Name.StartsWith ("Lemoine.")) {
          log.Error ($"IsAssemblyMatch: version mismatch in {x.FullName}, {vx} VS {vy}");
          return false;
        }
        else { // x.Name.StartsWith ("Lemoine.")
          if (vx.Major != vy.Major) {
            var strictVersionCheck = Lemoine.Info.ConfigSet.LoadAndGet (STRICT_VERSION_CHECK_KEY, STRICT_VERSION_CHECK_DEFAULT);
            log.Error ($"IsAssemblyMatch: major version does not match in {y.FullName}, {vx.Major} VS {vy.Major} => return {!strictVersionCheck}");
            return !strictVersionCheck;
          }
          else if (vx.Minor <= vy.Minor) {
            return true;
          }
          else {
            var strictVersionCheck = Lemoine.Info.ConfigSet.LoadAndGet (STRICT_VERSION_CHECK_KEY, STRICT_VERSION_CHECK_DEFAULT);
            log.Error ($"IsAssemblyMatch: minor version does not match in {y.FullName}, {vx.Major} VS {vy.Major} => return {!strictVersionCheck}");
            return !strictVersionCheck;
          }
        }
      }
      return true;
    }

    bool IsDisallowVersionCheck (AssemblyName x)
    {
      var disallowVersionCheck = Lemoine.Info.ConfigSet.LoadAndGet (DISALLOW_VERSION_CHECK_KEY, DISALLOW_VERSION_CHECK_DEFAULT);
      if (!string.IsNullOrEmpty (disallowVersionCheck)) {
        var vx = x.Version;
        var noVersionCheckAssemblyStrings = disallowVersionCheck.Split (new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var noVersionCheckAssemblyString in noVersionCheckAssemblyStrings) {
          try {
            var noVersionCheckAssemblyName = new AssemblyName (noVersionCheckAssemblyString);
            if (AssemblyName.ReferenceMatchesDefinition (noVersionCheckAssemblyName, x)) {
              if ((noVersionCheckAssemblyName.Version is null) || (0 == vx.CompareTo (noVersionCheckAssemblyName.Version))) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"IsAssemblyMatch: assembly {noVersionCheckAssemblyString} in {DISALLOW_VERSION_CHECK_KEY} matches {x.FullName} => return true without checking the version");
                }
                return true;
              }
              else if (log.IsDebugEnabled) {
                log.Debug ($"IsAssemblyMatch: assembly {noVersionCheckAssemblyString} in {DISALLOW_VERSION_CHECK_KEY} but version does not match, continue");
              }
            }
          }
          catch (Exception ex) {
            log.Error ($"IsAssemblyMatch: bad assembly {noVersionCheckAssemblyString}", ex);
          }
        }
      }
      return false;
    }

#if (NETCOREAPP && !NETCOREAPP2_2 && !NETCOREAPP2_1 && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0)
    Assembly LoadFromProgramDirectory (AssemblyName assemblyName)
    {
      var programDirectory = Lemoine.Info.ProgramInfo.AbsoluteDirectory;
      if (null != programDirectory) {
        var path = System.IO.Path.Combine (programDirectory, assemblyName.Name + ".dll");
        if (!System.IO.File.Exists (path)) {
          if (log.IsInfoEnabled) {
            log.Info ($"LoadFromProgramDirectory: {assemblyName} does not exist in programDirectory {programDirectory}");
          }
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"LoadFromProgramDirectory: try to load {assemblyName} from programDirectory {programDirectory}, path {path}");
          }
          try {
            var assembly = LoadFromAssemblyPath (path);
            if (log.IsInfoEnabled) {
              log.Info ($"LoadFromProgramDirectory: success for {assemblyName} with path {path} based on programDirectory {programDirectory}");
            }
            if (successLog.IsInfoEnabled) {
              successLog.Info ($"LoadFromProgramDirectory: {assemblyName} => {path}");
            }
            return assembly;
          }
          catch (Exception ex) {
            log.Warn ($"LoadFromProgramDirectory: {assemblyName} could not be loaded from programDirectory {programDirectory}, path {path}", ex);
          }
        }
      }

      return null;
    }

    Assembly LoadFromResolvers (AssemblyName assemblyName)
    {
      foreach (var resolver in m_resolvers) {
        string assemblyPath = resolver.ResolveAssemblyToPath (assemblyName);
        if (log.IsDebugEnabled && (null == assemblyPath)) {
          log.Debug ($"LoadFromResolvers: resolver could not resolve {assemblyName}");
        }
        if (assemblyPath != null) {
          if (log.IsDebugEnabled) {
            log.Debug ($"LoadFromResolvers: path is {assemblyPath} for {assemblyName}");
          }
          try {
            var assembly = LoadFromAssemblyPath (assemblyPath);
            if (log.IsInfoEnabled) {
              log.Info ($"LoadFromResolvers: success for {assemblyName} with path {assemblyPath} with resolver");
            }
            if (successLog.IsInfoEnabled) {
              successLog.Info ($"LoadFromResolvers: {assemblyName} => {assemblyPath}");
            }
            return assembly;
          }
          catch (Exception ex) {
            log.Warn ($"LoadFromResolvers: {assemblyName} could not be loaded from path {assemblyPath} with resolver", ex);
          }
        }
      }

      return null;
    }

    protected override Assembly Load (AssemblyName assemblyName)
    {
      if (Lemoine.Info.ConfigSet.LoadAndGet (TRY_CURRENT_DOMAIN_KEY, TRY_CURRENT_DOMAIN_DEFAULT)) {
        var assembly = LoadFromCurrentDomain (assemblyName);
        if (null != assembly) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Load: {assemblyName} was in current domain, return it");
          }
          return assembly;
        }
      }

      if (Lemoine.Info.ConfigSet.LoadAndGet (TRY_PROGRAM_DIRECTORY_KEY, TRY_PROGRAM_DIRECTORY_DEFAULT)) {
        var assembly = LoadFromProgramDirectory (assemblyName);
        if (null != assembly) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Load: {assemblyName} was in program directory, return it");
          }
          return assembly;
        }
      }

      if (Lemoine.Info.ConfigSet.LoadAndGet (TRY_RESOLVERS_KEY, TRY_RESOLVERS_DEFAULT)) {
        var assembly = LoadFromResolvers (assemblyName);
        if (null != assembly) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Load: {assemblyName} was resolved by resolvers, return it");
          }
          return assembly;
        }
      }

      if (log.IsWarnEnabled) {
        if (!assemblyName.Name.Equals ("netstandard")) {
          if (log.IsWarnEnabled) {
            log.Warn ($"Load: {assemblyName} could not be loaded, no valid path");
          }
        }
        else if (log.IsDebugEnabled) { // netstandard
          log.Debug ($"Load: {assemblyName} could not be loaded, no valid path");
        }
      }
      if (!assemblyName.Name.Equals ("netstandard") && successLog.IsWarnEnabled) {
        log.Warn ($"Load: {assemblyName} could not be loaded, no valid path");
      }

      return null;
    }

    protected override IntPtr LoadUnmanagedDll (string unmanagedDllName)
    {
      foreach (var resolver in m_resolvers) {
        string libraryPath = resolver.ResolveUnmanagedDllToPath (unmanagedDllName);
        if (libraryPath != null) {
          if (log.IsInfoEnabled) {
            log.Info ($"LoadUnmanagedDll: success for {unmanagedDllName} with resolver {resolver}");
          }
          return LoadUnmanagedDllFromPath (libraryPath);
        }
      }

      if (log.IsInfoEnabled) {
        log.Info ($"LoadUnmanagedRll: {unmanagedDllName} could not be loaded, no valid path");
      }

      return IntPtr.Zero;
    }
#endif // NETCOREAPP >= 3.0
  }
}

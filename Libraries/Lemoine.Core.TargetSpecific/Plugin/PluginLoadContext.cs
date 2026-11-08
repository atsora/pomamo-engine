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

    /// <summary>
    /// Resolve in the program directory the assemblies the default load context does not find
    /// in the deps.json file of the program (TPA list). Read once, when the first plugin load
    /// context is created
    /// </summary>
    static readonly string DEFAULT_CONTEXT_RESOLVING_KEY = "PluginLoadContext.DefaultContextResolving";
    static readonly bool DEFAULT_CONTEXT_RESOLVING_DEFAULT = false;

    static readonly ILog staticLog = LogManager.GetLogger ($"{typeof (PluginLoadContext).FullName}.DefaultContext");

    /// <summary>
    /// Log of the plugin status, where the dependencies that are loaded for each plugin are listed
    /// </summary>
    static readonly ILog pluginStatusLog = LogManager.GetLogger ("Lemoine.Extensions.PluginStatus");

    /// <summary>
    /// Names of the assemblies in the deps.json file of the program (TPA list)
    /// </summary>
    static readonly Lazy<HashSet<string>> s_trustedPlatformAssemblyNames = new Lazy<HashSet<string>> (GetTrustedPlatformAssemblyNames);
#endif // NETCOREAPP >= 3.0

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
    readonly string m_pluginName = "";
#endif // NETCOREAPP >= 3.0

#if (NETCOREAPP && !NETCOREAPP2_2 && !NETCOREAPP2_1 && !NETCOREAPP2_0 && !NETCOREAPP1_1 && !NETCOREAPP1_0)
    /// <summary>
    /// Static constructor: register once the resolution of the default load context in the
    /// program directory, if active
    /// </summary>
    static PluginLoadContext ()
    {
      try {
        if (Lemoine.Info.ConfigSet.LoadAndGet (DEFAULT_CONTEXT_RESOLVING_KEY, DEFAULT_CONTEXT_RESOLVING_DEFAULT)) {
          AssemblyLoadContext.Default.Resolving += ResolveInDefaultContextFromProgramDirectory;
          if (staticLog.IsInfoEnabled) {
            staticLog.Info ($"PluginLoadContext: resolution of the default load context in the program directory registered");
          }
        }
      }
      catch (Exception ex) {
        staticLog.Error ($"PluginLoadContext: the resolution of the default load context in the program directory could not be registered", ex);
      }
    }
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
      m_pluginName = System.IO.Path.GetFileNameWithoutExtension (pluginPath) ?? "";
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
        if (IsVersionCompatible (vx, vy)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"IsAssemblyMatch: version {vy} of {y.Name} is compatible with the requested version {vx} => accept it");
          }
          return true;
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

    /// <summary>
    /// Is an available version of an assembly compatible with the requested version?
    ///
    /// Compatible if the major versions are the same and the available minor version is the
    /// same or greater: a greater minor version only adds features, without any API break, and
    /// the build and revision numbers (third and fourth numbers) may be different
    /// </summary>
    /// <param name="requested">nullable: any version is compatible</param>
    /// <param name="available">nullable: unknown, considered compatible</param>
    /// <returns></returns>
    internal static bool IsVersionCompatible (Version requested, Version available)
    {
      if (requested is null || available is null) {
        return true;
      }
      return (requested.Major == available.Major) && (requested.Minor <= available.Minor);
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
    /// <summary>
    /// Names of the assemblies of the TPA list (deps.json file of the program), without extension
    /// </summary>
    /// <returns>not null</returns>
    static HashSet<string> GetTrustedPlatformAssemblyNames ()
    {
      var trustedPlatformAssemblies = AppContext.GetData ("TRUSTED_PLATFORM_ASSEMBLIES") as string;
      if (string.IsNullOrEmpty (trustedPlatformAssemblies)) {
        staticLog.Warn ($"GetTrustedPlatformAssemblyNames: no TRUSTED_PLATFORM_ASSEMBLIES, no assembly will be loaded in the default load context");
        return new HashSet<string> (StringComparer.OrdinalIgnoreCase);
      }
      return new HashSet<string> (trustedPlatformAssemblies
        .Split (System.IO.Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
        .Select (p => System.IO.Path.GetFileNameWithoutExtension (p)), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Is an assembly in the TPA list, which means in the deps.json file of the program (or of
    /// the framework)?
    /// </summary>
    /// <param name="assemblyName">not null</param>
    /// <returns></returns>
    static bool IsTrustedPlatformAssembly (AssemblyName assemblyName) =>
      !string.IsNullOrEmpty (assemblyName.Name) && s_trustedPlatformAssemblyNames.Value.Contains (assemblyName.Name);

    /// <summary>
    /// Is an assembly a dependency of the plugin, and not the plugin itself?
    /// </summary>
    /// <param name="assemblyName">not null</param>
    /// <returns></returns>
    bool IsDependency (AssemblyName assemblyName) =>
      !string.Equals (assemblyName.Name, m_pluginName, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Check the version of an assembly file against the requested version, see
    /// <see cref="IsVersionCompatible(Version, Version)"/>
    ///
    /// The runtime does not check the version of an assembly that is returned by a plugin
    /// load context: without this check, any version would be loaded, and an incompatible one
    /// would fail later (MissingMethodException, TypeLoadException...)
    /// </summary>
    /// <param name="assemblyName">requested assembly, not null</param>
    /// <param name="path">path of the assembly file, not null</param>
    /// <returns>the file can be loaded: compatible version, or not compatible but not in strict mode (PluginLoadContext.StrictVersionCheck)</returns>
    bool AcceptFileVersion (AssemblyName assemblyName, string path)
    {
      Version availableVersion;
      try {
        availableVersion = AssemblyName.GetAssemblyName (path).Version;
      }
      catch (Exception ex) {
        log.Warn ($"AcceptFileVersion: the version of {path} could not be read, consider it is compatible", ex);
        return true;
      }
      if (IsVersionCompatible (assemblyName.Version, availableVersion)) {
        if (log.IsDebugEnabled && !object.Equals (assemblyName.Version, availableVersion)) {
          log.Debug ($"AcceptFileVersion: version {availableVersion} of {path} is compatible with the requested version {assemblyName.Version}");
        }
        return true;
      }
      if (IsDisallowVersionCheck (assemblyName)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"AcceptFileVersion: version check disallowed for {assemblyName}");
        }
        return true;
      }
      var strictVersionCheck = Lemoine.Info.ConfigSet.LoadAndGet (STRICT_VERSION_CHECK_KEY, STRICT_VERSION_CHECK_DEFAULT);
      var message = $"version {availableVersion} of {path} is not compatible with the requested version {assemblyName.Version} (different major version or lower minor version) => {(strictVersionCheck ? "refuse it" : "load it anyway")}";
      log.Error ($"AcceptFileVersion: {message}");
      if (pluginStatusLog.IsErrorEnabled) {
        pluginStatusLog.Error ($"{m_pluginName}: dependency {assemblyName.Name}: {message}");
      }
      return !strictVersionCheck;
    }

    /// <summary>
    /// Load an assembly of the program directory
    ///
    /// If the assembly is in the deps.json file of the program (TPA list), it is loaded in the
    /// default load context, so that it is shared by the program and all the plugins, whatever
    /// the order they are loaded in. Else a plugin that is loaded before the program uses the
    /// assembly gets its own copy of it, with types that are different from the ones of the
    /// program (for example a GraphQL scalar type of a plugin that is not a HotChocolate schema
    /// type for the program). Its dependencies are in the TPA list too
    ///
    /// Else (an assembly the program does not know, for example a plugin, a CNC module or a
    /// library that is only used by a plugin), it is loaded in this plugin load context, where
    /// its dependencies are resolved by <see cref="Load(AssemblyName)"/>. The default load
    /// context would not find them, since it only knows the TPA list
    ///
    /// If the assembly could not be loaded in the default load context, it is loaded in this
    /// plugin load context
    ///
    /// Note: the static members of the assemblies of the TPA list are shared by the program and
    /// all the plugins too. If a plugin requires its own static state of a library, this library
    /// must stay in the plugin load context: in this case, add a per-plugin list of the
    /// assemblies to keep local, declared with an assembly attribute in the plugin and read by
    /// <see cref="AssemblyLoader"/> right after the plugin assembly is loaded, before its types
    /// are enumerated (the types enumeration already resolves the dependencies)
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <returns></returns>
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
          if (!AcceptFileVersion (assemblyName, path)) {
            return null;
          }
          try {
            Assembly assembly = null;
            var contextName = "plugin";
            if (IsTrustedPlatformAssembly (assemblyName)) {
              try {
                // Without the version, that was already checked: the default load context
                // refuses a lower version, even if only the build or revision number is lower
                assembly = AssemblyLoadContext.Default.LoadFromAssemblyName (new AssemblyName (assemblyName.Name));
                contextName = "default";
              }
              catch (Exception ex) {
                log.Warn ($"LoadFromProgramDirectory: {assemblyName} could not be loaded in the default load context => load it from path {path} in the plugin load context", ex);
              }
            }
            else if (log.IsDebugEnabled) {
              log.Debug ($"LoadFromProgramDirectory: {assemblyName} is not in the TPA list => load it in the plugin load context");
            }
            if (assembly is null) {
              assembly = LoadFromAssemblyPath (path);
            }
            if (log.IsInfoEnabled) {
              log.Info ($"LoadFromProgramDirectory: success for {assemblyName} with path {path} based on programDirectory {programDirectory} in the {contextName} load context");
            }
            if (successLog.IsInfoEnabled) {
              successLog.Info ($"LoadFromProgramDirectory: {assemblyName} => {path} ({contextName} load context)");
            }
            if (pluginStatusLog.IsInfoEnabled && IsDependency (assemblyName)) {
              pluginStatusLog.Info ($"{m_pluginName}: dependency {assemblyName.Name} {assemblyName.Version} loaded from the program directory {path} ({contextName} load context)");
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

    /// <summary>
    /// Resolve in the default load context an assembly of the program directory that is not
    /// in the deps.json file of the program (TPA list)
    ///
    /// <see cref="LoadFromProgramDirectory"/> only loads in the default load context the
    /// assemblies of the TPA list, whose dependencies are in the TPA list too, so this is
    /// normally not required. It remains as a fallback, for example for an assembly of the TPA
    /// list that would depend on an assembly of the program directory that is not in it
    ///
    /// Active only if the config key PluginLoadContext.DefaultContextResolving is true
    /// </summary>
    /// <param name="context">default load context</param>
    /// <param name="assemblyName">not null</param>
    /// <returns>null if not found</returns>
    static Assembly ResolveInDefaultContextFromProgramDirectory (AssemblyLoadContext context, AssemblyName assemblyName)
    {
      var programDirectory = Lemoine.Info.ProgramInfo.AbsoluteDirectory;
      if (string.IsNullOrEmpty (programDirectory) || string.IsNullOrEmpty (assemblyName.Name)) {
        return null;
      }
      var path = System.IO.Path.Combine (programDirectory, assemblyName.Name + ".dll");
      if (!System.IO.File.Exists (path)) {
        if (staticLog.IsDebugEnabled) {
          staticLog.Debug ($"ResolveInDefaultContextFromProgramDirectory: {assemblyName} not in programDirectory {programDirectory}");
        }
        return null;
      }
      try {
        var assembly = context.LoadFromAssemblyPath (path);
        if (staticLog.IsInfoEnabled) {
          staticLog.Info ($"ResolveInDefaultContextFromProgramDirectory: {assemblyName} => {path}");
        }
        return assembly;
      }
      catch (Exception ex) {
        staticLog.Error ($"ResolveInDefaultContextFromProgramDirectory: {assemblyName} could not be loaded from {path}", ex);
        return null;
      }
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
          if (!AcceptFileVersion (assemblyName, assemblyPath)) {
            continue;
          }
          try {
            var assembly = LoadFromAssemblyPath (assemblyPath);
            if (log.IsInfoEnabled) {
              log.Info ($"LoadFromResolvers: success for {assemblyName} with path {assemblyPath} with resolver");
            }
            if (successLog.IsInfoEnabled) {
              successLog.Info ($"LoadFromResolvers: {assemblyName} => {assemblyPath}");
            }
            if (pluginStatusLog.IsInfoEnabled && IsDependency (assemblyName)) {
              pluginStatusLog.Info ($"{m_pluginName}: dependency {assemblyName.Name} {assemblyName.Version} loaded from {assemblyPath} (plugin load context)");
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

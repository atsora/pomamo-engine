// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Lemoine.Core.Log;

namespace Lemoine.Info
{
  /// <summary>
  /// Singleton class to get some information on the currently run program
  /// </summary>
  public sealed class ProgramInfo
  {
    #region Members
    readonly Assembly m_assembly = Assembly.GetEntryAssembly ();
    string m_alternativeProgramName = null;
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (ProgramInfo).FullName);

    #region Getters / Setters
    /// <summary>
    /// Executing assembly
    /// </summary>
    public static Assembly Assembly
    {
      get { return Instance.m_assembly; }
    }
    
    /// <summary>
    /// Name of the program
    /// (can be null in case the entry assembly can't be got)
    /// </summary>
    public static string Name
    {
      get {
        if (null != Instance.m_alternativeProgramName) {
          log.InfoFormat ("Name.get: return the alternative program name {0}",
            Instance.m_alternativeProgramName);
          return Instance.m_alternativeProgramName;
        }
        else if (Assembly == null) {
          log.WarnFormat ("Could not get the name " +
                          "because the entry assembly is null assembly");
          return null;
        }
        else {
          System.Diagnostics.Debug.Assert (null != Assembly);
          return Assembly.GetName ().Name;
        }
      }
      set {
        log.InfoFormat ("Name.set: set an alternative program name {0}", value);
        Instance.m_alternativeProgramName = value;
      }
    }
    
    /// <summary>
    /// Version of the program
    /// (can be null in case the entry assembly can't be got)
    /// </summary>
    public static string Version
    {
      get {
        if (Assembly == null) {
          log.WarnFormat ("Could not get the version " +
                          "because the entry assembly is null assembly");
          return null;
        }
        else {
          System.Diagnostics.Debug.Assert (null != Assembly);
          return Assembly.GetName ().Version.ToString ();
        }
      }
    }
    
    /// <summary>
    /// Company
    /// (can be null in case the entry assembly can't be got)
    /// </summary>
    public static string Company
    {
      get {
        if (Assembly == null) {
          log.WarnFormat ("Could not get the company " +
                          "because the entry assembly is null assembly");
          return null;
        }
        else {
          System.Diagnostics.Debug.Assert (null != Assembly);
          return ((AssemblyCompanyAttribute)Assembly
                  .GetCustomAttributes (typeof (AssemblyCompanyAttribute),
                                        false) [0]).Company;
        }
      }
    }
    
    /// <summary>
    /// Full path of the program file
    /// (can be null in case the entry assembly can't be got)
    /// </summary>
    public static string AbsolutePath
    {
      get {
        if (Assembly == null) {
          log.WarnFormat ("AbsolutePath: Could not get the absolute path " +
                          "because the entry assembly is null");
          return null;
        }
        else {
          System.Diagnostics.Debug.Assert (null != Assembly);
          return Assembly.Location;
        }
      }
    }

    /// <summary>
    /// Full directory path where the program file is located
    /// (can be null in case the entry assembly can't be got)
    /// </summary>
    public static string AbsoluteDirectory
    {
      get
      {
        var absolutePath = ProgramInfo.AbsolutePath;
        if (absolutePath == null) {
          log.Warn ($"AbsoluteDirectory: absolute path is null");
          return null;
        }
        else {
          return System.IO.Path.GetDirectoryName (absolutePath);
        }
      }
    }

    /// <summary>
    /// Get the directory of the current assembly
    /// </summary>
    public static string AssemblyDirectory {
      get {
        string codeBase = Assembly.CodeBase;
        var uri = new UriBuilder(codeBase);
        string path = Uri.UnescapeDataString(uri.Path);
        return Path.GetDirectoryName(path);
      }
    }
    
    /// <summary>
    /// Get the physical memory of this process
    /// </summary>
    /// <returns></returns>
    public static long GetPhysicalMemory ()
    {
      return Process.GetCurrentProcess ().WorkingSet64;
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private ProgramInfo()
    {
    }
    #endregion
    
    #region Instance
    static ProgramInfo Instance
    {
      get { return Nested.instance; }
    }
    
    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested()
      {
      }

      internal static readonly ProgramInfo instance = new ProgramInfo ();
    }
    #endregion
  }
}

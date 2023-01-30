// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System.Reflection;
using Lemoine.Core.Log;

namespace Lemoine.Info
{
  /// <summary>
  /// Singleton class to get some information on the calling assembly
  /// </summary>
  public sealed class AssemblyInfo
  {
    #region Members
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (AssemblyInfo).FullName);

    #region Getters / Setters
    /// <summary>
    /// Calling assembly
    /// </summary>
    public static Assembly CallingAssembly
    {
      get { return Assembly.GetCallingAssembly (); }
    }

    /// <summary>
    /// Name of the calling assembly or program
    /// </summary>
    public static string Name
    {
      get {
        return Assembly.GetCallingAssembly ().GetName ().Name;
      }
    }

    /// <summary>
    /// Version of the calling assembly or program
    /// </summary>
    public static string Version
    {
      get {
        return Assembly.GetCallingAssembly ().GetName ().Version.ToString ();
      }
    }

    /// <summary>
    /// Company of the calling assembly or program
    /// </summary>
    public static string Company
    {
      get {
        return ((AssemblyCompanyAttribute)Assembly.GetCallingAssembly ()
                .GetCustomAttributes (typeof (AssemblyCompanyAttribute),
                                      false)[0]).Company;
      }
    }

    /// <summary>
    /// Full path of the program file
    /// </summary>
    public static string AbsolutePath
    {
      get {
        return Assembly.GetCallingAssembly ().Location;
      }
    }

    /// <summary>
    /// Full directory path where the program file is located
    /// </summary>
    public static string AbsoluteDirectory
    {
      get {
        var absolutePath = AssemblyInfo.AbsolutePath;
        if (null != absolutePath) {
          return System.IO.Path.GetDirectoryName (absolutePath);
        }
        else {
          return null;
        }
      }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private AssemblyInfo()
    {
    }
    #endregion

    #region Methods
    #endregion
    
    #region Instance
    static AssemblyInfo Instance
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

      internal static readonly AssemblyInfo instance = new AssemblyInfo ();
    }    
    #endregion
  }
}

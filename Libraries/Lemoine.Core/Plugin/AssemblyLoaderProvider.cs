// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Core.Plugin
{
  /// <summary>
  /// AssemblyLoaderProvider (singleton)
  /// </summary>
  public sealed class AssemblyLoaderProvider
  {
    ILog log = LogManager.GetLogger (typeof (AssemblyLoaderProvider).FullName);
    static readonly ILog slog = LogManager.GetLogger (typeof (AssemblyLoaderProvider).FullName);

    IAssemblyLoader m_assemblyLoader = null;

    #region Getters / Setters
    /// <summary>
    /// Assembly loader
    /// </summary>
    public static IAssemblyLoader AssemblyLoader
    {
      get {
        if (null == Instance.m_assemblyLoader) {
          slog.Error ($"AssemblyLoader.get: no assembly loader was set, use the default one");
          return new DefaultAssemblyLoader ();
        }
        else {
          return Instance.m_assemblyLoader;
        }
      }
      set {
        Instance.m_assemblyLoader = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor (private because singleton)
    /// </summary>
    AssemblyLoaderProvider ()
    {
    }
    #endregion // Constructors

    #region Instance
    static AssemblyLoaderProvider Instance
    {
      get { return Nested.instance; }
    }

    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested ()
      {
      }

      internal static readonly AssemblyLoaderProvider instance = new AssemblyLoaderProvider ();
    }
    #endregion // Instance
  }
}

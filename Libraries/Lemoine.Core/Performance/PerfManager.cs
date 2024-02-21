// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Core.Performance
{
  /// <summary>
  /// PerfRecorderManager (singleton)
  /// </summary>
  public sealed class PerfManager
  {
    ILog log = LogManager.GetLogger (typeof (PerfManager).FullName);
    static readonly ILog slog = LogManager.GetLogger (typeof (PerfManager).FullName);

    IPerfRecorder m_perfRecorder = null;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor (private because singleton)
    /// </summary>
    PerfManager ()
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Initialize the manager with a performance recorder
    /// 
    /// Not thread safe
    /// </summary>
    /// <param name="perfRecorder"></param>
    public static void SetRecorder (IPerfRecorder perfRecorder)
    {
      Instance.m_perfRecorder = perfRecorder;
    }

    /// <summary>
    /// Record a duration for a specific key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="duration"></param>
    public static void Record (string key, TimeSpan duration)
    {
      Instance.m_perfRecorder?.Record (key, duration);
    }

    #region Instance
    static PerfManager Instance
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

      internal static readonly PerfManager instance = new PerfManager ();
    }
    #endregion // Instance
  }
}

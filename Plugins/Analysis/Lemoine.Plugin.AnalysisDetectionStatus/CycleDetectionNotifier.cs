// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Core.Log;
using System.Linq;

namespace Lemoine.Plugin.AnalysisDetectionStatus
{
  /// <summary>
  /// Description of Notifier.
  /// </summary>
  internal sealed class CycleDetectionNotifier
  {
    #region Members
    IList<ICycleDetectionListener> m_listeners = new List<ICycleDetectionListener> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (CycleDetectionNotifier).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CycleDetectionNotifier ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a listener
    /// </summary>
    /// <param name="listener"></param>
    public static void AddListener (ICycleDetectionListener listener)
    {
      lock (Instance.m_listeners) {
        Instance.m_listeners.Add (listener);
      }
    }

    /// <summary>
    /// Notify a cycle detection
    /// </summary>
    /// <param name="dateTime"></param>
    public static void NotifyCycleDetection (IMachine machine, DateTime dateTime)
    {
      lock (Instance.m_listeners) {
        foreach (var listener in Instance.m_listeners.Where (l => l.Machine.Id == machine.Id)) {
          listener.NotifyCycleDetection (machine, dateTime);
        }
      }
    }
    #endregion // Methods

    #region Instance
    static CycleDetectionNotifier Instance
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

      internal static readonly CycleDetectionNotifier instance = new CycleDetectionNotifier ();
    }
    #endregion // Instance
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Core.Log;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Plugin.AnalysisDetectionStatus
{
  /// <summary>
  /// Description of Notifier.
  /// </summary>
  internal sealed class OperationDetectionNotifier
  {
    #region Members
    IList<IOperationDetectionListener> m_listeners = new List<IOperationDetectionListener> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (OperationDetectionNotifier).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public OperationDetectionNotifier ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a listener
    /// </summary>
    /// <param name="listener"></param>
    public static void AddListener (IOperationDetectionListener listener)
    {
      lock (Instance.m_listeners) {
        Instance.m_listeners.Add (listener);
      }
    }

    /// <summary>
    /// Notify a cycle detection
    /// </summary>
    /// <param name="dateTime"></param>
    public static void NotifyOperationDetection (IMachine machine, DateTime dateTime)
    {
      Debug.Assert (null != machine);

      lock (Instance.m_listeners) {
        foreach (var listener in Instance.m_listeners.Where (l => l.Machine.Id == machine.Id)) {
          listener.NotifyOperationDetection (machine, dateTime);
        }
      }
    }
    #endregion // Methods

    #region Instance
    static OperationDetectionNotifier Instance
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

      internal static readonly OperationDetectionNotifier instance = new OperationDetectionNotifier ();
    }
    #endregion // Instance
  }
}

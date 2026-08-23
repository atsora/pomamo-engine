// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.ProductionSwitcher
{
  /// <summary>
  /// Notify the registered listeners of any observation state slot change.
  ///
  /// The listeners are referenced weakly: a new OperationCycleDetectionExtension is built
  /// and registered here each time the activity analysis of a machine is created again
  /// (plugin reload, restart of the analysis threads, ...). A strong reference would keep
  /// all those obsolete instances alive for the whole lifetime of the process.
  /// A listener is therefore kept only as long as the analysis that owns it.
  /// </summary>
  internal sealed class ObservationStateSlotChangeNotifier
  {
    readonly List<WeakReference<IObservationStateSlotChangeListener>> m_listeners
      = new List<WeakReference<IObservationStateSlotChangeListener>> ();

    static readonly ILog log = LogManager.GetLogger (typeof (ObservationStateSlotChangeNotifier).FullName);

    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private ObservationStateSlotChangeNotifier ()
    {
    }

    /// <summary>
    /// Number of registered listeners, including the ones that were garbage collected
    /// but not purged yet. For the unit tests and the diagnostics.
    /// </summary>
    internal static int ListenerCount
    {
      get {
        lock (Instance.m_listeners) {
          return Instance.m_listeners.Count;
        }
      }
    }

    /// <summary>
    /// Add a listener
    ///
    /// The listener is not kept alive by this notifier: it is the responsibility
    /// of the caller to keep a reference on it as long as it must be notified
    /// </summary>
    /// <param name="listener">not null</param>
    public static void AddListener (IObservationStateSlotChangeListener listener)
    {
      Debug.Assert (null != listener);

      lock (Instance.m_listeners) {
        Instance.PurgeCollectedListeners ();
        Instance.m_listeners.Add (new WeakReference<IObservationStateSlotChangeListener> (listener));
      }
    }

    /// <summary>
    /// Notify changes
    /// </summary>
    /// <param name="newSlot">new slot</param>
    public static void NotifyChanges (IObservationStateSlot newSlot)
    {
      lock (Instance.m_listeners) {
        // Iterate backwards so that the collected listeners can be removed on the fly
        for (int i = Instance.m_listeners.Count - 1; 0 <= i; --i) {
          IObservationStateSlotChangeListener listener;
          if (Instance.m_listeners[i].TryGetTarget (out listener)) {
            listener.NotifyObservationStateSlotChange (newSlot);
          }
          else { // The listener was garbage collected
            Instance.m_listeners.RemoveAt (i);
          }
        }
      }
    }

    /// <summary>
    /// Remove the listeners that were garbage collected.
    ///
    /// The caller must own the lock on m_listeners
    /// </summary>
    void PurgeCollectedListeners ()
    {
      for (int i = m_listeners.Count - 1; 0 <= i; --i) {
        IObservationStateSlotChangeListener listener;
        if (!m_listeners[i].TryGetTarget (out listener)) {
          m_listeners.RemoveAt (i);
        }
      }
    }

    #region Instance
    static ObservationStateSlotChangeNotifier Instance
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

      internal static readonly ObservationStateSlotChangeNotifier instance = new ObservationStateSlotChangeNotifier ();
    }
    #endregion // Instance
  }
}

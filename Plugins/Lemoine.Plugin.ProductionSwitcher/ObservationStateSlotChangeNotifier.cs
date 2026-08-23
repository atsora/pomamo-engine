// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.ProductionSwitcher
{
  /// <summary>
  /// Description of ObservationStateSlotChangeNotifier.
  /// </summary>
  internal sealed class ObservationStateSlotChangeNotifier
  {
    #region Members
    IList<IObservationStateSlotChangeListener> m_listeners = new List<IObservationStateSlotChangeListener> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ObservationStateSlotChangeNotifier).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private ObservationStateSlotChangeNotifier()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a listener
    ///
    /// Warning: there is no way to remove a listener, and this list is static.
    /// Every OperationCycleDetectionExtension instance registers itself here in Initialize,
    /// and a new instance is built each time the activity analysis of a machine is re-created
    /// (plugin reload, service restart of the analysis threads, ...).
    /// The list then grows and the obsolete instances are kept alive by it.
    /// </summary>
    /// <param name="listener"></param>
    public static void AddListener (IObservationStateSlotChangeListener listener)
    {
      lock (Instance.m_listeners)
      {
        Instance.m_listeners.Add (listener);
      }
    }
    
    /// <summary>
    /// Notify changes
    /// </summary>
    /// <param name="newSlot">new slot</param>
    public static void NotifyChanges (IObservationStateSlot newSlot)
    {
      lock (Instance.m_listeners)
      {
        foreach (var listener in Instance.m_listeners) {
          listener.NotifyObservationStateSlotChange (newSlot);
        }
      }
    }
    #endregion // Methods
    
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

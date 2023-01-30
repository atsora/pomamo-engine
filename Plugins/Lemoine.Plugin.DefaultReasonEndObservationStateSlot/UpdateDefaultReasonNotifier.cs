// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.DefaultReasonEndObservationStateSlot
{
  /// <summary>
  /// Description of UpdateDefaultReasonNotifier.
  /// </summary>
  internal sealed class UpdateDefaultReasonNotifier
  {
    #region Members
    IList<IUpdateDefaultReasonListener> m_listeners = new List<IUpdateDefaultReasonListener> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (UpdateDefaultReasonNotifier).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public UpdateDefaultReasonNotifier ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a listener
    /// </summary>
    /// <param name="listener"></param>
    public static void AddListener (IUpdateDefaultReasonListener listener)
    {
      lock (Instance.m_listeners)
      {
        Instance.m_listeners.Add (listener);
      }
    }
    
    public static bool NotifyUpdateDefaultReason (IReasonSlot slot)
    {
      lock (Instance.m_listeners)
      {
        foreach (var listener in Instance.m_listeners) {
          if (object.Equals (slot.Machine, listener.Machine)) {
            var result = listener.UpdateDefaultReason (System.Threading.CancellationToken.None, slot);
            if (result) {
              return result;
            }
          }
        }
      }
      
      return false;
    }
    #endregion // Methods

    #region Instance
    static UpdateDefaultReasonNotifier Instance
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

      internal static readonly UpdateDefaultReasonNotifier instance = new UpdateDefaultReasonNotifier ();
    }
    #endregion // Instance
  }
}

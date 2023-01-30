// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of MachineCache.
  /// </summary>
  public sealed class MachineCache
  {
    #region Members
    IList<IMachine> m_machines = null;
    volatile bool m_active = false;
    object m_activeLock = new object ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (MachineCache).FullName);

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private MachineCache ()
    {
      log.Debug ("MachineCache: initialization");
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Is the cache active ?
    /// </summary>
    /// <returns></returns>
    public static bool IsActive ()
    {
      return Instance.m_active;
    }

    /// <summary>
    /// Activate the cache
    /// </summary>
    /// <returns></returns>
    public static void Activate ()
    {
      Instance.m_active = true;
    }

    /// <summary>
    /// Deactivate the cache
    /// </summary>
    /// <returns>active status of the cache before</returns>
    public static bool Deactivate ()
    {
      log.InfoFormat ("Deactivate");

      bool active;
      lock (Instance.m_activeLock) {
        active = Instance.m_active;
        Instance.m_active = false;
        Instance.m_machines = null;
      }
      log.InfoFormat ("Deactivate: " +
                      "MachineCache is now deactivated");
      return active;
    }

    /// <summary>
    /// Add the list of machines to the cache
    /// </summary>
    /// <param name="machines">not null</param>
    public static void Add (IList<IMachine> machines)
    {
      Debug.Assert (null != machines);

      if (!Instance.m_active) {
        log.Info ("Add: not active, do nothing");
        return;
      }

      lock (Instance.m_activeLock) {
        if (Instance.m_active) {
          Instance.m_machines = machines;
        }
      }
    }

    /// <summary>
    /// Try to get a value in cache
    /// </summary>
    /// <param name="machines"></param>
    public static bool TryGetMachines (out IList<IMachine> machines)
    {
      if (!Instance.m_active) {
        log.Info ("TryGetMachines: the cache is not active => return false");
        machines = null;
        return false;
      }

      lock (Instance.m_activeLock) {
        if (Instance.m_active) {
          machines = Instance.m_machines;
          if (null != Instance.m_machines) {
            return true;
          }
        }
      }

      log.Info ("TryGetMachines: the cache is not active or the cache is empty after lock => return false");
      machines = null;
      return false;
    }

    /// <summary>
    /// Clear the cache
    /// </summary>
    public static void Clear ()
    {
      lock (Instance.m_activeLock) {
        Instance.m_machines = null;
      }
    }
    #endregion // Methods

    #region Instance
    static MachineCache Instance
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

      internal static readonly MachineCache instance = new MachineCache ();
    }
    #endregion // Instance
  }

  /// <summary>
  /// Class to suspect the activation of the MachineCache with the using keyword
  /// </summary>
  public class MachineCacheSuspend : IDisposable
  {
    readonly bool m_active;

    /// <summary>
    /// Constructor
    /// </summary>
    public MachineCacheSuspend ()
    {
      m_active = DaySlotCache.Deactivate ();
    }

    /// <summary>
    /// <see cref="IDisposable" />
    /// </summary>
    public void Dispose ()
    {
      if (m_active) {
        MachineCache.Activate ();
      }
    }
  }
}

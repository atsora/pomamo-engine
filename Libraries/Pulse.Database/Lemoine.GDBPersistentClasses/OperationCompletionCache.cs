// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Collections;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Thread safe cache to get the operation completion from a sequence detection
  /// </summary>
  public sealed class OperationCompletionCache
  {
    static readonly int CACHE_SIZE = 8096; // Make it parametrizable
    
    #region Members
    LRUDictionary<int, double?> m_lru = new LRUDictionary<int, double?> (CACHE_SIZE); // Sequence ID => Completion
    volatile bool m_active = false;
    object m_lock  = new object ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (OperationCompletionCache).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private OperationCompletionCache()
    {
      log.Debug ("OperationCompletionCache: " +
                 "initialization");
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
      lock (Instance.m_lock)
      {
        active = Instance.m_active;
        Instance.m_active = false;
        Instance.m_lru.Clear ();
      }

      log.InfoFormat ($"Deactivate: OperationCompletionCache returns {active}");
      return active;
    }

    /// <summary>
    /// Set the maximum size of the cache
    /// </summary>
    /// <param name="size"></param>
    public static void SetSize (int size)
    {
      lock (Instance.m_lock)
      {
        Instance.m_lru.Size = size;
      }
    }
    
    /// <summary>
    /// Add a data in cache
    /// </summary>
    /// <param name="sequence">Not null</param>
    /// <param name="completion"></param>
    public static void Add (ISequence sequence, double? completion)
    {
      if (!IsActive ()) {
        log.DebugFormat ("Add: " +
                         "The cache is not active, return at once");
        return;
      }
      
      Debug.Assert (null != sequence);
      
      if (null == sequence) {
        log.FatalFormat ("Add: " +
                         "sequence is null");
        return;
      }
      
      lock (Instance.m_lock)
      {
        if (Instance.m_active) {
          Instance.m_lru.Add (((Lemoine.Collections.IDataWithId<int>)sequence).Id, completion);
        }
      }
    }
    
    /// <summary>
    /// Try to get a value in cache
    /// </summary>
    /// <param name="sequence"></param>
    /// <param name="completion"></param>
    public static bool TryGetValue (ISequence sequence, out double? completion)
    {
      return TryGetValue (((Lemoine.Collections.IDataWithId<int>)sequence).Id, out completion);
    }

      /// <summary>
    /// Try to get a value in cache
    /// </summary>
    /// <param name="sequenceId"></param>
    /// <param name="completion"></param>
    public static bool TryGetValue (int sequenceId, out double? completion)
    {
      if (!IsActive ()) {
        log.DebugFormat ("TryGetValue: " +
                         "The cache is not active, return false");
        completion = null;
        return false;
      }
      
      lock (Instance.m_lock)
      {
        if (Instance.m_active) {
          bool result = Instance.m_lru.TryGetValue (sequenceId, out completion);
          if (!result) {
            log.InfoFormat ("TryGetValue: " +
                            "sequence {0} was not in cache",
                            sequenceId);
          }
          return result;
        }
        else {
          log.Info ("TryGetValue: " +
                    "the cache is not active " +
                    "=> return false");
          completion = null;
          return false;
        }
      }
    }
    
    /// <summary>
    /// Clear the cache
    /// </summary>
    public static void Clear ()
    {
      lock (Instance.m_lock)
      {
        Instance.m_lru.Clear ();
      }
    }
    #endregion // Methods
    
    #region Instance
    static OperationCompletionCache Instance
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

      internal static readonly OperationCompletionCache instance = new OperationCompletionCache ();
    }
    #endregion // Instance
  }
  
  /// <summary>
  /// Class to suspect the activation of the OperationCompletionCache with the using keyword
  /// </summary>
  public class OperationCompletionCacheSuspend: IDisposable
  {
    readonly bool m_active;
    
    /// <summary>
    /// Constructor
    /// </summary>
    public OperationCompletionCacheSuspend ()
    {
      m_active = OperationCompletionCache.Deactivate ();
    }
    
    /// <summary>
    /// <see cref="IDisposable" />
    /// </summary>
    public void Dispose ()
    {
      if (m_active) {
        OperationCompletionCache.Activate ();
      }
    }
  }
}

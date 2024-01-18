// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETCOREAPP

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Lemoine.Core.Log;
using Lemoine.Core.Performance;
using Microsoft.Extensions.Caching.Memory;

namespace Lemoine.Core.Extensions.Cache
{
  /// <summary>
  /// MemoryCacheClient
  /// 
  /// Implement <see cref="Lemoine.Core.Cache.ICacheClient"/>
  /// with <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache"/>
  /// </summary>
  public sealed class MemoryCacheClient
    : Lemoine.Core.Cache.ICacheClient
  {
    readonly ILog log = LogManager.GetLogger (typeof (MemoryCacheClient).FullName);

    readonly IMemoryCache m_memoryCache;
    readonly ConcurrentDictionary<string, bool> m_keys = new ConcurrentDictionary<string, bool> (); // Approximative set of keys. It may contain a larger set of effective keys
    volatile int m_batchUpdateLevel = 0;
    readonly ConcurrentBag<string> m_batchUpdatePendingRegexes = new ConcurrentBag<string> ();

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="memoryCache"></param>
    public MemoryCacheClient (IMemoryCache memoryCache)
    {
      m_memoryCache = memoryCache;
    }
    #endregion // Constructors

    #region ICacheClient implementation
    /// <summary>
    /// <see cref="ICacheClient"/>
    /// </summary>
    public void StartBatchUpdate ()
    {
      using (new PerfTracker ("Cache.Memory.StartBatchUpdate")) {
        Debug.Assert (0 <= m_batchUpdateLevel);
        Interlocked.Increment (ref m_batchUpdateLevel);
      }
    }

    /// <summary>
    /// <see cref="ICacheClient"/>
    /// </summary>
    public void FinishBatchUpdate ()
    {
      if (m_batchUpdateLevel < 1) {
        log.Fatal ($"FinishBatchUpdate: level is {m_batchUpdateLevel} < 1, {System.Environment.StackTrace}");
        Debug.Assert (false);
        m_batchUpdateLevel = 0;
        return;
      }

      var exceptions = new List<Exception> ();
      using (new PerfTracker ("Cache.Memory.FinishBatchUpdate")) {
        try {
          if (1 < m_batchUpdateLevel) {
            if (log.IsDebugEnabled) {
              log.Debug ($"FinishBatchUpdate: level {m_batchUpdateLevel} is > 1 => return immediately");
            }
            return;
          }

          EmptyBatchUpdatePendingRegexes (out exceptions);
        }
        finally {
          if (m_batchUpdateLevel <= 0) {
            log.Fatal ($"FinishBatchUpdate: batch update level was already {m_batchUpdateLevel} <= 0, do not decrement it");
          }
          else {
            Interlocked.Decrement (ref m_batchUpdateLevel);
          }
        }
      }

      if (exceptions.Any ()) {
        log.Error ($"FinishBatchUpdate: {exceptions.Count ()} exceptions");
        throw new Exception ("Exception in FinishBatchUpdate", exceptions.First ());
      }
    }

    void EmptyBatchUpdatePendingRegexes (out List<Exception> exceptions)
    {
      exceptions = new List<Exception> ();

      try {
        if (!m_keys.Any ()) {
          if (log.IsDebugEnabled) {
            log.Debug ($"EmptyBatchUpdatePendingRegexes: no key, nothing to do");
          }
          return;
        }

        if (null != m_batchUpdatePendingRegexes) {
          foreach (var regex in m_batchUpdatePendingRegexes.Distinct ()) {
            try {
              ForceRemoveByRegex (regex);
            }
            catch (Exception ex) {
              log.Error ($"EmptyBatchUpdatePendingRegexes: removeByRegex failed for regex {regex}", ex);
              exceptions.Add (ex);
            }
          }
        }
      }
      finally {
        m_batchUpdatePendingRegexes.Clear ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Core.Cache.ICacheClient"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Add<T> (string key, T value)
    {
      using (new PerfTracker ("Cache.Memory.Add")) {
        if (!m_memoryCache.TryGetValue (key, out T? existing)) {
          return Set<T> (key, value);
        }
        else {
          return false;
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Core.Cache.ICacheClient"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiresAt"></param>
    /// <returns></returns>
    public bool Add<T> (string key, T value, DateTime expiresAt)
    {
      using (new PerfTracker ("Cache.Memory.AddExpiresAt")) {
        if (!m_memoryCache.TryGetValue (key, out T? existing)) {
          return Set<T> (key, value, expiresAt);
        }
        else {
          return false;
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Core.Cache.ICacheClient"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiresIn"></param>
    /// <returns></returns>
    public bool Add<T> (string key, T value, TimeSpan expiresIn)
    {
      using (new PerfTracker ("Cache.Memory.AddExpiresIn")) {
        if (!m_memoryCache.TryGetValue (key, out T? existing)) {
          return Set<T> (key, value, expiresIn);
        }
        else {
          return false;
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Core.Cache.ICacheClient"/>
    /// 
    /// Not implemented
    /// </summary>
    /// <param name="key"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public long Decrement (string key, uint amount)
    {
      throw new NotImplementedException ("Decrement not implemented");
    }

    /// <summary>
    /// <see cref="Lemoine.Core.Cache.ICacheClient"/>
    /// </summary>
    public void Dispose ()
    {
      using (new PerfTracker ("Cache.Memory.Dispose")) {
        m_keys.Clear ();
        m_memoryCache.Dispose ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Core.Cache.ICacheClient"/>
    /// </summary>
    public void FlushAll ()
    {
      using (new PerfTracker ("Cache.Memory.FlushAll")) {
        foreach (var keyValuePair in m_keys) {
          this.Remove (keyValuePair.Key);
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Core.Cache.ICacheClient"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T> (string key)
    {
      using (new PerfTracker ("Cache.Memory.Get")) {
        var result = m_memoryCache.Get<T> (key);
        if (result is null) {
          log.Fatal ($"Get: result is null");
          throw new NullReferenceException ();
        }
        else {
          return result;
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Core.Cache.ICacheClient"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="keys"></param>
    /// <returns></returns>
    public IDictionary<string, T> GetAll<T> (IEnumerable<string> keys)
    {
      using (new PerfTracker ("Cache.Memory.GetAll")) {
        IDictionary<string, T> result = new Dictionary<string, T> ();
        foreach (var key in keys) {
          T v = Get<T> (key);
          result.Add (key, v);
        }
        return result;
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Core.Cache.ICacheClient"/>
    /// 
    /// Not implemented
    /// </summary>
    /// <param name="key"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public long Increment (string key, uint amount)
    {
      throw new NotImplementedException ("Increment not implemented");
    }

    /// <summary>
    /// <see cref="Lemoine.Core.Cache.ICacheClient"/>
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Remove (string key)
    {
      using (new PerfTracker ("Cache.Memory.Remove")) {
        var existingKey = m_keys.TryUpdate (key, false, true); // Flag it as remove
        m_memoryCache.Remove (key);
        bool keyActive;
        m_keys.TryRemove (key, out keyActive);
        if (keyActive) { // Restore it: it was restored in the meantime
          AddKey (key);
        }
        return true;
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Core.Cache.ICacheClient"/>
    /// </summary>
    /// <param name="keys"></param>
    public void RemoveAll (IEnumerable<string> keys)
    {
      using (new PerfTracker ("Cache.Memory.RemoveAll")) {
        foreach (var key in keys) {
          Remove (key);
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Core.Cache.ICacheClient"/>
    /// </summary>
    /// <param name="pattern"></param>
    public void RemoveByRegex (string pattern)
    {
      using (new PerfTracker ("Cache.Memory.RemoveByRegex")) {
        if (!m_keys.Any ()) {
          return;
        }

        if (0 < m_batchUpdateLevel) {
          m_batchUpdatePendingRegexes.Add (pattern);
          return;
        }

        ForceRemoveByRegex (pattern);
      }
    }

    void ForceRemoveByRegex (string pattern)
    {
      if (1 < m_batchUpdateLevel) {
        log.Debug ($"ForceRemoveByRegex: level {m_batchUpdateLevel} > 1, but probably upgraded by another thread, continue");
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"ForceRemoveByRegex: pattern={pattern} level={m_batchUpdateLevel}"); // 1: from batch, 0: directly
      }

      var regex = new Regex (pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

      var keys = new HashSet<string> ();
      foreach (var keyValuePair in m_keys) {
        if (regex.IsMatch (keyValuePair.Key)) {
          this.Remove (keyValuePair.Key);
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Core.Cache.ICacheClient"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Replace<T> (string key, T value)
    {
      using (new PerfTracker ("Cache.Memory.Replace")) {
        if (m_memoryCache.TryGetValue<T> (key, out T? existing)) {
          return Set<T> (key, value);
        }
        else {
          return true;
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Core.Cache.ICacheClient"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiresAt"></param>
    /// <returns></returns>
    public bool Replace<T> (string key, T value, DateTime expiresAt)
    {
      using (new PerfTracker ("Cache.Memory.ReplaceExpiresAt")) {
        if (m_memoryCache.TryGetValue<T> (key, out T? existing)) {
          return Set<T> (key, value, expiresAt);
        }
        else {
          return true;
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Core.Cache.ICacheClient"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiresIn"></param>
    /// <returns></returns>
    public bool Replace<T> (string key, T value, TimeSpan expiresIn)
    {
      using (new PerfTracker ("Cache.Memory.ReplaceExpiresIn")) {
        if (m_memoryCache.TryGetValue<T> (key, out T? existing)) {
          return Set<T> (key, value, expiresIn);
        }
        else {
          return true;
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Core.Cache.ICacheClient"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool Set<T> (string key, T value)
    {
      using (new PerfTracker ("Cache.Memory.Set")) {
        AddKey (key);
        m_memoryCache.Set<T> (key, value);
        return true;
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Core.Cache.ICacheClient"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiresAt"></param>
    /// <returns></returns>
    public bool Set<T> (string key, T value, DateTime expiresAt)
    {
      using (new PerfTracker ("Cache.Memory.SetExpiresAt")) {
        AddKey (key);
        m_memoryCache.Set<T> (key, value, expiresAt);
        return true;
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Core.Cache.ICacheClient"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiresIn"></param>
    /// <returns></returns>
    public bool Set<T> (string key, T value, TimeSpan expiresIn)
    {
      using (new PerfTracker ("Cache.Memory.SetExpiresIn")) {
        AddKey (key);
        m_memoryCache.Set<T> (key, value, expiresIn);
        return true;
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Core.Cache.ICacheClient"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    public void SetAll<T> (IDictionary<string, T> values)
    {
      using (new PerfTracker ("Cache.Memory.SetAll")) {
        foreach (var keyValue in values) {
          Set<T> (keyValue.Key, keyValue.Value);
        }
      }
    }

    void AddKey (string key)
    {
      if (!m_keys.TryAdd (key, true)) {
        m_keys.TryUpdate (key, true, false); // Add it back
      }
    }
    #endregion // ICacheClient implementation
  }
}

#endif // NETCOREAPP

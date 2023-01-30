// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Lemoine.Collections;
using Lemoine.Core.Log;
using Lemoine.Core.Performance;

namespace Lemoine.Core.Cache
{
  /// <summary>
  /// Description of LruCacheClient.
  /// </summary>
  public sealed class LruCacheClient : ICacheClientWithCleanExtension
  {
    struct CacheItem
    {
      /// <summary>
      /// Associated value
      /// </summary>
      public object Value;
      /// <summary>
      /// Expiration date/time associated to the value
      /// 
      /// If null, it does not expire
      /// </summary>
      public DateTime? ExpirationDateTime;
    }

    #region Members
    readonly int m_size;
    LRUDictionary<string, CacheItem> m_lruDictionary;
    volatile int m_batchUpdateLevel = 0;
    readonly ConcurrentBag<string> m_batchUpdatePendingRegexes = new ConcurrentBag<string> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (LruCacheClient).FullName);

    #region Getters / Setters
    /// <summary>
    /// Flush the memory on Dispose
    /// </summary>
    public bool FlushOnDispose { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="size"></param>
    public LruCacheClient (int size)
    {
      m_size = size;
      m_lruDictionary = new LRUDictionary<string, CacheItem> (size);
    }
    #endregion // Constructors

    #region IDisposable implementation
    /// <summary>
    /// Implementation of IDisposable
    /// </summary>
    public void Dispose ()
    {
      if (!FlushOnDispose) {
        return;
      }

      m_lruDictionary = new LRUDictionary<string, CacheItem> (m_size);
    }
    #endregion // IDisposable implementation

    #region ICacheClient implementation
    /// <summary>
    /// <see cref="ICacheClient"/>
    /// </summary>
    public void StartBatchUpdate ()
    {
      Debug.Assert (0 <= m_batchUpdateLevel);
      using (new PerfTracker ("Cache.Lru.StartBatchUpdate")) {
        Interlocked.Increment (ref m_batchUpdateLevel);
      }
    }

    /// <summary>
    /// <see cref="ICacheClient"/>
    /// </summary>
    public void FinishBatchUpdate ()
    {
      if (m_batchUpdateLevel < 1) {
        log.Fatal ($"FinishBatchUpdate: level is {m_batchUpdateLevel} < 1");
        Debug.Assert (false, "Invalid batch update level < 1");
        m_batchUpdateLevel = 0;
        return;
      }

      var exceptions = new List<Exception> ();
      using (new PerfTracker ("Cache.Lru.FinishBatchUpdate")) {
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
            log.Fatal ($"FinishBatchUpdate: batch update was already {m_batchUpdateLevel} <= 0, do not decrement it");
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

#if (NETCOREAPP && !NETCOREAPP1_1 && !NETCOREAPP1_0)
      if (!m_lruDictionary.Keys.Any ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"FinishBatchUpdate: no key in LRU dictionary");
        }
        m_batchUpdatePendingRegexes.Clear ();
        return;
      }
#endif // NETCOREAPP >= 2.0

      // TODO: an alternative algorithm is to use the iterator and clear m_batchUpdatePendingRegexes afterwards
      // See CacheClientWithCleanExtension

      var processedRegexes = new HashSet<string> ();
      while (!m_batchUpdatePendingRegexes.IsEmpty) {
        if (log.IsDebugEnabled) {
          log.Debug ($"EmptyBatchUpdatePendingRegexes: there are pending regexes to process");
        }
        if (m_batchUpdatePendingRegexes.TryTake (out string regex)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"EmptyBatchUpdatePendingRegexes: take regex {regex}");
          }
          if (m_lruDictionary.Keys.Any () && !processedRegexes.Contains (regex)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"EmptyBatchUpdatePendingRegexes: about to remove regex {regex}");
            }
            try {
              ForceRemoveByRegex (regex);
            }
            catch (Exception ex) {
              log.Error ($"EmptyBatchUpdatePendingRegexes: ForceRemoveByRegex failed for regex {regex}", ex);
              exceptions.Add (ex);
            }
            processedRegexes.Add (regex);
          }
        }
      }
    }

    /// <summary>
    /// Removes the specified item from the cache.
    /// </summary>
    /// <param name="key">The identifier for the item to delete.</param>
    /// <returns>
    /// true if the item was successfully removed from the cache; false otherwise.
    /// </returns>
    public bool Remove (string key)
    {
      using (new PerfTracker ("Cache.Lru.Remove")) {
        m_lruDictionary.Remove (key);
      }
      return true;
    }

    /// <summary>
    /// Removes the cache for all the keys provided.
    /// </summary>
    /// <param name="keys">The keys.</param>
    public void RemoveAll (IEnumerable<string> keys)
    {
      using (new PerfTracker ("Cache.Lru.RemoveAll")) {
        foreach (var key in keys) {
          Remove (key);
        }
      }
    }

    /// <summary>
    /// Retrieves the specified item from the cache.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key">The identifier for the item to retrieve.</param>
    /// <returns>
    /// The retrieved item, or <value>null</value> if the key was not found.
    /// </returns>
    public T Get<T> (string key)
    {
      using (new PerfTracker ("Cache.Lru.Get")) {
        if (!m_lruDictionary.TryGetValue (key, out var item)) {
          return default;
        }

        if (item.ExpirationDateTime.HasValue && (item.ExpirationDateTime.Value < DateTime.UtcNow)) {
          m_lruDictionary.Remove (key);
          return default;
        }

        if (item.Value is T t) {
          return t;
        }
        else if (item.Value is null) {
          log.Fatal ($"Get: cached value of {key} is null, type can't be checked => return default(T)");
          // Note: this should not happen any more, the Lemoine.Business layer was changed accordingly
          //       else this may be a problem, the cache is not considered here, because default(T) is returned...
          //       This may a performance problem when nullable types are used
          Remove (key); // anyway the cache is not used, the key is inserted again. Leave Remove(key) here to avoid any regression
          return default;
        }
        else {
          if (log.IsFatalEnabled) {
            var stackTrace = System.Environment.StackTrace;
            if (stackTrace.Contains ("NServiceKit")) {
              // Lemoine.Core.Cache.LruCacheClient (null): Get: cached value ///Data/Translation/Find/?Locale=en&Key=StopAll&format=json=Lemoine.Web.CommonResponseDTO.ErrorDTO is of type Lemoine.Web.CommonResponseDTO.ErrorDTO, expected type is System.String. 
              // => a wrong type may be used => use level Warn
              log.WarnFormat ("Get: " +
                              "NServiceKit cached value {0}={1} is of type {2}, expected type is {3}. StackTrace={4}",
                              key, item.Value, item.Value?.GetType (), typeof (T),
                              stackTrace);
            }
            else {
              log.FatalFormat ("Get: " +
                               "cached value {0}={1} is of type {2}, expected type is {3}. StackTrace={4}",
                               key, item.Value, item.Value?.GetType (), typeof (T),
                               stackTrace);
            }
          }
          Remove (key);
          return default;
        }
      }
    }

    /// <summary>
    /// Increments the value of the specified key by the given amount.
    /// The operation is atomic and happens on the server.
    /// A non existent value at key starts at 0
    /// </summary>
    /// <param name="key">The identifier for the item to increment.</param>
    /// <param name="amount">The amount by which the client wants to increase the item.</param>
    /// <returns>
    /// The new value of the item or -1 if not found.
    /// </returns>
    /// <remarks>The item must be inserted into the cache before it can be changed. The item must be inserted as a <see cref="T:System.String"/>. The operation only works with <see cref="System.UInt32"/> values, so -1 always indicates that the item was not found.</remarks>
    public long Increment (string key, uint amount)
    {
      throw new NotImplementedException ("Increment not implemented");
    }

    /// <summary>
    /// Increments the value of the specified key by the given amount.
    /// The operation is atomic and happens on the server.
    /// A non existent value at key starts at 0
    /// </summary>
    /// <param name="key">The identifier for the item to increment.</param>
    /// <param name="amount">The amount by which the client wants to decrease the item.</param>
    /// <returns>
    /// The new value of the item or -1 if not found.
    /// </returns>
    /// <remarks>The item must be inserted into the cache before it can be changed. The item must be inserted as a <see cref="T:System.String"/>. The operation only works with <see cref="System.UInt32"/> values, so -1 always indicates that the item was not found.</remarks>
    public long Decrement (string key, uint amount)
    {
      throw new NotImplementedException ("Decrement not implemented");
    }

    /// <summary>
    /// Adds a new item into the cache at the specified cache key only if the cache is empty.
    /// </summary>
    /// <param name="key">The key used to reference the item.</param>
    /// <param name="v">The object to be inserted into the cache.</param>
    /// <returns>
    /// true if the item was successfully stored in the cache; false otherwise.
    /// </returns>
    /// <remarks>The item does not expire unless it is removed due memory pressure.</remarks>
    public bool Add<T> (string key, T v)
    {
      using (new PerfTracker ("Cache.Lru.Add")) {
        var x = Get<T> (key);
        if ((null == x) || x.Equals (default (T))) {
          return Set<T> (key, v);
        }
        else {
          return false;
        }
      }
    }

    /// <summary>
    /// Sets an item into the cache at the cache key specified regardless if it already exists or not.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public bool Set<T> (string key, T v)
    {
      using (new PerfTracker ("Cache.Lru.Set")) {
        var item = new CacheItem {
          Value = v
        };
        m_lruDictionary.Add (key, item);
        return true;
      }
    }

    /// <summary>
    /// Replaces the item at the cachekey specified only if an items exists at the location already.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public bool Replace<T> (string key, T v)
    {
      using (new PerfTracker ("Cache.Lru.Replace")) {
        var x = Get<T> (key);
        if ((null != x) || !object.Equals (x, default (T))) {
          return Set<T> (key, v);
        }
        else {
          return true;
        }
      }
    }

    /// <summary>
    /// ICacheClient implementation
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <param name="expiresAt"></param>
    /// <returns></returns>
    public bool Add<T> (string key, T v, DateTime expiresAt)
    {
      using (new PerfTracker ("Cache.Lru.AddExpiresAt")) {
        var x = Get<T> (key);
        if ((null == x) || x.Equals (default (T))) {
          return Set<T> (key, v, expiresAt);
        }
        else {
          return false;
        }
      }
    }

    /// <summary>
    /// ICacheClient implementation
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <param name="expiresAt"></param>
    /// <returns></returns>
    public bool Set<T> (string key, T v, DateTime expiresAt)
    {
      using (new PerfTracker ("Cache.Lru.SetExpiresAt")) {
        CacheItem item;
        item.Value = v;
        item.ExpirationDateTime = expiresAt;
        m_lruDictionary.Add (key, item);
        return true;
      }
    }

    /// <summary>
    /// ICacheClient implementation
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <param name="expiresAt"></param>
    /// <returns></returns>
    public bool Replace<T> (string key, T v, DateTime expiresAt)
    {
      using (new PerfTracker ("Cache.Lru.ReplaceExpiresAt")) {
        var x = Get<T> (key);
        if ((null != x) || x.Equals (default (T))) {
          return Set<T> (key, v, expiresAt);
        }
        else {
          return true;
        }
      }
    }

    /// <summary>
    /// ICacheClient implementation
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <param name="expiresIn"></param>
    /// <returns></returns>
    public bool Add<T> (string key, T v, TimeSpan expiresIn)
    {
      using (new PerfTracker ("Cache.Lru.AddExpiresIn")) {
        DateTime expirationDateTime = DateTime.UtcNow.Add (expiresIn);
        return Add<T> (key, v, expirationDateTime);
      }
    }

    /// <summary>
    /// ICacheClient implementation
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <param name="expiresIn"></param>
    /// <returns></returns>
    public bool Set<T> (string key, T v, TimeSpan expiresIn)
    {
      using (new PerfTracker ("Cache.Lru.SetExpiresIn")) {
        DateTime expirationDateTime = DateTime.UtcNow.Add (expiresIn);
        return Set<T> (key, v, expirationDateTime);
      }
    }

    /// <summary>
    /// ICacheClient implementation
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <param name="expiresIn"></param>
    /// <returns></returns>
    public bool Replace<T> (string key, T v, TimeSpan expiresIn)
    {
      using (new PerfTracker ("Cache.Lru.ReplaceExpiresIn")) {
        DateTime expirationDateTime = DateTime.UtcNow.Add (expiresIn);
        return Replace<T> (key, v, expirationDateTime);
      }
    }

    /// <summary>
    /// Invalidates all data on the cache.
    /// </summary>
    public void FlushAll ()
    {
      using (new PerfTracker ("Cache.Lru.FlushAll")) {
        m_lruDictionary.Clear ();
      }
    }

    /// <summary>
    /// Retrieves multiple items from the cache.
    /// The default value of T is set for all keys that do not exist.
    /// </summary>
    /// <param name="keys">The list of identifiers for the items to retrieve.</param>
    /// <returns>
    /// a Dictionary holding all items indexed by their key.
    /// </returns>
    public IDictionary<string, T> GetAll<T> (IEnumerable<string> keys)
    {
      using (new PerfTracker ("Cache.Lru.GetAll")) {
        IDictionary<string, T> result = new Dictionary<string, T> ();
        foreach (var key in keys) {
          T v = Get<T> (key);
          result.Add (key, v);
        }
        return result;
      }
    }

    /// <summary>
    /// Sets multiple items to the cache.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values">The values.</param>
    public void SetAll<T> (IDictionary<string, T> values)
    {
      using (new PerfTracker ("Cache.Lru.SetAll")) {
        foreach (var keyValue in values) {
          Set<T> (keyValue.Key, keyValue.Value);
        }
      }
    }

    /// <summary>
    /// Remove by regex
    /// </summary>
    /// <param name="pattern"></param>
    public void RemoveByRegex (string pattern)
    {
      using (new PerfTracker ("Cache.Lru.RemoveByRegex")) {
        var dictionaryKeys = m_lruDictionary.Keys;
        if (0 == dictionaryKeys.Count) {
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
        log.Debug ($"ForceRemoveByRegex: level {m_batchUpdateLevel} > 1 but probably upgraded by another thread, continue");
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"ForceRemoveByRegex: pattern={pattern} level={m_batchUpdateLevel}"); // 1: from batch, 0: directly
      }

      var regex = new Regex (pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

      var keys = new HashSet<string> ();
      foreach (var key in m_lruDictionary.Keys) {
        if (regex.IsMatch (key)) {
          keys.Add (key);
        }
      }

      this.RemoveAll (keys);
    }
    #endregion // ICacheClient implementation

    #region ICacheClientWithCleanExtension implementation
    /// <summary>
    /// Clean the cache
    /// 
    /// Note we accept from time to time some keys are not cleaned because of the concurrent accesses
    /// </summary>
    public void CleanCache ()
    {
      // Do nothing here for the moment, because the size of the lru dictionary is limited, this is not absolutely required
    }
    #endregion // ICacheClientWithCleanExtension
  }
}

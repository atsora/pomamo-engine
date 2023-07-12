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
using Lemoine.Core.Log;
using Lemoine.Core.Performance;

namespace Lemoine.Core.Cache
{
  /// <summary>
  /// Description of CacheClientWithCleanExtension.
  /// </summary>
  public sealed class CacheClientWithCleanExtension : ICacheClientWithCleanExtension
  {
    #region Members
    readonly ICacheClient m_cacheClient;

    readonly ConcurrentDictionary<string, DateTime> m_keyExpiration = new ConcurrentDictionary<string, DateTime> ();
    readonly ConcurrentDictionary<DateTime, ConcurrentDictionary<string, byte>> m_expirationKeys = new ConcurrentDictionary<DateTime, ConcurrentDictionary<string, byte>> ();

    volatile int m_batchUpdateLevel = 0;
#if NETCOREAPP && !NETCOREAPP1_1 && !NETCOREAPP1_0
    readonly
#endif // NETCOREAPP
    ConcurrentBag<string> m_batchUpdatePendingRegexes = new ConcurrentBag<string> ();
#if !NETCOREAPP || NETCOREAPP1_1 || NETCOREAPP1_0
    readonly SemaphoreSlim m_semaphore = new SemaphoreSlim (1, 1);
#endif // !NETCOREAPP
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (CacheClientWithCleanExtension).FullName);

    #region Getters / Setters
    /// <summary>
    /// Flush the memory on Dispose
    /// </summary>
    public bool FlushOnDispose { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cacheClientImplementation">not null</param>
    public CacheClientWithCleanExtension (ICacheClient cacheClientImplementation)
    {
      Debug.Assert (null != cacheClientImplementation);

      m_cacheClient = cacheClientImplementation;
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

      m_keyExpiration.Clear ();
      m_expirationKeys.Clear ();
      ClearBatchUpdatePendingRegexes ();
      m_cacheClient.Dispose ();
    }
    #endregion // IDisposable implementation

    #region ICacheClient implementation
    /// <summary>
    /// <see cref="ICacheClient"/>
    /// </summary>
    public void StartBatchUpdate ()
    {
      using (new PerfTracker ("Cache.WithClean.StartBatchUpdate")) {
        Debug.Assert (0 <= m_batchUpdateLevel, $"Invalid batch update level {m_batchUpdateLevel}");
        Interlocked.Increment (ref m_batchUpdateLevel);

        try {
          m_cacheClient.StartBatchUpdate ();
        }
        catch (Exception ex) {
          log.Fatal ($"StartBatchUpdate: exception", ex);
        }
      }
    }

    /// <summary>
    /// <see cref="ICacheClient"/>
    /// </summary>
    public void FinishBatchUpdate ()
    {
      if (m_batchUpdateLevel < 1) {
        log.Fatal ($"FinishBatchUpdate: level is {m_batchUpdateLevel} < 1, {System.Environment.StackTrace}");
        Debug.Assert (false, "Invalid batch update level < 1");
        return;
      }

      var exceptions = new List<Exception> ();
      using (new PerfTracker ("Cache.WithClean.FinishBatchUpdate")) {
        try {
          try {
            m_cacheClient.FinishBatchUpdate ();
          }
          catch (Exception ex) {
            log.Fatal ($"FinishBatchUpdate, exception in sub client", ex);
          }

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

      if (1 < m_batchUpdateLevel) {
        log.Debug ($"EmptyBatchUpdatePendingRegexes: batch update level is {m_batchUpdateLevel} > 1, probably upgraded by another thread, but continue");
      }

      CleanCache (); // To limit the number of keys to inspect

      try {
        if (m_keyExpiration.IsEmpty) { // Then there is nothing to do
          if (log.IsDebugEnabled) {
            log.Debug ($"EmptyBatchUpdatePendingRegexes: no key in keyExpiration, nothing to do");
          }
          return;
        }
        if (m_expirationKeys.IsEmpty) { // Then there is nothing to do
          if (log.IsDebugEnabled) {
            log.Debug ($"EmptyBatchUpdatePendingRegexes: no key in expirationKeys, nothing to do");
          }
          return;
        }

        if (null != m_batchUpdatePendingRegexes) {
          IList<string> regexes;
#if !NETCOREAPP || NETCOREAPP1_1 || NETCOREAPP1_0
          using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (m_semaphore)) {
            if (null != m_batchUpdatePendingRegexes) {
#endif // !NETCOREAPP
              regexes = m_batchUpdatePendingRegexes.Distinct ().ToList ();
#if !NETCOREAPP || NETCOREAPP1_1 || NETCOREAPP1_0
            }
            else {
              regexes = new List<string> ();
            }
          }
#endif // !NETCOREAPP
          foreach (var regex in regexes) {
            try {
              ForceRemoveByRegex (regex);
            }
            catch (Exception ex) {
              log.Error ($"EmptyBatchUpdatePendingRegexes: ForceRemoveByRegex failed for regex {regex}", ex);
              exceptions.Add (ex);
            }
          }
        }
      }
      finally {
        ClearBatchUpdatePendingRegexes ();
      }
    }

    void ClearBatchUpdatePendingRegexes ()
    {
#if (NETCOREAPP && !NETCOREAPP1_1 && !NETCOREAPP1_0)
      m_batchUpdatePendingRegexes.Clear ();
#else // !(NETCOREAPP >= 2.0)
      using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (m_semaphore)) {
        m_batchUpdatePendingRegexes = new ConcurrentBag<string> ();
      }
#endif // NETCOREAPP condition
    }

    void RemoveInternalKey (string key)
    {
      try {
        if (m_keyExpiration.TryRemove (key, out DateTime expiration)) {
          if (m_expirationKeys.TryGetValue (expiration, out ConcurrentDictionary<string, byte> expirationKeys)) {
            expirationKeys.TryRemove (key, out _);
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"RemoveInternalKey: key={key}", ex);
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
      using (new PerfTracker ("Cache.WithClean.Remove")) {
        RemoveInternalKey (key);
        return m_cacheClient.Remove (key);
      }
    }

    /// <summary>
    /// Removes the cache for all the keys provided.
    /// </summary>
    /// <param name="keys">The keys.</param>
    public void RemoveAll (IEnumerable<string> keys)
    {
      using (new PerfTracker ("Cache.WithClean.RemoveAll")) {
        foreach (var key in keys) {
          RemoveInternalKey (key);
        }
        m_cacheClient.RemoveAll (keys);
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
      using (new PerfTracker ("Cache.WithClean.Get")) {
        return m_cacheClient.Get<T> (key);
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
      using (new PerfTracker ("Cache.WithClean.Increment")) {
        return m_cacheClient.Increment (key, amount);
      }
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
      using (new PerfTracker ("Cache.WithClean.Decrement")) {
        return m_cacheClient.Decrement (key, amount);
      }
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
      using (new PerfTracker ("Cache.WithClean.Add")) {
        return m_cacheClient.Add<T> (key, v);
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
      using (new PerfTracker ("Cache.WithClean.Set")) {
        return m_cacheClient.Set<T> (key, v);
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
      using (new PerfTracker ("Cache.WithClean.Replace")) {
        return m_cacheClient.Replace<T> (key, v);
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
      using (new PerfTracker ("Cache.WithClean.AddExpiresAt")) {
        AddKeyExpiration (key, expiresAt);

        return m_cacheClient.Add<T> (key, v, expiresAt);
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
      using (new PerfTracker ("Cache.WithClean.SetExpiresAt")) {
        AddKeyExpiration (key, expiresAt);

        return m_cacheClient.Set<T> (key, v, expiresAt);
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
      using (new PerfTracker ("Cache.WithClean.ReplaceExpiresAt")) {
        AddKeyExpiration (key, expiresAt);

        return m_cacheClient.Replace<T> (key, v, expiresAt);
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
      using (new PerfTracker ("Cache.WithClean.AddExpiresIn")) {
        AddKeyExpiration (key, DateTime.UtcNow.Add (expiresIn));

        return m_cacheClient.Add<T> (key, v, expiresIn);
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
      using (new PerfTracker ("Cache.WithClean.SetExpiresIn")) {
        AddKeyExpiration (key, DateTime.UtcNow.Add (expiresIn));

        return m_cacheClient.Set<T> (key, v, expiresIn);
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
      using (new PerfTracker ("Cache.WithClean.ReplaceExpiresIn")) {
        AddKeyExpiration (key, DateTime.UtcNow.Add (expiresIn));

        return m_cacheClient.Replace<T> (key, v, expiresIn);
      }
    }

    /// <summary>
    /// Invalidates all data on the cache.
    /// </summary>
    public void FlushAll ()
    {
      using (new PerfTracker ("Cache.WithClean.FlushAll")) {
        ClearBatchUpdatePendingRegexes ();
        m_keyExpiration.Clear ();
        m_expirationKeys.Clear ();
        m_cacheClient.FlushAll ();
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
      using (new PerfTracker ("Cache.WithClean.GetAll")) {
        return m_cacheClient.GetAll<T> (keys);
      }
    }

    /// <summary>
    /// Sets multiple items to the cache.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values">The values.</param>
    public void SetAll<T> (IDictionary<string, T> values)
    {
      using (new PerfTracker ("Cache.WithClean.SetAll")) {
        m_cacheClient.SetAll<T> (values);
      }
    }

    /// <summary>
    /// Remove by regex implementation
    /// </summary>
    /// <param name="pattern"></param>
    public void RemoveByRegex (string pattern)
    {
      using (new PerfTracker ("Cache.WithClean.RemoveByRegex")) {
        CleanCache (); // To limit the number of keys to inspect

        if (m_keyExpiration.IsEmpty) { // Then there is nothing to do
          return;
        }
        if (m_expirationKeys.IsEmpty) { // Then there is nothing to do
          return;
        }

        if (0 < m_batchUpdateLevel) {
#if !NETCOREAPP || NETCOREAPP1_1 || NETCOREAPP1_0
          using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (m_semaphore)) {
#endif // !NETCOREAPP
            m_batchUpdatePendingRegexes.Add (pattern);
#if !NETCOREAPP || NETCOREAPP1_1 || NETCOREAPP1_0
          }
#endif // !NETCOREAPP
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

      // Check now the remaining keys
      var regex = new Regex (pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

      var keys = new HashSet<string> ();
      foreach (var key in m_keyExpiration.Keys) {
        if (regex.IsMatch (key)) {
          keys.Add (key);
        }
      }

      this.RemoveAll (keys);
    }
    #endregion // ICacheClient implementation

    #region Clean extension
    ConcurrentDictionary<string, byte> CreateExpirationKeysItem (string key)
    {
      var item = new ConcurrentDictionary<string, byte> ();
      item[key] = (byte)0;
      return item;
    }

    ConcurrentDictionary<string, byte> UpdateExpirationKeysItem (ConcurrentDictionary<string, byte> item, string key)
    {
      item.GetOrAdd (key, 0);
      return item;
    }

    ConcurrentDictionary<string, byte> RemoveFromExpirationKeysItem (ConcurrentDictionary<string, byte> item, string key)
    {
      item.TryRemove (key, out _);
      return item;
    }

    /// <summary>
    /// Add internally the key/expiration value for the clean process
    /// 
    /// This is thread safe
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expiresAt"></param>
    void AddKeyExpiration (string key, DateTime expiresAt)
    {
      if (DateTimeKind.Utc != expiresAt.Kind) {
        expiresAt = expiresAt.ToUniversalTime ();
      }

      if (m_keyExpiration.TryRemove (key, out DateTime previousExpirationDateTime)) {
        if (m_expirationKeys.TryGetValue (previousExpirationDateTime, out ConcurrentDictionary<string, byte> previousExpirationKeys)) {
          previousExpirationKeys.TryRemove (key, out _);
          m_expirationKeys.TryUpdate (previousExpirationDateTime, previousExpirationKeys, previousExpirationKeys);
        }
      }
      m_keyExpiration[key] = expiresAt;
      m_expirationKeys
        .AddOrUpdate (expiresAt, (dt) => CreateExpirationKeysItem (key), (dt, item) => UpdateExpirationKeysItem (item, key));
    }

    /// <summary>
    /// Clean the cache
    /// 
    /// Note we accept from time to time some keys are not cleaned because of the concurrent accesses
    /// </summary>
    public void CleanCache ()
    {
      using (new PerfTracker ("Cache.WithClean.CleanCache")) {
        if (m_cacheClient is ICacheClientWithCleanExtension cacheClientWithCleanExtension) {
          cacheClientWithCleanExtension.CleanCache ();
        }

        var keys = new HashSet<string> ();
        IList<DateTime> dateTimes = new List<DateTime> ();

        foreach (KeyValuePair<DateTime, ConcurrentDictionary<string, byte>> keyValue in m_expirationKeys) {
          DateTime expiration = keyValue.Key;
          if (DateTime.UtcNow < expiration) {
            continue;
          }
          foreach (string key in keyValue.Value.Keys) {
            if (!m_keyExpiration.TryGetValue (key, out DateTime keyExpiresAt)) {
              log.Warn ($"CleanCache: because of some concurrent case (this should be very rare), key {key} is not in in m_keyExpiration");
              continue;
            }
            if (object.Equals (keyExpiresAt, expiration)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"CleanCache: clean key {key}");
              }
              keys.Add (key);
            }
          }
          dateTimes.Add (expiration);
        }

        foreach (string key in keys) {
          m_keyExpiration.TryRemove (key, out _);
        }
        foreach (DateTime dateTime in dateTimes) {
          m_expirationKeys.TryRemove (dateTime, out _);
        }
        m_cacheClient.RemoveAll (keys);

        foreach (var validKey in m_keyExpiration.Keys) {
          var v = m_cacheClient.Get<object> (validKey);
          if (v is null) {
            if (log.IsDebugEnabled) {
              log.Debug ($"CleanCache: remove key {validKey} that is not in cache any more");
            }
            RemoveInternalKey (validKey);
          }
        }
      }
    }
    #endregion // Clean extension
  }
}

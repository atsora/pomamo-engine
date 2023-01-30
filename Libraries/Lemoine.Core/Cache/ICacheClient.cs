// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Core.Log;

namespace Lemoine.Core.Cache
{
  /// <summary>
  /// Interface for a cache client
  /// </summary>
  public interface ICacheClient: IDisposable
  {
    /// <summary>
    /// Start a batch update
    /// </summary>
    void StartBatchUpdate ();

    /// <summary>
    /// Finish a batch update
    /// </summary>
    void FinishBatchUpdate ();

    /// <summary>
    /// Remove a key from the cache
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    bool Remove(string key);

    /// <summary>
    /// Remove a set of keys
    /// </summary>
    /// <param name="keys"></param>
    void RemoveAll(IEnumerable<string> keys);

    /// <summary>
    /// Get the value of a key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    T Get<T>(string key);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    long Increment(string key, uint amount);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    long Decrement(string key, uint amount);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    bool Add<T>(string key, T value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    bool Set<T>(string key, T value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    bool Replace<T>(string key, T value);

    /// <summary>
    /// Add a key with an expiration date/time
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiresAt"></param>
    /// <returns></returns>
    bool Add<T>(string key, T value, DateTime expiresAt);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiresAt"></param>
    /// <returns></returns>
    bool Set<T>(string key, T value, DateTime expiresAt);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiresAt"></param>
    /// <returns></returns>
    bool Replace<T>(string key, T value, DateTime expiresAt);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiresIn"></param>
    /// <returns></returns>
    bool Add<T>(string key, T value, TimeSpan expiresIn);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiresIn"></param>
    /// <returns></returns>
    bool Set<T>(string key, T value, TimeSpan expiresIn);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="expiresIn"></param>
    /// <returns></returns>
    bool Replace<T>(string key, T value, TimeSpan expiresIn);

    /// <summary>
    /// 
    /// </summary>
    void FlushAll();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    IDictionary<string, T> GetAll<T>(IEnumerable<string> keys);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="values"></param>
    void SetAll<T>(IDictionary<string, T> values);
    
    /// <summary>
    /// Remove all the keys matching the regex pattern
    /// </summary>
    /// <param name="pattern"></param>
    void RemoveByRegex(string pattern);
  }

  /// <summary>
  /// Batch updater to run a batch update with using
  /// </summary>
  public sealed class CacheClientBatchUpdater
    : IDisposable
  {
    readonly ILog log = LogManager.GetLogger<CacheClientBatchUpdater> ();

    readonly ICacheClient m_cacheClient;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cacheClient">not null</param>
    public CacheClientBatchUpdater (ICacheClient cacheClient)
    {
      Debug.Assert (null != cacheClient);

      m_cacheClient = cacheClient;
      try {
        m_cacheClient.StartBatchUpdate ();
      }
      catch (Exception ex) {
        log.Fatal ($"CacheClientBatchUpdater: exception in StartBatchUpdate", ex);
      }
    }

    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    public void Dispose ()
    {
      try {
        m_cacheClient.FinishBatchUpdate ();
      }
      catch (Exception ex) {
        log.Fatal ($"Dispose: exception in FinishBatchUpdate", ex);
      }
    }
  }
}

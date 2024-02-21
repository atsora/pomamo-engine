// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Text.RegularExpressions;
using Lemoine.Core.Log;
using Lemoine.Core.Performance;

namespace Lemoine.Core.Cache
{
  /// <summary>
  /// Cache client that can process the cache value that implements the IExpiresAt interface
  /// 
  /// <see cref="IExpiresAt" />
  /// </summary>
  public sealed class CacheClientWithExpiresAtData : ICacheClientWithCleanExtension
  {
    #region Members
    readonly ICacheClientWithCleanExtension m_cacheClient;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (CacheClientWithExpiresAtData).FullName);

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
    public CacheClientWithExpiresAtData (ICacheClientWithCleanExtension cacheClientImplementation)
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
      m_cacheClient?.Dispose ();
    }
    #endregion // IDisposable implementation

    #region ICacheClient implementation
    /// <summary>
    /// <see cref="ICacheClient"/>
    /// </summary>
    public void StartBatchUpdate ()
    {
      using (new PerfTracker ("Cache.WithExpiresAt.StartBatchUpdate")) {
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
      using (new PerfTracker ("Cache.WithExpiresAt.FinishBatchUpdate")) {
        try {
          m_cacheClient.FinishBatchUpdate ();
        }
        catch (Exception ex) {
          log.Fatal ($"FinishBatchUpdate: exception", ex);
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
      using (new PerfTracker ("Cache.WithExpiresAt.Remove")) {
        return m_cacheClient.Remove (key);
      }
    }

    /// <summary>
    /// Removes the cache for all the keys provided.
    /// </summary>
    /// <param name="keys">The keys.</param>
    public void RemoveAll (IEnumerable<string> keys)
    {
      using (new PerfTracker ("Cache.WithExpiresAt.RemoveAll")) {
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
      using (new PerfTracker ("Cache.WithExpiresAt.Get")) {
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
      using (new PerfTracker ("Cache.WithExpiresAt.Increment")) {
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
      using (new PerfTracker ("Cache.WithExpiresAt.Decrement")) {
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
      using (new PerfTracker ("Cache.WithExpiresAt.Add")) {
        if (v is IExpiresAt) {
          return m_cacheClient.Add (key, v, ((IExpiresAt)v).ExpiresAt);
        }
        else {
          return m_cacheClient.Add<T> (key, v);
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
      using (new PerfTracker ("Cache.WithExpiresAt.Set")) {
        if (v is IExpiresAt) {
          return m_cacheClient.Set (key, v, ((IExpiresAt)v).ExpiresAt);
        }
        else {
          return m_cacheClient.Set<T> (key, v);
        }
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
      using (new PerfTracker ("Cache.WithExpiresAt.Replace")) {
        if (v is IExpiresAt) {
          return m_cacheClient.Replace (key, v, ((IExpiresAt)v).ExpiresAt);
        }
        else {
          return m_cacheClient.Replace<T> (key, v);
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
      using (new PerfTracker ("Cache.WithExpiresAt.AddExpiresAt")) {
        return m_cacheClient.Add<T> (key, v, GetExpiresAt (v, expiresAt));
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
      using (new PerfTracker ("Cache.WithExpiresAt.SetExpiresAt")) {
        return m_cacheClient.Set<T> (key, v, GetExpiresAt (v, expiresAt));
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
      using (new PerfTracker ("Cache.WithExpiresAt.ReplaceExpiresAt")) {
        return m_cacheClient.Replace<T> (key, v, GetExpiresAt (v, expiresAt));
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
      using (new PerfTracker ("Cache.WithExpiresAt.AddExpiresIn")) {
        return m_cacheClient.Add<T> (key, v, GetExpiresAt (v, expiresIn));
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
      using (new PerfTracker ("Cache.WithExpiresAt.SetExpiresIn")) {
        return m_cacheClient.Set<T> (key, v, GetExpiresAt (v, expiresIn));
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
      using (new PerfTracker ("Cache.WithExpiresAt.ReplaceExpiresIn")) {
        return m_cacheClient.Replace<T> (key, v, GetExpiresAt (v, expiresIn));
      }
    }

    /// <summary>
    /// Invalidates all data on the cache.
    /// </summary>
    public void FlushAll ()
    {
      using (new PerfTracker ("Cache.WithExpiresAt.FlushAll")) {
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
      using (new PerfTracker ("Cache.WithExpiresAt.GetAll")) {
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
      using (new PerfTracker ("Cache.WithExpiresAt.SetAll")) {
        m_cacheClient.SetAll<T> (values);
      }
    }

    /// <summary>
    /// Remove by regex implementation
    /// </summary>
    /// <param name="pattern"></param>
    public void RemoveByRegex (string pattern)
    {
      using (new PerfTracker ("Cache.WithExpiresAt.RemoveByRegex")) {
        m_cacheClient.RemoveByRegex (pattern);
      }
    }
    #endregion // ICacheClient implementation

    #region Clean extension
    /// <summary>
    /// Clean the cache
    /// 
    /// Note we accept from time to time some keys are not cleaned because of the concurrent accesses
    /// </summary>
    public void CleanCache ()
    {
      using (new PerfTracker ("Cache.WithExpiresAt.CleanCache")) {
        m_cacheClient.CleanCache ();
      }
    }
    #endregion // Clean extension

    DateTime GetExpiresAt<T> (T v, DateTime expiresAt)
    {
      if (v is IExpiresAt) {
        IExpiresAt d = (IExpiresAt)v;
        return d.ExpiresAt;
      }
      else {
        return expiresAt;
      }
    }

    DateTime GetExpiresAt<T> (T v, TimeSpan expiresIn)
    {
      return GetExpiresAt (v, DateTime.UtcNow.Add (expiresIn));
    }

  }
}

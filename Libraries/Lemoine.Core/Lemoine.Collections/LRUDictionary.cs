// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Lemoine.Core.Log;
using Lemoine.Threading;

namespace Lemoine.Collections
{
  /// <summary>
  /// Least Recently Used Cache
  /// 
  /// This class implements the 'Least Recently Used cache' concept.
  /// 
  /// Thread safe
  /// </summary>
  public class LRUDictionary<TKey, TValue>
  /*: IDictionary<TKey, TValue> // TODO: to complete later */
  {
    // disable once StaticFieldInGenericType
    const int DEFAULT_SIZE = 32;

    #region Members
    readonly LinkedList<KeyValuePair<TKey, TValue>> m_list = new LinkedList<KeyValuePair<TKey, TValue>> ();
    readonly SemaphoreSlim m_listSemaphore = new SemaphoreSlim (1, 1);
    readonly TimeSpan m_semaphoreWaitTimeout = TimeSpan.FromMilliseconds (200);
    readonly ConcurrentDictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> m_dictionary = new ConcurrentDictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> ();
    volatile int m_size = DEFAULT_SIZE;
    volatile bool m_sizeReached = false;
    #endregion // Members

    ILog log = LogManager.GetLogger (typeof (LRUDictionary<TKey, TValue>).FullName);

    #region Getters / Setters
    /// <summary>
    /// Maximum size
    /// </summary>
    public int Size
    {
      get { return m_size; }
      set { m_size = value; }
    }

    /// <summary>
    /// Collection of keys
    /// </summary>
    public ICollection<TKey> Keys
    {
      get {
        return m_dictionary.Keys;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="size"></param>
    public LRUDictionary (int size = DEFAULT_SIZE)
    {
      m_size = size;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Clear the whole cache
    /// </summary>
    public void Clear ()
    {
      m_dictionary.Clear ();
      using (var listSemaphoreHolder = SemaphoreSlimHolder.Create (m_listSemaphore)) {
        m_list.Clear ();
        // In case an item has been added in the meantime in m_dictionary, restore it
        foreach (var item in m_dictionary.Values) {
          m_list.AddFirst (item);
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public bool TryGetValue (TKey key, out TValue v)
    {
      LinkedListNode<KeyValuePair<TKey, TValue>> node;
      var result = m_dictionary.TryGetValue (key, out node);
      if (result) {
        Touch (key);
        v = node.Value.Value;
      }
      else {
        v = default (TValue);
      }
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    public void Add (KeyValuePair<TKey, TValue> item)
    {
      Add (item.Key, item.Value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Add (TKey key, TValue value)
    {
      LinkedListNode<KeyValuePair<TKey, TValue>> node;
      if (m_dictionary.TryGetValue (key, out node)) {
        bool removeSuccess;
        try {
          using (var listSemaphoreHolder = SemaphoreSlimHolder.Create (m_listSemaphore, m_semaphoreWaitTimeout)) {
            try {
              m_list.Remove (node);
              removeSuccess = true;
            }
            catch (InvalidOperationException) {
              removeSuccess = false;
            }
            catch (Exception) {
              throw;
            }
            node.Value = new KeyValuePair<TKey, TValue> (key, value);
            if (removeSuccess) {
              m_list.AddFirst (node);
            }
          }
          if (!removeSuccess) {
            log.Info ("Add: Removing node from the list failed (already removed), so was not added back to the list");
          }
        }
        catch (SemaphoreSlimTimeoutException ex) {
          log.Info ($"Add: did not get the semaphore in {m_semaphoreWaitTimeout}, skip the update of the list", ex);
        }
        catch (Exception ex) {
          log.Error ($"Add: unexpected exception", ex);
          throw;
        }
      }
      else {
        node = new LinkedListNode<KeyValuePair<TKey, TValue>> (new KeyValuePair<TKey, TValue> (key, value));
        bool cancelParallelInsertion = false;
        using (var listSemaphoreHolder = SemaphoreSlimHolder.Create (m_listSemaphore)) {
          m_list.AddFirst (node);
          if (!m_dictionary.TryAdd (key, node)) {
            cancelParallelInsertion = true;
            m_list.Remove (node);
          }
        }
        if (cancelParallelInsertion) {
          if (log.IsInfoEnabled) {
            log.Info ($"Add: key {key} was inserted in the meantime");
          }
        }
      }

      if (m_size < m_dictionary.Count) {
        if (!m_sizeReached) {
          log.Warn ($"Add: size of the LRU reached {m_size}");
          m_sizeReached = true;
        }
        LinkedListNode<KeyValuePair<TKey, TValue>> nodeToRemove;
        try {
          using (var listSemaphoreHolder = SemaphoreSlimHolder.Create (m_listSemaphore, m_semaphoreWaitTimeout)) {
            nodeToRemove = m_list.Last;
          }
        }
        catch (SemaphoreSlimTimeoutException ex) {
          log.Warn ($"Add: timeout {m_semaphoreWaitTimeout} reached when trying to remove an item because the size was reached", ex);
          nodeToRemove = null;
        }
        if (null != nodeToRemove) {
          Remove (nodeToRemove);
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns>a key was effectively removed</returns>
    public bool Remove (TKey key)
    {
      LinkedListNode<KeyValuePair<TKey, TValue>> node;
      var result = m_dictionary.TryRemove (key, out node);
      if (!result) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Remove: {key} not found");
        }
      }
      else if (null == node) {
        log.Fatal ($"Remove: returned node was null for key {key}");
        throw new InvalidOperationException ("Invalid value in Remove");
      }
      else { // result && null != node
        try {
          using (var listSemaphoreHolder = SemaphoreSlimHolder.Create (m_listSemaphore)) {
            m_list.Remove (node);
          }
        }
        catch (InvalidOperationException ex) {
          log.Info ("Remove: the node is not in list, it may have been already removed (concurrent access)", ex);
        }
        catch (Exception ex) {
          log.Error ("Remove: node was not removed from list", ex);
          throw;
        }
      }
      return result;
    }

    bool Remove (LinkedListNode<KeyValuePair<TKey, TValue>> node)
    {
      LinkedListNode<KeyValuePair<TKey, TValue>> oldValue;
      var result = m_dictionary.TryRemove (node.Value.Key, out oldValue); // oldValue should correspond to node most of the time
      try {
        using (var listSemaphoreHolder = SemaphoreSlimHolder.Create (m_listSemaphore)) {
          m_list.Remove (node);
        }
      }
      catch (InvalidOperationException ex) {
        log.Info ("Remove: the node is not in list, it may have been already removed (concurrent access)", ex);
      }
      catch (Exception ex) {
        log.Error ("Remove: node was not removed from list", ex);
        throw;
      }
      return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns>Success</returns>
    public bool Touch (TKey key)
    {
      LinkedListNode<KeyValuePair<TKey, TValue>> node;
      if (m_dictionary.TryGetValue (key, out node)) {
        try {
          using (var listSemaphoreHolder = SemaphoreSlimHolder.Create (m_listSemaphore, m_semaphoreWaitTimeout)) {
            m_list.Remove (node);
            m_list.AddFirst (node);
          }
        }
        catch (InvalidOperationException ex) {
          log.Warn ("Touch: node is not in the list, it may have been already removed (concurrent access), do nothing", ex);
          return false;
        }
        catch (SemaphoreSlimTimeoutException ex) {
          log.Warn ($"Touch: timeout {m_semaphoreWaitTimeout} reached, touch was not effective", ex);
          return false;
        }
        catch (Exception ex) {
          log.Error ("Touch: remove or AddFirst failed (unexpected)", ex);
          throw;
        }
        return true;
      }
      else {
        return false;
      }
    }
    #endregion // Methods
  }
}

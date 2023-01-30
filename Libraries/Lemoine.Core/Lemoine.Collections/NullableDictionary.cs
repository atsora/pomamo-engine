// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.Collections
{
  /// <summary>
  /// Dictionary that accepts null values in keys (contrary to the .NET implementation)
  /// 
  /// Not thread safe
  /// </summary>
  public sealed class NullableDictionary<TKey, TValue>
    : IDictionary<TKey, TValue>
    where TKey: class
  {
    #region Members
    bool m_containsNull = false;
    TValue m_nullValue;
    readonly IDictionary<TKey, TValue> m_dictionary = new Dictionary<TKey, TValue> ();
    #endregion // Members

    ILog log = LogManager.GetLogger(typeof (NullableDictionary<TKey,TValue>).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public NullableDictionary ()
    {
    }
    #endregion // Constructors

    #region IDictionary implementation

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(TKey key)
    {
      if (null == key) {
        return m_containsNull;
      }
      else {
        return m_dictionary.ContainsKey (key);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Add(TKey key, TValue value)
    {
      if (null == key) {
        m_containsNull = true;
        m_nullValue = value;
      }
      else {
        m_dictionary.Add (key, value);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Remove(TKey key)
    {
      if (null == key) {
        if (m_containsNull) {
          m_containsNull = false;
          m_nullValue = default(TValue);
          return true;
        }
        else {
          return false;
        }
      }
      else {
        return m_dictionary.Remove (key);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetValue(TKey key, out TValue value)
    {
      if (null == key) {
        if (m_containsNull) {
          value = m_nullValue;
          return true;
        }
        else {
          value = default(TValue);
          return false;
        }
      }
      else {
        return m_dictionary.TryGetValue (key, out value);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public TValue this[TKey key] {
      get {
        if (null == key) {
          return m_nullValue;
        }
        else {
          return m_dictionary[key];
        }
      }
      set {
        if (null == key) {
          m_containsNull = true;
          m_nullValue = value;
        }
        else {
          m_dictionary[key] = value;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public ICollection<TKey> Keys {
      get {
        if (m_containsNull) {
          IList<TKey> keys = new List<TKey> (m_dictionary.Keys);
          keys.Add (null);
          return keys;
        }
        else {
          return m_dictionary.Keys;
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public ICollection<TValue> Values {
      get {
        if (m_containsNull) {
          IList<TValue> values = new List<TValue> (m_dictionary.Values);
          values.Add (m_nullValue);
          return values;
        }
        else {
          return m_dictionary.Values;
        }
      }
    }

    #endregion

    #region ICollection implementation

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    public void Add(KeyValuePair<TKey, TValue> item)
    {
      if (null == item.Key) {
        m_containsNull = true;
        m_nullValue = item.Value;
      }
      else {
        m_dictionary.Add (item);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Clear()
    {
      m_containsNull = false;
      m_nullValue = default(TValue);
      m_dictionary.Clear ();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
      if (null == item.Key) {
        return m_containsNull;
      }
      else {
        return m_dictionary.Contains (item);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
      return Remove (item.Key);
    }

    /// <summary>
    /// 
    /// </summary>
    public int Count {
      get {
        return m_dictionary.Count + (m_containsNull?1:0);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool IsReadOnly {
      get {
        return false;
      }
    }

    #endregion

    #region IEnumerable implementation

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      foreach (var keyValue in m_dictionary) {
        yield return keyValue;
      }
      if (m_containsNull) {
        yield return new KeyValuePair<TKey, TValue> (null, m_nullValue);
      }
    }

    #endregion

    #region IEnumerable implementation

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion
  }
}

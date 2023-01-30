// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// List of consecutive slots that follow each other like this:
  /// aaaa
  ///     bb
  ///          cccc
  /// 
  /// There may be some holes between two slots
  /// </summary>
  public class ConsecutiveSlotList<T>: IEnumerable<T>
    where T: class, ISlot
  {
    readonly List<T> m_value = new List<T> ();
    T m_previouslyAddedItem = default(T);
    
    // disable once StaticFieldInGenericType
    static readonly ILog log = LogManager.GetLogger("ConsecutiveSlotList");

    /// <summary>
    /// First item
    /// </summary>
    public T First {
      get { return m_value [0]; }
    }
    
    /// <summary>
    /// Last item
    /// </summary>
    public T Last {
      get { return m_value [m_value.Count - 1] ;}
    }
    
    /// <summary>
    /// Number of items
    /// </summary>
    public int Count {
      get { return m_value.Count; }
    }
    
    /// <summary>
    /// Insert an item at the right place
    /// </summary>
    /// <param name="item">not null</param>
    public void Insert (T item)
    {
      Debug.Assert (null != item);
      
      if (item.DateTimeRange.IsStrictlyRightOf (Last.DateTimeRange)) {
        Push (item);
      }
      else { // Insert it...
        for (int i = 0; i < m_value.Count; ++i) {
          T existing = m_value [i];
          if (item.DateTimeRange.IsStrictlyLeftOf (existing.DateTimeRange)) {
            T next = existing;
            T previous;
            if (0 == i) {
              previous = null;
            }
            else {
              previous = m_value [i-1];
              Debug.Assert (previous.DateTimeRange.IsStrictlyLeftOf (item.DateTimeRange));
              if (item.ReferenceDataEquals (previous)
                  && previous.DateTimeRange.IsAdjacentTo (item.DateTimeRange)) { // merge with previous
                previous.Merge (item);
                Debug.Assert (previous.DateTimeRange.IsStrictlyLeftOf (next.DateTimeRange));
                if (previous.ReferenceDataEquals (next)
                    && previous.DateTimeRange.IsAdjacentTo (item.DateTimeRange)) { // merge both
                  previous.Merge (next);
                  m_value.RemoveAt (i);
                  return; // They are all merged together
                }
              }
            }
            if (item.ReferenceDataEquals (next)
                && item.DateTimeRange.IsAdjacentTo (next.DateTimeRange)) { // merge with next only
              item.Merge (next);
              m_value [i] = item;
              return;
            }
            else { // no merge
              m_value.Insert (i, item);
              return;
            }
          }
        }
        log.FatalFormat ("Insert: " +
                         "it should have already been inserted");
        throw new InvalidOperationException ();
      }
    }
    
    /// <summary>
    /// Push an item at the end of the list
    /// 
    /// The previously item must precede exactly the new item.
    /// 
    /// If two items that follow contain the same data, they are merged.
    /// </summary>
    /// <param name="item"></param>
    public void Push (T item)
    {
      System.Diagnostics.Debug.Assert ( (null == m_previouslyAddedItem)
                                       || m_previouslyAddedItem.DateTimeRange.IsStrictlyLeftOf (item.DateTimeRange));
      if ( (null != m_previouslyAddedItem)
          && (item.ReferenceDataEquals (m_previouslyAddedItem))
          && (m_previouslyAddedItem.DateTimeRange.IsStrictlyLeftOf (item.DateTimeRange))
          && (m_previouslyAddedItem.DateTimeRange.IsAdjacentTo (item.DateTimeRange))) {
        // Two identical data that follow, merge them:
        // make previousAddedSlot a bit longer
        m_previouslyAddedItem.Merge (item);
      }
      else { // New data
        m_value.Add (item);
        m_previouslyAddedItem = item;
      }
    }
    
    /// <summary>
    /// Remove the first item
    /// </summary>
    public void RemoveFirst ()
    {
      if (0 == this.Count) {
        log.ErrorFormat ("RemoveFirst: " +
                         "empty list");
        m_previouslyAddedItem = default (T);
      }
      else if (1 == this.Count) {
        m_previouslyAddedItem = default (T);
      }
      
      m_value.RemoveAt (0);
    }
    
    /// <summary>
    /// Clear the list
    /// </summary>
    public void Clear ()
    {
      m_previouslyAddedItem = default (T);
      m_value.Clear ();
    }
    
    /// <summary>
    /// Implementation of IEnumerable
    /// </summary>
    /// <returns></returns>
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return m_value.GetEnumerator ();
    }

    /// <summary>
    /// Implementation of IEnumerable
    /// </summary>
    /// <returns></returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return m_value.GetEnumerator ();
    }
    
  }
}

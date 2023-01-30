// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.Collections
{
  /// <summary>
  /// Basic IQueue implementation that uses a list
  /// </summary>
  public sealed class ListQueue<T>: IExtendedQueue<T>, IEnumerable<T>, IEnumerable
  {
    #region Members
    IList<T> m_list = new List<T> ();
    #endregion // Members

    readonly ILog log = LogManager.GetLogger(typeof (ListQueue<T>).FullName);

    #region Getters / Setters
    /// <summary>
    /// Implements <see cref="IQueue&lt;T&gt;" />
    /// </summary>
    public int Count {
      get { return m_list.Count; }
    }
    #endregion // Getters / Setters

    #region Implementation of IEnumerable<T>
    /// <summary>
    /// <see cref="IEnumerable{T}.GetEnumerator"></see>
    /// </summary>
    /// <returns></returns>
    public IEnumerator<T> GetEnumerator ()
    {
      return m_list.GetEnumerator ();
    }
    
    /// <summary>
    /// <see cref="IEnumerable.GetEnumerator"></see>
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator ()
    {
      return (IEnumerator) GetEnumerator ();
    }
    #endregion // Implementation of IEnumerable<T>
    
    #region IQueue implementation
    /// <summary>
    /// Implements <see cref="IQueue&lt;T&gt;" />
    /// </summary>
    public void Clear()
    {
      m_list.Clear ();
    }
    
    /// <summary>
    /// Implements <see cref="IQueue&lt;T&gt;" />
    /// </summary>
    /// <returns></returns>
    public T Dequeue()
    {
      if (0 < m_list.Count) {
        T item = m_list [0];
        m_list.RemoveAt (0);
        return item;
      }
      else {
        throw new InvalidOperationException ();
      }
    }
    
    /// <summary>
    /// Implements <see cref="IQueue&lt;T&gt;" />
    /// </summary>
    /// <param name="item"></param>
    public void Enqueue(T item)
    {
      m_list.Add (item);
    }
    
    /// <summary>
    /// Implements <see cref="IQueue&lt;T&gt;" />
    /// </summary>
    /// <returns></returns>
    public T Peek()
    {
      if (0 < m_list.Count) {
        T item = m_list [0];
        return item;
      }
      else {
        throw new InvalidOperationException ();
      }
    }

    /// <summary>
    /// <see cref="IQueue{T}"/>
    /// </summary>
    public void Close ()
    {
      // Nothing to do
    }

    /// <summary>
    /// Delete the queue
    /// </summary>
    public void Delete()
    {
      // Nothing to do: automatically deleted
    }
    #endregion // IQueue implementation
    
    #region Implementation of IExtendedQueue
    /// <summary>
    /// Peek up to n elements in the queue in the same time.
    /// 
    /// Given the implementation of the queue, less elements may be returned.
    /// </summary>
    /// <param name="nbElements"></param>
    /// <returns></returns>
    public IList<T> Peek (int nbElements)
    {
      IList<T> result = new List<T> ();
      for (int i = 0; (i < nbElements) && (i < m_list.Count); i++) {
        result.Add (m_list [i]);
      }
      return result;
    }
    
    /// <summary>
    /// Dequeue without checking for the queue emptyness
    /// Only use if one is sure the queue contains an element
    /// </summary>
    public void UnsafeDequeue()
    {
      m_list.RemoveAt (0);
    }
    
    /// <summary>
    /// Dequeue n elements without checking the queue has at least n elements
    /// Only use if one is sure the queue contains at least n elements
    /// </summary>
    public void UnsafeDequeue(int n)
    {
      for (int i = 0; i < n; i++) {
        m_list.RemoveAt (0);
      }
    }
    
    /// <summary>
    /// Vacuum queue if needed(useful for DB implementation);
    /// returns true if a vacuum did occur
    /// </summary>
    public bool VacuumIfNeeded()
    {
      // Nothing special to do
      return true;
    }
    #endregion // Implementation of IExtendedQueue
    
    #region Implementation of IDisposable
    /// <summary>
    /// Dispose implementation
    /// </summary>
    public void Dispose ()
    {
      m_list = null;
    }
    #endregion // Implementation of IDisposable
  }
}

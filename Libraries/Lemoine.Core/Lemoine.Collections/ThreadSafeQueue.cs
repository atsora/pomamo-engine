// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.Collections
{
  /// <summary>
  /// Thread safe queue
  /// 
  /// If applicable, please use instead ConcurrentExtendedQueue
  /// </summary>
  internal sealed class ThreadSafeQueue<T> : IExtendedQueue<T>
  {
    #region Members
    readonly Object m_lock = new Object ();
    readonly IExtendedQueue<T> m_queue;
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (ThreadSafeQueue<T>).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="queue"></param>
    public ThreadSafeQueue (IExtendedQueue<T> queue)
    {
      m_queue = queue;
    }
    #endregion // Constructors

    #region Implementation of IQueue
    /// <summary>
    /// Gets the number of elements contained in the Queue.
    /// </summary>
    public int Count
    {
      get
      {
        lock (m_lock) {
          return m_queue.Count;
        }
      }
    }

    /// <summary>
    /// Removes all objects from the queue
    /// </summary>
    public void Clear ()
    {
      lock (m_lock) {
        m_queue.Clear ();
      }
    }

    /// <summary>
    /// Removes and returns the object at the beginning of the Queue.
    /// 
    /// An exception is returned in case the offset is positive.
    /// </summary>
    /// <returns>The object that is removed from the beginning of the Queue.</returns>
    public T Dequeue ()
    {
      lock (m_lock) {
        return m_queue.Dequeue ();
      }
    }

    /// <summary>
    /// Adds an object to the end of the Queue.
    /// 
    /// An exception is returned in case the offset is positive.
    /// </summary>
    /// <param name="item">The object to add to the Queue. The value can be a null reference for reference types.</param>
    public void Enqueue (T item)
    {
      lock (m_lock) {
        m_queue.Enqueue (item);
      }
    }

    /// <summary>
    /// Returns the object at the beginning of the Queue without removing it.
    /// 
    /// InvalidOperationException is returned in case there is no item in the queue
    /// 
    /// An exception is returned in case the offset is positive.
    /// </summary>
    /// <returns>The object at the beginning of the Queue. </returns>
    public T Peek ()
    {
      lock (m_lock) {
        return m_queue.Peek ();
      }
    }

    /// <summary>
    /// <see cref="IQueue{T}"/>
    /// </summary>
    public void Close ()
    {
      m_queue.Close ();
    }

    /// <summary>
    /// Delete the queue
    /// </summary>
    public void Delete ()
    {
      m_queue.Delete ();
    }
    #endregion // Implementation of IQueue

    #region Implementation of IExtendedQueue
    /// <summary>
    /// Peek up to n elements in the queue in the same time.
    /// 
    /// Given the implementation of the queue, less elements may be returned.
    /// 
    /// An exception is returned in case the offset is positive.
    /// </summary>
    /// <param name="nbElements"></param>
    /// <returns></returns>
    public IList<T> Peek (int nbElements)
    {
      lock (m_lock) {
        return m_queue.Peek (nbElements);
      }
    }

    /// <summary>
    /// Dequeue without checking for the queue emptyness
    /// Only use if one is sure the queue contains an element
    /// </summary>
    public void UnsafeDequeue ()
    {
      UnsafeDequeue (1);
    }

    /// <summary>
    /// Dequeue n elements without checking the queue has at least n elements
    /// Only use if one is sure the queue contains at least n elements
    /// </summary>
    public void UnsafeDequeue (int n)
    {
      lock (m_lock) {
        m_queue.UnsafeDequeue (n);
      }
    }

    /// <summary>
    /// Vacuum queue if needed(useful for DB implementation);
    /// returns true if a vacuum did occur
    /// </summary>
    public bool VacuumIfNeeded ()
    {
      lock (m_lock) {
        return m_queue.VacuumIfNeeded ();
      }
    }
    #endregion // Implementation of IExtendedQueue

    #region Implementation of IDisposable
    /// <summary>
    /// Dispose implementation
    /// </summary>
    public void Dispose ()
    {
      lock (m_lock) {
        m_queue.Dispose ();
      }
    }
    #endregion // Implementation of IDisposable
  }
}

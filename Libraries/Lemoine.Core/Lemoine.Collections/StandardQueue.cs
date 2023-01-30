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
  /// Standard queue
  /// </summary>
  public sealed class StandardQueue<T>: IQueue<T>, IEnumerable<T>, IEnumerable
  {
    #region Members
    readonly Queue<T> m_implementation = new Queue<T> ();
    #endregion // Members

    readonly ILog log = LogManager.GetLogger(typeof (StandardQueue<T>).FullName);

    #region Getters / Setters
    /// <summary>
    /// Gets the number of elements contained in the Queue. 
    /// </summary>
    public int Count {
      get { return m_implementation.Count; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public StandardQueue ()
    {
    }
    #endregion // Constructors

    #region Implementation of IEnumerable<T>
    /// <summary>
    /// <see cref="IEnumerable{T}.GetEnumerator"></see>
    /// </summary>
    /// <returns></returns>
    public IEnumerator<T> GetEnumerator ()
    {
      return m_implementation.GetEnumerator ();
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
    /// Removes all objects from the queue
    /// </summary>
    public void Clear ()
    {
      m_implementation.Clear ();
    }
    
    /// <summary>
    /// Removes and returns the object at the beginning of the Queue. 
    /// </summary>
    /// <returns>The object that is removed from the beginning of the Queue.</returns>
    public T Dequeue ()
    {
      return m_implementation.Dequeue ();
    }
    
    /// <summary>
    /// Adds an object to the end of the Queue. 
    /// </summary>
    /// <param name="item">The object to add to the Queue. The value can be a null reference for reference types.</param>
    public void Enqueue (T item)
    {
      m_implementation.Enqueue (item);
    }
    
    /// <summary>
    /// Returns the object at the beginning of the Queue without removing it.
    /// 
    /// InvalidOperationException is returned in case there is no item in the queue
    /// </summary>
    /// <returns>The object at the beginning of the Queue. </returns>
    public T Peek ()
    {
      return m_implementation.Peek ();
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
  }
}

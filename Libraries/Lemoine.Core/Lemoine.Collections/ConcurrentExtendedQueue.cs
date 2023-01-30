// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETSTANDARD

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Collections
{
#if NETSTANDARD2_1
  /// <summary>
  /// ConcurrentExtendedQueue
  /// </summary>
  public sealed class ConcurrentExtendedQueue<T>
    : ConcurrentQueue<T>
    , IQueue<T>
    , IExtendedQueue<T>
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConcurrentExtendedQueue<T>).FullName);

  #region Getters / Setters
  #endregion // Getters / Setters

  #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ConcurrentExtendedQueue ()
    {
    }
  #endregion // Constructors

  #region IQueue implementation
    /// <summary>
    /// <see cref="IQueue{T}"/>
    /// </summary>
    public void Delete ()
    {
    }

    /// <summary>
    /// <see cref="IQueue{T}"/>
    /// </summary>
    /// <returns></returns>
    public T Dequeue ()
    {
      T result;
      if (this.TryDequeue (out result)) {
        return result;
      }
      else {
        throw new InvalidOperationException ();
      }
    }

    public void Dispose ()
    {
    }

    /// <summary>
    /// <see cref="IQueue{T}"/>
    /// </summary>
    /// <returns></returns>
    public T Peek ()
    {
      T result;
      if (this.TryPeek (out result)) {
        return result;
      }
      else {
        throw new InvalidOperationException ();
      }
    }
  #endregion // IQueue implementation

  #region // IExtendedQueue implementation
    /// <summary>
    /// <see cref="IExtendedQueue{T}"/>
    /// </summary>
    /// <param name="nbElements"></param>
    /// <returns></returns>
    public IList<T> Peek (int nbElements)
    {
      if (0 == nbElements) {
        return new List<T> ();
      }
      else if (1 == nbElements) {
        T item;
        if (this.TryPeek (out item)) {
          return new List<T> { item };
        }
        else {
          return new List<T> ();
        }
      }
      else { // May be less efficient
        return this.Take (nbElements).ToList ();
      }
    }

    /// <summary>
    /// <see cref="IExtendedQueue{T}"/>
    /// </summary>
    public void UnsafeDequeue ()
    {
      this.Dequeue ();
    }

    /// <summary>
    /// <see cref="IExtendedQueue{T}"/>
    /// </summary>
    /// <param name="n"></param>
    public void UnsafeDequeue (int n)
    {
      for (int i = 0; i < n; ++i) {
        T item;
        if (!this.TryDequeue (out item)) { // Abort here
          return;
        }
      }
    }

    /// <summary>
    /// <see cref="IExtendedQueue{T}"/>
    /// </summary>
    /// <returns></returns>
    public bool VacuumIfNeeded ()
    {
      return true;
    }
  #endregion // IExtendedQueue implementation
  }
#else // NETSTANDARD2_1
  /// <summary>
  /// ConcurrentExtendedQueue
  /// </summary>
  public sealed class ConcurrentExtendedQueue<T>
    : IQueue<T>
    , IExtendedQueue<T>
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConcurrentExtendedQueue<T>));

    ConcurrentQueue<T> m_implementation = new ConcurrentQueue<T> ();

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ConcurrentExtendedQueue ()
    {
    }

    #endregion // Constructors

    #region IQueue implementation
    /// <summary>
    /// <see cref="IQueue{T}"/>
    /// </summary>
    public int Count => m_implementation.Count;

    /// <summary>
    /// <see cref="IQueue{T}"/>
    /// 
    /// Not full thread safe
    /// </summary>
    public void Clear ()
    {
      // Clear exists only in ConcurrentQueue from .NET Core 3 / Net standard 2.1
      m_implementation = new ConcurrentQueue<T> ();
    }

    /// <summary>
    /// <see cref="IQueue{T}"/>
    /// </summary>
    public void Close ()
    { 
    }

    /// <summary>
    /// <see cref="IQueue{T}"/>
    /// </summary>
    public void Delete ()
    {
    }

    /// <summary>
    /// <see cref="IQueue{T}"/>
    /// </summary>
    /// <returns></returns>
    public T Dequeue ()
    {
      T result;
      if (m_implementation.TryDequeue (out result)) {
        return result;
      }
      else {
        throw new InvalidOperationException ();
      }
    }

    /// <summary>
    /// <see cref="IQueue{T}"/>
    /// </summary>
    public void Dispose ()
    {
    }

    /// <summary>
    /// <see cref="IQueue{T}"/>
    /// </summary>
    /// <param name="item"></param>
    public void Enqueue (T item) => m_implementation.Enqueue (item);

    /// <summary>
    /// <see cref="IQueue{T}"/>
    /// </summary>
    /// <returns></returns>
    public T Peek ()
    {
      T result;
      if (m_implementation.TryPeek (out result)) {
        return result;
      }
      else {
        throw new InvalidOperationException ();
      }
    }
    #endregion // IQueue implementation

    #region // IExtendedQueue implementation
    /// <summary>
    /// <see cref="IExtendedQueue{T}"/>
    /// </summary>
    /// <param name="nbElements"></param>
    /// <returns></returns>
    public IList<T> Peek (int nbElements)
    {
      if (0 == nbElements) {
        return new List<T> ();
      }
      else if (1 == nbElements) {
        T item;
        if (m_implementation.TryPeek (out item)) {
          return new List<T> { item };
        }
        else {
          return new List<T> ();
        }
      }
      else { // May be less efficient
        return m_implementation.Take (nbElements).ToList ();
      }
    }

    /// <summary>
    /// <see cref="IExtendedQueue{T}"/>
    /// </summary>
    public void UnsafeDequeue ()
    {
      this.Dequeue ();
    }

    /// <summary>
    /// <see cref="IExtendedQueue{T}"/>
    /// </summary>
    /// <param name="n"></param>
    public void UnsafeDequeue (int n)
    {
      for (int i = 0; i < n; ++i) {
        T item;
        if (!m_implementation.TryDequeue (out item)) { // Abort here
          return;
        }
      }
    }

    /// <summary>
    /// <see cref="IExtendedQueue{T}"/>
    /// </summary>
    /// <returns></returns>
    public bool VacuumIfNeeded ()
    {
      return true;
    }
    #endregion // IExtendedQueue implementation
  }
#endif // NETSTANDARD2_1
}

#endif // NETSTANDARD

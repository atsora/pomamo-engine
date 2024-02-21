// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Core.Log;

namespace Lemoine.Collections
{
  /// <summary>
  /// 
  /// </summary>
  public sealed class MultiEnumeration<T>: IEnumerator<T>, IEnumerable<T>
    where T: class
  {
    #region Members
    // Note: the two lists below keep the same number of elements
    IList<T> m_items = new List<T> ();
    IList<IEnumerable<T>> m_enumerables  = new List<IEnumerable<T>> ();
    
    int m_index = -1; // Index in the list
    IEnumerator<T> m_currentEnumerator = null; // In case of current IEnumerable
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MultiEnumeration<T>).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public MultiEnumeration ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add an item to the multi-enumerator
    /// </summary>
    /// <param name="item"></param>
    public void Add (T item)
    {
      m_items.Add (item);
      m_enumerables.Add (null);
    }
    
    /// <summary>
    /// Add a IEnumerable to the multi-enumerator
    /// </summary>
    /// <param name="enumerable"></param>
    public void Add (IEnumerable<T> enumerable)
    {
      m_items.Add (null);
      m_enumerables.Add (enumerable);
    }
    #endregion // Methods
    
    #region Implementation of IEnumerator
    /// <summary>
    /// Implementation of <see cref="IEnumerator&lt;T&gt;" />
    /// </summary>
    public void Dispose ()
    {
      m_currentEnumerator?.Dispose ();
      GC.SuppressFinalize (this);
    }
    
    /// <summary>
    /// Implementation of <see cref="IEnumerator&lt;T&gt;" />
    /// </summary>
    public T Current {
      get
      {
        Debug.Assert (m_index < m_items.Count);
        Debug.Assert (m_index < m_enumerables.Count);
        Debug.Assert (m_items.Count == m_enumerables.Count);
        
        if (null != m_items [m_index]) { // T item
          Debug.Assert (null == m_currentEnumerator);
          return m_items [m_index];
        }
        else { // IEnumerable
          if (null == m_currentEnumerator) {
            m_currentEnumerator = m_enumerables [m_index].GetEnumerator ();
            m_currentEnumerator.MoveNext ();
          }
          return m_currentEnumerator.Current;
        }
      }
    }
    
    object IEnumerator.Current
    {
      get { return Current; }
    }
    
    /// <summary>
    /// Implementation of <see cref="IEnumerator&lt;T&gt;" />
    /// </summary>
    /// <returns></returns>
    public bool MoveNext ()
    {
      if (null != m_currentEnumerator) { // Reading a sub-enumerator
        if (m_currentEnumerator.MoveNext ()) {
          return true;
        }
        else { // End was reached
          m_currentEnumerator.Dispose ();
          m_currentEnumerator = null;
        }
        // else end of the enumerator
      }
      
      ++m_index;
      if (m_items.Count <= m_index) {
        return false;
      }
      else {
        return true;
      }
    }
    
    /// <summary>
    /// Implementation of <see cref="IEnumerator.Reset" />
    /// </summary>
    public void Reset ()
    {
      if (null != m_currentEnumerator) {
        m_currentEnumerator.Dispose ();
        m_currentEnumerator = null;
      }
      m_index = -1;
    }
    #endregion // Implementation of IEnumerator
    
    #region Implementation of IEnumerable
    /// <summary>
    /// Implementation of <see cref="IEnumerable&lt;T&gt;" />
    /// </summary>
    /// <returns></returns>
    public IEnumerator<T> GetEnumerator ()
    {
      return this;
    }
    
    /// <summary>
    /// Implementation of <see cref="IEnumerable" />
    /// </summary>
    /// <returns></returns>
    IEnumerator IEnumerable.GetEnumerator ()
    {
      return this.GetEnumerator ();
    }
    #endregion // Implementation of IEnumerable
  }
}

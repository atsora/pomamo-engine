// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETSTANDARD

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Lemoine.Core.Log;
using Lemoine.Info.ConfigReader;
using Lemoine.Threading;

namespace Lemoine.Collections
{
  /// <summary>
  /// Queue with an offset at the begin
  /// 
  /// This queue is thread and inter-process safe
  /// (although there may be some security limitations if not compiled for .NET Framework)
  /// 
  /// You can dequeue/peek from this queue only if the offset is 0
  /// </summary>
  internal sealed class QueueWithOffsetAtBegin<T>
    : IExtendedQueue<T>
    , ICheckedCaller
    , IChecked
    , IConfigurable
    , INamedCollection
    , IDisposable
  {
    #region Members
    Func<string, Mutex> m_createMutex;
    volatile Mutex m_mutex = null;
    volatile ISharedValue<int> m_offset = null;
    readonly IExtendedQueue<T> m_queue;
    readonly string m_name;
    IChecked m_checkedCaller = null;
    #endregion // Members

    readonly ILog log = LogManager.GetLogger(typeof (QueueWithOffsetAtBegin<T>).FullName);

    #region Getters / Setters
    /// <summary>
    /// Current offset
    /// </summary>
    public int Offset {
      get
      {
        Initialize ();
        
        using (var holder = new MutexHolder (m_mutex))
        {
          return m_offset.Value;
        }
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="createMutex"></param>
    public QueueWithOffsetAtBegin (IExtendedQueue<T> queue, Func<string, Mutex> createMutex = null)
    {
      m_queue = queue;
      if (m_queue is INamedCollection) {
        m_name = ((INamedCollection)m_queue).Name;
        if (null == m_name) {
          log.Error ("Initialize: null queue name");
          throw new NullReferenceException ("queueName");
        }
      }
      else {
        m_name = new Random ().Next ().ToString ();
        log.Warn ($"QueueWithOffsetAtEnd: the queue is not a INamedCollection => a random name {m_name} was used instead");
      }

      if (null != createMutex) {
        m_createMutex = createMutex;
      }
      else {
        m_createMutex = CreateMutexDefault;
      }

      if (queue is ICheckedCaller) {
        ((ICheckedCaller)queue).SetCheckedCaller (this);
      }
    }

#if NET45
    static Mutex CreateMutexDefault (string queueName)
    {
      var allowEveryoneRule = new System.Security.AccessControl.MutexAccessRule (new System.Security.Principal.SecurityIdentifier (System.Security.Principal.WellKnownSidType.WorldSid, null), System.Security.AccessControl.MutexRights.FullControl, System.Security.AccessControl.AccessControlType.Allow);
      var securitySettings = new System.Security.AccessControl.MutexSecurity ();
      securitySettings.AddAccessRule (allowEveryoneRule);

      bool createdNew;
      return new Mutex (false, @"Global\" + queueName + "Mutex", out createdNew, securitySettings);
    }
#else // NOT NET45
    static Mutex CreateMutexDefault (string queueName)
    {
      bool createdNew;
      return new Mutex (false, @"Global\" + queueName + "Mutex", out createdNew);
    }
#endif // NET45
    #endregion // Constructors

    void Initialize ()
    {
      if ( (null != m_mutex) && (null != m_offset)) {
        return;
      }
      
      if (null == m_mutex) {
        m_mutex = m_createMutex (m_name);
      }
      
      if (null == m_offset) {
        using (var holder = new MutexHolder (m_mutex))
        {
          m_offset = new SharedMemoryValue<int> (m_name + "Offset");
          Debug.Assert (0 == m_offset.Value);
          if (0 != m_offset.Value) {
            log.Error ($"Initialize: offset was {m_offset.Value} for queue {m_name} => reset it");
            m_offset.Value = 0;
          }
        }
      }
    }
    
    /// <summary>
    /// Clear the offset
    /// </summary>
    internal void ClearOffset ()
    {
      Initialize ();
      
      using (var holder = new MutexHolder (m_mutex))
      {
        m_offset.Value = 0;
      }
    }

    /// <summary>
    /// Clear the queue and for the size of the offset
    /// </summary>
    /// <param name="n">New size</param>
    internal void EmptyForceSize (int n)
    {
      Initialize ();
      
      using (var holder = new MutexHolder (m_mutex))
      {
        m_queue.Clear ();
        m_offset.Value = n;
      }
    }
    
    /// <summary>
    /// Add an artificial offset
    /// </summary>
    /// <param name="n"></param>
    internal void AddOffset (int n)
    {
      Initialize ();
      
      using (var holder = new MutexHolder (m_mutex))
      {
        m_offset.Value += n;
      }
    }
    
    #region Implementation of IQueue
    /// <summary>
    /// Gets the number of elements contained in the Queue.
    /// </summary>
    public int Count {
      get
      {
        Initialize ();
        
        using (var holder = new MutexHolder (m_mutex))
        {
          return m_offset.Value + m_queue.Count;
        }
      }
    }
    
    /// <summary>
    /// Removes all objects from the queue
    /// </summary>
    public void Clear ()
    {
      Initialize ();
      
      using (var holder = new MutexHolder (m_mutex))
      {
        m_offset.Value = 0;
        m_queue.Clear ();
      }
    }
    
    /// <summary>
    /// Removes and returns the object at the beginning of the Queue.
    /// 
    /// An exception is returned if there is an existing offset
    /// </summary>
    /// <returns>The object that is removed from the beginning of the Queue.</returns>
    public T Dequeue ()
    {
      Initialize ();
      
      using (var holder = new MutexHolder (m_mutex))
      {
        if (0 < m_offset.Value) {
          throw new InvalidOperationException ("Existing offset");
        }
        
        return m_queue.Dequeue ();
      }
    }
    
    /// <summary>
    /// Adds an object to the end of the Queue.
    /// </summary>
    /// <param name="item">The object to add to the Queue. The value can be a null reference for reference types.</param>
    public void Enqueue (T item)
    {
      Initialize ();
      
      using (var holder = new MutexHolder (m_mutex))
      {
        m_queue.Enqueue (item);
      }
    }
    
    /// <summary>
    /// Returns the object at the beginning of the Queue without removing it.
    /// 
    /// InvalidOperationException is returned in case there is no item in the queue
    /// 
    /// An exception is returned if there is an existing offset
    /// </summary>
    /// <returns>The object at the beginning of the Queue. </returns>
    public T Peek ()
    {
      Initialize ();
      
      using (var holder = new MutexHolder (m_mutex))
      {
        if (0 < m_offset.Value) {
          throw new InvalidOperationException ("Existing offset");
        }

        return m_queue.Peek ();
      }
    }

    /// <summary>
    /// <see cref="IQueue{T}"/>
    /// </summary>
    public void Close ()
    {
      using (var holder = new MutexHolder (m_mutex)) {
        m_queue.Close ();
      }
    }

    /// <summary>
    /// Delete the queue
    /// </summary>
    public void Delete()
    {
      Initialize ();
      
      using (var holder = new MutexHolder (m_mutex))
      {
        m_queue.Delete();
      }
    }
    #endregion // Implementation of IQueue
    
    #region Implementation of IExtendedQueue
    /// <summary>
    /// Peek up to n elements in the queue in the same time.
    /// 
    /// Given the implementation of the queue, less elements may be returned.
    /// 
    /// An exception is returned if there is an existing offset
    /// </summary>
    /// <param name="nbElements"></param>
    /// <returns></returns>
    public IList<T> Peek (int nbElements)
    {
      Initialize ();
      
      using (var holder = new MutexHolder (m_mutex))
      {
        if (0 < m_offset.Value) {
          throw new InvalidOperationException ("Existing offset");
        }

        return m_queue.Peek (nbElements);
      }
    }
    
    /// <summary>
    /// Dequeue without checking for the queue emptyness
    /// Only use if one is sure the queue contains an element
    /// </summary>
    public void UnsafeDequeue()
    {
      UnsafeDequeue (1);
    }
    
    /// <summary>
    /// Dequeue n elements without checking the queue has at least n elements
    /// Only use if one is sure the queue contains at least n elements
    /// </summary>
    public void UnsafeDequeue(int n)
    {
      Initialize ();
      
      using (var holder = new MutexHolder (m_mutex))
      {
        if (m_offset.Value + m_queue.Count < n) {
          log.ErrorFormat ("UnsafeDequeue: " +
                           "trying to dequeue {0} elements, while there are only {1} elements: " +
                           "offset={2} queueSize={3}",
                           n, this.Count, m_offset, m_queue.Count);
          throw new InvalidOperationException ("Queue too small");
        }
        if (n <= m_offset.Value) { // Only the offset part is impacted
          m_offset.Value -= n;
        }
        else { // m_offset < n
          m_queue.UnsafeDequeue (n - m_offset.Value);
          m_offset.Value = 0;
        }
      }
    }
    
    /// <summary>
    /// Vacuum queue if needed(useful for DB implementation);
    /// returns true if a vacuum did occur
    /// </summary>
    public bool VacuumIfNeeded()
    {
      Initialize ();
      
      using (var holder = new MutexHolder (m_mutex))
      {
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
      if ( (null != m_queue) || (null != m_offset)) {
        Initialize (); // To create the mutex to free the queue
        Debug.Assert (null != m_mutex);
        
        using (var holder = new MutexHolder (m_mutex))
        {
          m_offset.Value = 0;
          m_queue.Dispose ();
        }
      }

      m_mutex?.Close ();
    }
    #endregion // Implementation of IDisposable
    
    #region Specific methods
    /// <summary>
    /// Return the internal queue at the end of the queue (without considering the offset)
    /// </summary>
    /// <returns></returns>
    public IExtendedQueue<T> GetInternalQueue ()
    {
      return m_queue;
    }
    #endregion // Specific methods
    
    /// <summary>
    /// Set a caller
    /// </summary>
    /// <param name="caller"></param>
    public void SetCheckedCaller (IChecked caller)
    {
      m_checkedCaller = caller;
    }
    
    /// <summary>
    /// Implements <see cref="IChecked" />
    /// </summary>
    public void SetActive ()
    {
      m_checkedCaller?.SetActive ();
    }

    /// <summary>
    /// Implements <see cref="IChecked" />
    /// </summary>
    public void PauseCheck()
    {
      m_checkedCaller?.PauseCheck ();
    }

    /// <summary>
    /// Implements <see cref="IChecked" />
    /// </summary>
    public void ResumeCheck()
    {
      m_checkedCaller?.ResumeCheck ();
    }

    #region IConfigurable implementation
    /// <summary>
    /// <see cref="IConfigurable"/>
    /// </summary>
    /// <param name="configReader"></param>
    public void SetConfigReader(IGenericConfigReader configReader)
    {
      if (m_queue is IConfigurable) {
        ((IConfigurable)m_queue).SetConfigReader (configReader);
      }
    }
    #endregion // IConfigurable implementation

    #region INamedCollection implementation
    /// <summary>
    /// <see cref="INamedCollection" />
    /// </summary>
    public string Name => m_name;
    #endregion // INamedCollection implementation
  }
}

#endif // NETSTANDARD

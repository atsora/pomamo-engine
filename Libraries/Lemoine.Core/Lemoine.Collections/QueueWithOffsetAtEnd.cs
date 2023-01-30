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
  /// Queue with an offset at the end
  /// 
  /// This queue is thread and inter-process safe
  /// (although there may be some security limitations if not compiled for .NET Framework)
  /// 
  /// You can enqueue to this queue only if the offset is 0.
  /// 
  /// You can dequeue from this queue only if the size of the queue is strictly greater that the offset
  /// </summary>
  internal class QueueWithOffsetAtEnd<T>
    : IExtendedQueue<T>
    , ICheckedCaller
    , IChecked
    , IConfigurable
    , INamedCollection
    , IDisposable
  {
    #region Members
    volatile Mutex m_mutex = null;
    readonly Func<string, Mutex> m_createMutex;
    volatile ISharedValue<int> m_offset = null;
    readonly IExtendedQueue<T> m_queue;
    readonly string m_name;
    IChecked m_checkedCaller = null;
    #endregion // Members

    readonly ILog log = LogManager.GetLogger(typeof (QueueWithOffsetAtEnd<T>).FullName);

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
    public QueueWithOffsetAtEnd (IExtendedQueue<T> queue, Func<string, Mutex> createMutex = null)
    {
      m_queue = queue;
      if (m_queue is INamedCollection namedQueue) {
        m_name = namedQueue.Name;
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
      return new Mutex (false, @"Global\" + queueName + "Mutex", out _);
    }
#endif // NET45
    #endregion // Constructors

    #region Specific methods
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
    /// Add an artificial offset
    /// </summary>
    /// <param name="n"></param>
    public void AddOffset (int n)
    {
      Initialize ();
      
      using (var holder = new MutexHolder (m_mutex))
      {
        m_offset.Value += n;
      }
    }
    
    /// <summary>
    /// Return the internal queue at the end of the queue (without considering the offset)
    /// </summary>
    /// <returns></returns>
    public IExtendedQueue<T> GetInternalQueue ()
    {
      return m_queue;
    }
    #endregion // Specific methods
    
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
    /// An exception is returned all the elements are in the offset part
    /// </summary>
    /// <returns>The object that is removed from the beginning of the Queue.</returns>
    public T Dequeue ()
    {
      Initialize ();
      
      using (var holder = new MutexHolder (m_mutex))
      {
        if (0 == m_queue.Count) {
          throw new InvalidOperationException ("All the elements in the offset part");
        }
        
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
      Initialize ();
      
      using (var holder = new MutexHolder (m_mutex))
      {
        if (0 < m_offset.Value) {
          throw new InvalidOperationException ("Existing offset");
        }
        else {
          m_queue.Enqueue (item);
        }
      }
    }
    
    /// <summary>
    /// Returns the object at the beginning of the Queue without removing it.
    /// 
    /// InvalidOperationException is returned in case there is no item in the queue
    /// 
    /// An exception is returned all the elements are in the offset part
    /// </summary>
    /// <returns>The object at the beginning of the Queue. </returns>
    public T Peek ()
    {
      Initialize ();
      
      using (var holder = new MutexHolder (m_mutex))
      {
        if (0 == m_queue.Count) {
          throw new InvalidOperationException ("All the elements in the offset part");
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
    /// </summary>
    /// <param name="nbElements"></param>
    /// <returns></returns>
    public IList<T> Peek (int nbElements)
    {
      Initialize ();
      
      using (var holder = new MutexHolder (m_mutex))
      {
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
        
        if (n <= m_queue.Count) { // Only the queue part is impacted
          m_queue.UnsafeDequeue (n);
          return;
        }
        else { // m_queueCount < n
          m_offset.Value -= n-m_queue.Count;
          m_queue.Clear ();
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

      if (null != m_mutex) {
        m_mutex.Close ();
      }
    }
    #endregion // Implementation of IDisposable
    
    #region Methods specific to EndOffsetQueue
    /// <summary>
    /// Increment the offset
    /// </summary>
    public void Enqueue ()
    {
      Initialize ();
      
      using (var holder = new MutexHolder (m_mutex))
      {
        ++m_offset.Value;
      }
    }
    
    /// <summary>
    /// Push one data in the offset part
    /// </summary>
    /// <param name="data"></param>
    public void PushInOffset (T data)
    {
      Initialize ();
      
      using (var holder = new MutexHolder (m_mutex))
      {
        if (0 < m_offset.Value) {
          m_queue.Enqueue (data);
          --m_offset.Value;
        }
        else { // 0 == m_offset
          throw new InvalidOperationException ("no offset");
        }
      }
    }
    #endregion // Methods specific to EndOffsetQueue

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
      if (null != m_checkedCaller) {
        m_checkedCaller.SetActive();
      }
    }

    /// <summary>
    /// Implements <see cref="IChecked" />
    /// </summary>
    public void PauseCheck()
    {
      if (null != m_checkedCaller) {
        m_checkedCaller.PauseCheck();
      }
    }

    /// <summary>
    /// Implements <see cref="IChecked" />
    /// </summary>
    public void ResumeCheck()
    {
      if (null != m_checkedCaller) {
        m_checkedCaller.ResumeCheck();
      }
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

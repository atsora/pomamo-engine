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
  /// IExtendedQueue implementation with both a volatile queue for the performance and another queue for the persistence
  /// 
  /// This queue is thread safe and inter-process safe 
  /// (although there may be some security limitations if not compiled for .NET Framework)
  /// </summary>
  public sealed class CachedQueue<T>
    : IExtendedQueue<T>
    , IConfigurable
    , IChecked
    , ICheckedCaller
    , INamedCollection
    , IDisposable
  {
    readonly string MAX_VOLATILE_ELEMENTS_KEY = "MaxVolatileElements";
    readonly int MAX_VOLATILE_ELEMENTS_DEFAULT = 50;
    readonly string VOLATILE_QUEUE_TYPE_KEY = "VolatileQueue";
    readonly string PERSISTENT_QUEUE_TYPE_KEY = "PersistentQueue";

    #region Members
    string m_name;
    Func<string, Mutex> m_createMutex;
    volatile Mutex m_mutex = null;
    string m_mutexName;
    volatile bool m_initialized = false;
    readonly SemaphoreSlim m_initializationSemaphore = new SemaphoreSlim (1, 1);
    QueueWithOffsetAtBegin<T> m_volatileQueue = null;
    QueueWithOffsetAtEnd<T> m_offsetPersistentQueue = null;

    readonly MemoryConfigReader m_defaultConfigReader = new MemoryConfigReader ();
    IGenericConfigReader m_configReader;

    IChecked m_checkedCaller = null;

    Int32 m_fillPersistentQueueRunning = 0;
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (CachedQueue<T>).FullName);

    #region Getters / Setters
    /// <summary>
    /// Maximum number of volatile elements
    /// 
    /// Default is 50
    /// </summary>
    public int MaxVolatileElements
    {
      get { return m_configReader.Get<int> (MAX_VOLATILE_ELEMENTS_KEY); }
      set
      {
        if (value < 0) {
          log.ErrorFormat ("MaxVolatileElements.set: " +
                           "negative value {0}, a positive value is expected",
                           value);
          throw new ArgumentOutOfRangeException ("value", "Not a positive value");
        }
        m_defaultConfigReader.Add (MAX_VOLATILE_ELEMENTS_KEY, value, true);
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public CachedQueue (IExtendedQueue<T> volatileQueue, IExtendedQueue<T> persistentQueue, Func<string, Mutex> createMutex = null)
      : this ()
    {
      if (null != createMutex) {
        m_createMutex = createMutex;
      }
      else {
        m_createMutex = CreateMutexDefault;
      }

      { // volatile
        m_volatileQueue = new QueueWithOffsetAtBegin<T> (volatileQueue);
        m_volatileQueue.SetCheckedCaller (this);
        MultiConfigReader multiVolatileConfigReader = new MultiConfigReader ();
        multiVolatileConfigReader.Add (new PrefixConfigReader ("Volatile.", m_configReader));
        multiVolatileConfigReader.Add (m_configReader);
        m_volatileQueue.SetConfigReader (multiVolatileConfigReader);
      }

      { // persistent
        m_offsetPersistentQueue = new QueueWithOffsetAtEnd<T> (persistentQueue);
        m_offsetPersistentQueue.SetCheckedCaller (this);
        MultiConfigReader multiPersistentConfigReader = new MultiConfigReader ();
        multiPersistentConfigReader.Add (new PrefixConfigReader ("Persistent.", m_configReader));
        multiPersistentConfigReader.Add (m_configReader);
        m_offsetPersistentQueue.SetConfigReader (multiPersistentConfigReader);
      }
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public CachedQueue ()
      : this (null)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public CachedQueue (Func<string, Mutex> createMutex)
    {
      if (null != createMutex) {
        m_createMutex = createMutex;
      }
      else {
        m_createMutex = CreateMutexDefault;
      }

      m_defaultConfigReader.Add (MAX_VOLATILE_ELEMENTS_KEY, MAX_VOLATILE_ELEMENTS_DEFAULT);

      MultiConfigReader multiConfigReader = new MultiConfigReader ();
      multiConfigReader.Add (Lemoine.Info.ConfigSet.ConfigReader);
      multiConfigReader.Add (m_defaultConfigReader);
      m_configReader = multiConfigReader;
    }

#if NET45
    static Mutex CreateMutexDefault (string mutexName)
    {
      var allowEveryoneRule = new System.Security.AccessControl.MutexAccessRule (new System.Security.Principal.SecurityIdentifier (System.Security.Principal.WellKnownSidType.WorldSid, null), System.Security.AccessControl.MutexRights.FullControl, System.Security.AccessControl.AccessControlType.Allow);
      var securitySettings = new System.Security.AccessControl.MutexSecurity ();
      securitySettings.AddAccessRule (allowEveryoneRule);

      bool createdNew;
      return new Mutex (false, @"Global\" + mutexName, out createdNew, securitySettings);
    }
#else // NOT NET45
    static Mutex CreateMutexDefault (string mutexName)
    {
      bool createdNew;
      return new Mutex (false, @"Global\" + mutexName, out createdNew);
    }
#endif // NET45
    #endregion // Constructors

    void Initialize ()
    {
      if (m_initialized) {
        return;
      }

      var nameFromOffsetPersistentQueue = false;
      using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (m_initializationSemaphore)) {
        if (m_initialized) {
          return;
        }

        if (null == m_offsetPersistentQueue) {
          var multiPersistentConfigReader = new MultiConfigReader ();
          multiPersistentConfigReader.Add (new PrefixConfigReader ("Persistent.", m_configReader));
          multiPersistentConfigReader.Add (m_configReader);
          var persistentQueue = CreateQueue (m_configReader.Get<string> (PERSISTENT_QUEUE_TYPE_KEY), multiPersistentConfigReader);
          m_offsetPersistentQueue = new QueueWithOffsetAtEnd<T> (persistentQueue);
          IConfigurable configurableVolatileQueue = m_offsetPersistentQueue as IConfigurable;
          m_offsetPersistentQueue.SetConfigReader (multiPersistentConfigReader);
        }

        if (null == m_volatileQueue) {
          MultiConfigReader multiVolatileConfigReader = new MultiConfigReader ();
          multiVolatileConfigReader.Add (new PrefixConfigReader ("Volatile.", m_configReader));
          multiVolatileConfigReader.Add (m_configReader);
          var volatileQueue = CreateQueue (m_configReader.Get<string> (VOLATILE_QUEUE_TYPE_KEY), multiVolatileConfigReader);

          m_volatileQueue = new QueueWithOffsetAtBegin<T> (volatileQueue);
          m_volatileQueue.SetConfigReader (multiVolatileConfigReader);
        }

        if (null == m_mutex) {
          if (m_offsetPersistentQueue.GetInternalQueue () is INamedCollection) {
            m_name = ((INamedCollection)m_offsetPersistentQueue.GetInternalQueue ()).Name;
            if (null == m_name) {
              log.Error ("Initialize: null queue name");
              throw new NullReferenceException ("queueName");
            }
          }
          else if (m_volatileQueue.GetInternalQueue () is INamedCollection) {
            m_name = ((INamedCollection)m_volatileQueue.GetInternalQueue ()).Name;
            if (null == m_name) {
              log.Error ("Initialize: null queue name");
              throw new NullReferenceException ("queueName");
            }
          }
          else {
            nameFromOffsetPersistentQueue = true;
            m_name = m_offsetPersistentQueue.Name;
          }

          m_mutexName = m_name + "CachedMutex";
          m_mutex = CreateMutexDefault (m_mutexName);
        }
      }
      if (nameFromOffsetPersistentQueue) {
        log.Warn ("Initialize: => use the name from offsetPersistentQueue, which may be a random name");
      }

      using (var holder = new MutexHolder (m_mutex)) {
        if (0 < m_offsetPersistentQueue.Count) {
          if (0 == m_volatileQueue.Count) {
            log.InfoFormat ("Initialize: " +
                            "The volatile queue is empty while the persistent queue size is {0} " +
                            "=> adjust the size of the volatile queue",
                            m_offsetPersistentQueue.Count);
            m_offsetPersistentQueue.ClearOffset ();
            m_volatileQueue.AddOffset (m_offsetPersistentQueue.Count);
          }
          else if (m_volatileQueue.Count == m_offsetPersistentQueue.Count) {
            // Check if the first stored element is the same
            int realPersistentNumber = m_offsetPersistentQueue.Count - m_offsetPersistentQueue.Offset;
            IList<T> commonPersistentElements = m_offsetPersistentQueue
              .Peek (realPersistentNumber);
            IList<T> commonVolatileElements = m_volatileQueue
              .GetInternalQueue ()
              .Peek (realPersistentNumber - m_volatileQueue.Count);
            // Compare the elements
            for (int i = 0; i < commonVolatileElements.Count; ++i) {
              if (!object.Equals (commonVolatileElements[i], commonPersistentElements[i + m_volatileQueue.Count])) {
                // The value differs
                log.ErrorFormat ("Initialize: " +
                                 "clear the volatile queue because the persistent queue and the volatile queue " +
                                 "are not aligned with each other");
                m_offsetPersistentQueue.ClearOffset ();
                m_volatileQueue.EmptyForceSize (m_offsetPersistentQueue.Count);
                break;
              }
            }
            // Else this is ok to keep both the volatile and the persistent queues
          }
          else { // The size differs
            log.ErrorFormat ("Initialize: " +
                             "the size of the volatile and the persistent queues differs {0} vs {1} " +
                             "=> this is hard to keep both, clear the volatile queue",
                             m_volatileQueue.Count, m_offsetPersistentQueue.Count);
            m_offsetPersistentQueue.ClearOffset ();
            m_volatileQueue.EmptyForceSize (m_offsetPersistentQueue.Count);
          }
        }
        else { // 0 == m_offsetPersistentQueue.Count
          if (0 < m_volatileQueue.Count) {
            log.InfoFormat ("Initialize: " +
                            "The persistent queue is empty while the volatile queue size is {0} " +
                            "=> adjust the size of the persistent queue",
                            m_volatileQueue.Count);
            m_volatileQueue.ClearOffset ();
            m_offsetPersistentQueue.AddOffset (m_volatileQueue.Count);
          }
        }
        Debug.Assert (m_volatileQueue.Count == m_offsetPersistentQueue.Count);

        m_offsetPersistentQueue.SetCheckedCaller (this);
        m_volatileQueue.SetCheckedCaller (this);

        m_initialized = true;
      }
    }

    IExtendedQueue<T> CreateQueue (string typeName, IGenericConfigReader configReader)
    {
      IExtendedQueue<T> queue;
      try {
        var typeLoader = new Lemoine.Core.Plugin.TypeLoader ();
        queue = typeLoader.Load<IExtendedQueue<T>> (typeName);

        if (null == queue) {
          string message = $"Not valid queue of type {typeName}";
          log.Error ($"CreateQueue: {message}");
          throw new Exception (message);
        }
      }
      catch (Exception ex) {
        log.Error ($"CreateQueue: Couldn't create type {typeName}", ex);
        throw;
      }

      if (queue is IConfigurable) {
        IConfigurable configurableQueue = queue as IConfigurable;
        configurableQueue.SetConfigReader (configReader);
      }

      return queue;
    }

    #region Implementation of IQueue
    /// <summary>
    /// Gets the number of elements contained in the Queue.
    /// </summary>
    public int Count
    {
      get
      {
        Initialize ();

        using (var holder = new MutexHolder (m_mutex)) {
          Debug.Assert (m_offsetPersistentQueue.Count == m_volatileQueue.Count);
          return m_offsetPersistentQueue.Count;
        }
      }
    }

    /// <summary>
    /// Removes all objects from the queue
    /// </summary>
    public void Clear ()
    {
      Initialize ();

      using (var holder = new MutexHolder (m_mutex)) {
        m_offsetPersistentQueue.Clear ();
        m_volatileQueue.Clear ();
      }
    }

    /// <summary>
    /// Removes and returns the object at the beginning of the Queue.
    /// </summary>
    /// <returns>The object that is removed from the beginning of the Queue.</returns>
    public T Dequeue ()
    {
      Initialize ();

      using (var holder = new MutexHolder (m_mutex)) {
        if (this.Count < 1) { // Empty queue => raise an exception
          log.ErrorFormat ("Dequeue: " +
                           "empty queue => throw InvalidOperationException");
          throw new InvalidOperationException ("Empty queue");
        }

        if (0 < m_volatileQueue.Offset) {
          // In the persistent queue only
          m_volatileQueue.UnsafeDequeue ();
          return m_offsetPersistentQueue.Dequeue ();
        }
        else { // 0 == m_volatileQueue.Offset => in the volatile queue
          m_offsetPersistentQueue.UnsafeDequeue ();
          return m_volatileQueue.Dequeue ();
        }
      }
    }

    /// <summary>
    /// Adds an object to the end of the Queue.
    /// </summary>
    /// <param name="item">The object to add to the Queue. The value can be a null reference for reference types.</param>
    public void Enqueue (T item)
    {
      Initialize ();

      int nbVolatileElements;

      using (var holder = new MutexHolder (m_mutex)) {
#if DEBUG
        var volatileQueueInitialSize = m_volatileQueue.Count;
#endif // DEBUG
        m_volatileQueue.Enqueue (item);
#if DEBUG
        Debug.Assert (volatileQueueInitialSize + 1 == m_volatileQueue.Count);
#endif // DEBUG
        m_offsetPersistentQueue.Enqueue ();
#if DEBUG
        Debug.Assert (volatileQueueInitialSize + 1 == m_volatileQueue.Count); // TODO: Bug: this is raised from time to time, I don't know why yet... the volatile queue size is 2 instead of 1
#endif // DEBUG
        nbVolatileElements = m_offsetPersistentQueue.Offset;
      }

      if (this.MaxVolatileElements < nbVolatileElements) {
        // Fill asynchronously the persistent queue
        if (1 == Interlocked.CompareExchange (ref m_fillPersistentQueueRunning, 1, 0)) {
          log.Debug ("Enqueue: " +
                     "a fill fersistent queue thread is already running");
        }
        else {
          var thread = new System.Threading.Thread (new ThreadStart (this.FillPersistentQueue));
          thread.Start ();
        }
      }
    }

    /// <summary>
    /// Returns the object at the beginning of the Queue without removing it.
    /// 
    /// InvalidOperationException is returned in case there is no item in the queue
    /// </summary>
    /// <returns>The object at the beginning of the Queue. </returns>
    public T Peek ()
    {
      Initialize ();

      using (var holder = new MutexHolder (m_mutex)) {
        if (0 == this.Count) {
          throw new InvalidOperationException ("Empty queue");
        }

        if (0 == m_volatileQueue.Offset) { // In the volatile queue
          return m_volatileQueue.Peek ();
        }
        else {
          return m_offsetPersistentQueue.Peek ();
        }
      }
    }

    /// <summary>
    /// <see cref="IQueue{T}"/>
    /// </summary>
    public void Close ()
    {
      using (var holder = new MutexHolder (m_mutex)) {
        // Close both volatile and persistent queue
        m_volatileQueue.Close ();
        m_offsetPersistentQueue.Close ();
      }
    }

    /// <summary>
    /// Delete the queue
    /// </summary>
    public void Delete ()
    {
      Initialize ();

      using (var holder = new MutexHolder (m_mutex)) {
        // Delete both volatile and persistent queue
        m_volatileQueue.Delete ();
        m_offsetPersistentQueue.Delete ();
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

      using (var holder = new MutexHolder (m_mutex)) {
        if (0 == this.Count) {
          return new List<T> ();
        }
        else if (0 == m_volatileQueue.Offset) { // In the volatile queue
          return m_volatileQueue.Peek (nbElements);
        }
        else {
          return m_offsetPersistentQueue.Peek (nbElements);
        }
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
      Initialize ();

      using (var holder = new MutexHolder (m_mutex)) {
        if (this.Count < n) { // Empty queue => raise an exception
          log.ErrorFormat ("UnsafeDequeue: " +
                           "only {0} elements while trying to dequeue {1} elements " +
                           "=> throw Queue too small",
                           this.Count, n);
          throw new InvalidOperationException ("Queue too small");
        }

        m_offsetPersistentQueue.UnsafeDequeue (n);
        m_volatileQueue.UnsafeDequeue (n);
      }
    }

    /// <summary>
    /// Vacuum queue if needed(useful for DB implementation);
    /// returns true if a vacuum did occur
    /// </summary>
    public bool VacuumIfNeeded ()
    {
      Initialize ();

      bool m_persistentVacuum;
      bool m_volatileVacuum;

      using (var holder = new MutexHolder (m_mutex)) {
        m_persistentVacuum = m_offsetPersistentQueue.VacuumIfNeeded ();
      }
      using (var holder = new MutexHolder (m_mutex)) {
        m_volatileVacuum = m_volatileQueue.VacuumIfNeeded ();
      }

      return m_persistentVacuum || m_volatileVacuum;
    }
    #endregion // Implementation of IExtendedQueue

    #region Implementation of IDisposable
    /// <summary>
    /// Dispose implementation
    /// </summary>
    public void Dispose ()
    {
      if ((null != m_offsetPersistentQueue) || (null != m_volatileQueue)) {
        Initialize (); // To create the mutex to free the queues
        Debug.Assert (null != m_mutex);

        using (var holder = new MutexHolder (m_mutex)) {
          m_offsetPersistentQueue?.Dispose ();
          m_volatileQueue?.Dispose ();
        }
      }

      m_mutex?.Close ();
    }
    #endregion // Implementation of IDisposable

    #region Methods specific to CachedQueue
    /// <summary>
    /// Fill the persistent queue with n additional items
    /// </summary>
    void FillPersistentQueue ()
    {
      try {
        using (var holder = new MutexHolder (m_mutex)) {
          IExtendedQueue<T> internalVolatileQueue =
            m_volatileQueue.GetInternalQueue ();
          int internalVolatileQueueSize = internalVolatileQueue.Count;
          IList<T> data = internalVolatileQueue.Peek (internalVolatileQueueSize); // Retrieve all the element in the volatile queue
          int overlap = internalVolatileQueueSize - m_offsetPersistentQueue.Offset;
          for (int i = overlap; i < internalVolatileQueueSize; i++) {
            m_offsetPersistentQueue.PushInOffset (data[i]);
          }
        }
      }
      finally { // Job was completed, free m_fillPersistentQueueRunning
        Interlocked.CompareExchange (ref m_fillPersistentQueueRunning, 0, 1);
      }
    }
    #endregion // Methods specific to CachedQueue

    #region IConfigurable implementation
    /// <summary>
    /// Set a config reader
    /// </summary>
    /// <param name="configReader"></param>
    public void SetConfigReader (IGenericConfigReader configReader)
    {
      MultiConfigReader multiConfigReader = new MultiConfigReader ();
      multiConfigReader.Add (configReader);
      multiConfigReader.Add (m_defaultConfigReader);
      m_configReader = multiConfigReader;

      if (null != m_volatileQueue) {
        MultiConfigReader multiVolatileConfigReader = new MultiConfigReader ();
        multiVolatileConfigReader.Add (new PrefixConfigReader (VOLATILE_QUEUE_TYPE_KEY + ".", m_configReader));
        multiVolatileConfigReader.Add (m_configReader);
        m_volatileQueue.SetConfigReader (multiVolatileConfigReader);
      }

      if (null != m_offsetPersistentQueue) {
        MultiConfigReader multiPersistentConfigReader = new MultiConfigReader ();
        multiPersistentConfigReader.Add (new PrefixConfigReader (PERSISTENT_QUEUE_TYPE_KEY + ".", m_configReader));
        multiPersistentConfigReader.Add (m_configReader);
        m_offsetPersistentQueue.SetConfigReader (multiPersistentConfigReader);
      }
    }
    #endregion

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
    public void PauseCheck ()
    {
      m_checkedCaller?.PauseCheck ();
    }

    /// <summary>
    /// Implements <see cref="IChecked" />
    /// </summary>
    public void ResumeCheck ()
    {
      m_checkedCaller?.ResumeCheck ();
    }

    #region INamedCollection implementation
    /// <summary>
    /// <see cref="INamedCollection" />
    /// </summary>
    public string Name
    {
      get
      {
        return m_name;
      }
    }
    #endregion // INamedCollection implementation
  }
}

#endif // NETSTANDARD

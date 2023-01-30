// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Cnc.Data;
using Lemoine.Collections;
using Lemoine.Info.ConfigReader;
using Lemoine.Threading;
using Lemoine.Core.Log;

namespace Lemoine.Cnc.SystemQueue
{
  /// <summary>
  /// 
  /// </summary>
  public sealed class SystemCncDataQueue
    : ICncDataQueue
    , IConfigurable
    , IDisposable
    , IChecked
    , ICheckedCaller
    , INamedCollection
  {
    ILog log = LogManager.GetLogger(typeof (SystemCncDataQueue).FullName);
    
    readonly string MACHINE_ID_KEY = "MachineId";
    readonly string MACHINE_MODULE_ID_KEY = "MachineModuleId";
    static readonly string QUEUE_PREFIX_KEY = "Prefix";
    static readonly string QUEUE_PREFIX_DEFAULT = "Single";
    static readonly string QUEUE_SUFFIX_KEY = "Suffix";
    static readonly string QUEUE_SUFFIX_DEFAULT = "";
    static readonly string PERSISTENCE_KEY = "Persistent";
    static readonly bool PERSISTENCE_DEFAULT = false;
    static readonly string RECEIVE_ONLY_KEY = "ReceiveOnly";
    static readonly bool RECEIVE_ONLY_DEFAULT = false;
    
    #region Members
    bool m_initialized = false;
    IGenericConfigReader m_configReader = null;
    readonly MemoryConfigReader m_defaultConfigReader = new MemoryConfigReader ();
    System.Messaging.MessageQueue m_queue;
    System.Messaging.QueueAccessMode m_accessMode;
    string m_name = null;
    string m_queueName = null;
    IChecked m_checkedCaller = null;
    bool m_disposed = false;
    #endregion // Members
    
    #region Getters / setters
    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public int MachineId {
      get { return m_configReader.Get<int> (MACHINE_ID_KEY); }
      set
      {
        if (value < 0) {
          log.ErrorFormat ("MachineId.set: " +
                           "negative value {0}, a positive value is expected",
                           value);
          throw new ArgumentOutOfRangeException("value", "Not a positive value");
        }
        m_defaultConfigReader.Add (MACHINE_ID_KEY, value, true);
      }
    }
    
    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public int MachineModuleId {
      get { return m_configReader.Get<int> (MACHINE_MODULE_ID_KEY); }
      set
      {
        if (value < 0) {
          log.ErrorFormat ("MachineModuleId.set: " +
                           "negative value {0}, a positive value is expected",
                           value);
          throw new ArgumentOutOfRangeException("value", "Not a positive value");
        }
        m_defaultConfigReader.Add (MACHINE_MODULE_ID_KEY, value, true);
      }
    }
    
    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    public int Count {
      get {
        // the commented code won't work on non-US machines (perf. counter. category names are localized etc)
        // there are workarounds but Count does not have to be efficient anyway
        // var queueCounter = new System.Diagnostics.PerformanceCounter("MSMQ Queue", "Messages in Queue", m_queueName);
        // return (int) queueCounter.NextValue();
        int nbMsg = 0;
        System.Messaging.MessageEnumerator msgEnumerator = m_queue.GetMessageEnumerator2();
        while(msgEnumerator.MoveNext()) {
          nbMsg++;
        }
        return nbMsg;
      }
    }
    
    /// <summary>
    /// <see cref="INamedCollection" />
    /// </summary>
    public string Name {
      get
      {
        Initialize ();
        
        return (null == m_name)?null:"SystemQueue" + m_name;
      }
    }
    #endregion // Getters / setters

    /// <summary>
    /// Constructor
    /// </summary>
    public SystemCncDataQueue ()
    {
      m_defaultConfigReader.Add (QUEUE_PREFIX_KEY, QUEUE_PREFIX_DEFAULT);
      m_defaultConfigReader.Add (QUEUE_SUFFIX_KEY, QUEUE_SUFFIX_DEFAULT);
      m_defaultConfigReader.Add (PERSISTENCE_KEY, PERSISTENCE_DEFAULT);
      m_defaultConfigReader.Add (RECEIVE_ONLY_KEY, RECEIVE_ONLY_DEFAULT);
    }
    
    #region IConfigurable implementation
    /// <summary>
    /// <see cref="IConfigurable" />
    /// </summary>
    /// <param name="configReader"></param>
    public void SetConfigReader(IGenericConfigReader configReader)
    {
      MultiConfigReader multiConfigReader = new MultiConfigReader ();
      multiConfigReader.Add (configReader);
      multiConfigReader.Add (m_defaultConfigReader);
      m_configReader = multiConfigReader;
    }
    #endregion
    
    #region Methods
    /// <summary>
    /// Initialize the queue based on configurations
    /// </summary>
    void Initialize()
    {
      if (m_initialized) {
        return;
      }
      
      Debug.Assert (null != m_configReader);
      
      log = LogManager.GetLogger (typeof (SystemCncDataQueue).FullName + "." + MachineId + "." + MachineModuleId);
      
      // Name of the queue
      string prefix = m_configReader.Get<string> (QUEUE_PREFIX_KEY);
      string suffix = m_configReader.Get<string> (QUEUE_SUFFIX_KEY);
      m_name = string.Format ("{0}{1}-{2}",
                                  prefix, suffix, this.MachineId);
      if (this.MachineModuleId != 0) {
        m_name += "-" + this.MachineModuleId;
      }
      m_queueName = ".\\Private$\\" + m_name;
      
      // Initialize a MessageQueue
      if (!System.Messaging.MessageQueue.Exists(m_queueName)) {
        System.Messaging.MessageQueue.Create(m_queueName);
      }
      
      if (m_configReader.Get<bool> (RECEIVE_ONLY_KEY)) {
        m_accessMode = System.Messaging.QueueAccessMode.Receive;
      }
      else {
        // not Send only (prevents use of count/clear)
        m_accessMode = System.Messaging.QueueAccessMode.SendAndReceive;
      }
      m_queue = new System.Messaging.MessageQueue(m_queueName, m_accessMode);
      m_queue.Formatter = new System.Messaging.BinaryMessageFormatter();
      
      // Keep queue content on reboot or not
      m_queue.DefaultPropertiesToSend.Recoverable = m_configReader.Get<bool> (PERSISTENCE_KEY);
      
      SetActive();
      
      m_initialized = true;
    }
    
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
    
    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    public void Clear ()
    {
      Initialize ();

      m_queue.Purge();
    }
    
    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    /// <returns></returns>
    public ExchangeData Peek ()
    {
      Initialize ();

      try {
        System.Messaging.Message msg = m_queue.Peek(TimeSpan.Zero);
        return (ExchangeData) msg.Body;
      } catch(System.Messaging.MessageQueueException ex) {
        if (ex.MessageQueueErrorCode == System.Messaging.MessageQueueErrorCode.IOTimeout) {
          
          log.InfoFormat("Peek: " +
                         "the queue is empty " +
                         "=> throw InvalidOperationException");
          throw new InvalidOperationException();
        } else {
          throw;
        }
      }
    }
    
    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    /// <returns></returns>
    public IList<ExchangeData> Peek (int nbElements)
    {
      Initialize ();

      int nbRead = 0;
      IList<ExchangeData> datas = new List<ExchangeData> ();
      try {
        System.Messaging.MessageEnumerator msgEnumerator = m_queue.GetMessageEnumerator2();
        while ((msgEnumerator.MoveNext()) && (nbRead < nbElements)) {
          var data = (ExchangeData) msgEnumerator.Current.Body;
          datas.Add(data);
          nbRead++;
        }
        log.DebugFormat("Peek: " +
                        "return {0} elements from the {1} asked elements",
                        datas.Count,
                        nbElements);
      }
      catch(System.Messaging.MessageQueueException ex) {
        if (ex.MessageQueueErrorCode == System.Messaging.MessageQueueErrorCode.IOTimeout) {
          
          log.InfoFormat("Peek: " +
                         "the queue is empty " +
                         "=> throw InvalidOperationException");
          throw new InvalidOperationException();
        }
        else {
          throw;
        }
      }
      return datas;
    }
    
    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    /// <returns></returns>
    public ExchangeData Dequeue ()
    {
      Initialize ();

      try {
        System.Messaging.MessageEnumerator msgEnumerator = m_queue.GetMessageEnumerator2();
        if (msgEnumerator.MoveNext()) {
          // at least one message in queue
          return (ExchangeData) m_queue.Receive().Body; // synchronous call
        } else {
          log.InfoFormat("Dequeue: " +
                         "the queue is empty " +
                         "=> throw InvalidOperationException");
          throw new InvalidOperationException();
        }
      } catch(System.Messaging.MessageQueueException ex) {
        log.ErrorFormat ("Dequeue: " +
                         "Queue {0} raised an exception " +
                         "{1}",
                         m_queueName,
                         ex);
        throw;
      }
    }
    
    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    /// <param name="data"></param>
    public void Enqueue(ExchangeData data)
    {
      Initialize ();

      try {
        m_queue.Send(data);
      }
      catch (System.Messaging.MessageQueueException ex) {
        log.ErrorFormat("Enqueue: " +
                        "Send on {0} failed " +
                        "{1}",
                        m_queueName,
                        ex);
        throw;
      }
    }
    
    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    /// <param name="n"></param>
    public void UnsafeDequeue(int n)
    {
      Initialize ();

      int nbDequeued = 0;
      try {
        System.Messaging.MessageEnumerator msgEnumerator = m_queue.GetMessageEnumerator2();
        while (msgEnumerator.MoveNext() && (nbDequeued < n)) {
          m_queue.Receive(); // synchronous call
          nbDequeued++;
        }
        if (nbDequeued < n) {
          log.ErrorFormat("UnsafeDequeue: " +
                          "Only managed to dequeue {0} elements out of {1}",
                          nbDequeued, n);
        }
        return;
      } catch(System.Messaging.MessageQueueException ex) {
        log.ErrorFormat ("Dequeue: " +
                         "Queue {0} raised an exception " +
                         "{1}",
                         m_queueName,
                         ex);
        throw;
      }
    }

    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    public void UnsafeDequeue()
    {
      UnsafeDequeue(1);
    }

    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    /// <returns></returns>
    public bool VacuumIfNeeded()
    {
      return false;
    }

    /// <summary>
    /// <see cref="ICncDataQueue"/>
    /// </summary>
    public void Close ()
    {      
    }

    /// <summary>
    /// Delete the queue
    /// </summary>
    public void Delete()
    {
      Initialize ();

      if (System.Messaging.MessageQueue.Exists(m_queueName)) {
        System.Messaging.MessageQueue.Delete(m_queueName);
      }
    }
    #endregion // Methods

    #region IDisposable
    /// <summary>
    /// Dispose method to free resources
    /// Do not make this method virtual.
    /// A derived class should not be able to override this method.
    /// <see cref="IDisposable.Dispose" />
    /// 
    /// Dispose all the modules, if they have one Dispose method
    /// </summary>
    public void Dispose ()
    {
      Dispose (true);
      // This object will be cleaned up by the Dispose method.
      // Therefore, you should call GC.SupressFinalize to
      // take this object off the finalization queue
      // and prevent finalization code for this object
      // from executing a second time.
      GC.SuppressFinalize (this);
    }

    /// <summary>
    /// Dispose(bool disposing) executes in two distinct scenarios.
    /// If disposing equals true, the method has been called directly
    /// or indirectly by a user's code. Managed and unmanaged resources
    /// can be disposed.
    /// If disposing equals false, the method has been called by the
    /// runtime from inside the finalizer and you should not reference
    /// other objects. Only unmanaged resources can be disposed.
    /// 
    /// Note all the variables are not really needed here.
    /// But they are nevertheless here because this class could be used an example
    /// for other classes that could need them.
    /// </summary>
    /// <param name="disposing">Dispose also the managed resources</param>
    void Dispose (bool disposing)
    {
      if (log.IsDebugEnabled) {
        log.Debug ("Dispose");
      }
      // Check to see if Dispose has already been called.
      if (!this.m_disposed) {
        // If disposing equals true, dispose all managed
        // and unmanaged resources.
        if (disposing) {
          // Dispose managed resources
          if (m_queue != null) {
            m_queue.Close ();
          }
        }

        // Call the appropriate methods to clean up
        // unmanaged resources here.
        // If disposing is false,
        // only the following code is executed.
      }
      m_disposed = true;
    }
    #endregion // IDisposable
  }
}

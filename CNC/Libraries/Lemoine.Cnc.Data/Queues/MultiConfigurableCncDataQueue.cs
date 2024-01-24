// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Lemoine.Collections;
using Lemoine.Info;
using Lemoine.Info.ConfigReader;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using System.Linq;

namespace Lemoine.Cnc.Data
{
  /// <summary>
  /// Description of MultiConfigurableCncDataQueue.
  /// </summary>
  public sealed class MultiConfigurableCncDataQueue
    : IMultiCncDataQueue
    , IConfigurable
    , IListConfigurable
    , IChecked
  {
    #region Members
    bool m_initialized = false;
    IGenericConfigReader m_externalConfigReader;
    MemoryConfigReader m_defaultConfigReader = new MemoryConfigReader ();
    IGenericConfigReader m_configReader;
    IListConfigReader m_listConfigReader;

    // Structure to write in the queues
    // filter on the command, then on the field
    readonly IDictionary<ExchangeDataCommand, IList<Tuple<int, Regex, IExtendedQueue<ExchangeData>>>> m_prioritizedWriteQueues =
      new Dictionary<ExchangeDataCommand, IList<Tuple<int, Regex, IExtendedQueue<ExchangeData>>>> ();

    // Structures to read the queues
    readonly IList<Tuple<int, IExtendedQueue<ExchangeData>>> m_prioritizedReadQueues = new List<Tuple<int, IExtendedQueue<ExchangeData>>> ();
    readonly IList<IExtendedQueue<ExchangeData>> m_notPrioritizedReadQueues = new List<IExtendedQueue<ExchangeData>> ();
    int m_readIndexPrioritized = 0;
    int m_readIndexNotPrioritized = 0;

    // Keep a reference on all queues (no duplicate)
    readonly IList<IExtendedQueue<ExchangeData>> m_allQueues = new List<IExtendedQueue<ExchangeData>> ();

    ILog log = LogManager.GetLogger (typeof (MultiConfigurableCncDataQueue).FullName);
    #endregion // Members

    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public int MachineId
    {
      get; set;
    }

    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public int MachineModuleId
    {
      get; set;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public MultiConfigurableCncDataQueue ()
    {
      // Default config reader
      m_externalConfigReader = ConfigSet.ConfigReader;
      MultiConfigReader multiConfigReader = new MultiConfigReader ();
      multiConfigReader.Add (m_externalConfigReader);
      multiConfigReader.Add (m_defaultConfigReader);
      m_configReader = multiConfigReader;
    }

    #region IConfigurable implementation
    /// <summary>
    /// <see cref="IConfigurable" />
    /// </summary>
    /// <param name="configReader"></param>
    public void SetConfigReader (IGenericConfigReader configReader)
    {
      m_externalConfigReader = configReader;
      MultiConfigReader multiConfigReader = new MultiConfigReader ();
      multiConfigReader.Add (m_externalConfigReader);
      multiConfigReader.Add (m_defaultConfigReader);
      m_configReader = multiConfigReader;
    }
    #endregion // IConfigurable implementation

    #region IConfigurable implementation
    /// <summary>
    /// <see cref="IListConfigurable" />
    /// </summary>
    /// <param name="listConfigReader"></param>
    public void SetListConfigReader (IListConfigReader listConfigReader)
    {
      m_listConfigReader = listConfigReader;
    }
    #endregion // IConfigurable implementation

    #region ICncDataQueue implementation
    /// <summary>
    /// Initialize the queue based on configurations
    /// </summary>
    void Initialize ()
    {
      if (m_initialized) {
        return;
      }

      log = LogManager.GetLogger (typeof (MultiConfigurableCncDataQueue).FullName +
                                  "." + MachineId + "." + MachineModuleId);

      var typeLoader = new Lemoine.Core.Plugin.TypeLoader ();

      // Construction of the subqueues
      foreach (var subqueueConf in m_listConfigReader.GetConfigs ("queue")) {
        var defaultConf = new MemoryConfigReader ();
        defaultConf.Add ("commandfilter", "");
        defaultConf.Add ("regexfieldfilter", ".*");
        defaultConf.Add ("writepriority", 0);
        defaultConf.Add ("readpriority", -1);
        var conf = new MultiConfigReader ();
        conf.Add (subqueueConf);
        conf.Add (defaultConf);
        conf.Add (m_configReader);

        // Properties of the subqueue
        var commandFilters = new List<ExchangeDataCommand> ();
        {
          var array = conf.Get<string> ("commandfilter").Split ('|', ',', ';');
          foreach (string commandFilter in array) {
            commandFilters.Add (
              (ExchangeDataCommand)Enum.Parse (typeof (ExchangeDataCommand), commandFilter.Trim (' ')));
          }
        }

        var regexFieldFilter = new Regex (conf.Get<string> ("regexfieldfilter"));
        int writePriority = conf.Get<int> ("writepriority");
        int readPriority = conf.Get<int> ("readpriority");

        // Create the subqueue and keep a reference to it
        ICncDataQueue subQueue = null;
        string queueType = conf.Get<string> ("QueueType");
        try {
          subQueue = typeLoader.Load<ICncDataQueue> (queueType);

          if (subQueue == null) {
            string txt = $"Unknown queue used: {queueType}";
            log.Error ($"Initialize: {txt}");
            throw new Exception (txt);
          }
        }
        catch (Exception ex) {
          log.Error ($"Initialize: couldn't create type {queueType}", ex);
          throw;
        }

        subQueue.MachineId = this.MachineId;
        subQueue.MachineModuleId = this.MachineModuleId;

        if (subQueue is ICheckedCaller) {
          ((ICheckedCaller)subQueue).SetCheckedCaller (this);
        }

        if (subQueue is IConfigurable) {
          IConfigurable configurableQueue = subQueue as IConfigurable;
          configurableQueue.SetConfigReader (conf);
        }

        m_allQueues.Add (subQueue);

        // Store the subqueue for being written
        {
          foreach (var commandFilter in commandFilters) {
            if (!m_prioritizedWriteQueues.ContainsKey (commandFilter)) {
              m_prioritizedWriteQueues[commandFilter] = new List<Tuple<int, Regex, IExtendedQueue<ExchangeData>>> ();
            }

            var triple = new Tuple<int, Regex, IExtendedQueue<ExchangeData>> (writePriority, regexFieldFilter, subQueue);
            int index = 0;
            while (index < m_prioritizedWriteQueues[commandFilter].Count &&
                   m_prioritizedWriteQueues[commandFilter][index].Item1 < writePriority) {
              index++;
            }

            m_prioritizedWriteQueues[commandFilter].Insert (index, triple);
          }
        }

        // Store the subqueue for being read
        if (readPriority == -1) {
          m_notPrioritizedReadQueues.Add (subQueue);
        }
        else {
          var pair = new Tuple<int, IExtendedQueue<ExchangeData>> (readPriority, subQueue);
          int index = 0;
          while (index < m_prioritizedReadQueues.Count && m_prioritizedReadQueues[index].Item1 < readPriority) {
            index++;
          }

          m_prioritizedReadQueues.Insert (index, pair);
        }
      }

      m_initialized = true;
    }
    #endregion // ICncDataQueue implementation

    #region IQueue<ExchangeData> implementation
    /// <summary>
    /// Gets the number of elements contained in the Queue.
    /// </summary>
    public int Count
    {
      get {
        Initialize ();

        int count = 0;
        foreach (var queue in m_allQueues) {
          SetActive ();
          count += queue.Count;
        }
        return count;
      }
    }

    /// <summary>
    /// Removes all objects from the queue
    /// </summary>
    public void Clear ()
    {
      Initialize ();

      foreach (var queue in m_allQueues) {
        SetActive ();
        queue.Clear ();
        m_readIndexPrioritized = 0;
        m_readIndexNotPrioritized = 0;
      }
    }

    /// <summary>
    /// Removes and returns the object at the beginning of the Queue.
    /// </summary>
    /// <returns>The object that is removed from the beginning of the Queue.</returns>
    public ExchangeData Dequeue ()
    {
      Initialize ();

      int initialIndex = CurrentQueueIndex;
      do {
        try {
          // Try to find a data in the current queue
          return GetCurrentReadQueue ().Dequeue ();
        }
        catch (InvalidOperationException) {
          // Go to the next queue
          MoveNextQueue ();
        }
        catch (Exception ex) {
          log.ErrorFormat ("Dequeue: an unexpected exception occurred {0}", ex);
          throw;
        }
        SetActive ();
      } while (CurrentQueueIndex != initialIndex);

      // All the queues have been visited without having found an element: raise InvalidOperationException
      throw new InvalidOperationException ();
    }

    /// <summary>
    /// Adds an object to the end of the Queue.
    /// </summary>
    /// <param name="item">The object to add to the Queue. The value can be a null reference for reference types.</param>
    public void Enqueue (ExchangeData item)
    {
      Initialize ();

      bool ok = false;
      if (m_prioritizedWriteQueues.ContainsKey (item.Command)) {
        foreach (var triple in m_prioritizedWriteQueues[item.Command]) {
          if (triple.Item2.IsMatch (item.Key ?? "")) {
            triple.Item3.Enqueue (item);
            ok = true;
            break;
          }
        }
      }

      if (!ok) {
        log.Warn ($"No queues configured for storing {item}");
      }
    }

    /// <summary>
    /// Returns the object at the beginning of the Queue without removing it.
    /// 
    /// InvalidOperationException is returned in case there is no item in the queue
    /// </summary>
    /// <returns>The object at the beginning of the Queue. </returns>
    public ExchangeData Peek ()
    {
      Initialize ();

      IList<ExchangeData> datas = Peek (1);
      if (0 == datas.Count) {
        log.Info ("Peek: the queue is empty => throw InvalidOperationException");
        throw new InvalidOperationException ();
      }

      return datas[0];
    }

    /// <summary>
    /// <see cref="ICncDataQueue"/>
    /// </summary>
    public void Close ()
    {
      foreach (var queue in m_allQueues) {
        queue.Close ();
      }
    }

    /// <summary>
    /// Delete the queue
    /// </summary>
    public void Delete ()
    {
      Initialize ();

      foreach (var queue in m_allQueues) {
        queue.Delete ();
      }
    }
    #endregion // IQueue<ExchangeData> implementation

    #region IExtendedQueue<ExchangeData> implementation
    /// <summary>
    /// Peek up to n elements in the queue in the same time.
    /// 
    /// Given the implementation of the queue, less elements may be returned.
    /// </summary>
    /// <param name="nbElements"></param>
    /// <returns></returns>
    public IList<ExchangeData> Peek (int nbElements)
    {
      Initialize ();

      int initialIndex = CurrentQueueIndex;
      do {
        // Get data from the current queue
        var datas = GetCurrentReadQueue ().Peek (nbElements);
        if (datas.Count > 0) {
          return datas;
        }

        // Move to next queue
        MoveNextQueue ();
        SetActive ();
      } while (CurrentQueueIndex != initialIndex);

      // All the queues have been visited without having found an element: return an empty list
      return new List<ExchangeData> ();
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

      GetCurrentReadQueue ().UnsafeDequeue (n);
    }

    /// <summary>
    /// Vacuum queue if needed (useful for DB implementation);
    /// returns true if a vacuum did occur
    /// </summary>
    public bool VacuumIfNeeded ()
    {
      Initialize ();

      bool vacuumPerformed = false;
      foreach (var queue in m_allQueues) {
        vacuumPerformed |= queue.VacuumIfNeeded ();
        SetActive ();
      }
      return vacuumPerformed;
    }
    #endregion // IExtendedQueue<ExchangeData> implementation

    #region MultiCncDataQueue implementation
    /// <summary>
    /// Index of current queue
    /// </summary>
    public int CurrentQueueIndex
    {
      get {
        Initialize ();

        return m_readIndexPrioritized < m_prioritizedReadQueues.Count ?
          m_readIndexPrioritized :
          m_prioritizedReadQueues.Count + m_readIndexNotPrioritized;
      }
    }

    /// <summary>
    /// Move to next queue (modulo number of queues)
    /// </summary>
    public void MoveNextQueue ()
    {
      Initialize ();

      if (m_readIndexPrioritized < m_prioritizedReadQueues.Count) {
        // First we browse the prioritized queues
        m_readIndexPrioritized++;
      }
      else {
        // Then the not prioritized queues
        m_readIndexNotPrioritized++;
        if (m_readIndexNotPrioritized == m_notPrioritizedReadQueues.Count) {
          // Back to the first prioritized queue
          m_readIndexPrioritized = 0;
          m_readIndexNotPrioritized = 0;
        }
      }
    }

    /// <summary>
    /// Go to the first internal single queue
    /// </summary>
    public void Reset ()
    {
      Initialize ();

      // Only reset the index for prioritized queues
      m_readIndexPrioritized = 0;
    }
    #endregion // MultiCncDataQueue implementation

    #region IDisposable implementation
    /// <summary>
    /// Dispose the queue
    /// </summary>
    public void Dispose ()
    {
      foreach (var queue in m_allQueues) {
        try {
          queue.Dispose ();
        }
        catch (Exception) { }
      }
      GC.SuppressFinalize (this);
    }
    #endregion // IDisposable implementation

    #region IChecked implementation
    IChecked m_checkedCaller = null;

    /// <summary>
    /// Method to call regularly to keep the thread active
    /// </summary>
    public void SetActive ()
    {
      if (m_checkedCaller != null) {
        m_checkedCaller.SetActive ();
      }
    }

    /// <summary>
    /// Pause the check of a thread
    /// </summary>
    public void PauseCheck ()
    {
      if (m_checkedCaller != null) {
        m_checkedCaller.PauseCheck ();
      }
    }

    /// <summary>
    /// Resume the check of a thread
    /// </summary>
    public void ResumeCheck ()
    {
      if (m_checkedCaller != null) {
        m_checkedCaller.ResumeCheck ();
      }
    }
    #endregion // IDisposable implementation

    #region Private methods
    IExtendedQueue<ExchangeData> GetCurrentReadQueue ()
    {
      Initialize ();

      return m_readIndexPrioritized == m_prioritizedReadQueues.Count ?
        m_notPrioritizedReadQueues[m_readIndexNotPrioritized] :
        m_prioritizedReadQueues[m_readIndexPrioritized].Item2;
    }
    #endregion // Private methods
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Collections;
using Lemoine.Threading;
using Lemoine.Core.Log;

namespace Lemoine.Cnc.Data
{
  /// <summary>
  /// Virtual queue that reads/writes in multiple ICncDataQueue queues,
  /// according to the kind of data
  /// </summary>
  public abstract class MultiGenericCncDataQueue
    : IMultiCncDataQueue, IDisposable, IChecked
  {
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

    #region Members
    /// <summary>
    /// Queue for machine modes
    /// </summary>
    protected IExtendedQueue<ExchangeData> m_machineModeQueue;

    /// <summary>
    /// Queue for stamp actions
    /// </summary>
    protected IExtendedQueue<ExchangeData> m_stampActionVariableQueue;

    /// <summary>
    /// Queues for cncvalues (with priority)
    /// </summary>
    protected IDictionary<string, IExtendedQueue<ExchangeData>> m_cncValueToQueue = new Dictionary<string, IExtendedQueue<ExchangeData>> ();

    /// <summary>
    /// Queue for activities
    /// </summary>
    protected IExtendedQueue<ExchangeData> m_machineModuleActivityQueue;

    /// <summary>
    /// Queue for other cnc values (lowest priority)
    /// </summary>
    protected IExtendedQueue<ExchangeData> m_otherCncValues;

    /// <summary>
    /// Queue for CNC alarms
    /// </summary>
    protected IExtendedQueue<ExchangeData> m_alarmQueue;

    /// <summary>
    /// All queues
    /// </summary>
    protected IList<IExtendedQueue<ExchangeData>> m_allQueues = new List<IExtendedQueue<ExchangeData>> ();

    /// <summary>
    /// Currently selected queue index
    /// </summary>
    protected int m_currentQueueIndex = 0;

    private IChecked m_checkedCaller = null;
    #endregion // Members

    ILog log = LogManager.GetLogger (typeof (MultiGenericCncDataQueue).FullName);

    #region Getters / setters
    /// <summary>
    /// Current queue index
    /// </summary>
    public int CurrentQueueIndex
    {
      get { return m_currentQueueIndex; }
      protected set { m_currentQueueIndex = value; }
    }

    /// <summary>
    /// CheckedCaller
    /// </summary>
    public IChecked CheckedCaller
    {
      get { return m_checkedCaller; }
      set { m_checkedCaller = value; }
    }

    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    public int Count
    {
      get
      {
        Initialize ();

        int count = 0;
        foreach (var queue in GetQueueEnumeration ()) {
          SetActive ();
          count += queue.Count;
        }
        return count;
      }
    }
    #endregion // Getters / setters

    #region Methods
    /// <summary>
    /// Set the caller
    /// </summary>
    /// <param name="checkedCaller"></param>
    public void SetCheckedCaller (IChecked checkedCaller)
    {
      m_checkedCaller = checkedCaller;
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

    /// <summary>
    /// Initialize the queue
    /// </summary>
    protected abstract void Initialize ();

    IEnumerable<IExtendedQueue<ExchangeData>> GetQueueEnumeration ()
    {
      var enumeration = new MultiEnumeration<IExtendedQueue<ExchangeData>> ();
      enumeration.Add (m_allQueues);
      return enumeration;
    }

    /// <summary>
    /// IDisposable implementation
    /// </summary>
    public void Dispose ()
    {
      Dispose (true);
      GC.SuppressFinalize (this);
    }

    /// <summary>
    /// IDisposable implementation
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose (bool disposing)
    {
      if (disposing) {
        // Dispose all owned managed objects
        foreach (var queue in GetQueueEnumeration ()) {
          try {
            queue.Dispose ();
          }
          catch (Exception) {
          }
        }
      }

      // Release unmanaged resources
    }

    /// <summary>
    /// Go to the first internal single Queue
    /// </summary>
    public void Reset ()
    {
      m_currentQueueIndex = 0;
    }

    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    public void Clear ()
    {
      Initialize ();

      foreach (var queue in GetQueueEnumeration ()) {
        SetActive ();
        queue.Clear ();
        m_currentQueueIndex = 0;
      }
    }

    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    /// <returns></returns>
    public ExchangeData Dequeue ()
    {
      Initialize ();

      for (int i = 0; i < m_allQueues.Count; i++) {
        SetActive ();
        int index = (m_currentQueueIndex + i) % (m_allQueues.Count);
        var queue = m_allQueues[index];
        try {
          ExchangeData data = queue.Dequeue ();
          // This is now the current queue !
          m_currentQueueIndex = index;
          return data;
        }
        catch (InvalidOperationException) {
          // Empty queue: go to next queue
          continue;
        }
        catch (Exception ex) {
          log.ErrorFormat ("Dequeue: " +
                           "an unexpected exception occurred {0}",
                           ex);
          throw;
        }
      }
      // All the queues have been visited without having found an element:
      // raise InvalidOperationException
      throw new InvalidOperationException ();
    }

    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    /// <returns></returns>
    public void UnsafeDequeue ()
    {
      UnsafeDequeue (1);
    }

    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    /// <returns></returns>
    public void UnsafeDequeue (int n)
    {
      Initialize ();

      // The previous Peek that precedes the call to this UnsafeDequeue
      // already selected the right queue
      m_allQueues[m_currentQueueIndex].UnsafeDequeue (n);
    }

    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    /// <param name="data"></param>
    public void Enqueue (ExchangeData data)
    {
      Initialize ();

      switch (data.Command) {
      case ExchangeDataCommand.MachineMode:
        m_machineModeQueue.Enqueue (data);
        break;
      case ExchangeDataCommand.MachineModuleActivity:
        m_machineModuleActivityQueue.Enqueue (data);
        break;
      case ExchangeDataCommand.Stamp:
      case ExchangeDataCommand.Action:
      case ExchangeDataCommand.DetectionTimeStamp:
      case ExchangeDataCommand.CncVariableSet:
      case ExchangeDataCommand.SequenceMilestone:
        m_stampActionVariableQueue.Enqueue (data);
        break;
      case ExchangeDataCommand.CncValue:
      case ExchangeDataCommand.StopCncValue:
        string fieldKey = data.Key;
        if (m_cncValueToQueue.ContainsKey (fieldKey)) {
          m_cncValueToQueue[fieldKey].Enqueue (data);
        }
        else {
          m_otherCncValues.Enqueue (data);
        }
        break;
      case ExchangeDataCommand.CncAlarm:
        m_alarmQueue.Enqueue (data);
        break;
      }
    }

    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    /// <returns></returns>
    public ExchangeData Peek ()
    {
      Initialize ();

      IList<ExchangeData> datas = Peek (1);
      if (0 == datas.Count) {
        log.InfoFormat ("Peek: " +
                        "the queue is empty " +
                        "=> throw InvalidOperationException");
        throw new InvalidOperationException ();
      }
      else {
        return datas[0];
      }
    }

    /// <summary>
    /// ICncDataQueue implementation
    /// </summary>
    /// <returns></returns>
    public IList<ExchangeData> Peek (int nbElements)
    {
      Initialize ();

      for (int i = 0; i < m_allQueues.Count; i++) {
        SetActive ();
        int index = (m_currentQueueIndex + i) % (m_allQueues.Count);
        var queue = m_allQueues[index];
        IList<ExchangeData> datas = queue.Peek (nbElements);
        if (0 < datas.Count) {
          // This is now the current queue !
          m_currentQueueIndex = index;
          return datas;
        }
        else {
          // Empty queue: go to next queue
          continue;
        }
      }
      // All the queues have been visited without having found an element:
      // return an empty list
      return new List<ExchangeData> ();
    }

    /// <summary>
    /// Move to the next internal queue
    /// </summary>
    public void MoveNextQueue ()
    {
      m_currentQueueIndex = (m_currentQueueIndex + 1) % (m_allQueues.Count);
    }

    /// <summary>
    /// Vacuums dbs if needed
    /// </summary>
    /// <returns></returns>
    public bool VacuumIfNeeded ()
    {
      Initialize ();

      bool vacuumPerformed = false;
      for (int i = 0; i < m_allQueues.Count; i++) {
        SetActive ();
        var queue = m_allQueues[i];
        vacuumPerformed |= queue.VacuumIfNeeded ();
      }
      return vacuumPerformed;
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

      Close ();

      foreach (var queue in m_allQueues) {
        queue.Delete ();
      }
    }
    #endregion // Methods
  }
}

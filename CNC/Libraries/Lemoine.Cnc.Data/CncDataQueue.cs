// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Collections;
using Lemoine.Info.ConfigReader;
using Lemoine.Core.Log;

namespace Lemoine.Cnc.Data
{
  /// <summary>
  /// Convert a queue into a ICncDataQueue
  /// </summary>
  public class CncDataQueue<T>
    : ICncDataQueue
    , IConfigurable
    where T : IExtendedQueue<ExchangeData>
  {
    readonly string MACHINE_ID_KEY = "MachineId";
    readonly string MACHINE_MODULE_ID_KEY = "MachineModuleId";
    readonly string QUEUE_NAME_KEY = "QueueName";

    readonly IExtendedQueue<ExchangeData> m_extendedQueue;
    readonly MemoryConfigReader m_machineConfigReader = new MemoryConfigReader ();
    int m_machineId;
    int m_machineModuleId;

    ILog log = LogManager.GetLogger (typeof (CncDataQueue<T>).FullName);

    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public int MachineId
    {
      get {
        return m_machineId;
      }
      set {
        m_machineId = value;
        m_machineConfigReader.Add (MACHINE_ID_KEY, m_machineId, true);
        m_machineConfigReader.Add (QUEUE_NAME_KEY, GetQueueName (), true);
      }
    }

    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public int MachineModuleId
    {
      get {
        return m_machineModuleId;
      }
      set {
        m_machineModuleId = value;
        m_machineConfigReader.Add (MACHINE_MODULE_ID_KEY, m_machineModuleId, true);
        m_machineConfigReader.Add (QUEUE_NAME_KEY, GetQueueName (), true);
      }
    }

    string GetQueueName ()
    {
      if (0 < m_machineModuleId) {
        return $"{m_machineId}-{m_machineModuleId}";
      }
      else {
        return m_machineId.ToString ();
      }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public CncDataQueue ()
    {
      try {
        var typeLoader = new Lemoine.Core.Plugin.TypeLoader ();
        m_extendedQueue = typeLoader.Load<IExtendedQueue<ExchangeData>, T> ();

        if (null == m_extendedQueue) {
          string message = $"CncDataQueue: Unknown queue used: {typeof (T)}";
          log.Error ($"CncDataQueue: queue is null, {message}");
          throw new Exception (message);
        }
      }
      catch (Exception ex) {
        log.Error ($"CncDataQueue: Couldn't create type {typeof (T)}", ex);
        throw;
      }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="extendedQueue"></param>
    public CncDataQueue (IExtendedQueue<ExchangeData> extendedQueue)
    {
      m_extendedQueue = extendedQueue;
    }

    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public IList<ExchangeData> Peek (int nbElements)
    {
      return m_extendedQueue.Peek (nbElements);
    }

    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public void UnsafeDequeue ()
    {
      m_extendedQueue.UnsafeDequeue ();
    }

    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public void UnsafeDequeue (int n)
    {
      m_extendedQueue.UnsafeDequeue (n);
    }

    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public bool VacuumIfNeeded ()
    {
      return m_extendedQueue.VacuumIfNeeded ();
    }

    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public void Dispose ()
    {
      m_extendedQueue.Dispose ();
    }

    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public void Clear ()
    {
      m_extendedQueue.Clear ();
    }

    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public ExchangeData Dequeue ()
    {
      return m_extendedQueue.Dequeue ();
    }

    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public void Enqueue (ExchangeData item)
    {
      m_extendedQueue.Enqueue (item);
    }

    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public ExchangeData Peek ()
    {
      return m_extendedQueue.Peek ();
    }

    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public void Close ()
    {
      m_extendedQueue.Close ();
    }

    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public void Delete ()
    {
      m_extendedQueue.Delete ();
    }

    /// <summary>
    /// <see cref="ICncDataQueue" />
    /// </summary>
    public int Count => m_extendedQueue.Count;

    #region IConfigurable implementation
    /// <summary>
    /// <see cref="IConfigurable" />
    /// </summary>
    /// <param name="configReader"></param>
    public void SetConfigReader (IGenericConfigReader configReader)
    {
      if (m_extendedQueue is IConfigurable) {
        IConfigurable configurableQueue = m_extendedQueue as IConfigurable;
        MultiConfigReader multiConfigReader = new MultiConfigReader ();
        multiConfigReader.Add (configReader);
        multiConfigReader.Add (m_machineConfigReader);
        configurableQueue.SetConfigReader (multiConfigReader);
      }
    }
    #endregion
  }
}

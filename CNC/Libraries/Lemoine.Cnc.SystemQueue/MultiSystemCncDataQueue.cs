// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Cnc.Data;
using Lemoine.Collections;
using Lemoine.Info.ConfigReader;
using Lemoine.Threading;
using Lemoine.Core.Log;

namespace Lemoine.Cnc.SystemQueue
{
  /// <summary>
  /// Multiple system queues for CncData
  /// </summary>
  public class MultiSystemCncDataQueue
    : MultiGenericCncDataQueue
    , IConfigurable
  {
    ILog log = LogManager.GetLogger(typeof (MultiSystemCncDataQueue).FullName);
    
    static readonly string QUEUE_PREFIX_KEY = "Prefix";
    static readonly string QUEUE_PREFIX_DEFAULT = "Multi";
    
    IGenericConfigReader m_externalConfigReader;
    MemoryConfigReader m_defaultConfigReader = new MemoryConfigReader ();
    IGenericConfigReader m_configReader;

    /// <summary>
    /// Constructor
    /// </summary>
    public MultiSystemCncDataQueue ()
    {
      m_defaultConfigReader.Add (QUEUE_PREFIX_KEY, QUEUE_PREFIX_DEFAULT);
    }
    
    #region IConfigurable implementation
    /// <summary>
    /// <see cref="IConfigurable" />
    /// </summary>
    /// <param name="configReader"></param>
    public void SetConfigReader(IGenericConfigReader configReader)
    {
      m_externalConfigReader = configReader;
      MultiConfigReader multiConfigReader = new MultiConfigReader ();
      multiConfigReader.Add (m_externalConfigReader);
      multiConfigReader.Add (m_defaultConfigReader);
      m_configReader = multiConfigReader;
    }
    #endregion
    
    #region Methods
    /// <summary>
    /// Initialize
    /// </summary>
    protected override void Initialize()
    {
      log = LogManager.GetLogger (typeof (MultiSystemCncDataQueue).FullName +
                                  "." + MachineId + "." + MachineModuleId);
      
      // Machine mode
      m_machineModeQueue = CreateQueue ("");
      m_allQueues.Add(m_machineModeQueue);

      // Stamp action
      m_stampActionVariableQueue = CreateQueue ("-StampAction");
      m_allQueues.Add(m_stampActionVariableQueue);
      
      // Machine module activity
      m_machineModuleActivityQueue = CreateQueue ("-MachineModuleActivity");
      m_allQueues.Add(m_machineModuleActivityQueue);
      
      // Cnc alarm
      m_alarmQueue = CreateQueue ("-CncAlarm");
      m_allQueues.Add(m_alarmQueue);
      
      // Note: the association between a Cnc Value key and a Cnc Value Queue # must never be changed !
      // 1: Feedrate / FeedrateUS / RapidTraverseRate / RapidTraverseRateUS
      var newQueue = CreateQueue ("-CV-01");
      m_allQueues.Add (newQueue);
      m_cncValueToQueue["Feedrate"] = newQueue;
      m_cncValueToQueue["FeedrateUS"] = newQueue;
      m_cncValueToQueue["RapidTraverseRate"] = newQueue;
      m_cncValueToQueue["RapidTraverseRateUS"] = newQueue;
      
      // 2: SpindleSpeed
      newQueue = CreateQueue ("-CV-02");
      m_allQueues.Add(newQueue);
      m_cncValueToQueue["SpindleSpeed"] = newQueue;
      
      // 3: SpindleLoad
      newQueue = CreateQueue ("-CV-03");
      m_allQueues.Add(newQueue);
      m_cncValueToQueue["SpindleLoad"] = newQueue;
      
      // 4: FeedrateOverride / RapidTraverseOverride
      newQueue = CreateQueue ("-CV-04");
      m_allQueues.Add(newQueue);
      m_cncValueToQueue["FeedrateOverride"] = newQueue;
      m_cncValueToQueue["RapidTraverseOverride"] = newQueue;
      
      // 5: SpindleSpeedOverride
      newQueue = CreateQueue ("-CV-05");
      m_allQueues.Add(newQueue);
      m_cncValueToQueue["SpindleSpeedOverride"] = newQueue;
      
      // 6: CncModes
      newQueue = CreateQueue ("-CV-06");
      m_allQueues.Add(newQueue);
      m_cncValueToQueue["CncModes"] = newQueue;
      
      // 7: ProgramName
      newQueue = CreateQueue ("-CV-07");
      m_allQueues.Add (newQueue);
      m_cncValueToQueue ["ProgramName"] = newQueue;
      
      // 0: Others
      newQueue = CreateQueue ("-CV-Others");
      m_otherCncValues = newQueue;
      m_allQueues.Add(m_otherCncValues);
    }
    
    IExtendedQueue<ExchangeData> CreateQueue (string suffix)
    {
      var subDefaultConfigReader = new MemoryConfigReader ();
      subDefaultConfigReader.Add ("Suffix", suffix);
      MultiConfigReader subConfigReader = new MultiConfigReader ();
      subConfigReader.Add (subDefaultConfigReader);
      subConfigReader.Add (m_configReader);
      var queue = new SystemCncDataQueue ();
      queue.SetCheckedCaller (this);
      queue.MachineId = this.MachineId;
      queue.MachineModuleId = this.MachineModuleId;
      queue.SetConfigReader (subConfigReader);
      return queue;
    }
    #endregion // Methods
  }
}

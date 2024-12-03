// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using Lemoine.Cnc.Data;
using Lemoine.Collections;
using Lemoine.Info;
using Lemoine.Info.ConfigReader;
using Lemoine.Threading;
using Lemoine.Core.Log;
using System.Diagnostics;

namespace Lemoine.Cnc.SQLiteQueue
{
  /// <summary>
  /// Virtual queue that reads/writes in multiple SQLiteCncDataQueue queues,
  /// according to the kind of data
  /// </summary>
  public sealed class MultiSQLiteCncDataQueue
    : MultiGenericCncDataQueue
    , IConfigurable
    , IChecked
  {
    #region Members
    bool m_initialized = false;
    IGenericConfigReader m_externalConfigReader;
    MemoryConfigReader m_defaultConfigReader = new MemoryConfigReader ();
    IGenericConfigReader m_configReader;
    ILog log = LogManager.GetLogger(typeof (MultiSQLiteCncDataQueue).FullName);
    #endregion // Members
    
    static readonly string FILENAME_PREFIX_KEY = "Prefix";
    static readonly string FILENAME_PREFIX_DEFAULT = "CncDataFifoDb-";
    static readonly string CNC_DIRECTORY_KEY = "Directory";
    
    /// <summary>
    /// Constructor
    /// </summary>
    public MultiSQLiteCncDataQueue ()
    {
      m_defaultConfigReader.Add (FILENAME_PREFIX_KEY, FILENAME_PREFIX_DEFAULT);
      string cncDirectory = Path.Combine (PulseInfo.LocalConfigurationDirectory, "Cnc");
      if (!Directory.Exists (cncDirectory)) {
        cncDirectory = PulseInfo.LocalConfigurationDirectory;
      }
      m_defaultConfigReader.Add (CNC_DIRECTORY_KEY, cncDirectory);

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
    public void SetConfigReader(IGenericConfigReader configReader)
    {
      m_externalConfigReader = configReader;
      MultiConfigReader multiConfigReader = new MultiConfigReader ();
      multiConfigReader.Add (m_externalConfigReader);
      multiConfigReader.Add (m_defaultConfigReader);
      m_configReader = multiConfigReader;
    }
    #endregion // IConfigurable implementation

    /// <summary>
    /// Initialize
    /// </summary>
    protected override void Initialize ()
    {
      if (m_initialized) {
        return;
      }

      Debug.Assert (0 != this.MachineId);

      try {
        log = LogManager.GetLogger (typeof (MultiSQLiteCncDataQueue).FullName +
                                    "." + MachineId + "." + MachineModuleId);

        // Machine mode
        m_machineModeQueue = CreateQueue ("");
        m_allQueues.Add (m_machineModeQueue);

        // Stamp action
        m_stampActionVariableQueue = CreateQueue ("-StampAction");
        m_allQueues.Add (m_stampActionVariableQueue);

        // Machine module activity
        m_machineModuleActivityQueue = CreateQueue ("-MachineModuleActivity");
        m_allQueues.Add (m_machineModuleActivityQueue);

        // Cnc alarms
        m_alarmQueue = CreateQueue ("-CncAlarm");
        m_allQueues.Add (m_alarmQueue);

        // Note: the association between a Cnc Value key and a Cnc Value Queue # must never be changed !
        IExtendedQueue<ExchangeData> cncValueQueue;

        // 1: Feedrate / FeedrateUS / RapidTraverseRate / RapidTraverseRateUS
        cncValueQueue = CreateQueue ("-CV-01");
        m_allQueues.Add (cncValueQueue);
        m_cncValueToQueue["Feedrate"] = cncValueQueue;
        m_cncValueToQueue["FeedrateUS"] = cncValueQueue;
        m_cncValueToQueue["RapidTraverseRate"] = cncValueQueue;
        m_cncValueToQueue["RapidTraverseRateUS"] = cncValueQueue;

        // 2: SpindleSpeed
        cncValueQueue = CreateQueue ("-CV-02");
        m_allQueues.Add (cncValueQueue);
        m_cncValueToQueue["SpindleSpeed"] = cncValueQueue;

        // 3: SpindleLoad
        cncValueQueue = CreateQueue ("-CV-03");
        m_allQueues.Add (cncValueQueue);
        m_cncValueToQueue["SpindleLoad"] = cncValueQueue;

        // 4: FeedrateOverride / RapidTraverseOverride
        cncValueQueue = CreateQueue ("-CV-04");
        m_allQueues.Add (cncValueQueue);
        m_cncValueToQueue["FeedrateOverride"] = cncValueQueue;
        m_cncValueToQueue["RapidTraverseOverride"] = cncValueQueue;

        // 5: SpindleSpeedOverride
        cncValueQueue = CreateQueue ("-CV-05");
        m_allQueues.Add (cncValueQueue);
        m_cncValueToQueue["SpindleSpeedOverride"] = cncValueQueue;

        // 6: CncModes
        cncValueQueue = CreateQueue ("-CV-06");
        m_allQueues.Add (cncValueQueue);
        m_cncValueToQueue["CncModes"] = cncValueQueue;

        // 7: ProgramName
        cncValueQueue = CreateQueue ("-CV-07");
        m_allQueues.Add (cncValueQueue);
        m_cncValueToQueue["ProgramName"] = cncValueQueue;

        // 0: Others
        m_otherCncValues = CreateQueue ("-CV-Others");
        m_allQueues.Add (m_otherCncValues);
      }
      catch (Exception ex) {
        log.Error ("Initialize: exception", ex);
        m_otherCncValues.Clear ();
        m_cncValueToQueue.Clear ();
        m_allQueues.Clear ();
        throw;
      }
      finally {
        m_initialized = true;
      }
    }

    IExtendedQueue<ExchangeData> CreateQueue (string suffix)
    {
      Debug.Assert (0 != this.MachineId);

      MemoryConfigReader subDefaultConfigReader = new MemoryConfigReader ();
      subDefaultConfigReader.Add ("Suffix", suffix);
      MultiConfigReader subConfigReader = new MultiConfigReader ();
      subConfigReader.Add (subDefaultConfigReader);
      subConfigReader.Add (m_configReader);
      var queue = new SQLiteCncDataQueue ();
      queue.SetCheckedCaller (this);
      queue.SetConfigReader (subConfigReader);
      queue.MachineId = this.MachineId;
      queue.MachineModuleId = this.MachineModuleId;
      return queue;
    }
  }
}

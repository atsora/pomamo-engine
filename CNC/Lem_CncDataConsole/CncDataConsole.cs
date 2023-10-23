// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using Lemoine.CncDataImport;
using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions;
using Lemoine.FileRepository;
using Lemoine.ModelDAO.Interfaces;
using Lemoine.Core.Hosting;
using Task = System.Threading.Tasks.Task;
using System.Diagnostics;

namespace Lem_CncDataConsole
{
  /// <summary>
  /// Main class for Lem_CncDataConsole
  /// </summary>
  public class CncDataConsole : IConsoleRunner<Options>
  {
    #region Members
    readonly IApplicationInitializer m_applicationInitializer;
    int m_machineModuleId;
    bool m_useStampFile = true;
    int m_parentProcessId = 0;
    // Options...
    TimeSpan? m_sleepTime = null;
    TimeSpan? m_breakFrequency = null;
    TimeSpan? m_breakTime = null;
    int? m_fetchDataNumber = null;
    TimeSpan? m_whicheverNbOfDataProcessAfter = null;
    int? m_minNumberOfDataToProcess = null;
    TimeSpan? m_visitMachineModesEvery = null;
    #endregion // Members

    ILog log = LogManager.GetLogger (typeof (CncDataConsole).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public CncDataConsole (IApplicationInitializer applicationInitializer)
    {
      Debug.Assert (null != applicationInitializer);

      m_applicationInitializer = applicationInitializer;
    }
    #endregion // Constructors

    #region Methods
    internal void ParseOptions (Options options)
    {
      if (-1 != options.SleepTimeMs) {
        m_sleepTime = TimeSpan.FromMilliseconds (options.SleepTimeMs);
        log.Info ($"ParseOptions: Sleep time is {m_sleepTime}");
      }
      if (-1 != options.BreakFrequency) {
        m_breakFrequency = TimeSpan.FromMilliseconds (options.BreakFrequency);
        log.Info ($"ParseOptions: Break frequency is {m_breakFrequency}");
      }
      if (-1 != options.BreakTime) {
        m_breakTime = TimeSpan.FromMilliseconds (options.BreakTime);
        log.Info ($"ParseOptions: Break time is {m_breakTime}");
      }

      if (-1 == options.FetchDataNumber) { // Default
        log.Info ("ParseOptions: Use the default FetchDataNumber");
        m_fetchDataNumber = 60;
      }
      else {
        log.Info ($"ParseOptions: FetchDataNumber is {options.FetchDataNumber}");
        m_fetchDataNumber = options.FetchDataNumber;
      }
      if (m_fetchDataNumber.HasValue && (m_fetchDataNumber.Value < 1)) {
        log.Error ("ParseOptions: FetchDataNumber must be at least 1");
        m_fetchDataNumber = 1;
      }

      if (null != options.WhicheverNbOfDataProcessAfter) {
        try {
          m_whicheverNbOfDataProcessAfter = TimeSpan.Parse (options.WhicheverNbOfDataProcessAfter);
          log.Info ($"ParseOptions: WhicheverNbOfDataProcessAfter is {options.WhicheverNbOfDataProcessAfter}");
        }
        catch (Exception ex) {
          log.Error ($"ParseOptions: parsing WhicheverNbOfDataProcessAfter {options.WhicheverNbOfDataProcessAfter} failed", ex);
        }
      }

      if (-1 != options.MinDataOfNumberToProcess) {
        m_minNumberOfDataToProcess = options.MinDataOfNumberToProcess;
        log.Info ($"ParseOptions: MinNbOfDataToProcess is {m_minNumberOfDataToProcess}");
      }
      if (m_minNumberOfDataToProcess.HasValue && (m_minNumberOfDataToProcess.Value < 1)) {
        log.Error ("ParseOptions: MinNbOfDataToProcess must be at least 1");
        m_minNumberOfDataToProcess = 1;
      }

      if (null != options.VisitMachineModesEvery) {
        try {
          m_visitMachineModesEvery = TimeSpan.Parse (options.VisitMachineModesEvery);
          log.InfoFormat ("ParseOptions: " +
                          "VisitMachineModesEvery is {0}",
                          m_visitMachineModesEvery);
        }
        catch (Exception ex) {
          log.Error ($"ParseOptions: parsing VisitMachineModesEvery {options.VisitMachineModesEvery} failed", ex);
        }
      }
    }

    /// <summary>
    /// Main method to run
    /// </summary>
    /// <param name="cancellationToken"></param>
    public void Run (CancellationToken cancellationToken)
    {
      Lemoine.GDBPersistentClasses.DaySlotCache.Activate ();

      Lemoine.Core.ExceptionManagement.ExceptionTest
        .AddTest (new Lemoine.Cnc.SQLiteQueue.SQLiteExceptionTest ());

      IMachineModule machineModule;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO.FindByIdWithMonitoredMachine (m_machineModuleId);
      }
      if (machineModule is null) {
        log.Error ($"Run: machine module with ID {m_machineModuleId} does not exist");
        System.Console.Error.WriteLine ($"Machine module with ID {m_machineModuleId} does not exist, exit");
        System.Environment.Exit (1);
      }

      var import = new ImportCncDataFromQueue (machineModule);
      if (m_sleepTime.HasValue) {
        import.SleepTime = m_sleepTime.Value;
      }
      if (m_breakFrequency.HasValue) {
        import.BreakFrequency = m_breakFrequency.Value;
      }
      if (m_breakTime.HasValue) {
        import.BreakTime = m_breakTime.Value;
      }
      if (m_fetchDataNumber.HasValue) {
        import.FetchDataNumber = m_fetchDataNumber.Value;
      }
      if (m_whicheverNbOfDataProcessAfter.HasValue) {
        import.WhicheverNbOfDataProcessAfter = m_whicheverNbOfDataProcessAfter.Value;
      }
      if (m_minNumberOfDataToProcess.HasValue) {
        import.MinNbOfDataToProcess = m_minNumberOfDataToProcess.Value;
      }
      if (m_visitMachineModesEvery.HasValue) {
        import.VisitMachineModesEvery = m_visitMachineModesEvery.Value;
      }

      import.UseStampFile = m_useStampFile;
      import.ParentProcessId = m_parentProcessId;
      import.RunDirectly (cancellationToken);
    }
    #endregion // Methods

    #region IConsoleRunner
    public void SetOptions (Options options)
    {
      m_machineModuleId = options.MachineModuleId;
      log = LogManager.GetLogger ($"{typeof (CncDataConsole).FullName}.{m_machineModuleId}");
      m_useStampFile = options.Stamp;
      m_parentProcessId = options.ParentProcessId;
      ParseOptions (options);
    }

    public async Task RunConsoleAsync (CancellationToken cancellationToken = default)
    {
      await m_applicationInitializer.InitializeApplicationAsync ();

      if (cancellationToken.IsCancellationRequested) {
        return;
      }

      Run (cancellationToken);
    }
    #endregion // IConsoleRunner

  }
}

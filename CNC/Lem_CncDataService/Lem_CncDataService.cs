// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using Lemoine.CncDataImport;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Threading;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using System.Linq;
using Lemoine.FileRepository;
using Lemoine.Core.Hosting;

namespace Lem_CncDataService
{
  /// <summary>
  /// Main class of service Lem_CncDataService
  /// </summary>
  public sealed class Lem_CncDataService
    : Lemoine.Threading.IThreadServiceAsync, IDisposable
  {
    #region Members
    const string SLEEP = "Sleep";
    const string BREAK_FREQUENCY = "BreakFrequency";
    const string BREAK_TIME = "BreakTime";
    const string FETCH_DATA_NUMBER = "FetchDataNumber";
    const string MIN_NB_OF_DATA_TO_PROCESS = "MinNbOfDataToProcess";
    const string WHICHEVER_NB_OF_DATA_PROCESS_AFTER = "WhicheverNbOfDataProcessAfter";
    const string VISIT_MACHINE_MODES_EVERY = "VisitMachineModesEvery";

    readonly IApplicationInitializer m_applicationInitializer;

    readonly CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource ();
    bool m_disposed = false;
    readonly IDictionary<int, ImportCncDataFromQueue> m_threadImports = new Dictionary<int, ImportCncDataFromQueue> ();
    readonly IDictionary<int, ImportProcessExecution> m_processImports = new Dictionary<int, ImportProcessExecution> ();
    readonly CheckThreadsAndProcesses m_check = new CheckThreadsAndProcesses ();
    bool m_useProcess = false;
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (Lem_CncDataService).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Lem_CncDataService (IApplicationInitializer applicationInitializer)
    {
      m_applicationInitializer = applicationInitializer;

      var configUpdateChecker = new ConfigUpdateCheckerWithAvailableFileRepository ();
      var fileRepoChecker = new FileRepoChecker ();
      m_check.AddAdditionalCheckers (configUpdateChecker, fileRepoChecker);
    }
    #endregion // Constructors

    #region Methods
    void CreateImportClasses (CancellationToken cancellationToken)
    {
      // Get the machine modules
      ICollection<IMachineModule> machineModules = null;
      while (null == machineModules) {
        if (cancellationToken.IsCancellationRequested) {
          return;
        }

        try {
          machineModules = GetMachineModules ();
          if (machineModules is null) { // No acquisition server matches
            log.Error ($"CreateImportClasses: no acquisition server matches, return at once");
            return;
          }
        }
        catch (Exception ex) {
          log.Error ("CreateImportClasses: Exception while trying to get the monitored machines => try again in a 1 minute", ex);
          cancellationToken.WaitHandle.WaitOne (TimeSpan.FromMinutes (1));
        }
      }

      Debug.Assert (null != machineModules);

      if (cancellationToken.IsCancellationRequested) {
        return;
      }

      string sleepString = Lemoine.Info.OptionsFile.GetOption (SLEEP);
      string breakFrequencyString = Lemoine.Info.OptionsFile.GetOption (BREAK_FREQUENCY);
      string breakTimeString = Lemoine.Info.OptionsFile.GetOption (BREAK_TIME);
      string fetchDataNumberString =
        Lemoine.Info.OptionsFile.GetOption (FETCH_DATA_NUMBER);
      string whicheverNbOfDataProcessAfterString =
        Lemoine.Info.OptionsFile.GetOption (WHICHEVER_NB_OF_DATA_PROCESS_AFTER);
      string minNbOfDataToProcessString =
        Lemoine.Info.OptionsFile.GetOption (MIN_NB_OF_DATA_TO_PROCESS);
      string visitMachineModesEveryString =
        Lemoine.Info.OptionsFile.GetOption (VISIT_MACHINE_MODES_EVERY);

      int sleepTime = -1;
      int breakFrequency = -1;
      int breakTime = -1;

      int? fetchDataNumber = null;
      TimeSpan? whicheverNbOfDataProcessAfter = null;
      int? minNumberOfDataToProcess = null;
      TimeSpan? visitMachineModesEvery = null;

      if (!string.IsNullOrEmpty (sleepString)) {
        try {
          sleepTime = int.Parse (sleepString);
          log.Info ($"CreateImportClasses: Sleep is {sleepTime}");
        }
        catch (Exception ex) {
          log.Error ($"CreateImportClasses: parsing Sleep {sleepString} failed", ex);
        }
      }

      if (!string.IsNullOrEmpty (breakFrequencyString)) {
        try {
          breakFrequency = int.Parse (breakFrequencyString);
          log.Info ($"CreateImportClasses: BreakFrequency is {breakFrequency}");
        }
        catch (Exception ex) {
          log.Error ($"CreateImportClasses: parsing BreakFrequency {breakFrequencyString} failed", ex);
        }
      }

      if (!string.IsNullOrEmpty (breakTimeString)) {
        try {
          breakTime = int.Parse (breakTimeString);
          log.Info ($"CreateImportClasses: BreakTime is {breakTime}");
        }
        catch (Exception ex) {
          log.Error ($"CreateImportClasses: parsing BreakTime {breakTimeString} failed", ex);
        }
      }

      if (null != fetchDataNumberString) {
        try {
          fetchDataNumber = int.Parse (fetchDataNumberString);
          log.Info ($"CreateImportClasses: FetchDataNumber is {fetchDataNumber}");
          if (fetchDataNumber < 1) {
            log.Error ("CreateImportClasses: FetchDataNumber must be at least 1");
            fetchDataNumber = 1;
          }
        }
        catch (Exception ex) {
          log.Error ($"CreateImportClasses: parsing FetchDataNumber {fetchDataNumberString} failed", ex);
        }
      }

      if (null != whicheverNbOfDataProcessAfterString) {
        try {
          whicheverNbOfDataProcessAfter = TimeSpan.Parse (whicheverNbOfDataProcessAfterString);
          log.Info ($"CreateImportClasses: WhicheverNbOfDataProcessAfter is {whicheverNbOfDataProcessAfterString}");
        }
        catch (Exception ex) {
          log.Error ($"CreateImportClasses: parsing WhicheverNbOfDataProcessAfter {whicheverNbOfDataProcessAfterString} failed", ex);
        }
      }

      if (null != minNbOfDataToProcessString) {
        try {
          minNumberOfDataToProcess = int.Parse (minNbOfDataToProcessString);
          log.Info ($"CreateImportClasses: MinNbOfDataToProcess is {minNumberOfDataToProcess}");
          if (minNumberOfDataToProcess < 1) {
            log.Error ("CreateImportClasses: MinNbOfDataToProcess must be at least 1");
            minNumberOfDataToProcess = 1;
          }
        }
        catch (Exception ex) {
          log.Error ($"CreateImportClasses: parsing MinNbOfDataToProcess {minNbOfDataToProcessString} failed", ex);
        }
      }

      if (null != visitMachineModesEveryString) {
        try {
          visitMachineModesEvery = TimeSpan.Parse (visitMachineModesEveryString);
          log.Info ($"CreateImportClasses: VisitMachineModesEvery is {visitMachineModesEvery}");
        }
        catch (Exception ex) {
          log.Error ($"CreateImportClasses: parsing VisitMachineModesEvery {visitMachineModesEveryString} failed", ex);
        }
      }

      // Create the corresponding threads
      foreach (IMachineModule machineModule in machineModules) {
        var machineImport = new ImportCncDataFromQueue (machineModule);

        if (-1 != sleepTime) {
          machineImport.SleepTime = TimeSpan.FromMilliseconds (sleepTime);
        }
        if (-1 != breakFrequency) {
          machineImport.BreakFrequency = TimeSpan.FromMilliseconds (breakFrequency);
        }
        if (-1 != breakTime) {
          machineImport.BreakTime = TimeSpan.FromMilliseconds (breakTime);
        }
        if (fetchDataNumber.HasValue) {
          machineImport.FetchDataNumber = fetchDataNumber.Value;
        }
        if (whicheverNbOfDataProcessAfter.HasValue) {
          machineImport.WhicheverNbOfDataProcessAfter = whicheverNbOfDataProcessAfter.Value;
        }
        if (minNumberOfDataToProcess.HasValue) {
          machineImport.MinNbOfDataToProcess = minNumberOfDataToProcess.Value;
        }
        if (visitMachineModesEvery.HasValue) {
          machineImport.VisitMachineModesEvery = visitMachineModesEvery.Value;
        }

        if (m_useProcess) {
          m_processImports[machineModule.Id] = new ImportProcessExecution (machineImport);
        }
        else {
          m_threadImports[machineModule.Id] = machineImport;
        }
      }

      log.Info ($"CreateImportClasses: {machineModules.Count} processes/threads (one by machine module)");
    }

    /// <summary>
    /// <see cref="IThreadService"/>
    /// </summary>
    public void Initialize ()
    {
      InitializeThreads (CancellationToken.None);
    }

    void InitializeThreads (CancellationToken cancellationToken)
    {
      using (var linkedCancellationTokenSource = CancellationTokenSource
        .CreateLinkedTokenSource (m_cancellationTokenSource.Token, cancellationToken)) {
        var linkedToken = linkedCancellationTokenSource.Token;

        try {
          m_applicationInitializer.InitializeApplication (linkedToken);
        }
        catch (OperationCanceledException) {
          return;
        }
        catch (Exception ex) {
          log.Error ("InitializeThreads: InitializeApplication failed", ex);
          throw;
        }

        if (linkedToken.IsCancellationRequested) {
          return;
        }

        Lemoine.GDBPersistentClasses.DaySlotCache.Activate ();

        m_useProcess = Lemoine.Info.ConfigSet
          .LoadAndGet<bool> (ConfigKeys.GetCncConfigKey (CncConfigKey.CncDataUseProcess), false);

        m_check.InitializeAdditionalCheckers (linkedToken);

        if (linkedToken.IsCancellationRequested) {
          return;
        }

        Lemoine.Core.ExceptionManagement.ExceptionTest
          .AddTest (new Lemoine.Cnc.SQLiteQueue.SQLiteExceptionTest ());

        CreateImportClasses (linkedToken);
        if (linkedToken.IsCancellationRequested) {
          return;
        }
        if (!m_threadImports.Any () && !m_processImports.Any ()) {
          log.Warn ($"InitializeThreads: there is no import classes");
        }

        // Start the threads and processes
        foreach (ImportCncDataFromQueue import in m_threadImports.Values) {
          if (linkedToken.IsCancellationRequested) {
            return;
          }
          import.Start (linkedToken);
          m_check.AddThread (import);
        }
        foreach (ImportProcessExecution import in m_processImports.Values) {
          if (linkedToken.IsCancellationRequested) {
            return;
          }
          import.Start ();
          m_check.AddProcess (import);
        }

        // - Start the thread that checks the other threads
        if (linkedToken.IsCancellationRequested) {
          return;
        }
        m_check.Start (linkedToken);
      }
    }

    /// <summary>
    /// Use the default OnStart method
    /// </summary>
    public async System.Threading.Tasks.Task InitializeAsync (CancellationToken cancellationToken)
    {
      try {
        await InitializeThreadsAsync (cancellationToken);
      }
      catch (Exception ex) {
        log.Error ($"InitializeAsync: exception", ex);
        throw;
      }
    }

    async System.Threading.Tasks.Task InitializeThreadsAsync (CancellationToken cancellationToken)
    {
      using (var linkedCancellationTokenSource = CancellationTokenSource
        .CreateLinkedTokenSource (m_cancellationTokenSource.Token, cancellationToken)) {
        var linkedToken = linkedCancellationTokenSource.Token;

        try {
          await m_applicationInitializer.InitializeApplicationAsync (linkedToken);
        }
        catch (OperationCanceledException) {
          return;
        }
        catch (Exception ex) {
          log.Error ("InitializeThreadsAsync: InitializeApplication failed", ex);
          throw;
        }

        Lemoine.GDBPersistentClasses.DaySlotCache.Activate ();

        m_useProcess = Lemoine.Info.ConfigSet
          .LoadAndGet<bool> (ConfigKeys.GetCncConfigKey (CncConfigKey.CncDataUseProcess), false);

        m_check.InitializeAdditionalCheckers (linkedToken);

        if (linkedToken.IsCancellationRequested) {
          return;
        }

        Lemoine.Core.ExceptionManagement.ExceptionTest
          .AddTest (new Lemoine.Cnc.SQLiteQueue.SQLiteExceptionTest ());

        CreateImportClasses (linkedToken);
        if (linkedToken.IsCancellationRequested) {
          return;
        }
        if (!m_threadImports.Any () && !m_processImports.Any ()) {
          log.Warn ($"InitializeThreadsAsync: there is no import classes");
        }

        // Start the threads and processes
        foreach (ImportCncDataFromQueue import in m_threadImports.Values) {
          if (linkedToken.IsCancellationRequested) {
            return;
          }
          import.Start (linkedToken);
          m_check.AddThread (import);
        }
        foreach (ImportProcessExecution import in m_processImports.Values) {
          if (linkedToken.IsCancellationRequested) {
            return;
          }
          import.Start ();
          m_check.AddProcess (import);
        }

        // - Start the thread that checks the other threads
        if (linkedToken.IsCancellationRequested) {
          return;
        }
        m_check.Start (linkedToken);
      }
    }

    /// <summary>
    /// Stop this service.
    /// </summary>
    public void OnStop ()
    {
      LogManager.SetApplicationStopping ();

      m_cancellationTokenSource?.Cancel ();

      // - Stop the checking thread
      m_check?.Abort ();

      // - Try to stop first the imports
      foreach (ImportCncDataFromQueue import in m_threadImports.Values) {
        import.Stop ();
      }

      // - Stop the processes
      foreach (ImportProcessExecution import in m_processImports.Values) {
        import.Abort ();
      }
    }

    IComputer GetLPost ()
    {
      IEnumerable<IComputer> lposts;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Lem_CncDataService.GetLPost")) {
          lposts = ModelDAOHelper.DAOFactory.ComputerDAO.GetLposts ();
        }
      }

      var lpost = lposts.FirstOrDefault (x => x.IsLocal ());
      if (null == lpost) {
        log.Error ("GetLPost: no lpost in database corresponds to this computer");
      }
      return lpost;
    }

    /// <summary>
    /// Get the list of the monitored machine modules that corresponds to this LPost
    /// 
    /// Return null if no acquisition server corresponds to this computer
    /// </summary>
    /// <returns></returns>
    ICollection<IMachineModule> GetMachineModules ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("CncDataService.GetMachineModules")) {
          var lpost = GetLPost ();
          if (null == lpost) {
            log.Error ("GetMachineModules: no acquisition server in database corresponds to this computer");
            return null;
          }

          // 3. Get all the CncAcquisition for this lpost
          if (log.IsDebugEnabled) {
            log.DebugFormat ("GetMachineModules: get the cnc acquisitions for lpost {0}", lpost.Id);
          }
          IList<ICncAcquisition> cncAcquisitions =
            ModelDAOHelper.DAOFactory.CncAcquisitionDAO
            .FindAllForComputer (lpost);
          if (log.IsDebugEnabled) {
            log.DebugFormat ("GetMachineModules: got {0} cnc acquisitions for lpost {1}", cncAcquisitions.Count, lpost.Id);
          }

          // 4. Get all the monitored machines
          ICollection<IMachineModule> machineModules =
            new HashSet<IMachineModule> ();
          foreach (ICncAcquisition cncAcquisition in cncAcquisitions) {
            foreach (IMachineModule machineModule in cncAcquisition.MachineModules) {
              machineModules.Add (machineModule);
            }
          }

          if (log.IsDebugEnabled) {
            log.Debug ($"GetMachineModules: got {machineModules.Count} machine modules");
          }
          return machineModules;
        }
      }
    }
    #endregion // Methods

    #region IDisposable implementation
    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    ~Lem_CncDataService () => Dispose (false);

    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    public void Dispose ()
    {
      Dispose (true);
      GC.SuppressFinalize (this);
    }

    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    /// <param name="disposing"></param>
    void Dispose (bool disposing)
    {
      if (m_disposed) {
        return;
      }

      if (disposing) {
        m_check?.Dispose ();
        m_cancellationTokenSource?.Dispose ();
      }

      m_disposed = true;
    }
    #endregion // IDisposable implementation  }  }
  }
}

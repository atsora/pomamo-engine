// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.ExceptionManagement;
using Lemoine.Extensions.Analysis;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Analysis.Detection;
using System.Threading;

namespace Lemoine.Analysis
{
  /// <summary>
  /// Detection analysis by monitored machine
  /// </summary>
  public sealed class DetectionAnalysis : ISingleAnalysis, Lemoine.Threading.IChecked
  {
    /// <summary>
    /// Maximum number of detections that are processed in the same time
    /// </summary>
    static readonly string MAX_NUMBER_OF_DETECTIONS_KEY = "Analysis.Activity.Detections.MaxNumber";
    static readonly int MAX_NUMBER_OF_DETECTIONS_DEFAULT = 300;

    /// <summary>
    /// Number of attempt to complete a transaction in case of a serialization failure
    /// </summary>
    static readonly string NB_ATTEMPT_SERIALIZATION_FAILURE_KEY = "Analysis.Activity.NbAttemptSerializationFailure";
    static readonly int NB_ATTEMPT_SERIALIZATION_FAILURE_DEFAULT = 2;

    #region Members
    readonly IMonitoredMachine m_monitoredMachine;
    readonly TransactionLevel m_restrictedTransactionLevel;
    readonly Lemoine.Threading.IChecked m_caller;
    readonly MonitoredMachineActivityAnalysis m_machineActivityAnalysis;
    readonly OperationDetection m_operationDetection;
    readonly OperationCycleDetection m_operationCycleDetection;
    readonly IDictionary<int, SequenceDetection> m_sequenceDetections = new Dictionary<int, SequenceDetection> ();
    readonly IDictionary<int, SequenceMilestoneDetection> m_sequenceMilestoneDetections = new Dictionary<int, SequenceMilestoneDetection> ();

    IEnumerable<Lemoine.Extensions.Analysis.IDetectionAnalysisExtension> m_extensions;
    readonly IDictionary<int, IEnumerable<IDetectionAnalysisByMachineModuleExtension>> m_extensionsByMachineModule =
      new Dictionary<int, IEnumerable<IDetectionAnalysisByMachineModuleExtension>> ();

    IEnumerable<Lemoine.Extensions.Analysis.IDetectionExtension> m_detectionExtensions;
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (DetectionAnalysis).FullName);

    #region Getters / Setters
    /// <summary>
    /// Reference to OperationDetection
    /// </summary>
    internal OperationDetection OperationDetection
    {
      get { return m_operationDetection; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="machineActivityAnalysis">Not null (including its associated machine)</param>
    /// <param name="extensions"></param>
    public DetectionAnalysis (MonitoredMachineActivityAnalysis machineActivityAnalysis,
                              IEnumerable<Lemoine.Extensions.Analysis.IDetectionExtension> extensions)
    {
      Debug.Assert (null != machineActivityAnalysis);
      Debug.Assert (null != machineActivityAnalysis.MonitoredMachine);

      m_monitoredMachine = machineActivityAnalysis.MonitoredMachine;
      m_detectionExtensions = extensions;
      m_restrictedTransactionLevel = machineActivityAnalysis.RestrictedTransactionLevel;
      m_caller = machineActivityAnalysis;
      m_machineActivityAnalysis = machineActivityAnalysis;

      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                machineActivityAnalysis.MonitoredMachine.Id));

      m_operationDetection = new OperationDetection (machineActivityAnalysis.MonitoredMachine,
                                                     m_detectionExtensions.OfType<Lemoine.Extensions.Analysis.IOperationDetectionExtension> (), this);
      m_operationCycleDetection =
        new OperationCycleDetection (machineActivityAnalysis.MonitoredMachine,
                                     m_detectionExtensions, this);

      log.DebugFormat ("DetectionAnalysis: constructor completed");
    }
    #endregion // Constructors

    #region IChecked implementation
    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void SetActive ()
    {
      if (null != m_caller) {
        m_caller.SetActive ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void PauseCheck ()
    {
      if (null != m_caller) {
        m_caller.PauseCheck ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void ResumeCheck ()
    {
      if (null != m_caller) {
        m_caller.ResumeCheck ();
      }
    }
    #endregion // IChecked implementation

    #region ManageDetections part
    /// <summary>
    /// ISingleAnalysis implementation
    /// </summary>
    public void Initialize ()
    {
      m_extensions = Lemoine.Extensions.ExtensionManager
        .GetExtensions<IDetectionAnalysisExtension> (checkedThread: this)
        .Where (i => i.Initialize (m_monitoredMachine, m_operationDetection, m_operationCycleDetection))
        .ToList (); // ToList is mandatory else the result of the Linq command is not cached
      foreach (var extension in m_extensions) {
        extension.RestrictedTransactionLevel = this.m_restrictedTransactionLevel;
      }
      log.DebugFormat ("Initialize: {0} extensions are loaded for machine {1}",
        m_extensions.Count (), m_monitoredMachine.Id);
    }

    /// <summary>
    /// <see cref="ISingleAnalysis"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxDetectionAnalysisDateTime"></param>
    /// <param name="minTimePerMachineModule"></param>
    /// <param name="numberOfItems">number of items to process if applicable. If null, a configuration key is considered instead</param>
    public bool RunOnce (CancellationToken cancellationToken, DateTime maxDetectionAnalysisDateTime, TimeSpan minTimePerMachineModule, int? numberOfItems = null)
    {
      var processValidityChecker = new ProcessValidityChecker (m_machineActivityAnalysis, maxDetectionAnalysisDateTime, minTimePerMachineModule, log);

      if (!RunAll (cancellationToken, processValidityChecker, numberOfItems)) {
        log.Info ($"RunOnce: detection analysis was interrupted");
      }
      else if (log.IsDebugEnabled) {
        log.Debug ($"RunOnce: RunAll completed successfully");
      }

      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="processValidityChecker"></param>
    /// <param name="numberOfItems">If applicable, number of items to consider. If null, consider a configuration key</param>
    /// <returns>false if the detection analysis must be interrupted</returns>
    bool RunAll (CancellationToken cancellationToken, ProcessValidityChecker processValidityChecker, int? numberOfItems = null)
    {
      // Do not start a session here because the session may be closed
      // in case a transaction is replayed after a serialization failure

      try {
        TriggerDetectionProcessStart ();

        foreach (IMachineModule machineModule in m_monitoredMachine.MachineModules) {
          SetActive ();

          processValidityChecker.ResetStartDateTime (); // To give enough time to each machine module

          if (!processValidityChecker.IsValid ()) {
            log.Info ("RunAll: requested by processValidityChecker to interrupd the detection analysis");
            return false;
          }

          if (cancellationToken.IsCancellationRequested) {
            log.Info ($"RunAll: cancellation requested");
            return false;
          }

          var runMachineModuleResult = RunMachineModule (cancellationToken, processValidityChecker, machineModule, numberOfItems);
          if (!runMachineModuleResult) {
            log.Info ($"RunAll: RunMachineModule returned {runMachineModuleResult} => interrupt the detection analysis");
            return false;
          }
        }
      }
      finally {
        TriggerDetectionProcessComplete ();
      }

      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="processValidityChecker"></param>
    /// <param name="machineModule">not null</param>
    /// <param name="numberOfItems"></param>
    /// <returns>false if the detection analysis must be interrupted</returns>
    bool RunMachineModule (CancellationToken cancellationToken, ProcessValidityChecker processValidityChecker, IMachineModule machineModule, int? numberOfItems = null)
    {
      Debug.Assert (null != machineModule);

      if (log.IsDebugEnabled) {
        log.Debug ($"RunMachineModule: machineModule={machineModule.Id}");
      }

      IList<IMachineModuleDetection> machineModuleDetections;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IMachineModuleAnalysisStatus analysisStatus;
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.Detections.MachineModuleInitialization",
                                                                               TransactionLevel.ReadCommitted)) {
          analysisStatus = ModelDAOHelper.DAOFactory.MachineModuleAnalysisStatusDAO
            .FindById (machineModule.Id);
        } // auto commit because read-only

        // - Check if there are some new detections to process
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.Detections.Find")) {
          int maxNumberOfDetections = numberOfItems ??
            ConfigSet.LoadAndGet<int> (MAX_NUMBER_OF_DETECTIONS_KEY, MAX_NUMBER_OF_DETECTIONS_DEFAULT);
          machineModuleDetections = ModelDAOHelper.DAOFactory.MachineModuleDetectionDAO
            .FindAfter (machineModule,
                        analysisStatus.LastMachineModuleDetectionId,
                        maxNumberOfDetections);
        } // auto commit because read-only
      }
      if (0 == machineModuleDetections.Count) {
        if (log.IsDebugEnabled) {
          log.Debug ($"RunMachineModule: no machine module detections for machine module Id={machineModule.Id}");
        }
        return true;
      }
      else { // There are some machine module detections to process

        int nbAttemptSerializationFailure = ConfigSet.LoadAndGet<int> (NB_ATTEMPT_SERIALIZATION_FAILURE_KEY,
                                                                       NB_ATTEMPT_SERIALIZATION_FAILURE_DEFAULT);

        // - process them one after the other
        foreach (IMachineModuleDetection machineModuleDetection in machineModuleDetections) {
          SetActive ();

          if (log.IsDebugEnabled) {
            log.Debug ($"RunMachineModule: machine module detection id={machineModuleDetection.Id}");
          }

          // - Try not to spend too much time in this method
          if (!processValidityChecker.IsValid ()) {
            log.Info ("RunMachineModule: requested by processValidityChecker to interrupt the detection analysis");
            return false;
          }

          if (cancellationToken.IsCancellationRequested) {
            log.Info ($"RunMachineModule: cancellation requested => return false");
            return false;
          }

          var runDetectionResult = RunDetection (machineModule, machineModuleDetection, cancellationToken, processValidityChecker, nbAttemptSerializationFailure, out bool maxAttemptReached);
          if (!runDetectionResult) {
            log.Info ($"RunMachineModule: RunOnDetection returned {runDetectionResult} => interrupt the detection analysis");
            return false;
          }
          if (maxAttemptReached) {
            log.Warn ($"RunMachineModule: max number of attempts reached, try the next machine module");
            return true;
          }
        }

        if (log.IsDebugEnabled) {
          log.Debug ($"RunMachineModule: all retrieved machine module detections were processed");
        }
      }
      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="machineModuleDetection">not null</param>
    /// <param name="processValidityChecker"></param>
    /// <param name="nbAttemptSerializationFailure"></param>
    /// <returns>false if the detection analysis must be interrupted</returns>
    bool RunDetection (IMachineModule machineModule, IMachineModuleDetection machineModuleDetection, CancellationToken cancellationToken, ProcessValidityChecker processValidityChecker, int nbAttemptSerializationFailure, out bool maxAttemptReached)
    {
      Debug.Assert (null != machineModuleDetection);
      Debug.Assert (0 < nbAttemptSerializationFailure);

      maxAttemptReached = false;

      int attempt;
      for (attempt = 0; attempt < nbAttemptSerializationFailure; ++attempt) {
        SetActive ();

        if (log.IsDebugEnabled) {
          log.Debug ($"RunDetection: machine module detection id={machineModuleDetection.Id} attempt={attempt}");
        }

        // - Try not to spend too much time in this method
        if (!processValidityChecker.IsValid ()) {
          log.Info ("RunDetection: requested to return at once by processValidityChecker => return false");
          return false;
        }

        if (cancellationToken.IsCancellationRequested) {
          log.Info ($"RunDetection: cancellation requested => return false");
          return false;
        }

        try {
          TryRunOnDetection (machineModule, machineModuleDetection, attempt);
          if (log.IsDebugEnabled) {
            log.Debug ($"RunDetection: detectio id={machineModuleDetection.Id} completed successfully");
          }
          return true;
        }
        catch (OperationCanceledException) {
          throw;
        }
        catch (Lemoine.Threading.AbortException) {
          throw;
        }
        catch (Exception ex) {
          TriggerDetectionProcessError (machineModule, ex);
          if (ExceptionTest.IsStale (ex)) {
            log.Warn ($"RunDetection: stale object state exception with attempt {attempt} => try again", ex);
          }
          else if (ExceptionTest.IsTemporary (ex)) {
            log.Warn ($"RunDetection: temporary failure (serialization) with attempt {attempt} => try again", ex);
          }
          else {
            log.Exception (ex, "RunDetection");
            throw;
          }
        }
      }

      log.Error ($"RunDetection: interrupt the analysis, too many serialization failures, {attempt} attempts");
      maxAttemptReached = true;
      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="machineModuleDetection"></param>
    /// <param name="processValidityChecker"></param>
    /// <param name="attempt"></param>
    void TryRunOnDetection (IMachineModule machineModule, IMachineModuleDetection machineModuleDetection, int attempt)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"TryRunOnDetection: machine module detection id={machineModuleDetection.Id} attempt={attempt}");
      }

      var useUniqueSerializableTransaction = m_extensions.Any (i => i.UseUniqueSerializableTransaction (machineModuleDetection)) || GetExtensionsByMachineModule (machineModule).Any (i => i.UseUniqueSerializableTransaction (machineModuleDetection));
      if (log.IsDebugEnabled) {
        log.Debug ($"TryRunOnDetection: useUniqueSerializableTransaction={useUniqueSerializableTransaction} for machine module detection id {machineModuleDetection.Id}");
      }
      if (useUniqueSerializableTransaction) {
        // To be used when the process must be done in one step in one unique serialization transaction

        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginTransaction ("Analysis.Detection.UniqueSerializableTransaction",
                                                             m_restrictedTransactionLevel)) {
            transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data

            try {
              ProcessDetection (machineModule, machineModuleDetection);
            }
            catch (Exception ex) {
              log.Error ("TryRunOnDetection: exception in ProcessDetection", ex);
              throw;
            }

            IMachineModuleAnalysisStatus analysisStatus = ModelDAOHelper.DAOFactory.MachineModuleAnalysisStatusDAO
              .FindById (machineModule.Id);
            analysisStatus.LastMachineModuleDetectionId = machineModuleDetection.Id;
            ModelDAOHelper.DAOFactory.MachineModuleAnalysisStatusDAO
              .MakePersistent (analysisStatus);

            if (log.IsDebugEnabled) {
              log.Debug ($"TryRunOnDetection: machine module detection id={machineModuleDetection.Id} completed in a unique transaction");
            }

            transaction.Commit ();
            return;
          }
        }
      }
      else {
        // When it is ok to process a machine module detection twice,
        // do this code in two transactions

        // - Process it
        if (log.IsDebugEnabled) {
          log.Debug ($"TryRunOnDetection: process machine module detection id={machineModuleDetection.Id}");
        }
        ProcessDetection (machineModule, machineModuleDetection);
        if (log.IsDebugEnabled) {
          log.Debug ($"TryRunOnDetection: machine module detection id={machineModuleDetection.Id} completed in the 2-step process (without record)");
        }

        // - This is completed
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session
            .BeginTransaction ("Analysis.MachineModuleAnalysisStatusUpdate",
                               TransactionLevel.ReadCommitted)) { // ReadCommitted is sufficient here because this is just an update of MachineModuleAnalysisStatus
            transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data

            IMachineModuleAnalysisStatus analysisStatus = ModelDAOHelper.DAOFactory.MachineModuleAnalysisStatusDAO
              .FindById (machineModule.Id);
            analysisStatus.LastMachineModuleDetectionId = machineModuleDetection.Id;
            ModelDAOHelper.DAOFactory.MachineModuleAnalysisStatusDAO
              .MakePersistent (analysisStatus);

            transaction.Commit ();
          }
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"TryRunOnDetection: record machine module detection id={machineModuleDetection.Id} was completed in the 2-step process");
        }
        return;
      }
    }

    void ProcessDetection (IMachineModule machineModule, IMachineModuleDetection detection)
    {
      // Note: ProcessDetection is not run in a transaction
      // because we suppose this is ok to process a machine module detection twice
      Debug.Assert (null != detection);

      foreach (var extension in GetExtensionsByMachineModule (machineModule)) {
        var startDateTime = DateTime.UtcNow;
        extension.ProcessDetection (detection);
        log.DebugFormat ("ProcessDetection: process detection with extension {0} on detection id {1} completed in {2}",
          extension, detection.Id, DateTime.UtcNow.Subtract (startDateTime));
      }
      foreach (var extension in m_extensions) {
        var startDateTime = DateTime.UtcNow;
        extension.ProcessDetection (detection);
        log.DebugFormat ("ProcessDetection: process detection with extension {0} on detection id {1} completed in {2}",
          extension, detection.Id, DateTime.UtcNow.Subtract (startDateTime));
      }
    }

    void TriggerDetectionProcessStart ()
    {
      m_operationCycleDetection.DetectionProcessStart ();
    }

    void TriggerDetectionProcessComplete ()
    {
      m_operationCycleDetection.DetectionProcessComplete ();
    }

    void TriggerDetectionProcessError (IMachineModule machineModule, Exception ex)
    {
      m_operationCycleDetection.DetectionProcessError (machineModule, ex);
    }

    IEnumerable<IDetectionAnalysisByMachineModuleExtension> GetExtensionsByMachineModule (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);

      IEnumerable<IDetectionAnalysisByMachineModuleExtension> extensions;
      if (!m_extensionsByMachineModule.TryGetValue (machineModule.Id, out extensions)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetExtensionsByMachineModule: get the extensions by machine module for machinemoduleid={machineModule.Id} (cache not set)");
        }
        SequenceDetection sequenceDetection = GetSequenceDetection (machineModule);
        var sequenceMilestoneDetection = GetSequenceMilestoneDetection (machineModule);
        extensions = Lemoine.Extensions.ExtensionManager
         .GetExtensions<IDetectionAnalysisByMachineModuleExtension> (null)
         .Where (i => i.Initialize (machineModule, m_operationDetection, m_operationCycleDetection, sequenceDetection, sequenceMilestoneDetection))
         .ToList (); // ToList is mandatory else the result of the Linq command is not cached
        foreach (var extension in extensions) {
          extension.RestrictedTransactionLevel = m_restrictedTransactionLevel;
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"GetExtensionsByMachineModule: {extensions.Count ()} extensions initialized for machine module {machineModule.Id}");
        }
        m_extensionsByMachineModule[machineModule.Id] = extensions;
      }
      Debug.Assert (null != extensions);
      return extensions;
    }

    SequenceDetection GetSequenceDetection (IMachineModule machineModule)
    {
      SequenceDetection sequenceDetection;
      if (!m_sequenceDetections.TryGetValue (machineModule.Id, out sequenceDetection)) {
        var sequenceMilestoneDetection = GetSequenceMilestoneDetection (machineModule);
        sequenceDetection = new SequenceDetection (m_operationDetection, sequenceMilestoneDetection, machineModule, this);
        sequenceDetection.RestrictedTransactionLevel = m_machineActivityAnalysis.RestrictedTransactionLevel;
      }
      return sequenceDetection;
    }

    SequenceMilestoneDetection GetSequenceMilestoneDetection (IMachineModule machineModule)
    {
      if (!m_sequenceMilestoneDetections.TryGetValue (machineModule.Id, out var sequenceMilestoneDetection)) {
        sequenceMilestoneDetection = new SequenceMilestoneDetection (machineModule, this);
        sequenceMilestoneDetection.RestrictedTransactionLevel = m_machineActivityAnalysis.RestrictedTransactionLevel;
      }
      return sequenceMilestoneDetection;
    }
    #endregion // ManageDetections part
  }
}

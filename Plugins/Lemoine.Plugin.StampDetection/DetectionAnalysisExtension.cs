// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Analysis;
using System;
using System.Collections.Generic;
using Lemoine.Extensions.Analysis.Detection;
using Lemoine.Model;
using System.Diagnostics;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Core.ExceptionManagement;
using Pulse.Extensions.Database;
using Pulse.PluginImplementation.CycleDetectionStatus;
using Pulse.PluginImplementation.OperationDetectionStatus;

namespace Lemoine.Plugin.StampDetection
{
  public class DetectionAnalysisExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IOperationDetectionStatusExtension
    , ICycleDetectionStatusExtension
    , IDetectionAnalysisByMachineModuleExtension
  {
    ILog log = LogManager.GetLogger (typeof (DetectionAnalysisExtension).FullName);

    IMachineModule m_machineModule;
    IOperationDetection m_operationDetection;
    IOperationCycleDetection m_operationCycleDetection;
    ISequenceDetection m_sequenceDetection;
    ISequenceMilestoneDetection m_sequenceMilestoneDetection;
    readonly IDictionary<int, bool> m_coherentCache = new Dictionary<int, bool> ();
    Configuration m_configuration = null;

    ICycleDetectionStatusExtension m_cycleDetectionStatus;
    IOperationDetectionStatusExtension m_operationDetectionStatus;

    public TransactionLevel RestrictedTransactionLevel { get; set; }

    public int OperationDetectionStatusPriority
    {
      get {
        return m_operationDetectionStatus.OperationDetectionStatusPriority;
      }
    }

    public int CycleDetectionStatusPriority
    {
      get {
        return m_cycleDetectionStatus.CycleDetectionStatusPriority;
      }
    }

    public bool Initialize (IMachineModule machineModule, IOperationDetection operationDetection, IOperationCycleDetection operationCycleDetection, ISequenceDetection sequenceDetection, ISequenceMilestoneDetection sequenceMilestoneDetection)
    {
      m_machineModule = machineModule;
      m_operationDetection = operationDetection;
      m_operationCycleDetection = operationCycleDetection;
      m_sequenceDetection = sequenceDetection;
      m_sequenceMilestoneDetection = sequenceMilestoneDetection;

      if (!Initialize (machineModule.MonitoredMachine)) {
        return false;
      }

      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{machineModule.MonitoredMachine.Id}.{machineModule.Id}");

      return true;
    }

    public bool Initialize (IMachine machine)
    {
      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{machine.Id}");

      Configuration configuration;
      if (!LoadConfiguration (out configuration)) {
        log.Error ("Initialize: wrong configuration, skip this instance");
        return false;
      }

      if (!CheckMachineFilter (configuration, machine)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Initialize: machine {machine.Id} does not match machine filter => return false (skip this extension)");
        }
        return false;
      }

      m_cycleDetectionStatus = new CycleDetectionStatusFromAnalysisStatus (configuration.CycleDetectionStatusPriority);
      if (!m_cycleDetectionStatus.Initialize (machine)) {
        log.Error ("Initialize: initialization of cycle detection status failed");
        return false;
      }

      m_operationDetectionStatus = new OperationDetectionStatusFromAnalysisStatus (configuration.OperationDetectionStatusPriority);
      if (!m_operationDetectionStatus.Initialize (machine)) {
        log.Error ("Initialize: initialization of operation detection status failed");
        return false;
      }

      m_configuration = configuration;

      return true;
    }

    bool CheckMachineFilter (Configuration configuration, IMachine machine)
    {
      IMachineFilter machineFilter = null;
      if (0 == configuration.MachineFilterId) { // Machine filter
        return true;
      }
      else { // Machine filter
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("StampDetection.InitializeConfiguration.MachineFilter")) {
            int machineFilterId = configuration.MachineFilterId;
            machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
              .FindById (machineFilterId);
            if (null == machineFilter) {
              log.Error ($"CheckMachineFilter: machine filter id {machineFilterId} does not exist");
              return false;
            }
            else {
              return machineFilter.IsMatch (machine);
            }
          }
        }
      }
    }

    public void ProcessDetection (IMachineModuleDetection detection)
    {
      // Note: ProcessDetection is not run in a transaction
      // because we suppose this is ok to process a machine module detection twice
      Debug.Assert (null != detection);

      ISequence sequence = null;
      if (null != detection.Stamp) {
        IStamp stamp = detection.Stamp;
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          // Re-associate first the stamp to a session
          // Note: StampDAO.Load (stamp) does not work correctly with some delayed initialization
          stamp = ModelDAOHelper.DAOFactory.StampDAO.Reload (stamp);
          sequence = stamp.Sequence;
          StartStamp (stamp, detection.DateTime);
        }
      }
      if (m_configuration.IncludeMilestone && detection.SequenceMilestone.HasValue) {
        SetSequenceMilestone (detection.DateTime, detection.SequenceMilestone.Value, sequence);
      }
      if (detection.StopNcProgram) {
        StopIsoFile (detection.DateTime);
      }
    }

    /// <summary>
    /// Check the coherence of the data in a stamp.
    /// 
    /// In case of incoherence, an analysis log is added
    /// </summary>
    /// <param name="stamp">not null</param>
    /// <returns></returns>
    bool CheckStampCoherence (IStamp stamp)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        Debug.Assert (null != stamp);

        if (m_coherentCache.ContainsKey (((Lemoine.Collections.IDataWithId)stamp).Id)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"CheckStampCoherence: return coherence from cache for stamp {stamp}: {m_coherentCache[((Lemoine.Collections.IDataWithId)stamp).Id]}");
          }
          return m_coherentCache[((Lemoine.Collections.IDataWithId)stamp).Id];
        }
        m_coherentCache[((Lemoine.Collections.IDataWithId)stamp).Id] = false;

        IModelFactory modelFactory =
          ModelDAOHelper.ModelFactory;
        IDAOFactory daoFactory =
          ModelDAOHelper.DAOFactory;

        if (stamp.OperationCycleBegin && stamp.OperationCycleEnd) { // They should not be simultaneous => raise a warning
          log.Warn ($"CheckStampCoherence: the stamp {stamp} triggers in the same time an operation cycle begin and an operation cycle end");
          using (IDAOTransaction transaction = session.BeginTransaction ("Detection.LogBadCoherenceBeginEnd",
                                                                         TransactionLevel.ReadCommitted)) {
            transaction.SynchronousCommitOption = SynchronousCommit.Off;

            IDetectionAnalysisLog detectionAnalysisLog =
              modelFactory
              .CreateDetectionAnalysisLog (LogLevel.WARN,
                                           $"Stamp {stamp} is both an operation cycle begin and end",
                                           m_machineModule.MonitoredMachine,
                                           m_machineModule);
            daoFactory.DetectionAnalysisLogDAO
              .MakePersistent (detectionAnalysisLog);

            transaction.Commit ();
          }
          // But continue, this is only a warning
        }

        if (null != stamp.Sequence) {
          if (stamp.IsoFileEnd) {
            log.Error ("CheckStampCoherence: Sequence and IsoFileEnd in the same time");
            using (IDAOTransaction transaction = session.BeginTransaction ("Detection.LogBadCoherenceSequenceIsoFileEnd",
                                                                           TransactionLevel.ReadCommitted)) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off;

              IDetectionAnalysisLog detectionAnalysisLog =
                modelFactory
                .CreateDetectionAnalysisLog (LogLevel.ERROR,
                                             $"Sequence {stamp.Sequence} and IsoFileEnd in the same time",
                                             m_machineModule.MonitoredMachine,
                                             m_machineModule);
              daoFactory.DetectionAnalysisLogDAO
                .MakePersistent (detectionAnalysisLog);

              transaction.Commit ();
            }
            return false;
          }
          if ((null != stamp.Operation)
              && (!object.Equals (stamp.Sequence.Operation,
                                  stamp.Operation))) {
            log.Error ("CheckStampCoherence: Sequence and Operation are not compatible ");
            using (IDAOTransaction transaction = session.BeginTransaction ("Detection.LogBadCoherenceSequenceOperation",
                                                                           TransactionLevel.ReadCommitted)) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off;

              IDetectionAnalysisLog detectionAnalysisLog =
                modelFactory
                .CreateDetectionAnalysisLog (LogLevel.ERROR,
                                             $"Sequence {stamp.Sequence} and Operation {stamp.Operation} not compatible",
                                             m_machineModule.MonitoredMachine,
                                             m_machineModule);
              daoFactory.DetectionAnalysisLogDAO
                .MakePersistent (detectionAnalysisLog);

              transaction.Commit ();
            }
            return false;
          }
          if (null != stamp.Component) {
            bool coherent = false;
            foreach (IIntermediateWorkPiece intermediateWorkPiece in
                     stamp.Sequence.Operation.IntermediateWorkPieces) {
              foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in stamp.Component.ComponentIntermediateWorkPieces) {
                if (componentIntermediateWorkPiece.IntermediateWorkPiece.Equals (intermediateWorkPiece)) {
                  coherent = true;
                  break;
                }
              }
              if (coherent) {
                break;
              }
            }
            if (!coherent) {
              log.Error ("CheckStampCoherence: Sequence and Component are not compatible ");
              using (IDAOTransaction transaction = session.BeginTransaction ("Detection.LogBadCoherenceSequenceComponent",
                                                                             TransactionLevel.ReadCommitted)) {
                transaction.SynchronousCommitOption = SynchronousCommit.Off;

                IDetectionAnalysisLog detectionAnalysisLog =
                  modelFactory
                  .CreateDetectionAnalysisLog (LogLevel.ERROR,
                                               $"Sequence {stamp.Sequence} and Component {stamp.Component} not compatible",
                                               m_machineModule.MonitoredMachine,
                                               m_machineModule);
                daoFactory.DetectionAnalysisLogDAO
                  .MakePersistent (detectionAnalysisLog);

                transaction.Commit ();
              }
              return false;
            }
          }
        }
        else if (null != stamp.Operation) {
          if (null != stamp.Component) {
            bool coherent = false;
            foreach (IIntermediateWorkPiece intermediateWorkPiece in
                     stamp.Operation.IntermediateWorkPieces) {
              foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in stamp.Component.ComponentIntermediateWorkPieces) {
                if (componentIntermediateWorkPiece.IntermediateWorkPiece.Equals (intermediateWorkPiece)) {
                  coherent = true;
                  break;
                }
              }
              if (coherent) {
                break;
              }
            }
            if (!coherent) {
              log.Error ("CheckStampCoherence: Operation and Component are not compatible ");
              using (IDAOTransaction transaction = session.BeginTransaction ("Detection.LogBadCoherenceOperationComponent",
                                                                             TransactionLevel.ReadCommitted)) {
                transaction.SynchronousCommitOption = SynchronousCommit.Off;

                IDetectionAnalysisLog detectionAnalysisLog =
                  modelFactory
                  .CreateDetectionAnalysisLog (LogLevel.ERROR,
                                               $"Operation {stamp.Operation} and Component {stamp.Component} not compatible",
                                               m_machineModule.MonitoredMachine,
                                               m_machineModule);
                daoFactory.DetectionAnalysisLogDAO
                  .MakePersistent (detectionAnalysisLog);

                transaction.Commit ();
              }
              return false;
            }
          }
        }

        m_coherentCache[((Lemoine.Collections.IDataWithId)stamp).Id] = true;
        return true;
      }
    }

    /// <summary>
    /// Make all the changes in database to start a stamp
    /// </summary>
    /// <param name="stamp"></param>
    /// <param name="dateTime"></param>
    /// <exception cref="System.Exception">if the data is not coherent</exception>
    /// <exception cref="T:System.NotImplementedException">in case stamp is associated to a component or an operation without a cycle information</exception>
    public void StartStamp (IStamp stamp,
                            DateTime dateTime)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        try {
          // - Reload m_machineModule for this session
          m_machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO
            .FindById (m_machineModule.Id);

          // - Check coherence of the data
          if (!CheckStampCoherence (stamp)) {
            log.Error ($"StartStamp: stamp {((Lemoine.Collections.IDataWithId)stamp).Id} is not coherent");
            throw new Exception ("Stamp not coherent");
          }

          // - Make the different analysis
          //   given the properties of the stamp
          IIsoFileMachineModuleAssociation isoFileAssociation = ModelDAOHelper.ModelFactory
            .CreateIsoFileMachineModuleAssociation (m_machineModule, new UtcDateTimeRange (dateTime));
          if (stamp.IsoFileEnd) {
            if (log.IsDebugEnabled) {
              log.Debug ($"StartStamp: stamp {((Lemoine.Collections.IDataWithId)stamp).Id} is a IsoFileEnd");
            }
            StopIsoFile (dateTime);

            // Process Modification MachineModule => No iso file from date time
            isoFileAssociation.IsoFile = null;
          }
          else {
            // Process Modification MachineModule => new iso file from date time
            isoFileAssociation.IsoFile = stamp.IsoFile;
          }
          using (IDAOTransaction transaction = session.BeginTransaction ("Detection.Master.StartStampIsoFileAssociation",
                                                                         RestrictedTransactionLevel)) {
            transaction.SynchronousCommitOption = SynchronousCommit.Off;
            isoFileAssociation.Apply ();
            transaction.Commit ();
          }

          // Extend the operation slot in case of operation cycle begin / end
          if (stamp.OperationCycleBegin) {
            IOperation operation = null;
            if (null != stamp.Operation) {
              operation = stamp.Operation;
            }
            else if (null != stamp.Sequence) {
              Debug.Assert (null != stamp.Sequence.Operation);
              operation = stamp.Sequence.Operation;
            }
            if (null != operation) {
              if (log.IsDebugEnabled) {
                log.Debug ($"StartStamp: stamp {((Lemoine.Collections.IDataWithId)stamp).Id} with a cycle begin and an operation {operation} => begin the operation right now");
              }
              using (IDAOTransaction transaction = session.BeginTransaction ("Detection.Master.StartStampOperation",
                                                                             RestrictedTransactionLevel)) {
                transaction.SynchronousCommitOption = SynchronousCommit.Off;
                m_operationDetection.StartOperation (operation,
                                                     dateTime);
                transaction.Commit ();
              }
              m_sequenceDetection.StartAutoOnlyOperation (operation,
                                                          dateTime);
            }
          }
          if (stamp.OperationCycleEnd) {
            if (log.IsDebugEnabled) {
              log.Debug ($"StartStamp: stamp {((Lemoine.Collections.IDataWithId)stamp).Id} with a cycle end and an operation {stamp.Operation} => end the operation");
            }
            using (IDAOTransaction transaction = session
              .BeginTransaction ("Detection.Master.StartStampExtendOperation", RestrictedTransactionLevel)) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off;
              m_operationDetection.ExtendOperation (stamp.Operation,
                                                    dateTime);
              transaction.Commit ();
            }
          }

          if (null != stamp.Sequence) {
            if (log.IsDebugEnabled) {
              log.Debug ($"StartStamp: stamp {((Lemoine.Collections.IDataWithId)stamp).Id} is a sequence start {stamp.Sequence}");
            }
            m_sequenceDetection.StartSequence (stamp.Sequence,
                                               dateTime);
          }
          if (null != stamp.Operation) {
            if (!stamp.OperationCycleBegin && !stamp.OperationCycleEnd) {
              // TODO: to implement Stamp.Operation
              log.Error ($"StartStamp: stamp {((Lemoine.Collections.IDataWithId)stamp).Id} is an operation NYI");
              throw new NotImplementedException ();
            }
            // else stamp.Operation may be used for the operation cycle begin/end
          }
          if (null != stamp.Component) {
            // TODO: to implement Stamp.Component
            log.Error ($"StartStamp: stamp {((Lemoine.Collections.IDataWithId)stamp).Id} is a component NYI");
            throw new NotImplementedException ();
          }

          // Cycle begin/end
          if (stamp.OperationCycleBegin) {
            if (log.IsDebugEnabled) {
              log.Debug ($"StartStamp: stamp {((Lemoine.Collections.IDataWithId)stamp).Id} is a cycle start");
            }
            using (var transaction = session.BeginTransaction ("Detection.Master.StartStampStartCycle",
                                                               RestrictedTransactionLevel)) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off;
              m_operationCycleDetection.StartCycle (dateTime);
              transaction.Commit ();
            }
          }
          if (stamp.OperationCycleEnd) {
            if (log.IsDebugEnabled) {
              log.Debug ($"StartStamp: stamp {((Lemoine.Collections.IDataWithId)stamp).Id} is a cycle end");

            }
            using (var transaction = session.BeginTransaction ("Detection.Master.StartStampStopCycle",
                                                               RestrictedTransactionLevel)) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off;
              m_operationCycleDetection.StopCycle (null, dateTime);
              transaction.Commit ();
            }
          }
        }
        catch (Exception ex) {
          if (ExceptionTest.IsStale (ex)) {
            log.Warn ($"StartStamp: stamp {stamp} at {dateTime} ended with a stale object state exception", ex);
          }
          else if (ExceptionTest.IsTemporary (ex)) {
            log.Warn ($"StartStamp: stamp {stamp} at {dateTime} ended with a temporary (serialization) failure", ex);
          }
          else {
            log.Exception (ex, $"StartStamp: stamp {stamp} at {dateTime} ended with exception");
          }
          if (stamp.OperationCycleBegin || stamp.OperationCycleEnd) {
            m_operationCycleDetection.DetectionProcessError (m_machineModule, ex);
          }
          throw;
        }
      }
    }

    /// <summary>
    /// Make all the changes in database to stop an Iso File
    /// </summary>
    /// <param name="dateTime"></param>
    public void StopIsoFile (DateTime dateTime)
    {
      m_sequenceDetection.StopSequence (dateTime);
    }

    public DateTime? GetOperationDetectionDateTime ()
    {
      return m_operationDetectionStatus.GetOperationDetectionDateTime ();
    }

    public DateTime? GetCycleDetectionDateTime ()
    {
      return m_cycleDetectionStatus.GetCycleDetectionDateTime ();
    }

    public void SetSequenceMilestone (DateTime dateTime, TimeSpan sequenceMilestone, ISequence sequence = null)
    {
      m_sequenceMilestoneDetection.SetSequenceMilestone (dateTime, sequenceMilestone, sequence);
    }
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Core.ExceptionManagement;
using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Collections;

namespace Lemoine.Analysis.Detection
{
  /// <summary>
  /// Generic utility class to manage each time a stamp or any other type of detection is detected
  /// 
  /// This class is obsolete. It is only kept for the moment:
  /// <item>as a reference, if in the future, some new plugins need to be created</item>
  /// <item>for the unit tests</item>
  /// 
  /// After some time, and after the unit tests are updated to use for example Lemoine.Plugin.StampDetection,
  /// this class can be removed
  /// </summary>
  internal class MasterDetection : Lemoine.Threading.IChecked
  {
    static readonly string CREATE_OPERATION_FROM_CODE_KEY = "Detection.CreateOperationFromCode";
    static readonly bool CREATE_OPERATION_FROM_CODE_DEFAULT = true;

    ILog log;

    #region Members
    readonly IMachineModule m_machineModule;
    readonly IDictionary<int, bool> m_coherentCache = new Dictionary<int, bool> ();
    readonly SequenceDetection m_sequenceDetection;
    readonly OperationDetection m_operationDetection;
    readonly OperationCycleDetection m_operationCycleDetection;
    readonly SequenceMilestoneDetection m_sequenceMilestoneDetection;
    readonly Lemoine.Threading.IChecked m_caller;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Constructor to set the machine module and sequence
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="restrictedTransactionLevel"></param>
    /// <param name="operationDetection"></param>
    /// <param name="operationCycleDetection"></param>
    /// <param name="sequenceDetection"></param>
    /// <param name="caller"></param>
    public MasterDetection (IMachineModule machineModule, TransactionLevel restrictedTransactionLevel,
                            OperationDetection operationDetection, OperationCycleDetection operationCycleDetection,
                            SequenceDetection sequenceDetection, Lemoine.Threading.IChecked caller)
    {
      Debug.Assert (null != machineModule);

      m_machineModule = machineModule;
      m_operationDetection = operationDetection;
      m_operationCycleDetection = operationCycleDetection;
      m_sequenceDetection = sequenceDetection;
      m_sequenceMilestoneDetection = new SequenceMilestoneDetection (machineModule, caller);
      m_caller = caller;
      this.RestrictedTransactionLevel = restrictedTransactionLevel;

      log = LogManager.GetLogger (string.Format ("{0}.{1}.{2}",
                                                 this.GetType ().FullName,
                                                 machineModule.MonitoredMachine.Id,
                                                 machineModule.Id));
    }

    /// <summary>
    /// Constructor for the unit tests
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="restrictedTransactionLevel"></param>
    /// <param name="operationDetection"></param>
    /// <param name="operationCycleDetection"></param>
    internal protected MasterDetection (IMachineModule machineModule, TransactionLevel restrictedTransactionLevel,
                            OperationDetection operationDetection, OperationCycleDetection operationCycleDetection)
      : this (machineModule, restrictedTransactionLevel, operationDetection, operationCycleDetection,
          new SequenceDetection (operationDetection, new SequenceMilestoneDetection (machineModule, null), machineModule, null), null)
    {
      m_sequenceDetection.RestrictedTransactionLevel = restrictedTransactionLevel;
      m_sequenceMilestoneDetection.RestrictedTransactionLevel = restrictedTransactionLevel;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// Restricted transaction level
    /// </summary>
    public TransactionLevel RestrictedTransactionLevel { get; set; }
    #endregion // Getters / Setters

    #region IChecked implementation
    /// <summary>
    /// Lemoine.Threading.IChecked implementation
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

    /// <summary>
    /// Check the coherence of the data in a stamp.
    /// 
    /// In case of incoherence, an analysis log is added
    /// </summary>
    /// <param name="stamp">not null</param>
    /// <param name="quantity">optional quantity</param>
    /// <returns></returns>
    bool CheckStampCoherence (IStamp stamp, int? quantity)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        Debug.Assert (null != stamp);

        // Re-associate m_machineModule to this session
        ModelDAOHelper.DAOFactory.MachineModuleDAO.Lock (m_machineModule);

        if (m_coherentCache.ContainsKey (((Lemoine.Collections.IDataWithId)stamp).Id)) {
          log.DebugFormat ("CheckCoherence: " +
                           "return coherence from cache for stamp {0}: {1}",
                           stamp, m_coherentCache[((Lemoine.Collections.IDataWithId)stamp).Id]);
          return m_coherentCache[((Lemoine.Collections.IDataWithId)stamp).Id];
        }
        m_coherentCache[((Lemoine.Collections.IDataWithId)stamp).Id] = false;

        IModelFactory modelFactory =
          ModelDAOHelper.ModelFactory;
        IDAOFactory daoFactory =
          ModelDAOHelper.DAOFactory;

        if (quantity.HasValue && !stamp.OperationCycleEnd) {
          log.WarnFormat ("CheckCoherence: " +
                          "a quantity {0} was defined but the stamp {1} is not a cycle end",
                          quantity.Value, stamp);
          using (IDAOTransaction transaction = session.BeginTransaction ("Detection.LogBadCoherenceQuantityCycleEnd",
                                                                         TransactionLevel.ReadCommitted)) {
            transaction.SynchronousCommitOption = SynchronousCommit.Off;

            IDetectionAnalysisLog detectionAnalysisLog =
              modelFactory
              .CreateDetectionAnalysisLog (LogLevel.WARN,
                                           string.Format ("Quantity {0} with a stamp {0} that is not a cycle end",
                                                          quantity.Value, stamp),
                                           m_machineModule.MonitoredMachine,
                                           m_machineModule);
            daoFactory.DetectionAnalysisLogDAO
              .MakePersistent (detectionAnalysisLog);

            transaction.Commit ();
          }
          // But continue, this is only a warning
        }

        if (stamp.OperationCycleBegin && stamp.OperationCycleEnd) { // They should not be simultaneous => raise a warning
          log.WarnFormat ("CheckCoherence: " +
                          "the stamp {0} triggers in the same time an operation cycle begin and an operation cycle end",
                          stamp);
          using (IDAOTransaction transaction = session.BeginTransaction ("Detection.LogBadCoherenceBeginEnd",
                                                                         TransactionLevel.ReadCommitted)) {
            transaction.SynchronousCommitOption = SynchronousCommit.Off;

            IDetectionAnalysisLog detectionAnalysisLog =
              modelFactory
              .CreateDetectionAnalysisLog (LogLevel.WARN,
                                           string.Format ("Stamp {0} is both an operation cycle begin and end",
                                                          stamp),
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
            log.ErrorFormat ("CheckCoherence: " +
                             "Sequence and IsoFileEnd in the same time");
            using (IDAOTransaction transaction = session.BeginTransaction ("Detection.LogBadCoherenceSequenceIsoFileEnd",
                                                                           TransactionLevel.ReadCommitted)) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off;

              IDetectionAnalysisLog detectionAnalysisLog =
                modelFactory
                .CreateDetectionAnalysisLog (LogLevel.ERROR,
                                             string.Format ("Sequence {0} and IsoFileEnd " +
                                                            "in the same time",
                                                            stamp.Sequence),
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
            log.ErrorFormat ("CheckCoherence: " +
                             "Sequence and Operation are not compatible ");
            using (IDAOTransaction transaction = session.BeginTransaction ("Detection.LogBadCoherenceSequenceOperation",
                                                                           TransactionLevel.ReadCommitted)) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off;

              IDetectionAnalysisLog detectionAnalysisLog =
                modelFactory
                .CreateDetectionAnalysisLog (LogLevel.ERROR,
                                             string.Format ("Sequence {0} and Operation {1} " +
                                                            "not compatible",
                                                            stamp.Sequence,
                                                            stamp.Operation),
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
              log.ErrorFormat ("CheckCoherence: " +
                               "Sequence and Component are not compatible ");
              using (IDAOTransaction transaction = session.BeginTransaction ("Detection.LogBadCoherenceSequenceComponent",
                                                                             TransactionLevel.ReadCommitted)) {
                transaction.SynchronousCommitOption = SynchronousCommit.Off;

                IDetectionAnalysisLog detectionAnalysisLog =
                  modelFactory
                  .CreateDetectionAnalysisLog (LogLevel.ERROR,
                                               string.Format ("Sequence {0} and Component {1} " +
                                                              "not compatible",
                                                              stamp.Sequence,
                                                              stamp.Component),
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
              log.ErrorFormat ("CheckCoherence: " +
                               "Operation and Component are not compatible ");
              using (IDAOTransaction transaction = session.BeginTransaction ("Detection.LogBadCoherenceOperationComponent",
                                                                             TransactionLevel.ReadCommitted)) {
                transaction.SynchronousCommitOption = SynchronousCommit.Off;

                IDetectionAnalysisLog detectionAnalysisLog =
                  modelFactory
                  .CreateDetectionAnalysisLog (LogLevel.ERROR,
                                               string.Format ("Operation {0} and Component {1} " +
                                                              "not compatible",
                                                              stamp.Operation,
                                                              stamp.Component),
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
    /// Get or create an operation from a code
    /// </summary>
    /// <param name="operationCode"></param>
    /// <returns></returns>
    IOperation GetOrCreateOperationFromCode (string operationCode)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        if (string.IsNullOrEmpty (operationCode)) {
          log.DebugFormat ("GetOrCreateOperationFromCode: " +
                           "empty operation code");
          return null;
        }
        else { // !IsNullOrEmpty
          IList<IOperation> operations = ModelDAOHelper.DAOFactory.OperationDAO
            .FindByCode (operationCode);
          if (1 < operations.Count) {
            log.ErrorFormat ("GetOrCreateOperationFromCode: " +
                             "there are {0} operations with the same operation code {1} " +
                             "=> give up",
                             operations.Count, operationCode);
            return null;
          }
          else if (0 == operations.Count) {
            if (Lemoine.Info.ConfigSet.LoadAndGet<bool> (CREATE_OPERATION_FROM_CODE_KEY,
                                                         CREATE_OPERATION_FROM_CODE_DEFAULT)) {
              log.WarnFormat ("StartCycle: " +
                              "no operation was found with code {0} " +
                              "=> create it",
                              operationCode);
              IOperation operation;
              using (IDAOTransaction transaction = daoSession.BeginTransaction ("Detection.CreateOperationFromCode",
                                                                                RestrictedTransactionLevel)) {
                transaction.SynchronousCommitOption = SynchronousCommit.Off;

                IOperationType operationType = ModelDAOHelper.DAOFactory.OperationTypeDAO
                  .FindById (1); // Default
                operation = ModelDAOHelper.ModelFactory.CreateOperation (operationType);
                operation.Code = operationCode;
                ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation);

                transaction.Commit ();
              }
              return operation;
            }
            else { // Option CREATE_OPERATION_FROM_CODE off
              log.ErrorFormat ("StartCycle: " +
                               "operation code {0} does not exist => skip it",
                               operationCode);
              return null;
            }
          }
          else { // Only one operation
            Debug.Assert (1 == operations.Count);
            return operations[0];
          }
        }
      }
    }

    /// <summary>
    /// Start an OperationCycle
    /// </summary>
    /// <param name="dateTime"></param>
    public void StartCycle (DateTime dateTime)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        try {
          using (IDAOTransaction transaction = daoSession.BeginTransaction ("Detection.StampStartCycle",
                                                                            RestrictedTransactionLevel)) {
            transaction.SynchronousCommitOption = SynchronousCommit.Off;

            m_operationCycleDetection.StartCycle (dateTime);

            transaction.Commit ();
          }
        }
        catch (OperationCanceledException) {
          throw;
        }
        catch (Lemoine.Threading.AbortException) {
          throw;
        }
        catch (Exception ex) {
          if (ExceptionTest.IsStale (ex, log)) {
            log.Warn ($"StartCycle: at {dateTime} ended with a stale object state exception", ex);
          }
          else if (ExceptionTest.IsTemporary (ex, log)) {
            log.Warn ($"StartCycle: at {dateTime} ended with a temporary failure (serialization)", ex);
          }
          else {
            log.Exception (ex, $"StartCycle: at {dateTime} ended with exception");
          }
          m_operationCycleDetection.DetectionProcessError (m_machineModule, ex);
          throw;
        }
      }
    }

    /// <summary>
    /// Start an OperationCycle with an OperationCode
    /// </summary>
    /// <param name="operationCode"></param>
    /// <param name="dateTime"></param>
    public void StartCycle (string operationCode,
                            DateTime dateTime)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        try {
          IOperation operation = GetOrCreateOperationFromCode (operationCode);
          if (null != operation) {
            log.DebugFormat ("StartCycle: " +
                             "cycle begin with operation {0} " +
                             "=> begin the operation right now",
                             operation);
            using (IDAOTransaction transaction = daoSession.BeginTransaction ("Detection.Master.StartCycleOperationCodeStartOperation",
                                                                              RestrictedTransactionLevel)) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off;
              m_operationDetection.StartOperation (operation,
                                                   dateTime);
              transaction.Commit ();
            }
            m_sequenceDetection.StartAutoOnlyOperation (operation,
                                                        dateTime);
          }
          using (IDAOTransaction transaction = daoSession.BeginTransaction ("Detection.Master.StartCycleOperationCodeStartCycle",
                                                                            RestrictedTransactionLevel)) {
            transaction.SynchronousCommitOption = SynchronousCommit.Off;
            m_operationCycleDetection.StartCycle (dateTime);
            transaction.Commit ();
          }
        }
        catch (OperationCanceledException) {
          throw;
        }
        catch (Lemoine.Threading.AbortException) {
          throw;
        }
        catch (Exception ex) {
          if (ExceptionTest.IsStale (ex, log)) {
            log.Warn ($"StartCycle: with operationCode {operationCode} at {dateTime} ended with a stale object state exception", ex);
          }
          else if (ExceptionTest.IsTemporary (ex, log)) {
            log.Warn ($"StartCycle: with operationCode {operationCode} at {dateTime} ended with a temporary (serialization) failure", ex);
          }
          else {
            log.Exception (ex, $"StartCycle: with operationCode {operationCode} at {dateTime} ended with exception");
          }
          m_operationCycleDetection.DetectionProcessError (m_machineModule, ex);
          throw;
        }
      }
    }

    /// <summary>
    /// Stop an OperationCycle
    /// </summary>
    /// <param name="dateTime"></param>
    public void StopCycle (DateTime dateTime)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        try {
          using (IDAOTransaction transaction = daoSession.BeginTransaction ("Detection.Master.StopCycle",
                                                                            RestrictedTransactionLevel)) {
            transaction.SynchronousCommitOption = SynchronousCommit.Off;

            m_operationCycleDetection.StopCycle (null, dateTime); // Default quantity

            transaction.Commit ();
          }
        }
        catch (OperationCanceledException) {
          throw;
        }
        catch (Lemoine.Threading.AbortException) {
          throw;
        }
        catch (Exception ex) {
          if (ExceptionTest.IsStale (ex, log)) {
            log.Warn ($"StopCycle: at {dateTime} ended with a stale object state exception", ex);
          }
          else if (ExceptionTest.IsTemporary (ex, log)) {
            log.Warn ($"StopCycle: at {dateTime} ended with a temporary (serialization) failure", ex);
          }
          else {
            log.Exception (ex, $"StopCycle: at {dateTime} ended with exception");
          }
          m_operationCycleDetection.DetectionProcessError (m_machineModule, ex);
          throw;
        }
      }
    }

    /// <summary>
    /// Stop an OperationCycle with an OperationCode
    /// </summary>
    /// <param name="operationCode"></param>
    /// <param name="quantity"></param>
    /// <param name="dateTime"></param>
    public void StopCycle (string operationCode,
                           int? quantity,
                           DateTime dateTime)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        try {
          IOperation operation = GetOrCreateOperationFromCode (operationCode);
          if (null != operation) {
            log.DebugFormat ("StopCycle: " +
                             "with operation {0} " +
                             "=> extend the operation right now",
                             ((IDataWithId<int>)operation).Id);
            using (IDAOTransaction transaction = daoSession.BeginTransaction ("Detection.Master.StopCycleExtendOperation",
                                                                              RestrictedTransactionLevel)) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off;
              m_operationDetection.ExtendOperation (operation,
                                                    dateTime);
              transaction.Commit ();
            }
          }
          using (IDAOTransaction transaction = daoSession.BeginTransaction ("Detection.Master.StopCycleStopCycle",
                                                                            RestrictedTransactionLevel)) {
            transaction.SynchronousCommitOption = SynchronousCommit.Off;
            m_operationCycleDetection.StopCycle (quantity, dateTime);
            transaction.Commit ();
          }
        }
        catch (OperationCanceledException) {
          throw;
        }
        catch (Lemoine.Threading.AbortException) {
          throw;
        }
        catch (Exception ex) {
          if (ExceptionTest.IsStale (ex, log)) {
            log.Warn ($"StopCycle: with operationCode {operationCode} at {dateTime} ended with a stale object state exception", ex);
          }
          else if (ExceptionTest.IsTemporary (ex, log)) {
            log.Warn ($"StopCycle: with operationCode {operationCode} at {dateTime} ended with a serialization failure", ex);
          }
          else {
            log.Exception (ex, $"StopCycle: with operationCode {operationCode} at {dateTime} ended with exception");
          }
          m_operationCycleDetection.DetectionProcessError (m_machineModule, ex);
          throw;
        }
      }
    }

    /// <summary>
    /// Start and stop an OperationCycle
    /// </summary>
    /// <param name="startDateTime"></param>
    /// <param name="stopDateTime"></param>
    public void StartStopCycle (DateTime startDateTime, DateTime stopDateTime)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        try {
          using (IDAOTransaction transaction = daoSession.BeginTransaction ("Detection.Master.StartStopCycle",
                                                                            RestrictedTransactionLevel)) {
            transaction.SynchronousCommitOption = SynchronousCommit.Off;

            m_operationCycleDetection.StartStopCycle (startDateTime, stopDateTime);

            transaction.Commit ();
          }
        }
        catch (OperationCanceledException) {
          throw;
        }
        catch (Lemoine.Threading.AbortException) {
          throw;
        }
        catch (Exception ex) {
          if (ExceptionTest.IsStale (ex)) {
            log.Warn ($"StartStopCycle: {startDateTime}-{stopDateTime} ended with a stale object state exception failure", ex);
          }
          else if (ExceptionTest.IsTemporary (ex)) {
            log.Warn ($"StartStopCycle: {startDateTime}-{stopDateTime} ended with a temporary (serialization) failure", ex);
          }
          else {
            log.Exception (ex, $"StartStopCycle: {startDateTime}-{stopDateTime} ended with exception");
          }
          m_operationCycleDetection.DetectionProcessError (m_machineModule, ex);
          throw;
        }
      }
    }

    /// <summary>
    /// Make all the changes in database to start a stamp
    /// </summary>
    /// <param name="stamp"></param>
    /// <param name="quantity">optional quantity</param>
    /// <param name="dateTime"></param>
    /// <exception cref="System.Exception">if the data is not coherent</exception>
    /// <exception cref="T:System.NotImplementedException">in case stamp is associated to a component or an operation without a cycle information</exception>
    public void StartStamp (IStamp stamp,
                            int? quantity,
                            DateTime dateTime)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        try {
          // - Check coherence of the data
          if (!CheckStampCoherence (stamp, quantity)) {
            log.ErrorFormat ("StartStamp: " +
                             "stamp {0} is not coherent",
                             ((Lemoine.Collections.IDataWithId)stamp).Id);
            throw new Exception ("Stamp not coherent");
          }

          // - Make the different analysis
          //   given the properties of the stamp
          IIsoFileMachineModuleAssociation isoFileAssociation =
            new IsoFileMachineModuleAssociation (m_machineModule, new UtcDateTimeRange (dateTime));
          if (stamp.IsoFileEnd) {
            log.DebugFormat ("StartStamp: " +
                            "stamp {0} is a IsoFileEnd",
                            ((Lemoine.Collections.IDataWithId)stamp).Id);
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
              log.DebugFormat ("StartStamp: " +
                               "stamp {0} with a cycle begin and an operation {1} " +
                               "=> begin the operation right now",
                               ((Lemoine.Collections.IDataWithId)stamp).Id, operation);
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
            log.DebugFormat ("StartStamp: " +
                             "stamp {0} with a cycle end and an operation {1} " +
                             "=> end the operation",
                             ((Lemoine.Collections.IDataWithId)stamp).Id, stamp.Operation);
            using (IDAOTransaction transaction = session.BeginTransaction ("Detection.Master.StartStampExtendOperation",
                                                                           RestrictedTransactionLevel)) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off;
              m_operationDetection.ExtendOperation (stamp.Operation,
                                                    dateTime);
              transaction.Commit ();
            }
          }

          if (null != stamp.Sequence) {
            log.DebugFormat ("StartStamp: " +
                            "stamp {0} is a sequence start {1}",
                            ((Lemoine.Collections.IDataWithId)stamp).Id, stamp.Sequence);
            m_sequenceDetection.StartSequence (stamp.Sequence,
                                               dateTime);
          }
          if (null != stamp.Operation) {
            if (!stamp.OperationCycleBegin && !stamp.OperationCycleEnd) {
              // UNDONE: Stamp.Operation not implemented
              log.ErrorFormat ("StartStamp: " +
                              "stamp {0} is an operation NYI",
                              ((Lemoine.Collections.IDataWithId)stamp).Id);
              throw new NotImplementedException ();
            }
            // else stamp.Operation may be used for the operation cycle begin/end
          }
          if (null != stamp.Component) {
            // UNDONE: Stamp.Component not implemented
            log.ErrorFormat ("StartStamp: " +
                            "stamp {0} is a component NYI",
                            ((Lemoine.Collections.IDataWithId)stamp).Id);
            throw new NotImplementedException ();
          }

          // Cycle begin/end
          if (stamp.OperationCycleBegin) {
            if (log.IsDebugEnabled) {
              log.Debug ($"StartStamp: stamp {((Lemoine.Collections.IDataWithId)stamp).Id} is a cycle start");
            }
            using (IDAOTransaction transaction = session.BeginTransaction ("Detection.Master.StartStampStartCycle",
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
            using (IDAOTransaction transaction = session.BeginTransaction ("Detection.Master.StartStampStopCycle",
                                                                           RestrictedTransactionLevel)) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off;
              m_operationCycleDetection.StopCycle (quantity, dateTime);
              transaction.Commit ();
            }
          }
        }
        catch (OperationCanceledException) {
          throw;
        }
        catch (Lemoine.Threading.AbortException) {
          throw;
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
  }
}

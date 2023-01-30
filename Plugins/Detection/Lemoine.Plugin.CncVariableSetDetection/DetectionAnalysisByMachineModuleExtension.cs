// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (c) Lemoine Automation TechnoGetLogger ()ies. All Rights Reserved.
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.ExceptionManagement;
using Lemoine.Core.Log;
using Lemoine.Extensions.Analysis;
using Lemoine.Extensions.Analysis.Detection;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Extensions.Database;

namespace Lemoine.Plugin.CncVariableSetDetection
{
  public class DetectionAnalysisByMachineModuleExtension
    : Pulse.Extensions.Analysis.Implementation.CncVariablesDetectionAnalysisByMachineModule<Configuration>
    , IOperationDetectionStatusExtension
    , ICycleDetectionStatusExtension
    , IDetectionAnalysisByMachineModuleExtension
  {
    readonly IDictionary<int, bool> m_coherentCache = new Dictionary<int, bool> ();
    int m_milestonePart = 0;

    /// <summary>
    /// Initialize implementation
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="operationDetection"></param>
    /// <param name="operationCycleDetection"></param>
    /// <param name="sequenceDetection"></param>
    /// <param name="sequenceMilestoneDetection"></param>
    /// <returns></returns>
    public override bool Initialize (IMachineModule machineModule, IOperationDetection operationDetection, IOperationCycleDetection operationCycleDetection, ISequenceDetection sequenceDetection, ISequenceMilestoneDetection sequenceMilestoneDetection)
    {
      if (!base.Initialize (machineModule, operationDetection, operationCycleDetection, sequenceDetection, sequenceMilestoneDetection)) {
        GetLogger ().Info ($"Initialize: base method failed, return false");
        return false;
      }
      m_milestonePart = this.Configuration.MilestonePart;
      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ($"Initialize: milestone part is {m_milestonePart}");
      }
      return true;
    }

    protected override void RunMilestoneVariableAction (object oldMilestoneVariableValue, object newMilestoneVariableValue, DateTime dateTime, object sequenceVariableValue)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IStamp stamp = null;
        if (null != sequenceVariableValue) {
          var stampId = int.Parse (sequenceVariableValue.ToString ());
          stamp = ModelDAOHelper.DAOFactory.StampDAO.FindById (stampId);
          if (stamp is null) {
            GetLogger ().Error ($"RunMilestoneVariableAction: unknown stamp {stampId}");
          }
        }
        if (null != newMilestoneVariableValue) {
          var milestone = double.Parse (newMilestoneVariableValue.ToString (), System.Globalization.CultureInfo.InvariantCulture);
          SetSequenceMilestone (dateTime, TimeSpan.FromMinutes (milestone), stamp?.Sequence);
        }
      }
    }

    protected override void RunSequenceVariableAction (object oldSequenceVariableValue, object newSequenceVariableValue, DateTime dateTime, object milestoneVariableValue)
    {
      if (string.Equals ("0", newSequenceVariableValue.ToString (), StringComparison.InvariantCultureIgnoreCase)) {
        StopIsoFile (dateTime);
        return;
      }
      else {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var (stampId, milestoneFromSequenceVariable) = ExtractStampIdMilestoneFromSequenceVariable (newSequenceVariableValue);
          if (0 == stampId) {
            GetLogger ().Warn ($"RunSequenceVariableAction: stampId = 0 with a milestone = {milestoneFromSequenceVariable} => Stop Iso file");
            StopIsoFile (dateTime);
            return;
          }
          var stamp = ModelDAOHelper.DAOFactory.StampDAO.FindById (stampId);
          if (stamp is null) {
            GetLogger ().Error ($"RunSequenceVariableAction: unknown stamp {stampId}");
            return;
          }
          StartStamp (stamp, dateTime);
          if (null != milestoneVariableValue) {
            try {
              var milestone = double.Parse (milestoneVariableValue.ToString (), System.Globalization.CultureInfo.InvariantCulture);
              SetSequenceMilestone (dateTime, TimeSpan.FromMinutes (milestone), stamp.Sequence);
            }
            catch (Exception ex) {
              GetLogger ().Error ($"RunSequenceVariableAction: invalid milestone {milestoneVariableValue}", ex);
            }
          }
          else if (0 != milestoneFromSequenceVariable) {
            try {
              SetSequenceMilestone (dateTime, TimeSpan.FromMinutes (milestoneFromSequenceVariable), stamp.Sequence);
            }
            catch (Exception ex) {
              GetLogger ().Error ($"RunSequenceVariableAction: invalid milestone {milestoneFromSequenceVariable} from sequence variable", ex);
            }
          }
        }
      }
    }

    (int, int) ExtractStampIdMilestoneFromSequenceVariable (object sequenceVariableValue)
    {
      if (0 == m_milestonePart) {
        return (int.Parse (sequenceVariableValue.ToString ()), 0);
      }
      else if (sequenceVariableValue is decimal) {
        var d = (decimal)sequenceVariableValue;
        var stampId = (int)Math.Floor (d);
        var r = d - stampId;
        var milestone = (int)((int)Math.Pow (10, m_milestonePart) * r);
        return (stampId, milestone);
      }
      else if (sequenceVariableValue is double) {
        var d = (double)sequenceVariableValue;
        var stampId = (int)Math.Floor (d);
        var r = d - stampId;
        var milestone = (int)((int)Math.Pow (10, m_milestonePart) * r);
        return (stampId, milestone);
      }
      else if (sequenceVariableValue is string) {
        var s = (string)sequenceVariableValue;
        if (s.Contains ('.')) {
          var d = double.Parse (s, System.Globalization.CultureInfo.InvariantCulture);
          return ExtractStampIdMilestoneFromSequenceVariable (d);
        }
        else if (s.Contains (',')) {
          var d = double.Parse (s);
          return ExtractStampIdMilestoneFromSequenceVariable (d);
        }
        else {
          return (int.Parse (sequenceVariableValue.ToString ()), 0);
        }
      }
      else {
        GetLogger ().Error ($"ExtractStampIdMilestoneFromSequenceVariable: invalid variable {sequenceVariableValue}");
        throw new ArgumentException ("Invalid sequence variable value", "sequenceVariableValue");
      }
    }

    protected override bool TriggerMilestoneVariableAction (object newMilestoneVariableValue)
    {
      return null != newMilestoneVariableValue;
    }

    protected override bool TriggerSequenceVariableAction (object newSequenceVariableValue)
    {
      return null != newSequenceVariableValue;
    }

    /// <summary>
    /// Check the coherence of the data in a stamp.
    /// 
    /// In case of incoherence, an analysis GetLogger () is added
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="stamp">not null</param>
    /// <returns></returns>
    bool CheckStampCoherence (IMachineModule machineModule, IStamp stamp)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != stamp);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {

        if (m_coherentCache.ContainsKey (((Lemoine.Collections.IDataWithId)stamp).Id)) {
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ($"CheckStampCoherence: return coherence from cache for stamp {stamp}: {m_coherentCache[((Lemoine.Collections.IDataWithId)stamp).Id]}");
          }
          return m_coherentCache[((Lemoine.Collections.IDataWithId)stamp).Id];
        }
        m_coherentCache[((Lemoine.Collections.IDataWithId)stamp).Id] = false;

        IModelFactory modelFactory =
          ModelDAOHelper.ModelFactory;
        IDAOFactory daoFactory =
          ModelDAOHelper.DAOFactory;

        if (stamp.OperationCycleBegin && stamp.OperationCycleEnd) { // They should not be simultaneous => raise a warning
          GetLogger ().Warn ($"CheckStampCoherence: the stamp {stamp} triggers in the same time an operation cycle begin and an operation cycle end");
          using (IDAOTransaction transaction = session.BeginTransaction ("Detection.LogBadCoherenceBeginEnd",
                                                                         TransactionLevel.ReadCommitted)) {
            transaction.SynchronousCommitOption = SynchronousCommit.Off;

            IDetectionAnalysisLog detectionAnalysisLog =
              modelFactory
              .CreateDetectionAnalysisLog (LogLevel.WARN,
                                           $"Stamp {stamp} is both an operation cycle begin and end",
                                           machineModule.MonitoredMachine,
                                           machineModule);
            daoFactory.DetectionAnalysisLogDAO
              .MakePersistent (detectionAnalysisLog);

            transaction.Commit ();
          }
          // But continue, this is only a warning
        }

        if (null != stamp.Sequence) {
          if (stamp.IsoFileEnd) {
            GetLogger ().Error ("CheckStampCoherence: Sequence and IsoFileEnd in the same time");
            using (IDAOTransaction transaction = session.BeginTransaction ("Detection.LogBadCoherenceSequenceIsoFileEnd",
                                                                           TransactionLevel.ReadCommitted)) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off;

              IDetectionAnalysisLog detectionAnalysisLog =
                modelFactory
                .CreateDetectionAnalysisLog (LogLevel.ERROR,
                                             $"Sequence {stamp.Sequence} and IsoFileEnd in the same time",
                                             machineModule.MonitoredMachine,
                                             machineModule);
              daoFactory.DetectionAnalysisLogDAO
                .MakePersistent (detectionAnalysisLog);

              transaction.Commit ();
            }
            return false;
          }
          if ((null != stamp.Operation)
              && (!object.Equals (stamp.Sequence.Operation,
                                  stamp.Operation))) {
            GetLogger ().Error ("CheckStampCoherence: Sequence and Operation are not compatible ");
            using (IDAOTransaction transaction = session.BeginTransaction ("Detection.LogBadCoherenceSequenceOperation",
                                                                           TransactionLevel.ReadCommitted)) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off;

              IDetectionAnalysisLog detectionAnalysisLog =
                modelFactory
                .CreateDetectionAnalysisLog (LogLevel.ERROR,
                                             $"Sequence {stamp.Sequence} and Operation {stamp.Operation} not compatible",
                                             machineModule.MonitoredMachine,
                                             machineModule);
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
              GetLogger ().Error ($"CheckStampCoherence: Sequence and Component are not compatible ");
              using (var transaction = session.BeginTransaction ("Detection.LogBadCoherenceSequenceComponent",
                                                                 TransactionLevel.ReadCommitted)) {
                transaction.SynchronousCommitOption = SynchronousCommit.Off;

                IDetectionAnalysisLog detectionAnalysisLog =
                  modelFactory
                  .CreateDetectionAnalysisLog (LogLevel.ERROR,
                                               $"Sequence {stamp.Sequence} and Component {stamp.Component} not compatible",
                                               machineModule.MonitoredMachine,
                                               machineModule);
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
              GetLogger ().Error ($"CheckStampCoherence: Operation and Component are not compatible ");
              using (IDAOTransaction transaction = session.BeginTransaction ("Detection.LogBadCoherenceOperationComponent",
                                                                             TransactionLevel.ReadCommitted)) {
                transaction.SynchronousCommitOption = SynchronousCommit.Off;

                IDetectionAnalysisLog detectionAnalysisLog =
                  modelFactory
                  .CreateDetectionAnalysisLog (LogLevel.ERROR,
                                               $"Operation {stamp.Operation} and Component {stamp.Component} not compatible",
                                               machineModule.MonitoredMachine,
                                               machineModule);
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
      var machineModule = this.MachineModule;

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        try {
          // - Reload m_machineModule for this session
          machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO
            .FindById (this.MachineModule.Id);

          // - Check coherence of the data
          if (!CheckStampCoherence (machineModule, stamp)) {
            GetLogger ().Error ($"StartStamp: stamp {((Lemoine.Collections.IDataWithId)stamp).Id} is not coherent");
            throw new Exception ("Stamp not coherent");
          }

          // - Make the different analysis
          //   given the properties of the stamp
          IIsoFileMachineModuleAssociation isoFileAssociation = ModelDAOHelper.ModelFactory
            .CreateIsoFileMachineModuleAssociation (machineModule, new UtcDateTimeRange (dateTime));
          if (stamp.IsoFileEnd) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"StartStamp: stamp {((Lemoine.Collections.IDataWithId)stamp).Id} is a IsoFileEnd");
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
              if (GetLogger ().IsDebugEnabled) {
                GetLogger ().Debug ($"StartStamp: stamp {((Lemoine.Collections.IDataWithId)stamp).Id} with a cycle begin and an operation {operation} => begin the operation right now");
              }
              using (IDAOTransaction transaction = session.BeginTransaction ("Detection.Master.StartStampOperation",
                                                                             RestrictedTransactionLevel)) {
                transaction.SynchronousCommitOption = SynchronousCommit.Off;
                this.OperationDetection.StartOperation (operation,
                                                     dateTime);
                transaction.Commit ();
              }
              this.SequenceDetection.StartAutoOnlyOperation (operation,
                                                          dateTime);
            }
          }
          if (stamp.OperationCycleEnd) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"StartStamp: stamp {((Lemoine.Collections.IDataWithId)stamp).Id} with a cycle end and an operation {stamp.Operation} => end the operation");
            }
            using (IDAOTransaction transaction = session.BeginTransaction ("Detection.Master.StartStampExtendOperation",
                                                                           RestrictedTransactionLevel)) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off;
              this.OperationDetection.ExtendOperation (stamp.Operation,
                                                    dateTime);
              transaction.Commit ();
            }
          }

          if (null != stamp.Sequence) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"StartStamp: stamp {((Lemoine.Collections.IDataWithId)stamp).Id} is a sequence start {stamp.Sequence}");
            }
            this.SequenceDetection.StartSequence (stamp.Sequence,
                                               dateTime);
          }
          if (null != stamp.Operation) {
            if (!stamp.OperationCycleBegin && !stamp.OperationCycleEnd) {
              // TODO: to implement Stamp.Operation
              GetLogger ().Error ($"StartStamp: stamp {((Lemoine.Collections.IDataWithId)stamp).Id} is an operation, not yet implemented");
              throw new NotImplementedException ();
            }
            // else stamp.Operation may be used for the operation cycle begin/end
          }
          if (null != stamp.Component) {
            // TODO: to implement Stamp.Component
            GetLogger ().Error ($"StartStamp: stamp {((Lemoine.Collections.IDataWithId)stamp).Id} is a component, not yet implemented");
            throw new NotImplementedException ();
          }

          // Cycle begin/end
          if (stamp.OperationCycleBegin) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"StartStamp: stamp {((Lemoine.Collections.IDataWithId)stamp).Id} is a cycle start");
            }
            using (IDAOTransaction transaction = session.BeginTransaction ("Detection.Master.StartStampStartCycle",
                                                                           RestrictedTransactionLevel)) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off;
              this.OperationCycleDetection.StartCycle (dateTime);
              transaction.Commit ();
            }
          }
          if (stamp.OperationCycleEnd) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"StartStamp: stamp {((Lemoine.Collections.IDataWithId)stamp).Id} is a cycle end");
            }
            using (IDAOTransaction transaction = session.BeginTransaction ("Detection.Master.StartStampStopCycle",
                                                                           RestrictedTransactionLevel)) {
              transaction.SynchronousCommitOption = SynchronousCommit.Off;
              this.OperationCycleDetection.StopCycle (null, dateTime);
              transaction.Commit ();
            }
          }
        }
        catch (Exception ex) {
          if (ExceptionTest.IsStale (ex)) {
            GetLogger ().Warn ($"StartStamp: stamp {stamp} at {dateTime} ended with a stale object state exception", ex);
          }
          else if (ExceptionTest.IsTemporary (ex)) {
            GetLogger ().Warn ($"StartStamp: stamp {stamp} at {dateTime} ended with a temporary (serialization) failure", ex);
          }
          else {
            GetLogger ().Exception (ex, $"StartStamp: stamp {stamp} at {dateTime} ended with exception");
          }
          if (stamp.OperationCycleBegin || stamp.OperationCycleEnd) {
            this.OperationCycleDetection.DetectionProcessError (machineModule, ex);
          }
          throw;
        }
      }
    }

    /// <summary>
    /// Make all the changes in database to stop an Iso File
    /// </summary>
    /// <param name="dateTime"></param>
    void StopIsoFile (DateTime dateTime)
    {
      this.SequenceDetection.StopSequence (dateTime);
    }

    public void SetSequenceMilestone (DateTime dateTime, TimeSpan sequenceMilestone, ISequence sequence = null)
    {
      this.SequenceMilestoneDetection.SetSequenceMilestone (dateTime, sequenceMilestone, sequence);
    }
  }
}

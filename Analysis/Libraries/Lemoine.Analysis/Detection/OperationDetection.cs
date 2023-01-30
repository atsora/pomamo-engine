// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Core.ExceptionManagement;
using Lemoine.Extensions.Analysis;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Linq;
using Lemoine.Business;
using Lemoine.Collections;

namespace Lemoine.Analysis.Detection
{
  /// <summary>
  /// Class to insert the right data in the database each time an operation begin or end is detected
  /// </summary>
  public class OperationDetection : Lemoine.Extensions.Analysis.Detection.IOperationDetection, Lemoine.Threading.IChecked
  {
    readonly ILog log;

    #region Members
    readonly IMonitoredMachine m_monitoredMachine;
    readonly Lemoine.Threading.IChecked m_caller;
    IEnumerable<Lemoine.Extensions.Analysis.IOperationDetectionExtension> m_operationDetectionExtensions = null;
    readonly IEnumerable<IAfterOperationDetectionExtension> m_afterExtensions;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Associated machine
    /// </summary>
    public IMonitoredMachine Machine
    {
      get { return m_monitoredMachine; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor to set the monitored machine
    /// </summary>
    /// <param name="monitoredMachine">not null</param>
    /// <param name="extensions">not null</param>
    /// <param name="caller"></param>
    public OperationDetection (IMonitoredMachine monitoredMachine, IEnumerable<Lemoine.Extensions.Analysis.IOperationDetectionExtension> extensions,
      Lemoine.Threading.IChecked caller)
    {
      Debug.Assert (null != monitoredMachine);
      Debug.Assert (null != extensions);

      m_monitoredMachine = monitoredMachine;
      m_operationDetectionExtensions = extensions;
      m_caller = caller;
      m_afterExtensions = ServiceProvider
        .Get<IEnumerable<IAfterOperationDetectionExtension>> (new Lemoine.Business.Extension.MonitoredMachineExtensions<IAfterOperationDetectionExtension> (monitoredMachine, (ext, m) => ext.Initialize (m)));

      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                 this.GetType ().FullName,
                                                 monitoredMachine.Id));
    }

    /// <summary>
    /// Constructor to set the monitored machine for the unit tests
    /// </summary>
    /// <param name="monitoredMachine">not null</param>
    /// <param name="extensions">not null</param>
    internal protected OperationDetection (IMonitoredMachine monitoredMachine, IEnumerable<Lemoine.Extensions.Analysis.IOperationDetectionExtension> extensions)
      : this (monitoredMachine, extensions, null)
    {
    }
    #endregion // Constructors

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
    /// Get the extensions and load them if needed
    /// </summary>
    /// <returns></returns>
    IEnumerable<Lemoine.Extensions.Analysis.IOperationDetectionExtension> GetExtensions ()
    {
      return m_operationDetectionExtensions;
    }

    /// <summary>
    /// Start an operation
    /// 
    /// Only begin a 1-second operation slot
    /// 
    /// The top transaction must be created by the parent function
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="dateTime"></param>
    public void StartOperation (IOperation operation,
                                DateTime dateTime)
    {
      try {
        log.DebugFormat ("StartOperation: " +
                         "insert operation {0} from {1}",
                         operation, dateTime);
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ("Detection.StartOperation")) {
          AddOperation (operation, new UtcDateTimeRange (dateTime, dateTime.AddSeconds (1)), true);
          transaction.Commit ();
        }
      }
      catch (Exception ex) {
        log.Error ($"StartOperation: Operation={operation} dateTime={dateTime}", ex);
        throw;
      }

      try {
        foreach (var extension in m_afterExtensions) {
          extension.StartOperation (operation, dateTime);
        }
      }
      catch (Exception ex) {
        log.Error ("StartOperation: extension exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Add an operation
    /// 
    /// The top transaction must be created by the parent function
    /// </summary>
    /// <param name="operation">not null</param>
    /// <param name="range"></param>
    public void AddOperation (IOperation operation, UtcDateTimeRange range)
    {
      try {
        log.DebugFormat ("AddOperation: " +
                         "add operation {0} in {1}",
                         operation, range);
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ("Detection.AddOperation", notTop: true)) {
          AddOperation (operation, range, true);
          transaction.Commit ();
        }
      }
      catch (Exception ex) {
        log.ErrorFormat ("AddOperation: " +
                         "Operation={0} range={1} " +
                         "exception={2}",
                         operation, range,
                         ex);
        throw;
      }

      try {
        foreach (var extension in m_afterExtensions) {
          extension.AddOperation (operation, range);
        }
      }
      catch (Exception ex) {
        log.Error ("AddOperation: extension exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Add an operation
    /// 
    /// Note:
    /// - for multi-machine module machines, the call of AddOperation is not always chronological
    /// - because 'Auto-Only' and 'Not auto-only' sequences are processed separately, the call to AddOperation
    ///   could not be chronological
    /// </summary>
    /// <param name="operation">Not null</param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <param name="autoOperation"></param>
    internal void AddOperation (IOperation operation, DateTime begin, UpperBound<DateTime> end, bool autoOperation)
    {
      AddOperation (operation, new UtcDateTimeRange (begin, end), autoOperation);
    }

    /// <summary>
    /// Add an operation
    /// 
    /// Note:
    /// - for multi-machine module machines, the call of AddOperation is not always chronological
    /// - because 'Auto-Only' and 'Not auto-only' sequences are processed separately, the call to AddOperation
    ///   could not be chronological
    /// </summary>
    /// <param name="operation">Not null</param>
    /// <param name="range"></param>
    /// <param name="autoOperation"></param>
    internal void AddOperation (IOperation operation, UtcDateTimeRange range, bool autoOperation)
    {
      Debug.Assert (null != operation);
      Debug.Assert (!range.IsEmpty ());

      if (log.IsDebugEnabled) {
        log.Debug ($"AddOperation: operation={((Lemoine.Collections.IDataWithId<int>)operation)?.Id} in period {range}");
      }

      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = daoSession.BeginTransaction ("Detection.AddOperation", notTop: true)) {
          // Note: The transaction level is set by the top transaction

          try {
            // Note: ModelDAOHelper.DAOFactory.OperationDAO.Lock
            //       can't be used here because of the NonUniqueObjectException
            //       The operation may have been already referenced from another sequence
            var localOperation = ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (((IDataWithId)operation).Id);

            LowerBound<DateTime> applyOperationFrom = range.Lower;
            IWorkOrder workOrder = null;
            IComponent component = null;
            ILine line = null;
            ITask task = null;
            IOperationSlot previousOperationSlot = null;

            // - Get the previous operation slot for an eventual auto-operation or a task change
            //   or if the extension requires it
            bool previousOperationSlotRequired = autoOperation || Lemoine.Business.Config.AnalysisConfigHelper.TaskManagement;
            foreach (var extension in GetExtensions ()) {
              previousOperationSlotRequired = previousOperationSlotRequired || extension.IsPreviousOperationSlotRequired ();
            }
            if (previousOperationSlotRequired) {
              if (range.Lower.HasValue) {
                previousOperationSlot =
                  ModelDAOHelper.DAOFactory.OperationSlotDAO
                  .GetLastOperationNotNullBefore (m_monitoredMachine,
                                                  range.Lower.Value);
              }
            }

            // - Next task management if the operation changes
            if (((null == previousOperationSlot)
                  || !localOperation.Equals (previousOperationSlot.Operation))
                && Lemoine.Business.Config.AnalysisConfigHelper.TaskManagement) {
              Debug.Assert (range.Lower.HasValue);
              ITask nextTask = GuessNextTask (localOperation, previousOperationSlot);
              if (null != nextTask) {
                ApplyTask (nextTask, range.Lower.Value);
              }
            }

            // - auto-operation / auto-component / auto-workorder process
            if (autoOperation && (null != previousOperationSlot)) {
              if (localOperation.Equals (previousOperationSlot.Operation)) { // Previous operation slot with the same operation
                if (Bound.Compare<DateTime> (applyOperationFrom, previousOperationSlot.EndDateTime) <= 0) { // No gap
                  // There is no gap between the operation slot,
                  // and the sequence detection,
                  // there is no need to search for an auto-operation

                  // Note: for single machine module machines, normally this case is never executed because:
                  //       in this case the sequence slot is extended,
                  //       and the AddOperation method is called with autoOperation=false
                  //       But for multi-machine module machines, this is not true
                  if (log.IsDebugEnabled) {
                    log.Debug ($"AddOperation: no gap between latest operation slot end={previousOperationSlot.EndDateTime} and sequence detection at {applyOperationFrom}");
                  }
                  workOrder = previousOperationSlot.WorkOrder;
                  component = previousOperationSlot.Component;
                  line = previousOperationSlot.Line;
                  task = previousOperationSlot.Task;
                }
                else { // Gap between previousOperationSlot and applyOperationFrom
                  Debug.Assert (previousOperationSlot.EndDateTime.HasValue);
                  Debug.Assert (applyOperationFrom.HasValue);
                  UtcDateTimeRange gap = new UtcDateTimeRange (previousOperationSlot.EndDateTime.Value,
                                                               applyOperationFrom.Value);
                  Debug.Assert (!gap.IsEmpty ());
                  Debug.Assert (gap.Duration.HasValue);

                  // Check the past time does not exceed the configuration
                  if (gap.Duration.Value < Lemoine.Business.Config.AnalysisConfigHelper.AutoOperationSame) {
                    // If there is an auto-sequence activity that does not match
                    // the previous criteria, there is no auto-operation to apply
                    if (IsAutoSequenceActivity (previousOperationSlot.EndDateTime.Value,
                                                applyOperationFrom.Value)) {
                      log.Debug ("AddOperation: no auto-operation because some auto-sequence activity was detected");
                    }
                    else { // !IsAutoSequenceActivity
                      if (log.IsDebugEnabled) {
                        log.Debug ($"AddOperation: auto operation operation={localOperation} from {previousOperationSlot.EndDateTime} because there is no auto-sequence activity in corrected period by the auto-operation margin");
                      }
                      applyOperationFrom = previousOperationSlot.EndDateTime.Value;
                      workOrder = previousOperationSlot.WorkOrder;
                      component = previousOperationSlot.Component;
                      line = previousOperationSlot.Line;
                      task = previousOperationSlot.Task;
                    } // IsAutoSequenceActivity
                  } // Duration ok < AutoOperationSame
                } // EndIf Gap / No gap
              } // EndIf previous operation slot with the same operation
              else { // Not the same operation: check auto-component and auto-workorder
                ProcessAutoComponent (previousOperationSlot.Component, previousOperationSlot.WorkOrder,
                                      previousOperationSlot.EndDateTime, localOperation,
                                      applyOperationFrom, out component, out workOrder);
              }
            }

            // Extension
            foreach (var extension in GetExtensions ()) {
              extension.AddOperation (m_monitoredMachine, localOperation, range, applyOperationFrom, previousOperationSlot);
            }

            if (log.IsDebugEnabled) {
              log.Debug ($"AddOperation: insert operation {localOperation} between {applyOperationFrom} and {range.Upper}");
            }
            var association =
              new Lemoine.GDBPersistentClasses.OperationMachineAssociation (m_monitoredMachine,
                                                                            new UtcDateTimeRange (applyOperationFrom, range.Upper),
                                                                            null, // Transient !
                                                                            true);
            association.Operation = localOperation;
            // Propagate the work order / component / line / task
            if (autoOperation && Lemoine.Business.Config.AnalysisConfigHelper.AutoOperationPropagation.IsActive ()
                && ((null != workOrder) || (null != component) || (null != line) || (null != task))) {
              Debug.Assert (null != previousOperationSlot); // Else all of workOrder/component/line/task would be null
              Debug.Assert (range.Lower.HasValue); // Else previousOperationSlot would be null
              IList<IOperationSlot> betweenSlots = new List<IOperationSlot> ();
              if (!previousOperationSlot.DateTimeRange.ContainsRange (range)) {
                Debug.Assert (previousOperationSlot.DateTimeRange.Upper.HasValue); // Else it would contain range
                betweenSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
                  .FindOverlapsRange (m_monitoredMachine, new UtcDateTimeRange (previousOperationSlot.DateTimeRange.Upper.Value,
                                                                                range.Upper));
              }
              Propagate (Lemoine.Business.Config.AnalysisConfigHelper.AutoOperationPropagation,
                         association, workOrder, component, line, task, betweenSlots);
            }
            association.Option = AssociationOption.Detected;
            association.Caller = m_caller;
            association.Apply ();
          }
          catch (Exception ex) {
            if (ExceptionTest.IsStale (ex, log)) {
              log.Warn ($"AddOperation: stale object state exception OperationId={((Lemoine.Collections.IDataWithId<int>)operation).Id} range={range} autoOperation={autoOperation}",
               ex);
              transaction.Rollback (); // To remove some error logs that are associated to an implicit rollback
            }
            else if (ExceptionTest.IsTemporary (ex, log)) {
              log.Warn ($"AddOperation: temporary (serialization) failure OperationId={((Lemoine.Collections.IDataWithId<int>)operation).Id} range={range} autoOperation={autoOperation}", ex);
              transaction.Rollback (); // To remove some error logs that are associated to an implicit rollback
            }
            else {
              log.Error ($"AddOperation: OperationId={((Lemoine.Collections.IDataWithId<int>)operation).Id} range={range} autoOperation={autoOperation}", ex);
            }
            throw;
          }

          transaction.Commit ();
        }
      }
    }

    bool ProcessAutoComponent (IComponent previousComponent, IWorkOrder previousWorkOrder,
                               UpperBound<DateTime> previousPeriodEnd,
                               IOperation newOperation, LowerBound<DateTime> newPeriodBegin,
                               out IComponent component, out IWorkOrder workOrder)
    {
      component = null;
      workOrder = null;

      if ((null == previousComponent) && (null == previousWorkOrder)) { // Nothing to do
        log.Debug ("ProcessAutoComponent: both previous component and work order are null => return false");
        return false;
      }

      // Note: OperationDAO.InitializeIntermediateWorkPieces may be called here on newOperation
      // because of the previous OperationDAO.Lock (newOperation) (it may have been detached before)

      if (Bound.Compare<DateTime> (newPeriodBegin, previousPeriodEnd) <= 0) { // No gap
        log.Debug ("ProcessAutoComponent: no gap => return true");
        // - Check component
        if (null != previousComponent) {
          ModelDAOHelper.DAOFactory.OperationDAO
            .InitializeIntermediateWorkPieces (newOperation);
          IComponent newComponent = Lemoine.Business.Operation.OperationRelations
            .TryToGuessComponentFromOperation (newOperation);
          if (object.Equals (previousComponent, newComponent)) {
            // Check there is not another component in database in this period ?
            // No for the moment because it does not make sense to have a very short period with a new component
            // but no operation
            component = previousComponent;
          }
        }
        // - Check work order
        if (null != previousWorkOrder) {
          ModelDAOHelper.DAOFactory.OperationDAO.InitializeIntermediateWorkPieces (newOperation);
          IWorkOrder newWorkOrder = Lemoine.Business.Operation.OperationRelations
            .TryToGuessWorkOrderFromOperation (newOperation);
          if (object.Equals (previousWorkOrder, newWorkOrder)) {
            // Check there is not another work order in database
            // No for the moment because it does not make sense to have a very short period with a new work order
            // but no operation
            workOrder = previousWorkOrder;
          }
        }
        return (null != component) || (null != workOrder);
      }
      else { // There is a gap
        Debug.Assert (newPeriodBegin.HasValue);
        Debug.Assert (previousPeriodEnd.HasValue);
        UtcDateTimeRange gap = new UtcDateTimeRange (previousPeriodEnd.Value, newPeriodBegin.Value);
        Debug.Assert (gap.Duration.HasValue);

        if (IsAutoSequenceActivity (gap.Lower.Value, gap.Upper.Value)) {
          // Auto-sequence activity in gap, return false
          log.DebugFormat ("ProcessAutoComponent: " +
                           "auto-sequence activity in gap {0} " +
                           "=> return false",
                           gap);
          return false;
        }

        // - Check component
        if ((null != previousComponent)
            && (gap.Duration.Value < Lemoine.Business.Config.AnalysisConfigHelper.AutoComponentSame)) {
          ModelDAOHelper.DAOFactory.OperationDAO
            .InitializeIntermediateWorkPieces (newOperation);
          IComponent newComponent = Lemoine.Business.Operation.OperationRelations
            .TryToGuessComponentFromOperation (newOperation);
          if (object.Equals (previousComponent, newComponent)) {
            // Check there is not another component in database in this period ?
            // No for the moment because it does not make sense to have a very short period with a new component
            // but no operation
            component = previousComponent;
          }
        }
        // - Check work order
        if ((null != previousWorkOrder)
            && (gap.Duration.Value < Lemoine.Business.Config.AnalysisConfigHelper.AutoWorkOrderSame)) {
          ModelDAOHelper.DAOFactory.OperationDAO.InitializeIntermediateWorkPieces (newOperation);
          IWorkOrder newWorkOrder = Lemoine.Business.Operation.OperationRelations
            .TryToGuessWorkOrderFromOperation (newOperation);
          if (object.Equals (previousWorkOrder, newWorkOrder)) {
            // Check there is not another work order in database
            // No for the moment because it does not make sense to have a very short period with a new work order
            // but no operation
            workOrder = previousWorkOrder;
          }
        }

        // - Apply the changes
        if (null != component) { // Apply the component asynchronously
          IComponentMachineAssociation association =
            ModelDAOHelper.ModelFactory.CreateComponentMachineAssociation (m_monitoredMachine,
                                                                           component,
                                                                           gap);
          association.Option = AssociationOption.Detected;
          association.Auto = true;
          ModelDAOHelper.DAOFactory.ComponentMachineAssociationDAO
            .MakePersistent (association);
        }
        if (null != workOrder) { // Apply the work order asynchronously
          IWorkOrderMachineAssociation association =
            ModelDAOHelper.ModelFactory.CreateWorkOrderMachineAssociation (m_monitoredMachine,
                                                                           workOrder,
                                                                           gap);
          association.Option = AssociationOption.Detected;
          association.Auto = true;
          ModelDAOHelper.DAOFactory.WorkOrderMachineAssociationDAO
            .MakePersistent (association);
        }
        return (null != component) || (null != workOrder);
      }
    }

    /// <summary>
    /// Check if there is an auto-sequence activity between two date/times
    /// 
    /// A margin AutOperationMargin is applied here
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    bool IsAutoSequenceActivity (DateTime begin, DateTime end)
    {
      TimeSpan autoOperationMargin = Lemoine.Business.Config.AnalysisConfigHelper.AutoOperationMargin;
      DateTime noSequencePeriodBegin = begin.Add (autoOperationMargin);
      DateTime noSequencePeriodEnd = end.Subtract (autoOperationMargin);
      if (Bound.Compare<DateTime> (noSequencePeriodBegin, noSequencePeriodEnd) < 0) {
        bool existsNoSequenceSlotBetween =
          ModelDAOHelper.DAOFactory.SequenceSlotDAO
          .ExistsNoSequenceBetween (m_monitoredMachine,
                                    noSequencePeriodBegin,
                                    noSequencePeriodEnd);
        if (existsNoSequenceSlotBetween) {
          log.DebugFormat ("IsAutoSequenceActivity: " +
                           "some auto-sequence activity was detected " +
                           "in corrected period {0}-{1}",
                           noSequencePeriodBegin, noSequencePeriodEnd);
          return true;
        }
        else {
          log.DebugFormat ("IsAutoSequenceActivity: " +
                           "no auto-sequence activity in corrected period {0}-{1}",
                           noSequencePeriodBegin, noSequencePeriodEnd);
          return false;
        }
      }
      else {
        log.DebugFormat ("IsAutoSequenceActivity: " +
                         "corrected period {0}-{1} is empty " +
                         "=> return false",
                         noSequencePeriodBegin, noSequencePeriodEnd);
        return false;
      }
    }

    void Propagate (PropagationOption propagationOption, IOperationMachineAssociation association, IWorkOrder workOrder, IComponent component, ILine line, ITask task, IList<IOperationSlot> betweenSlots)
    {
      if (propagationOption.IsActive ()
          && ((null != workOrder) || (null != component) || (null != line) || (null != task))) {
        // Note: The case when there is a change of workOrder / component / line or task in range,
        //       is not specifically handled because it would complexify a little bit too much the process.
        //       To simplify it, in case there is such a case, the auto-operation propagation is not processed
        bool workOrderPropagation = propagationOption.HasFlag (PropagationOption.WorkOrder);
        bool componentPropagation = propagationOption.HasFlag (PropagationOption.Component);
        bool linePropagation = propagationOption.HasFlag (PropagationOption.Line);
        bool taskPropagation = propagationOption.HasFlag (PropagationOption.Task);
        if (workOrderPropagation && (null != workOrder)) {
          foreach (var betweenSlot in betweenSlots) {
            if ((null != betweenSlot.WorkOrder)
                && !object.Equals (betweenSlot.WorkOrder, workOrder)) {
              log.DebugFormat ("Propagate: " +
                               "do not propagate the work order {0} because of the existing slot {1}",
                               workOrder, betweenSlot);
              workOrderPropagation = false;
              break;
            }
          }
        }
        if (componentPropagation && (null != component)) {
          foreach (var betweenSlot in betweenSlots) {
            if ((null != betweenSlot.Component)
                && !object.Equals (betweenSlot.Component, component)) {
              log.DebugFormat ("Propagate: " +
                               "do not propagate the component {0} because of the existing slot {1}",
                               component, betweenSlot);
              componentPropagation = false;
              break;
            }
          }
        }
        if (linePropagation && (null != line)) {
          foreach (var betweenSlot in betweenSlots) {
            if ((null != betweenSlot.Line)
                && !object.Equals (betweenSlot.Line, line)) {
              log.DebugFormat ("Propagate: " +
                               "do not propagate the line {0} because of the existing slot {1}",
                               line, betweenSlot);
              linePropagation = false;
              workOrderPropagation = false;
              break;
            }
          }
        }
        if (taskPropagation && (null != task)) {
          foreach (var betweenSlot in betweenSlots) {
            if ((null != betweenSlot.Task)
                && !object.Equals (betweenSlot.Task, task)) {
              log.DebugFormat ("Propagate: " +
                               "do not propagate the task {0} because of the existing slot {1}",
                               task, betweenSlot);
              taskPropagation = false;
              workOrderPropagation = false;
              break;
            }
          }
        }
        if (workOrderPropagation) {
          ((Lemoine.GDBPersistentClasses.OperationMachineAssociation)association).WorkOrder = workOrder;
        }
        if (componentPropagation) {
          ((Lemoine.GDBPersistentClasses.OperationMachineAssociation)association).Component = component;
        }
        if (linePropagation) {
          ((Lemoine.GDBPersistentClasses.OperationMachineAssociation)association).Line = line;
        }
        if (taskPropagation) {
          ((Lemoine.GDBPersistentClasses.OperationMachineAssociation)association).Task = task;
        }
      }
    }

    /// <summary>
    /// Guess the next task for a specified operation
    /// </summary>
    /// <param name="operation">not null</param>
    /// <param name="previousOperationSlot"></param>
    /// <returns></returns>
    ITask GuessNextTask (IOperation operation, IOperationSlot previousOperationSlot)
    {
      Debug.Assert (null != operation);

      IList<ITask> nextTasks = ModelDAOHelper.DAOFactory.TaskDAO.GetNext (m_monitoredMachine,
                                                                          operation);
      if (0 == nextTasks.Count) {
        if ((null != previousOperationSlot) && (null != previousOperationSlot.Task)) {
          // Log it only for the moment if a task was previously defined
          string message = string.Format ("No next task found for {0}",
                                          operation);
          AddTaskDetectionLog (LogLevel.NOTICE,
                               message);
          log.InfoFormat ("GuessNextTask: " +
                          "{0}",
                          message);
        }
        return null;
      }
      else {
        if (1 < nextTasks.Count) {
          if ((null != nextTasks[0].Machine)
              && nextTasks[0].Order.HasValue
              && (!nextTasks[1].Order.HasValue
                  || !object.Equals (nextTasks[0].Order.Value,
                                     nextTasks[1].Order.Value))) {
            log.DebugFormat ("GuessNextTask: " +
                             "the next task was fully determined by a machine and an order attribute");
          }
          else {
            // Log it, if there are more than one possible task
            string message = string.Format ("{0} next possible tasks for {1}",
                                            nextTasks.Count, operation);
            AddTaskDetectionLog (LogLevel.INFO,
                                 message);
            log.DebugFormat ("GuessNextTask: " +
                             "{0}",
                             message);
          }
        }
        return nextTasks[0];
      }
    }

    /// <summary>
    /// Apply the next task
    /// </summary>
    /// <param name="task">not null</param>
    /// <param name="from"></param>
    void ApplyTask (ITask task, DateTime from)
    {
      Debug.Assert (null != task);

      log.InfoFormat ("ApplyTask: " +
                      "apply task {0}",
                      task);
      var taskAssociation = new Lemoine.GDBPersistentClasses
        .WorkOrderMachineAssociation (m_monitoredMachine,
                                      new UtcDateTimeRange (from),
                                      null, // transient
                                      true);
      taskAssociation.WorkOrder = task.WorkOrder;
      taskAssociation.Task = task;
      ((Lemoine.GDBPersistentClasses.WorkOrderMachineAssociation)taskAssociation).AutoTask = true;
      taskAssociation.Caller = m_caller;
      taskAssociation.Apply ();
      // TODO: event ?
    }

    /// <summary>
    /// Extend an operation (the operation if specified) until the specified date/time
    /// 
    /// The top transaction is set in the parent function
    /// </summary>
    /// <param name="operation">may be null</param>
    /// <param name="dateTime"></param>
    public void ExtendOperation (IOperation operation,
                                 DateTime dateTime)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ("Detection.ExtendOperation")) {
        bool success = true;
        try {
          log.DebugFormat ("ExtendOperation: " +
                           "extend operation {0} until {1}",
                           operation, dateTime);
          IOperationSlot operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .GetLastOperationNotNullBefore (m_monitoredMachine, dateTime);
          if (null != operationSlot) {
            Debug.Assert (null != operationSlot.Operation);
            if ((null == operation) || operation.Equals (operationSlot.Operation)) {
              // Last operation slot matches
              if (Bound.Compare<DateTime> (operationSlot.EndDateTime, dateTime) < 0) {
                // Not long enough, extend it
                log.DebugFormat ("ExtendOperation: " +
                                 "extend the last operation slot {0} to {1}",
                                 operationSlot, dateTime);
                var association =
                  new Lemoine.GDBPersistentClasses.OperationMachineAssociation (m_monitoredMachine,
                                                                                operationSlot.BeginDateTime.Value,
                                                                                null, // Transient !
                                                                                true);
                association.Operation = operationSlot.Operation;
                // Note: extend the work order / part / line and task as well
                if (Lemoine.Business.Config.AnalysisConfigHelper.AutoOperationPropagation.IsActive ()
                    && ((null != operationSlot.WorkOrder) || (null != operationSlot.Component)
                        || (null != operationSlot.Line) || (null != operationSlot.Task))) {
                  IList<IOperationSlot> betweenSlots = new List<IOperationSlot> ();
                  if (!operationSlot.DateTimeRange.ContainsElement (dateTime)) {
                    Debug.Assert (operationSlot.DateTimeRange.Upper.HasValue); // Else it would contain range
                    betweenSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
                      .FindOverlapsRange (m_monitoredMachine, new UtcDateTimeRange (operationSlot.DateTimeRange.Upper.Value,
                                                                                    dateTime));
                  }
                  Propagate (Lemoine.Business.Config.AnalysisConfigHelper.ExtendOperationPropagation,
                             association, operationSlot.WorkOrder, operationSlot.Component,
                             operationSlot.Line, operationSlot.Task, betweenSlots);
                }
                association.End = dateTime;
                association.Caller = m_caller;
                association.Apply ();
                return;
              }
              else { // Nothing to do, the operation slot is already associated to the right operation
                log.DebugFormat ("ExtendOperation: " +
                                 "nothing to do, the operation slot at {0} is already associated to {1}",
                                 dateTime, operation);
                return;
              }
            }

            // The previous operation slot did not match, give up
            log.WarnFormat ("ExtendOperation: " +
                            "the previous operation slot did not match, give up");
            { // Log it
              IDetectionAnalysisLog detectionAnalysisLog = ModelDAOHelper.ModelFactory
                .CreateDetectionAnalysisLog (LogLevel.WARN,
                                             string.Format ("Operation {0} could not be extended to {1} " +
                                                            "because the previous operation slot did not match",
                                                            operation, dateTime),
                                             m_monitoredMachine,
                                             null);
              ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
                .MakePersistent (detectionAnalysisLog);
            }
            return;
          }
          else { // null == operationSlot
            log.WarnFormat ("ExtendOperation: " +
                            "there is no operation slot strictly before {0}",
                            dateTime);
            { // Log it
              IDetectionAnalysisLog detectionAnalysisLog = ModelDAOHelper.ModelFactory
                .CreateDetectionAnalysisLog (LogLevel.WARN,
                                             string.Format ("Operation {0} could not be extended to {1} " +
                                                            "because there is no operation slot before {1}",
                                                            operation, dateTime),
                                             m_monitoredMachine,
                                             null);
              ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
                .MakePersistent (detectionAnalysisLog);
            }
            return;
          }
        }
        catch (Exception ex) {
          log.ErrorFormat ("ExtendOperation: " +
                           "Operation={0} dateTime={1} " +
                           "exception={2}",
                           operation, dateTime,
                           ex);
          success = false;
          throw;
        }
        finally {
          if (success) {
            transaction.Commit ();

            try {
              foreach (var extension in m_afterExtensions) {
                extension.ExtendOperation (operation, dateTime);
              }
            }
            catch (Exception ex) {
              log.Error ("ExtendOperation: extension exception", ex);
              throw;
            }
          }
        }
      } // session / transaction
    }

    /// <summary>
    /// Add a task detection log
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    void AddTaskDetectionLog (LogLevel level,
                              string message)
    {
      AddDetectionAnalysisLog (level,
                               "taskdetection",
                               message);
    }

    /// <summary>
    /// Add a detection analysis log
    /// </summary>
    /// <param name="level"></param>
    /// <param name="module"></param>
    /// <param name="message"></param>
    void AddDetectionAnalysisLog (LogLevel level,
                                  string module,
                                  string message)
    {
      log.DebugFormat ("AddDetectionAnalysisLog: " +
                       "add a detection analysis log with message={0} level={1} module={2}",
                       message, level, module);
      IDetectionAnalysisLog detectionAnalysisLog = ModelDAOHelper.ModelFactory
        .CreateDetectionAnalysisLog (level, message, m_monitoredMachine);
      ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
        .MakePersistent (detectionAnalysisLog);
    }

  }
}

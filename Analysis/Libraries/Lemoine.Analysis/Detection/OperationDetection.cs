// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
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

    readonly IMonitoredMachine m_monitoredMachine;
    readonly Lemoine.Threading.IChecked m_caller;
    IEnumerable<Lemoine.Extensions.Analysis.IOperationDetectionExtension> m_operationDetectionExtensions = null;
    readonly IEnumerable<IAfterOperationDetectionExtension> m_afterExtensions;

    /// <summary>
    /// Associated machine
    /// </summary>
    public IMonitoredMachine Machine => m_monitoredMachine;

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
      m_operationDetectionExtensions = extensions.ToList ();
      m_caller = caller;
      m_afterExtensions = ServiceProvider
        .Get<IEnumerable<IAfterOperationDetectionExtension>> (new Lemoine.Business.Extension.MonitoredMachineExtensions<IAfterOperationDetectionExtension> (monitoredMachine, (ext, m) => ext.Initialize (m)))
        .ToList ();

      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{monitoredMachine.Id}");
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
        if (log.IsDebugEnabled) {
          log.Debug ($"StartOperation: insert operation {((Lemoine.Collections.IDataWithId<int>)operation)?.Id} from {dateTime}");
        }
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (var transaction = session.BeginTransaction ("Detection.StartOperation", notTop: true)) {
          AddOperationOnly (operation, new UtcDateTimeRange (dateTime, dateTime.AddSeconds (1)), true);
          transaction.Commit ();
        }
      }
      catch (Exception ex) {
        log.Error ($"StartOperation: Operation={((Lemoine.Collections.IDataWithId<int>)operation)?.Id} dateTime={dateTime}", ex);
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
    /// <param name="autoOperation">use auto-operation, set false to optimize the process if it is not required</param>
    public void AddOperation (IOperation operation, UtcDateTimeRange range, bool autoOperation = true)
    {
      try {
        if (log.IsDebugEnabled) {
          log.Debug ($"AddOperation: add operation {((Lemoine.Collections.IDataWithId<int>)operation)?.Id} in {range}");
        }
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ("Detection.AddOperation", notTop: true)) {
          AddOperationOnly (operation, range, autoOperation);
          transaction.Commit ();
        }
      }
      catch (Exception ex) {
        log.Error ($"AddOperation: Operation={((Lemoine.Collections.IDataWithId<int>)operation)?.Id} range={range}", ex);
        throw;
      }

      try {
        foreach (var extension in m_afterExtensions) {
          extension.AddOperation (operation, range, autoOperation);
        }
      }
      catch (Exception ex) {
        log.Error ("AddOperation: extension exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Add an operation (without calling the extension point afterwards)
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
    public void AddOperationOnly (IOperation operation, DateTime begin, UpperBound<DateTime> end, bool autoOperation)
    {
      AddOperationOnly (operation, new UtcDateTimeRange (begin, end), autoOperation);
    }

    /// <summary>
    /// Add an operation (without calling the extension point afterwards)
    /// 
    /// Note:
    /// - for multi-machine module machines, the call of AddOperation is not always chronological
    /// - because 'Auto-Only' and 'Not auto-only' sequences are processed separately, the call to AddOperation
    ///   could not be chronological
    /// </summary>
    /// <param name="operation">Not null</param>
    /// <param name="range"></param>
    /// <param name="autoOperation"></param>
    public void AddOperationOnly (IOperation operation, UtcDateTimeRange range, bool autoOperation)
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
            IManufacturingOrder manufacturingOrder = null;
            IOperationSlot previousOperationSlot = null;

            // - Get the previous operation slot for an eventual auto-operation or a manufacturingOrder change
            //   or if the extension requires it
            bool previousOperationSlotRequired = autoOperation || Lemoine.Business.Config.AnalysisConfigHelper.ManufacturingOrderManagement;
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

            // - Next manufacturingOrder management if the operation changes
            if (((null == previousOperationSlot)
                  || !localOperation.Equals (previousOperationSlot.Operation))
                && Lemoine.Business.Config.AnalysisConfigHelper.ManufacturingOrderManagement) {
              Debug.Assert (range.Lower.HasValue);
              IManufacturingOrder nextManufacturingOrder = GuessNextManufacturingOrder (localOperation, previousOperationSlot);
              if (null != nextManufacturingOrder) {
                ApplyManufacturingOrder (nextManufacturingOrder, range.Lower.Value);
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
                  manufacturingOrder = previousOperationSlot.ManufacturingOrder;
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
                      manufacturingOrder = previousOperationSlot.ManufacturingOrder;
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
            // Propagate the work order / component / line / manufacturingOrder
            if (autoOperation && Lemoine.Business.Config.AnalysisConfigHelper.AutoOperationPropagation.IsActive ()
                && ((null != workOrder) || (null != component) || (null != line) || (null != manufacturingOrder))) {
              Debug.Assert (null != previousOperationSlot); // Else all of workOrder/component/line/manufacturingOrder would be null
              Debug.Assert (range.Lower.HasValue); // Else previousOperationSlot would be null
              IList<IOperationSlot> betweenSlots = new List<IOperationSlot> ();
              if (!previousOperationSlot.DateTimeRange.ContainsRange (range)) {
                Debug.Assert (previousOperationSlot.DateTimeRange.Upper.HasValue); // Else it would contain range
                betweenSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
                  .FindOverlapsRange (m_monitoredMachine, new UtcDateTimeRange (previousOperationSlot.DateTimeRange.Upper.Value,
                                                                                range.Upper));
              }
              Propagate (Lemoine.Business.Config.AnalysisConfigHelper.AutoOperationPropagation,
                         association, workOrder, component, line, manufacturingOrder, betweenSlots);
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
          if (log.IsDebugEnabled) {
            log.Debug ($"IsAutoSequenceActivity: some auto-sequence activity was detected in corrected period {noSequencePeriodBegin}-{noSequencePeriodEnd}");
          }
          return true;
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"IsAutoSequenceActivity: no auto-sequence activity in corrected period {noSequencePeriodBegin}-{noSequencePeriodEnd}");
          }
          return false;
        }
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsAutoSequenceActivity: corrected period {noSequencePeriodBegin}-{noSequencePeriodEnd} is empty => return false");
        }
        return false;
      }
    }

    void Propagate (PropagationOption propagationOption, IOperationMachineAssociation association, IWorkOrder workOrder, IComponent component, ILine line, IManufacturingOrder manufacturingOrder, IList<IOperationSlot> betweenSlots)
    {
      if (propagationOption.IsActive ()
          && ((null != workOrder) || (null != component) || (null != line) || (null != manufacturingOrder))) {
        // Note: The case when there is a change of workOrder / component / line or manufacturingOrder in range,
        //       is not specifically handled because it would complexify a little bit too much the process.
        //       To simplify it, in case there is such a case, the auto-operation propagation is not processed
        bool workOrderPropagation = propagationOption.HasFlag (PropagationOption.WorkOrder);
        bool componentPropagation = propagationOption.HasFlag (PropagationOption.Component);
        bool linePropagation = propagationOption.HasFlag (PropagationOption.Line);
        bool manufacturingOrderPropagation = propagationOption.HasFlag (PropagationOption.ManufacturingOrder);
        if (workOrderPropagation && (null != workOrder)) {
          foreach (var betweenSlot in betweenSlots) {
            if ((null != betweenSlot.WorkOrder)
                && !object.Equals (betweenSlot.WorkOrder, workOrder)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"Propagate: do not propagate the work order {workOrder} because of the existing slot {betweenSlot}");
              }
              workOrderPropagation = false;
              break;
            }
          }
        }
        if (componentPropagation && (null != component)) {
          foreach (var betweenSlot in betweenSlots) {
            if ((null != betweenSlot.Component)
                && !object.Equals (betweenSlot.Component, component)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"Propagate: do not propagate the component {component} because of the existing slot {betweenSlot}");
              }
              componentPropagation = false;
              break;
            }
          }
        }
        if (linePropagation && (null != line)) {
          foreach (var betweenSlot in betweenSlots) {
            if ((null != betweenSlot.Line)
                && !object.Equals (betweenSlot.Line, line)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"Propagate: do not propagate the line {line} because of the existing slot {betweenSlot}");
              }
              linePropagation = false;
              workOrderPropagation = false;
              break;
            }
          }
        }
        if (manufacturingOrderPropagation && (null != manufacturingOrder)) {
          foreach (var betweenSlot in betweenSlots) {
            if ((null != betweenSlot.ManufacturingOrder)
                && !object.Equals (betweenSlot.ManufacturingOrder, manufacturingOrder)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"Propagate: do not propagate the manufacturingOrder {manufacturingOrder} because of the existing slot {betweenSlot}");
              }
              manufacturingOrderPropagation = false;
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
        if (manufacturingOrderPropagation) {
          ((Lemoine.GDBPersistentClasses.OperationMachineAssociation)association).ManufacturingOrder = manufacturingOrder;
        }
      }
    }

    /// <summary>
    /// Guess the next manufacturingOrder for a specified operation
    /// </summary>
    /// <param name="operation">not null</param>
    /// <param name="previousOperationSlot"></param>
    /// <returns></returns>
    IManufacturingOrder GuessNextManufacturingOrder (IOperation operation, IOperationSlot previousOperationSlot)
    {
      Debug.Assert (null != operation);

      IList<IManufacturingOrder> nextManufacturingOrders = ModelDAOHelper.DAOFactory.ManufacturingOrderDAO.GetNext (m_monitoredMachine,
                                                                          operation);
      if (0 == nextManufacturingOrders.Count) {
        if ((null != previousOperationSlot) && (null != previousOperationSlot.ManufacturingOrder)) {
          // Log it only for the moment if a manufacturingOrder was previously defined
          var message = $"No next manufacturingOrder found for {operation}";
          AddManufacturingOrderDetectionLog (LogLevel.NOTICE,
                               message);
          log.Info ($"GuessNextManufacturingOrder: {message}");
        }
        return null;
      }
      else {
        if (1 < nextManufacturingOrders.Count) {
          if ((null != nextManufacturingOrders[0].Machine)
              && nextManufacturingOrders[0].Order.HasValue
              && (!nextManufacturingOrders[1].Order.HasValue
                  || !object.Equals (nextManufacturingOrders[0].Order.Value,
                                     nextManufacturingOrders[1].Order.Value))) {
            log.Debug ("GuessNextManufacturingOrder: the next manufacturingOrder was fully determined by a machine and an order attribute");
          }
          else {
            // Log it, if there are more than one possible manufacturingOrder
            string message = $"{nextManufacturingOrders.Count} next possible manufacturingOrders for {operation}";
            AddManufacturingOrderDetectionLog (LogLevel.INFO,
                                 message);
            if (log.IsDebugEnabled) {
              log.Debug ($"GuessNextManufacturingOrder: {message}");
            }
          }
        }
        return nextManufacturingOrders[0];
      }
    }

    /// <summary>
    /// Apply the next manufacturingOrder
    /// </summary>
    /// <param name="manufacturingOrder">not null</param>
    /// <param name="from"></param>
    void ApplyManufacturingOrder (IManufacturingOrder manufacturingOrder, DateTime from)
    {
      Debug.Assert (null != manufacturingOrder);

      log.Info ($"ApplyManufacturingOrder: apply manufacturingOrder {manufacturingOrder}");
      var manufacturingOrderAssociation = new Lemoine.GDBPersistentClasses
        .WorkOrderMachineAssociation (m_monitoredMachine,
                                      new UtcDateTimeRange (from),
                                      null, // transient
                                      true);
      manufacturingOrderAssociation.WorkOrder = manufacturingOrder.WorkOrder;
      manufacturingOrderAssociation.ManufacturingOrder = manufacturingOrder;
      ((Lemoine.GDBPersistentClasses.WorkOrderMachineAssociation)manufacturingOrderAssociation).AutoManufacturingOrder = true;
      manufacturingOrderAssociation.Caller = m_caller;
      manufacturingOrderAssociation.Apply ();
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
          if (log.IsDebugEnabled) {
            log.Debug ($"ExtendOperation: extend operation {operation} until {dateTime}");
          }
          IOperationSlot operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .GetLastOperationNotNullBefore (m_monitoredMachine, dateTime);
          if (null != operationSlot) {
            Debug.Assert (null != operationSlot.Operation);
            if ((null == operation) || operation.Equals (operationSlot.Operation)) {
              // Last operation slot matches
              if (Bound.Compare<DateTime> (operationSlot.EndDateTime, dateTime) < 0) {
                // Not long enough, extend it
                if (log.IsDebugEnabled) {
                  log.Debug ($"ExtendOperation: extend the last operation slot {operationSlot} to {dateTime}");
                }
                var association =
                  new Lemoine.GDBPersistentClasses.OperationMachineAssociation (m_monitoredMachine,
                                                                                operationSlot.BeginDateTime.Value,
                                                                                null, // Transient !
                                                                                true);
                association.Operation = operationSlot.Operation;
                // Note: extend the work order / part / line and manufacturing order as well
                if (Lemoine.Business.Config.AnalysisConfigHelper.AutoOperationPropagation.IsActive ()
                    && ((null != operationSlot.WorkOrder) || (null != operationSlot.Component)
                        || (null != operationSlot.Line) || (null != operationSlot.ManufacturingOrder))) {
                  IList<IOperationSlot> betweenSlots = new List<IOperationSlot> ();
                  if (!operationSlot.DateTimeRange.ContainsElement (dateTime)) {
                    Debug.Assert (operationSlot.DateTimeRange.Upper.HasValue); // Else it would contain range
                    betweenSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
                      .FindOverlapsRange (m_monitoredMachine, new UtcDateTimeRange (operationSlot.DateTimeRange.Upper.Value,
                                                                                    dateTime));
                  }
                  Propagate (Lemoine.Business.Config.AnalysisConfigHelper.ExtendOperationPropagation,
                             association, operationSlot.WorkOrder, operationSlot.Component,
                             operationSlot.Line, operationSlot.ManufacturingOrder, betweenSlots);
                }
                association.End = dateTime;
                association.Caller = m_caller;
                association.Apply ();
                return;
              }
              else { // Nothing to do, the operation slot is already associated to the right operation
                if (log.IsDebugEnabled) {
                  log.Debug ($"ExtendOperation: nothing to do, the operation slot at {dateTime} is already associated to {operation}");
                }
                return;
              }
            }

            // The previous operation slot did not match, give up
            log.Warn ("ExtendOperation: the previous operation slot did not match, give up");
            { // Log it
              IDetectionAnalysisLog detectionAnalysisLog = ModelDAOHelper.ModelFactory
                .CreateDetectionAnalysisLog (LogLevel.WARN,
                                             $"Operation {operation} could not be extended to {dateTime} because the previous operation slot did not match",
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
                                             $"Operation {operation} could not be extended to {dateTime} because there is no operation slot before {dateTime}",
                                             m_monitoredMachine,
                                             null);
              ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
                .MakePersistent (detectionAnalysisLog);
            }
            return;
          }
        }
        catch (Exception ex) {
          log.Error ($"ExtendOperation: Operation={operation} dateTime={dateTime}", ex);
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
    /// Stop an operation at a specific date/time
    /// 
    /// The top transaction must be created by the parent function
    /// </summary>
    /// <param name="range"></param>
    public void StopOperation (DateTime dateTime)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"StopOperation: stop operation at {dateTime}");
      }
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ("Detection.StopOperation", notTop: true)) {
        try {
          // Extension
          foreach (var extension in GetExtensions ()) {
            extension.StopOperation (m_monitoredMachine, dateTime);
          }

          if (log.IsDebugEnabled) {
            log.Debug ($"StopOperation: after extensions, stop operation at {dateTime}");
          }
          var association =
            new Lemoine.GDBPersistentClasses.OperationMachineAssociation (m_monitoredMachine, new UtcDateTimeRange (dateTime), null, true); // Transient
          association.Operation = null;
          association.Option = AssociationOption.Detected;
          association.Caller = m_caller;
          association.Apply ();
          transaction.Commit ();
        }
        catch (Exception ex) {
          if (ExceptionTest.IsStale (ex, log)) {
            log.Warn ($"StopOperation: stale object state exception", ex);
            transaction.Rollback (); // To remove some error logs that are associated to an implicit rollback
          }
          else if (ExceptionTest.IsTemporary (ex, log)) {
            log.Warn ($"StopOperation: temporary (serialization) failure", ex);
            transaction.Rollback (); // To remove some error logs that are associated to an implicit rollback
          }
          else {
            log.Error ($"StopOperation: exception", ex);
          }
          throw;
        }
      }

      try {
        foreach (var extension in m_afterExtensions) {
          extension.StopOperation (dateTime);
        }
      }
      catch (Exception ex) {
        log.Error ("StopOperation: extension exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Add a manufacturing order detection log
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    void AddManufacturingOrderDetectionLog (LogLevel level,
                              string message)
    {
      AddDetectionAnalysisLog (level,
                               "manufacturingorderdetection",
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
      if (log.IsDebugEnabled) {
        log.Debug ($"AddDetectionAnalysisLog: add a detection analysis log with message={message} level={level} module={module}");
      }
      IDetectionAnalysisLog detectionAnalysisLog = ModelDAOHelper.ModelFactory
        .CreateDetectionAnalysisLog (level, message, m_monitoredMachine);
      ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
        .MakePersistent (detectionAnalysisLog);
    }

  }
}

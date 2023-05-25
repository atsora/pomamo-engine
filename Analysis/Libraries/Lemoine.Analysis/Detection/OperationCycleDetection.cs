// Copyright (C) 2009-2023 Lemoine Automation Technologies, 2023 Nicolas Relange
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business;
using Lemoine.Extensions.Analysis;
using Lemoine.Collections;

namespace Lemoine.Analysis.Detection
{
  /// <summary>
  /// Utility class to use each time an operation cycle begin/end is detected
  /// </summary>
  public class OperationCycleDetection : Lemoine.Extensions.Analysis.Detection.IOperationCycleDetection, Lemoine.Threading.IChecked
  {
    readonly ILog log;

    static readonly string SKIP_EMPTY_BETWEENCYCLES_KEY = "Analysis.SkipEmptyBetweenCycles";
    static readonly bool SKIP_EMPTY_BETWEENCYCLES_DEFAULT = false;

    static readonly string SKIP_EMPTY_BETWEEN_IF_PREVIOUS_EMPTY_KEY = "Analysis.SkipEmptyBetweenIfPreviousEmpty";
    static readonly bool SKIP_EMPTY_BETWEEN_IF_PREVIOUS_EMPTY_DEFAULT = true;

    #region Members
    readonly IMonitoredMachine m_monitoredMachine;
    readonly Lemoine.Threading.IChecked m_caller;

    IEnumerable<Lemoine.Extensions.Analysis.IOperationCycleDetectionExtension> m_operationCycleDetectionExtensions;
    IEnumerable<Lemoine.Extensions.Analysis.IOperationCycleFullDetectionExtension> m_operationCycleFullDetectionExtensions;
    readonly IEnumerable<IAfterCycleDetectionExtension> m_afterExtensions;
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
    public OperationCycleDetection (IMonitoredMachine monitoredMachine,
                                    IEnumerable<Lemoine.Extensions.Analysis.IDetectionExtension> extensions,
                                    Lemoine.Threading.IChecked caller)
    {
      Debug.Assert (null != monitoredMachine);
      Debug.Assert (null != extensions);

      m_monitoredMachine = monitoredMachine;
      m_operationCycleDetectionExtensions = extensions.OfType<Lemoine.Extensions.Analysis.IOperationCycleDetectionExtension> ();
      m_operationCycleFullDetectionExtensions = extensions.OfType<Lemoine.Extensions.Analysis.IOperationCycleFullDetectionExtension> ();
      m_caller = caller;
      m_afterExtensions = ServiceProvider
        .Get<IEnumerable<IAfterCycleDetectionExtension>> (new Lemoine.Business.Extension.MonitoredMachineExtensions<IAfterCycleDetectionExtension> (monitoredMachine, (ext, m) => ext.Initialize (m)));

      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                 this.GetType ().FullName,
                                                 monitoredMachine.Id));
    }

    /// <summary>
    /// Constructor to set the monitored machine for the unit tests
    /// </summary>
    /// <param name="monitoredMachine">not null</param>
    /// <param name="extensions">not null</param>
    internal protected OperationCycleDetection (IMonitoredMachine monitoredMachine,
                                    IEnumerable<Lemoine.Extensions.Analysis.IDetectionExtension> extensions)
      : this (monitoredMachine, extensions, null)
    { }
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
    /// Get the extensions of type IOperationCycleDetectionExtension
    /// </summary>
    /// <returns></returns>
    IEnumerable<Lemoine.Extensions.Analysis.IOperationCycleDetectionExtension> GetExtensions ()
    {
      return m_operationCycleDetectionExtensions;
    }

    bool IsFull (IOperationCycle operationCycle)
    {
      foreach (var fullExtension in m_operationCycleFullDetectionExtensions) {
        var extensionResult = fullExtension.IsFull (operationCycle);
        if (extensionResult.HasValue) {
          log.DebugFormat ("IsFull: " +
                           "{0} from extension {1}",
                           extensionResult.Value, fullExtension);
          return extensionResult.Value;
        }
      }

      return operationCycle.End.HasValue
        && !operationCycle.Status.HasFlag (OperationCycleStatus.EndEstimated);
    }

    /// <summary>
    /// Start an OperationCycle
    /// 
    /// The top transaction must be created in the parent function
    /// </summary>
    /// <param name="dateTime"></param>
    public void StartCycle (DateTime dateTime)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ("Detection.StartCycle")) // This is not the top transaction
    {
        // . Check there the specified date/time
        //   is valid regarding the existing OperationCycles
        //   else raise an error and skip the process
        bool noOperationCycleStrictlyAfter =
          ModelDAOHelper.DAOFactory.OperationCycleDAO
          .CheckNoOperationCycleStrictlyAfter (m_monitoredMachine,
                                               dateTime);
        if (!noOperationCycleStrictlyAfter) {
          RaiseInvalidDateTime (dateTime);
          transaction.Commit (); // To store the log
          return;
        }

        // Get last operation cycle
        IOperationCycle lastOperationCycle =
          ModelDAOHelper.DAOFactory.OperationCycleDAO.GetLast (m_monitoredMachine);
        if (null != lastOperationCycle) {
          lastOperationCycle.SetCheckedCaller (this);
        }

        // . Start a new OperationCycle
        if (log.IsDebugEnabled) {
          log.Debug ($"StartCycle: Create a new operation cycle starting at {dateTime}");
        }

        IOperationCycle operationCycle =
          ModelDAOHelper.ModelFactory
          .CreateOperationCycle (m_monitoredMachine);
        operationCycle.SetCheckedCaller (this);
        operationCycle.Begin = dateTime;
        ModelDAOHelper.DAOFactory.OperationCycleDAO
          .MakePersistent (operationCycle);

        // . Check if any OperationSlot matches
        //   this new OperationCycle
        // .. Get the operation slot at the given date/time
        IOperationSlot operationSlot =
          ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindAt (m_monitoredMachine,
                   dateTime);
        if (null != operationSlot) {
          operationSlot.SetCheckedCaller (this);
          // .. If it exists, associate the new operation cycle to it
          if (log.IsDebugEnabled) {
            log.DebugFormat ("StartCycle: " +
                             "associate the new partial operation cycle {0}" +
                             "to operation slot {1}",
                             operationCycle,
                             operationSlot);
          }
          operationCycle.OperationSlot = operationSlot;
          if (operationSlot.EndDateTime.HasValue) {
            operationCycle.SetEstimatedEnd (operationSlot.EndDateTime.Value);
          }
        }

        // if previous cycle has an estimated end and is in the same slot (or both have no slot)
        // it gets an estimated end
        Debug.Assert (null != operationCycle);
        if ((lastOperationCycle != null)
            && (!lastOperationCycle.End.HasValue
                || lastOperationCycle.Status.HasFlag (OperationCycleStatus.EndEstimated))) {
          if (null == operationCycle.OperationSlot) {
            if (null == lastOperationCycle.OperationSlot) {
              // in case there is no slot, check there is no operation slot between the two of them
              IOperationSlot nextOperationSlot = ModelDAOHelper.DAOFactory
                .OperationSlotDAO.GetFirstBeginStrictlyBetween (m_monitoredMachine,
                                                                lastOperationCycle.Begin.Value,
                                                                dateTime);
              if (null != nextOperationSlot) {
                Debug.Assert (Bound.Compare<DateTime> (lastOperationCycle.Begin.Value, nextOperationSlot.BeginDateTime) < 0);
                Debug.Assert (Bound.Compare<DateTime> (nextOperationSlot.EndDateTime, dateTime) < 0);
                lastOperationCycle.SetEstimatedEnd (nextOperationSlot.BeginDateTime.Value);
              }
              else {
                lastOperationCycle.SetEstimatedEnd (dateTime);
              }
            }
          }
          else if (AreInSameOperationSlot (lastOperationCycle, operationCycle)) { // null != operationCycle.OperationSlot
            lastOperationCycle.SetEstimatedEnd (dateTime);
          }
        }

        if ((null != lastOperationCycle)
          && lastOperationCycle.End.HasValue
          && lastOperationCycle.Status.HasFlag (OperationCycleStatus.EndEstimated)) {
          if (dateTime < lastOperationCycle.End.Value) {
            if (log.IsWarnEnabled) {
              log.Warn ($"StartCycle: correct estimated end of last cycle from {lastOperationCycle.End} to {dateTime}");
            }
            lastOperationCycle.SetEstimatedEnd (dateTime);
          }
        }

        // Is lastOperationCycle a full cycle ? Update its full property
        if (null != lastOperationCycle) {
          var previousFull = lastOperationCycle.Full;
          lastOperationCycle.Full = IsFull (lastOperationCycle);
          if (!previousFull && lastOperationCycle.Full) {
            if (log.IsDebugEnabled) {
              log.Debug ("StartCycle: consider now the last operation cycle is full");
            }
            // Extension
            foreach (var extension in GetExtensions ()) {
              extension.StopCycle (lastOperationCycle);
            }
          }
          else if (previousFull && !lastOperationCycle.Full) {
            if (log.IsWarnEnabled) {
              log.Warn ("StartCycle: last operation cycle was full and it is now not full any more");
            }
          }
        }

        // Extension
        foreach (var extension in GetExtensions ()) {
          extension.StartCycle (operationCycle);
        }

        // Create a between cycle item if applicable
        CreateBetweenCycle (lastOperationCycle,
                            operationCycle);

        transaction.Commit ();
      }

      try {
        foreach (var extension in m_afterExtensions) {
          extension.StartCycle (dateTime);
        }
      }
      catch (Exception ex) {
        log.Error ("StartCycle: extension exception", ex);
        throw;
      }
    }

    void CreateBetweenCycle (IOperationCycle previousCycle,
                             IOperationCycle nextCycle)
    {
      Debug.Assert (null != nextCycle);
      Debug.Assert (nextCycle.Begin.HasValue);

      if ((null != previousCycle) && previousCycle.Full) { // previousCycle must be full to create a BetweenCycles
        Debug.Assert (previousCycle.End.HasValue);
        Debug.Assert (previousCycle.End.Value <= nextCycle.Begin.Value);

        var betweenCycles = ModelDAOHelper.ModelFactory
          .CreateBetweenCycles (previousCycle, nextCycle);
        if (previousCycle.End.Value < nextCycle.Begin.Value) { // Gap between the cycles
          ModelDAOHelper.DAOFactory.BetweenCyclesDAO.MakePersistent (betweenCycles);
        }
        else if (!Lemoine.Info.ConfigSet.LoadAndGet (SKIP_EMPTY_BETWEENCYCLES_KEY, SKIP_EMPTY_BETWEENCYCLES_DEFAULT)) {
          if (!previousCycle.Begin.HasValue || (previousCycle.Begin.Value != previousCycle.End.Value)) {
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO.MakePersistent (betweenCycles);
          }
          else if (!Lemoine.Info.ConfigSet.LoadAndGet (SKIP_EMPTY_BETWEEN_IF_PREVIOUS_EMPTY_KEY, SKIP_EMPTY_BETWEEN_IF_PREVIOUS_EMPTY_DEFAULT)) {
            ModelDAOHelper.DAOFactory.BetweenCyclesDAO.MakePersistent (betweenCycles);
          }
        }

        foreach (var extension in GetExtensions ()) {
          extension.CreateBetweenCycle (betweenCycles);
        }
      }
    }

    /// <summary>
    /// The cycle ends during an existing slot and the last operation cycle is not a partial cycle,
    /// associate the new full cycle to the slot
    /// 
    /// The new operation full cycle has no begin and its end is in operationSlot.
    /// 
    /// The previous cycle (if any) is a full cycle
    /// </summary>
    /// <param name="lastOperationCycle"></param>
    /// <param name="operationCycle">not null</param>
    /// <param name="operationSlot"></param>
    void AssociateNewFullCycleWithNoPreviousPartialCycleToSlot (IOperationCycle lastOperationCycle,
                                                                IOperationCycle operationCycle,
                                                                IOperationSlot operationSlot)
    {
      // Pre-condition:
      Debug.Assert (null != operationCycle);
      Debug.Assert ((null == lastOperationCycle) || (lastOperationCycle.Full));
      Debug.Assert (operationCycle.End.HasValue);
      Debug.Assert (Bound.Compare<DateTime> (operationCycle.End.Value, operationSlot.EndDateTime) <= 0);
      Debug.Assert (Bound.Compare<DateTime> (operationSlot.BeginDateTime, operationCycle.End.Value) < 0);

      if (log.IsDebugEnabled) {
        log.DebugFormat ("AssociateNewFullCycleWithNoPreviousPartialCycleToSlot: " +
                         "the new operation cycle {0} " +
                         "ends in an existing slot {1}",
                         operationCycle,
                         operationSlot);
      }

      if ((null == lastOperationCycle)
          || (lastOperationCycle.DateTime <= operationSlot.BeginDateTime)) {
        Debug.Assert (operationSlot.BeginDateTime.HasValue);
        if (log.IsDebugEnabled) {
          log.Debug ("AssociateNewFullCycleWithNoPreviousPartialCycleToSlot: no previous operation cycle in the same operation slot");
        }
        if (0 != operationSlot.TotalCycles) { // Ok because only one cycle detection per transaction
          RaiseIncoherentTotalCycles (operationSlot);
        }
        operationCycle.SetEstimatedBegin (operationSlot.BeginDateTime.Value);
      }
      else if (operationSlot.TotalCycles < 1) { // Ok because only one cycle detection per transaction
        Debug.Assert (null != lastOperationCycle);
        Debug.Assert (lastOperationCycle.Full);
        Debug.Assert (lastOperationCycle.End.HasValue);
        // There is a cycle end in the same slot
        // => there must be at least one total cycle
        RaisePreviousCycleNotMatchingSlot (lastOperationCycle,
                                           operationSlot);
        // Fallback: do not take into account
        // this previous cycle
        if (lastOperationCycle.End.Value <= operationCycle.End.Value) {
          operationCycle.SetEstimatedBegin (lastOperationCycle.End.Value);
        }
        else if (operationSlot.BeginDateTime.HasValue) {
          operationCycle.SetEstimatedBegin (operationSlot.BeginDateTime.Value);
        }
      }
      else {
        Debug.Assert (1 <= operationSlot.TotalCycles);
        Debug.Assert (lastOperationCycle != null);
        Debug.Assert (lastOperationCycle.End.Value <= operationCycle.End.Value);

        operationCycle.SetEstimatedBegin (lastOperationCycle.End.Value);
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"StopCycle: associate cycle {operationCycle} to slot {operationSlot}");
      }
      operationCycle.OperationSlot = operationSlot;
    }

    /// <summary>
    /// Both dateTime and lastOperationCycle do not belong to any operation slot
    /// and lastOperationCycle is a cycle with no real end
    /// </summary>
    /// <param name="quantity"></param>
    /// <param name="dateTime"></param>
    /// <param name="lastOperationCycle"></param>
    /// <returns>Cycle that corresponds to the slot</returns>
    IOperationCycle StopPartialCycle (int? quantity,
                                      DateTime dateTime,
                                      IOperationCycle lastOperationCycle)
    {
      Debug.Assert (!lastOperationCycle.HasRealEnd ());

      if (log.IsDebugEnabled) {
        log.Debug ($"StopCycleNoOperationSlot: No current operation slot and last partial cycle {lastOperationCycle} has no operation slot");
      }

      // No operation slot between lastOperationCycle and dateTime,
      // consider the start and ends are part of the same period
      // Transform the partial cycle into a full cycle
      lastOperationCycle.Quantity = quantity;
      lastOperationCycle.SetRealEnd (dateTime);
      lastOperationCycle.Full = IsFull (lastOperationCycle);
      return lastOperationCycle;
    }

    /// <summary>
    /// Stop a cycle when the last operation cycle has no real end and belongs to a different operation slot
    /// </summary>
    /// <param name="quantity"></param>
    /// <param name="dateTime"></param>
    /// <param name="lastOperationCycle"></param>
    /// <param name="operationSlot"></param>
    /// <returns>Cycle that corresponds to the slot</returns>
    IOperationCycle StopCycleWhenLastCycleIsPartialAndBelongsToDifferentSlot (int? quantity,
                                                                              DateTime dateTime,
                                                                              IOperationCycle lastOperationCycle,
                                                                              IOperationSlot operationSlot)
    {
      Debug.Assert (!lastOperationCycle.HasRealEnd (), "last operation cycle has alredy a real end");
      Debug.Assert (lastOperationCycle.Begin.HasValue, "last operation cycle has no start");
      Debug.Assert (!object.Equals (lastOperationCycle.OperationSlot, operationSlot), "not a new operation slot");

      // The new cycle will be the first cycle of operation slot
      if (log.IsErrorEnabled && (null != operationSlot)) {
        if (0 != operationSlot.TotalCycles) {
          log.Fatal ($"StopCycleWhenLastCycleIsPartialAndBelongsToDifferentSlot: operationSlot at {dateTime}, id={operationSlot.Id}, range={operationSlot.DateTimeRange} has already {operationSlot.TotalCycles} cycles");
        }
        if (operationSlot.AverageCycleTime.HasValue) {
          log.Fatal ($"StopCycleWhenLastCycleIsPartialAndBelongsToDifferentSlot: operationSlot at {dateTime}, id={operationSlot.Id}, range={operationSlot.DateTimeRange} has already an average cycle time {operationSlot.AverageCycleTime.Value}");
        }
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"StopCycleWhenLastCycleIsPartialAndBelongsToDifferentSlot: two different operation slots between last cycle {lastOperationCycle} and slot {operationSlot} at {dateTime}");
      }

      // .... If last partial cycle starts close to new slot begin,
      //      reassociate it to the slot and make it full
      var operationCycleAssociationMargin = Lemoine.Info.ConfigSet
        .LoadAndGet<TimeSpan> (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.OperationCycleAssociationMargin),
                               TimeSpan.FromSeconds (0));
      if ((null != operationSlot) && (Bound.Compare<DateTime> (lastOperationCycle.Begin.Value, operationSlot.BeginDateTime) < 0)
          && (operationSlot.BeginDateTime.Value
              .Subtract (lastOperationCycle.Begin.Value) < operationCycleAssociationMargin)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"StopCycleWhenLastCycleIsPartialAndBelongsToDifferentSlot: make the previous partial cycle {lastOperationCycle} full with end set to {lastOperationCycle} because its start is close enough to start of slot {dateTime}");
        }
        lastOperationCycle.Quantity = quantity;
        lastOperationCycle.SetRealEnd (dateTime); // switch from estimated to real end
        lastOperationCycle.OperationSlot = operationSlot;
        lastOperationCycle.Full = IsFull (lastOperationCycle);
        return lastOperationCycle;
      }

      // .... Else if the operation, component and work order did not change between the two operation slots,
      //      reassociate it to the slot and make it full
      if ((null != lastOperationCycle.OperationSlot) && (null != operationSlot)
        && AreSameOperationComponentWorkOrder (lastOperationCycle.OperationSlot, operationSlot)) {
        Debug.Assert (lastOperationCycle.OperationSlot.EndDateTime.HasValue);
        // Check there is no operation slot with a different operation / component / work order
        // between lastOperationCycle and operationSlot
        if (operationSlot.BeginDateTime.HasValue
            && !ModelDAOHelper.DAOFactory.OperationSlotDAO
            .ExistsDifferentWorkOrderComponentOperationBetween (m_monitoredMachine,
                                                                lastOperationCycle.OperationSlot.EndDateTime.Value,
                                                                operationSlot.BeginDateTime.Value,
                                                                operationSlot.Operation,
                                                                operationSlot.Component,
                                                                operationSlot.WorkOrder)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"StopCycleWhenLastCycleIsPartialAndBelongsToDifferentSlot: make the previous partial cycle {lastOperationCycle} full with end set to {dateTime} because the operation / component / work order did not change between them");
          }
          lastOperationCycle.Quantity = quantity;
          lastOperationCycle.SetRealEnd (dateTime); // switch from estimated to real end
          lastOperationCycle.OperationSlot = operationSlot;
          lastOperationCycle.Full = IsFull (lastOperationCycle);
          return lastOperationCycle;
        }
      }

      // .... Else begin and end must be considered as being
      //      part of two different operation cycles
      //      => Start a new OperationCycle
      IOperationCycle operationCycle =
        ModelDAOHelper.ModelFactory
        .CreateOperationCycle (m_monitoredMachine);
      operationCycle.Quantity = quantity;
      operationCycle.SetRealEnd (dateTime);

      if (log.IsDebugEnabled) {
        log.Debug ($"StopCycleWhenLastCycleIsPartialAndBelongsToDifferentSlot: create a new operation cycle {operationCycle} ending at {dateTime} because the previous partial cycle {lastOperationCycle} could not be associated to slot {operationSlot}");
      }

      if (operationSlot != null) {
        if (operationSlot.BeginDateTime.HasValue) {
          operationCycle.SetEstimatedBegin (operationSlot.BeginDateTime.Value);
        }
        operationCycle.OperationSlot = operationSlot;
      }

      operationCycle.Full = IsFull (operationCycle);
      ModelDAOHelper.DAOFactory.OperationCycleDAO
        .MakePersistent (operationCycle);

      return operationCycle;
    }

    bool AreSameOperationComponentWorkOrder (IOperationSlot a, IOperationSlot b)
    {
      Debug.Assert (null != a);
      Debug.Assert (null != b);

      bool sameOperation = Lemoine.Model.Comparison
        .EqualsNullable (a.Operation, b.Operation, (x, y) => ((IDataWithId)x).Id == ((IDataWithId)y).Id);
      if (!sameOperation) {
        return false;
      }
      bool sameComponent = Lemoine.Model.Comparison
        .EqualsNullable (a.Component, b.Component, (x, y) => ((IDataWithId)x).Id == ((IDataWithId)y).Id);
      if (!sameComponent) {
        return false;
      }
      bool sameWorkOrder = Lemoine.Model.Comparison
        .EqualsNullable (a.WorkOrder, b.WorkOrder, (x, y) => ((IDataWithId)x).Id == ((IDataWithId)y).Id);
      return sameWorkOrder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lastOperationCycle"></param>
    /// <param name="dateTime"></param>
    /// <param name="operationSlot">that corresponds to dateTime</param>
    /// <returns></returns>
    bool IsSameContinuousOperation (IOperationCycle lastOperationCycle, DateTime dateTime, IOperationSlot operationSlot)
    {
      Debug.Assert (null != lastOperationCycle);

      var machine = lastOperationCycle.Machine;
      IOperationSlot lastOperationSlot = lastOperationCycle.OperationSlot;
      if (null == operationSlot) {
        if (null == lastOperationSlot) {
          return ModelDAOHelper.DAOFactory.OperationSlotDAO
            .IsContinuousOperationInRange (machine,
                                           new UtcDateTimeRange (lastOperationCycle.DateTime,
                                                                 dateTime),
                                           null);
        }
        else { // null != lastOperationSlot
          lastOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindById (lastOperationSlot.Id, machine);
          Debug.Assert (null != lastOperationSlot);
          if (null == lastOperationSlot.Operation) {
            return ModelDAOHelper.DAOFactory.OperationSlotDAO
              .IsContinuousOperationInRange (machine,
                                             new UtcDateTimeRange (lastOperationCycle.DateTime,
                                                                   dateTime),
                                             null);
          }
          else {
            return false;
          }
        }
      }
      else { // null != operationSlot
        if (null == lastOperationSlot) {
          var operation = operationSlot.Operation;
          if (null != operation) {
            return false;
          }
          else {
            Debug.Assert (Bound.Compare<DateTime> (lastOperationCycle.DateTime, operationSlot.BeginDateTime) < 0);
            Debug.Assert (operationSlot.BeginDateTime.HasValue, "operation slot has no start");
            if (log.IsDebugEnabled) {
              log.Debug ($"IsSameContinuousOperation: last operation slot is null, while current operation is {operationSlot?.Operation?.Id}");
            }
            return ModelDAOHelper.DAOFactory.OperationSlotDAO
              .IsContinuousOperationInRange (machine,
                                             new UtcDateTimeRange (lastOperationCycle.DateTime,
                                                                   operationSlot.BeginDateTime.Value),
                                             null);
          }
        }
        else { // null != lastOperationSlot
          if (operationSlot.Id == lastOperationSlot.Id) {
            return true;
          }
          else { // Different IDs
            lastOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindById (lastOperationSlot.Id, machine);
            Debug.Assert (null != lastOperationSlot);
            if (!object.Equals (lastOperationSlot.Operation, operationSlot.Operation)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"IsSameContinuousOperation: {lastOperationSlot?.Operation?.Id} != {operationSlot?.Operation?.Id}");
              }
              return false;
            }
            else {
              Debug.Assert (Bound.Compare<DateTime> (lastOperationCycle.DateTime, operationSlot.BeginDateTime) < 0);
              Debug.Assert (operationSlot.BeginDateTime.HasValue);
              if (log.IsDebugEnabled) {
                log.Debug ($"IsSameContinuousOperation: {lastOperationSlot?.Operation?.Id} VS {operationSlot?.Operation?.Id}");
              }
              return ModelDAOHelper.DAOFactory.OperationSlotDAO
                .IsContinuousOperationInRange (machine,
                                               new UtcDateTimeRange (lastOperationCycle.DateTime,
                                                                     operationSlot.BeginDateTime.Value),
                                               operationSlot.Operation);
            }
          }
        }
      }
    }

    void ExtendFullCycle (IOperationCycle operationCycle, int? quantity, DateTime dateTime)
    {
      Debug.Assert (null != operationCycle);
      Debug.Assert (operationCycle.Full);
      Debug.Assert (operationCycle.End.HasValue);
      Debug.Assert (operationCycle.End.Value <= dateTime);

      // . Extend NextBegin of any sequence slot that would correspond to the old cycle end
      foreach (IMachineModule machineModule in m_monitoredMachine.MachineModules) {
        IList<ISequenceSlot> sequenceSlots = ModelDAOHelper.DAOFactory.SequenceSlotDAO
          .FindAllNextBeginFrom (machineModule, operationCycle.End.Value);
        if ((1 <= sequenceSlots.Count)
            && (sequenceSlots[0].NextBegin.Equals (operationCycle.End.Value))) {
          // Potentially, the NextBegin value was set to the previous cycle end
          Debug.Assert (sequenceSlots[0].EndDateTime.HasValue);
          if ((2 <= sequenceSlots.Count)
              && (Bound.Compare<DateTime> (sequenceSlots[1].BeginDateTime, dateTime) < 0)) {
            // case where there is a already a slot after the impacted slot
            Debug.Assert (sequenceSlots[0].EndDateTime <= sequenceSlots[1].BeginDateTime);
            sequenceSlots[0].NextBegin = (DateTime?)sequenceSlots[1].BeginDateTime;
          }
          else {
            Debug.Assert (sequenceSlots[0].EndDateTime <= operationCycle.End.Value);
            sequenceSlots[0].NextBegin = dateTime;
          }
        }
      }

      // . Extend the full cycle and return
      if (log.IsDebugEnabled) {
        log.Debug ($"ExtendFullCycle: option to extend the full cycle {operationCycle} with the new cycle end {dateTime}");
      }
      operationCycle.SetRealEnd (dateTime);
      if (quantity.HasValue
          && !object.Equals (operationCycle.Quantity, quantity)) {
        if (log.IsWarnEnabled) {
          log.Warn ($"ExtendFullCycle: the cycle is extended but the quantity was updated from {operationCycle.Quantity} to {quantity}");
        }
        if (operationCycle.Quantity.HasValue) {
          RaiseQuantityChange (operationCycle, quantity);
        }
        operationCycle.Quantity = quantity;
      }
      operationCycle.Full = IsFull (operationCycle);
    }

    /// <summary>
    /// Stop an OperationCycle
    /// 
    /// Note the operation slot is not extended with this method
    /// 
    /// The top transaction must be created in the parent function
    /// </summary>
    /// <param name="quantity"></param>
    /// <param name="dateTime"></param>
    public void StopCycle (int? quantity, DateTime dateTime)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("Detection.StopCycle")) // Not a top transaction
        {
          // . Get the last operation cycle
          //   and process the new cycle differently
          //   if this last operation cycle is a full or a partial one
          IOperationCycle lastOperationCycle =
            ModelDAOHelper.DAOFactory.OperationCycleDAO
            .GetLast (m_monitoredMachine);

          if ((null != lastOperationCycle)
              && (!lastOperationCycle.End.HasValue
                  || (dateTime < lastOperationCycle.End.Value))) {
            // Last operation cycle is a full operation cycle:
            // Check the dateTime is valid regarding this last operation cycle
            // and adjust the previous estimated end date/time if needed
            if (!lastOperationCycle.End.HasValue) {
              lastOperationCycle.SetEstimatedEnd (dateTime);
              lastOperationCycle.Full = IsFull (lastOperationCycle);
            }
            else if (lastOperationCycle.Status.HasFlag (OperationCycleStatus.EndEstimated)) {
              // Shorten the previous full cycle
              if (log.IsInfoEnabled) {
                log.Info ($"StopCycle: shorten the full cycle {lastOperationCycle} because its estimated end is before the new cycle end {dateTime}");
              }
              lastOperationCycle.SetEstimatedEnd (dateTime);
              lastOperationCycle.Full = IsFull (lastOperationCycle);
            }
            else { // Real end
              RaiseInvalidDateTime (dateTime);
              transaction.Commit (); // To store the log
              return;
            }
          }

          IOperationSlot operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .GetOperationSlotAtConsideringEnd (m_monitoredMachine, dateTime);

          // Extend a cycle with already an end ?
          if ((null != lastOperationCycle) && lastOperationCycle.HasRealEnd ()) {
            bool extendCycle = Lemoine.Info.ConfigSet
              .LoadAndGet<bool> (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.ExtendFullCycleWhenNewCycleEnd),
                                 false);
            if (extendCycle) {
              if (IsSameContinuousOperation (lastOperationCycle, dateTime, operationSlot)) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"StopCycle: last operation cycle {lastOperationCycle} is in the same operation slot");
                }
                ExtendFullCycle (lastOperationCycle, quantity, dateTime);
                transaction.Commit ();

                try {
                  foreach (var extension in m_afterExtensions) {
                    extension.StopCycle (quantity, dateTime);
                  }
                }
                catch (Exception ex) {
                  log.Error ("StopCycle: extension exception", ex);
                  throw;
                }

                return;
              }
            }
          }

          IOperationCycle operationCycle; // Cycle that corresponds to the Stop

          if ((null == lastOperationCycle)
            || lastOperationCycle.HasRealEnd ()) {
            // No last operation cycle
            // or last operation cycle is a full operation cycle

            // .. Add a new operation cycle without begin
            operationCycle =
              ModelDAOHelper.ModelFactory.CreateOperationCycle (m_monitoredMachine);
            operationCycle.Quantity = quantity;
            operationCycle.SetRealEnd (dateTime);
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);

            if (log.IsDebugEnabled) {
              log.Debug ($"StopCycle: last operation cycle {lastOperationCycle} is a full operation cycle or is null => add a new operation cycle {operationCycle} ending at  {dateTime}");
            }

            // .. Check if this new operation cycle may belong to
            //    an existing operation slot
            if (null != operationSlot) {
              // the new operation cycle may belong to the slot at dateTime
              AssociateNewFullCycleWithNoPreviousPartialCycleToSlot (lastOperationCycle,
                                                                     operationCycle,
                                                                     operationSlot);
            }
            else { // null == operationSlot
              // no slot yet, but see if you can set estimated begin to end of last full cycle
              if ((lastOperationCycle != null) &&
                  (lastOperationCycle.End.HasValue)) {
                // Check there is no operation slot between lastOperationCycle.End and dateTime
                IOperationSlot previousOperationSlot =
                  ModelDAOHelper.DAOFactory.OperationSlotDAO
                  .GetLastStrictlyBefore (m_monitoredMachine,
                                          dateTime);
                if ((null == previousOperationSlot)
                    || (previousOperationSlot.EndDateTime.HasValue
                        && previousOperationSlot.EndDateTime.Value <= lastOperationCycle.End.Value)) {
                  // No operation slot between lastOperationCycle and dateTime,
                  // the estimated begin is lastOperationCycle.End.Value
                  operationCycle.SetEstimatedBegin (lastOperationCycle.End.Value);
                  if (log.IsDebugEnabled) {
                    log.Debug ($"StopCycle: cycle {operationCycle} gets an estimated begin equal to last cycle {lastOperationCycle} end");
                  }
                }
                else { // estimated begin is previousOperationSlot.EndDateTime
                  Debug.Assert (previousOperationSlot.EndDateTime.HasValue);
                  operationCycle.SetEstimatedBegin (previousOperationSlot.EndDateTime.Value);
                }
              } // Test on lastOperationCycle
            } // Test on operationSlot
          }
          else { // Last operation cycle has an estimated end
            Debug.Assert (lastOperationCycle.Begin.HasValue);
            if (log.IsDebugEnabled) {
              log.Debug ($"StopCycle: last operation cycle {lastOperationCycle} is a partial cycle, check if the new end may {dateTime} be associated to it");
            }

            // .. Check the dateTime is valid regarding this last operation cycle
            if (Bound.Compare<DateTime> (dateTime, lastOperationCycle.Begin.Value) < 0) {
              RaiseInvalidDateTime (dateTime);
              transaction.Commit (); // To store the log
              return;
            }

            // .. Check if the new end may be associated to
            //    this last operation cycle
            if (IsSameContinuousOperation (lastOperationCycle, dateTime, operationSlot)) {
              // ... Same operation slot ! Or no operation slot
              //     => end the existing operation cycle
              operationCycle = StopPartialCycle (quantity, dateTime, lastOperationCycle);
            }
            else {
              // ... Two different operation slots
              //     This should be the first detected cycle
              //     in this operation slot
              if (log.IsDebugEnabled) {
                log.Debug ($"StopCycle: new cycle at {dateTime} is not for the same operation than {lastOperationCycle}");
              }
              operationCycle = StopCycleWhenLastCycleIsPartialAndBelongsToDifferentSlot (quantity, dateTime, lastOperationCycle, operationSlot);
            }
          }

          // Extension
          foreach (var extension in GetExtensions ()) {
            extension.StopCycle (operationCycle);
          }

          // . Set the NextBegin value to last sequence slot
          Debug.Assert (null != operationCycle);
          foreach (IMachineModule machineModule in m_monitoredMachine.MachineModules) {
            ISequenceSlot sequenceSlot = ModelDAOHelper.DAOFactory.SequenceSlotDAO
              .FindLastBefore (machineModule, dateTime);
            if (null != sequenceSlot) {
              Debug.Assert (Bound.Compare<DateTime> (sequenceSlot.BeginDateTime, dateTime) < 0);
              if (Bound.Compare<DateTime> (dateTime, sequenceSlot.EndDateTime) < 0) { // The sequence slot ends after the cycle end ! Correct this
                if (log.IsWarnEnabled) {
                  log.Warn ($"StopCycle: correct the sequence slot {sequenceSlot} because it ends after the cycle stop {dateTime}");
                }
                sequenceSlot.EndDateTime = dateTime;
              }
              Debug.Assert (sequenceSlot.EndDateTime.HasValue);
              Debug.Assert (sequenceSlot.EndDateTime.Value <= dateTime);
              if (!sequenceSlot.NextBegin.HasValue
                  || (Bound.Compare<DateTime> (dateTime, sequenceSlot.NextBegin.Value) < 0)) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"StopCycle: set the NextBegin {dateTime} to sequence slot {sequenceSlot}");
                }
                sequenceSlot.NextBegin = dateTime;
              }
            }
          }

          transaction.Commit ();
        }
      }

      try {
        foreach (var extension in m_afterExtensions) {
          extension.StopCycle (quantity, dateTime);
        }
      }
      catch (Exception ex) {
        log.Error ("StopCycle: extension exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Start and then Stop an OperationCycle
    /// in one step
    /// 
    /// Note the operation slot is not extended with this method
    /// 
    /// The top transaction is defined in the parent function
    /// </summary>
    /// <param name="startDateTime"></param>
    /// <param name="stopDateTime"></param>
    public void StartStopCycle (DateTime startDateTime,
                                  DateTime stopDateTime)
    {
      Debug.Assert (startDateTime <= stopDateTime);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ("Detection.StartStopCycle")) {
        // . Get the last operation cycle to add correctly the betweencycles item at the end
        IOperationCycle lastOperationCycle =
          ModelDAOHelper.DAOFactory.OperationCycleDAO.GetLast (m_monitoredMachine);

        // . Get a possible matching operation slot
        IOperationSlot operationSlot =
          ModelDAOHelper.DAOFactory.OperationSlotDAO
          .GetOperationSlotAtConsideringEnd (m_monitoredMachine, stopDateTime);
        if (null != operationSlot) {
          bool split = false;
          if (log.IsDebugEnabled) {
            log.Debug ($"StartStopCycle: the new operation cycle belongs to operation slot {operationSlot.Id}");
          }

          // .. Check the startDateTime may be included
          //    in this operation slot
          TimeSpan operationCycleAssociationMargin = Lemoine.Info.ConfigSet
            .LoadAndGet<TimeSpan> (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.OperationCycleAssociationMargin),
                                   TimeSpan.FromTicks (0));
          if ((Bound.Compare<DateTime> (startDateTime, operationSlot.BeginDateTime) < 0)
              && (operationCycleAssociationMargin <
                  operationSlot.BeginDateTime.Value
                  .Subtract (startDateTime))) {
            split = true;
            // Check there is no operation slot with a different operation
            // between operationCycle.Begin and operationSlot
            if (ModelDAOHelper.DAOFactory.OperationSlotDAO
                .IsContinuousOperationInRange (m_monitoredMachine,
                                               new UtcDateTimeRange (startDateTime,
                                                                     operationSlot.BeginDateTime.Value),
                                               operationSlot.Operation)) {
              split = false;
              log.Debug ("StartStopCycle: do not split the cycle because the operation did not change");
            }
          }
          if (split) {
            if (log.IsDebugEnabled) {
              log.Debug ($"StartStopCycle: the start date/time {startDateTime} may not be associated to this operation slot {operationSlot}, process it separately");
            }
            Debug.Assert (operationSlot.BeginDateTime.HasValue);
            StartCycle (startDateTime);
            StopCycle (null, stopDateTime);
            transaction.Commit ();

            try {
              foreach (var extension in m_afterExtensions) {
                extension.StartStopCycle (startDateTime, stopDateTime);
              }
            }
            catch (Exception ex) {
              log.Error ("StartStopCycle: extension exception", ex);
              throw;
            }

            return;
          }
        }

        // . Create the new operation cycle
        IOperationCycle operationCycle =
          ModelDAOHelper.ModelFactory.CreateOperationCycle (m_monitoredMachine);
        operationCycle.Begin = startDateTime;
        operationCycle.SetRealEnd (stopDateTime);
        operationCycle.Full = IsFull (operationCycle);
        operationCycle.OperationSlot = operationSlot;
        ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (operationCycle);

        foreach (var extension in GetExtensions ()) {
          extension.StartCycle (operationCycle);
          extension.StopCycle (operationCycle);
        }

        // . Add the BetweenCycles item
        CreateBetweenCycle (lastOperationCycle, operationCycle);

        // . Commit the transaction
        transaction.Commit ();
      }

      try {
        foreach (var extension in m_afterExtensions) {
          extension.StartStopCycle (startDateTime, stopDateTime);
        }
      }
      catch (Exception ex) {
        log.Error ("StartStopCycle: extension exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Trigger a detection process start
    /// </summary>
    public void DetectionProcessStart ()
    {
      foreach (var extension in GetExtensions ()) {
        extension.DetectionProcessStart ();
      }
    }

    /// <summary>
    /// Trigger a detection process complete
    /// </summary>
    public void DetectionProcessComplete ()
    {
      foreach (var extension in GetExtensions ()) {
        extension.DetectionProcessComplete ();
      }
    }

    /// <summary>
    /// Trigger a detection process error
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="ex"></param>
    public void DetectionProcessError (IMachineModule machineModule, Exception ex)
    {
      foreach (var extension in GetExtensions ()) {
        extension.DetectionProcessError (machineModule, ex);
      }

      try {
        foreach (var extension in m_afterExtensions) {
          extension.DetectionProcessError (machineModule, ex);
        }
      }
      catch (Exception ex1) {
        log.Error ("DetectionProcessError: extension exception", ex1);
        throw;
      }
    }

    /// <summary>
    /// Raise an error the input date/time is invalid
    /// regarding the existing operation cycles
    /// </summary>
    /// <param name="dateTime"></param>
    void RaiseInvalidDateTime (DateTime dateTime)
    {
      string message = $"already an operation cycle after {dateTime}";
      log.Error ($"RaiseInvalidDateTime: {message} => log it");
      IDetectionAnalysisLog detectionAnalysisLog =
        ModelDAOHelper.ModelFactory
        .CreateDetectionAnalysisLog (LogLevel.ERROR,
                                     message,
                                     m_monitoredMachine);
      ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
        .MakePersistent (detectionAnalysisLog);
    }

    /// <summary>
    /// Raise an error because the previous cycle was
    /// expected to be in the same operation slot
    /// </summary>
    /// <param name="operationCycle"></param>
    /// <param name="operationSlot"></param>
    void RaisePreviousCycleNotMatchingSlot (IOperationCycle operationCycle,
                                            IOperationSlot operationSlot)
    {
      string message = $"The previous cycle ending at {operationCycle.End} was expected to be in operation slot {operationSlot.Id} but it was not, the average cycle time may be wrong";
      log.Error ($"RaisePreviousCycleNotMatchingSlot: {message} => log it");
      IDetectionAnalysisLog detectionAnalysisLog =
        ModelDAOHelper.ModelFactory
        .CreateDetectionAnalysisLog (LogLevel.ERROR,
                                     message,
                                     m_monitoredMachine);
      ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
        .MakePersistent (detectionAnalysisLog);
    }

    /// <summary>
    /// Raise an error because the number of full cycles
    /// found in an operation slot is incoherent
    /// </summary>
    /// <param name="operationSlot"></param>
    void RaiseIncoherentTotalCycles (IOperationSlot operationSlot)
    {
      string message = $"The number of total cycles in operation slot {operationSlot.Id} is incoherent with the read operation cycles";
      log.Error ($"RaiseIncoherentTotalCycles: {message} => log it");
      IDetectionAnalysisLog detectionAnalysisLog =
        ModelDAOHelper.ModelFactory
        .CreateDetectionAnalysisLog (LogLevel.ERROR,
                                     message,
                                     m_monitoredMachine);
      ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
        .MakePersistent (detectionAnalysisLog);
    }

    /// <summary>
    /// The quantity of a cycle was changed
    /// </summary>
    /// <param name="operationCycle"></param>
    /// <param name="newQuantity"></param>
    void RaiseQuantityChange (IOperationCycle operationCycle, int? newQuantity)
    {
      string message = string.Format ("At {0} the quantity of the full operation cycle is changed " +
                                      "from {1} to {2}",
                                      operationCycle.End, operationCycle.Quantity, newQuantity);
      log.WarnFormat ("RaiseQuantityChange: {0} " +
                      "=> log it",
                      message);
      IDetectionAnalysisLog detectionAnalysisLog =
        ModelDAOHelper.ModelFactory
        .CreateDetectionAnalysisLog (LogLevel.WARN,
                                     message,
                                     m_monitoredMachine);
      ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
        .MakePersistent (detectionAnalysisLog);
    }

    bool AreInSameOperationSlot (IOperationCycle operationCycle1, IOperationCycle operationCycle2)
    {
      Debug.Assert (null != operationCycle1);
      Debug.Assert (null != operationCycle2);
      Debug.Assert (operationCycle1.DateTime <= operationCycle2.DateTime);

      if (null == operationCycle1.OperationSlot) {
        if (null == operationCycle2.OperationSlot) {
          // Check there is no operation slot between these two cycles
          DateTime dateTime1 = operationCycle1.Begin.HasValue ? operationCycle1.Begin.Value : operationCycle1.End.Value;
          DateTime dateTime2 = operationCycle2.End.HasValue ? operationCycle2.End.Value : operationCycle2.Begin.Value;
          Debug.Assert (dateTime1 <= dateTime2);
          var firstOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .GetFirstBeginStrictlyBetween (m_monitoredMachine,
                                           dateTime1, dateTime2);
          return (null == firstOperationSlot);
        }
        else { // null != operationCycle2.OperationSlot
          return false;
        }
      }
      else { // null != operationCycle1.OperationSlot
        if (null == operationCycle2.OperationSlot) {
          return false;
        }
        else { // null != operationCycle2.OperationSlot
          return operationCycle1.OperationSlot.Id == operationCycle2.OperationSlot.Id;
        }
      }
    }
  }
}

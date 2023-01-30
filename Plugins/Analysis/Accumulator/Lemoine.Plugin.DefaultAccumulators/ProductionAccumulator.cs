// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using Pulse.Extensions.Database.Accumulator.Impl;
using Pulse.Extensions.Database.Accumulator;
using Lemoine.GDBPersistentClasses;
using System.Linq;

namespace Lemoine.Plugin.DefaultAccumulators
{
  public sealed class ProductionAccumulatorExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IAccumulatorExtension
  {
    public double Priority => 451;

    public bool Initialize ()
    {
      return AnalysisConfigHelper.OperationSlotProductionDuration;
    }

    public IAccumulator Create ()
    {
      return new ProductionAccumulator ();
    }
  }

  /// <summary>
  /// Accumulator to track the production duration changes
  /// </summary>
  public class ProductionAccumulator
    : Accumulator
    , IObservationStateSlotAccumulator
  {
    ILog log = LogManager.GetLogger<ProductionAccumulator> ();

    readonly IDictionary<int, ProductionAccumulatorByMachine> m_accumulators =
      new Dictionary<int, ProductionAccumulatorByMachine> ();

    /// <summary>
    /// Store
    /// </summary>
    /// <param name="transactionName"></param>
    public override void Store (string transactionName)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Store: {m_accumulators.Count ()} machine accumulators");
      }
      foreach (var accumulator in m_accumulators.Values) {
        accumulator.Store (transactionName);
      }
    }

    /// <summary>
    /// Implementation of <see cref="IObservationStateSlotAccumulator"/>
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="range"></param>
    public void AddObservationStateSlotPeriod (IObservationStateSlot slot, UtcDateTimeRange range)
    {
      if (slot.Production.HasValue && slot.Production.Value) {
        AddProductionPeriod (slot.Machine, range);
      }
    }

    /// <summary>
    /// Implementation of <see cref="IObservationStateSlotAccumulator"/>
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="range"></param>
    public void RemoveObservationStateSlotPeriod (IObservationStateSlot slot, UtcDateTimeRange range)
    {
      if (slot.Production.HasValue && slot.Production.Value) {
        RemoveProductionPeriod (slot.Machine, range);
      }
    }

    /// <summary>
    /// Add a production period
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    void AddProductionPeriod (IMachine machine, UtcDateTimeRange range)
    {
      ProductionAccumulatorByMachine accumulator;
      if (!m_accumulators.TryGetValue (machine.Id, out accumulator)) {
        m_accumulators[machine.Id] = accumulator = new ProductionAccumulatorByMachine (machine);
      }
      accumulator.AddProductionPeriod (range);
    }

    /// <summary>
    /// Remove a production period
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    void RemoveProductionPeriod (IMachine machine, UtcDateTimeRange range)
    {
      ProductionAccumulatorByMachine accumulator;
      if (!m_accumulators.TryGetValue (machine.Id, out accumulator)) {
        m_accumulators[machine.Id] = accumulator = new ProductionAccumulatorByMachine (machine);
      }
      accumulator.RemoveProductionPeriod (range);
    }
  }

  /// <summary>
  /// Accumulator to track the production duration changes for a specific machine
  /// </summary>
  internal class ProductionAccumulatorByMachine : DateTimeRangeValueAccumulator<int, int>
  {
    #region Members
    readonly IMachine m_machine;
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (ProductionAccumulatorByMachine).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ProductionAccumulatorByMachine (IMachine machine)
      : base (a => a,
              (a, b) => a + b,
              a => -a,
              (a, b) => a - b,
              a => (0 == a))
    {
      m_machine = machine;
      log = LogManager.GetLogger ($"{typeof (ProductionAccumulatorByMachine).FullName}.{machine.Id}");
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a production period
    /// </summary>
    /// <param name="range"></param>
    public void AddProductionPeriod (UtcDateTimeRange range)
    {
      this.Add (range, +1);
    }

    /// <summary>
    /// Remove a production period
    /// </summary>
    /// <param name="range"></param>
    public void RemoveProductionPeriod (UtcDateTimeRange range)
    {
      this.Remove (range, +1);
    }

    /// <summary>
    /// Store
    /// </summary>
    /// <param name="transactionName"></param>
    public override void Store (string transactionName)
    {
      if (!AnalysisConfigHelper.OperationSlotProductionDuration) {
        log.Debug ("Store: return immediately because the option is not active");
        return;
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"Store: run with transaction={transactionName}");
      }

      this.Purge ();

      // Global range from the accumulator
      var globalRange = this.GlobalRange;
      if (globalRange.IsEmpty ()) {
        log.Debug ("Store: return because the accumulator is empty");
      }

      // Adjusted range with ProductionAnalysisStatus
      IProductionAnalysisStatus productionAnalysisStatus = ModelDAOHelper.DAOFactory.ProductionAnalysisStatusDAO
        .FindById (m_machine.Id);
      if (null == productionAnalysisStatus) {
        log.Error ("Store: productionAnalysisStatus has not been created yet => return");
        return;
      }
      UtcDateTimeRange adjustedRange = new UtcDateTimeRange (new UtcDateTimeRange (new LowerBound<DateTime> (null),
                                                                                   productionAnalysisStatus.AnalysisDateTime)
                                                             .Intersects (globalRange));
      if (adjustedRange.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.Debug ("Store: adjusted range is empty (in the future ?) => do nothing");
        }
        return;
      }
      else if (log.IsDebugEnabled) {
        log.Debug ($"Store consider adjusted range {adjustedRange}");
      }

      // Visit the operation slots and the accumulator items
      IList<IOperationSlot> operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
        .FindOverlapsRange (m_machine, adjustedRange);
      foreach (IOperationSlot operationSlot in operationSlots) {
        foreach (var dateTimeRangeValue in this.DateTimeRangeValues) {
          var intersection = new UtcDateTimeRange (adjustedRange
                                                   .Intersects (operationSlot.DateTimeRange)
                                                   .Intersects (dateTimeRangeValue.Range));
          if (!intersection.IsEmpty () && intersection.Duration.HasValue) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Store: intersection is {intersection}");
            }
            Debug.Assert ((1 == dateTimeRangeValue.Value) || (-1 == dateTimeRangeValue.Value));
            TimeSpan offset = TimeSpan.FromSeconds (intersection.Duration.Value.TotalSeconds * dateTimeRangeValue.Value);
            if (operationSlot.ProductionDuration.HasValue) {
              ((OperationSlot)operationSlot).ProductionDuration =
                operationSlot.ProductionDuration.Value.Add (offset);
              if (operationSlot.ProductionDuration.Value.TotalSeconds < 0) {
                log.FatalFormat ("Store: about to store a negative production duration {0} in {1} => reconsolidate the operation slot",
                                 operationSlot.ProductionDuration, operationSlot);
                Debug.Assert (false);
                ((OperationSlot)operationSlot).ConsolidateProduction ();
              }
            }
            else { // !operationSlot.ProductionDuration.HasValue
              if (offset.TotalSeconds < 0) {
                log.Fatal ("Store: about to set an initial production duration that is negative => reconsolidate the operation slot");
                Debug.Assert (false);
                ((OperationSlot)operationSlot).ConsolidateProduction ();
              }
              else {
                ((OperationSlot)operationSlot).ProductionDuration = offset;
              }
            }
            ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (operationSlot);
          }
        }
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"Store: transaction={transactionName} completed");
      }
    }
    #endregion // Methods
  }
}

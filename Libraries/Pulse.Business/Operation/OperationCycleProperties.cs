// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Core.Log;
using Lemoine.Model;
using System.Linq;
using Lemoine.ModelDAO;
using System.Threading.Tasks;
using Lemoine.Collections;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Request class to get ...
  /// </summary>
  public sealed class OperationCycleProperties
    : IRequest<OperationCyclePropertiesResponse>
  {
    static readonly string LONG_CYCLE_MIN_DURATION_KEY = "Business.Operation.Properties.LongCycleMinDuration";
    static readonly TimeSpan LONG_CYCLE_MIN_DURATION_DEFAULT = TimeSpan.FromMinutes (60);

    #region Members
    readonly IMachine m_machine;
    IOperation m_operation;
    readonly IManufacturingOrder m_manufacturingOrder;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (OperationCycleProperties).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="operation">not null</param>
    /// <param name="manufacturingOrder">optional, nullable</param>
    public OperationCycleProperties (IMachine machine, IOperation operation, IManufacturingOrder manufacturingOrder = null)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != operation);

      if (machine is null) {
        log.Error ($"OperationCycleProperties: machine is null");
        throw new ArgumentNullException ("machine");
      }
      if (operation is null) {
        log.Error ($"OperationCycleProperties: operation is null");
        throw new ArgumentNullException ("operation");
      }

      m_machine = machine;
      m_operation = operation;
      m_manufacturingOrder = manufacturingOrder;
    }

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public OperationCyclePropertiesResponse Get ()
    {
      IMonitoredMachine monitoredMachine;
      if (m_machine is IMonitoredMachine) {
        monitoredMachine = (IMonitoredMachine)m_machine;
      }
      else {
        var monitoredMachineBusinessRequest = new Lemoine.Business.Machine.MonitoredMachineFromId (m_machine.Id);
        monitoredMachine = ServiceProvider.Get (monitoredMachineBusinessRequest);
        // Note: here monitoredMachine may remain null if not monitored
      }
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        TimeSpan? cycleDuration = GetCycleDuration (monitoredMachine);
        bool isLongCycle = IsLongCycle (cycleDuration);
        int nbPiecesByCycle = GetNbPiecesByCycle ();
        return new OperationCyclePropertiesResponse (cycleDuration, isLongCycle, nbPiecesByCycle);
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<OperationCyclePropertiesResponse> GetAsync ()
    {
      IMonitoredMachine monitoredMachine;
      if (m_machine is IMonitoredMachine) {
        monitoredMachine = (IMonitoredMachine)m_machine;
      }
      else {
        var monitoredMachineBusinessRequest = new Lemoine.Business.Machine.MonitoredMachineFromId (m_machine.Id);
        monitoredMachine = await ServiceProvider.GetAsync (monitoredMachineBusinessRequest);
        // Note: here monitoredMachine may remain null if not monitored
      }
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        TimeSpan? cycleDuration = GetCycleDuration (monitoredMachine);
        bool isLongCycle = IsLongCycle (cycleDuration);
        int nbPiecesByCycle = GetNbPiecesByCycle ();
        return new OperationCyclePropertiesResponse (cycleDuration, isLongCycle, nbPiecesByCycle);
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      var cacheKey = "Business.Operation.OperationProperties."
        + m_machine.Id
        + "." + ((IDataWithId)m_operation).Id;
      if (null != m_manufacturingOrder) {
        cacheKey += "." + ((IDataWithId)m_manufacturingOrder).Id;
      }
      return cacheKey;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (OperationCyclePropertiesResponse data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<OperationCyclePropertiesResponse> data)
    {
      return true;
    }
    #endregion // IRequest implementation

    /// <summary>
    /// Get the cycle duration of an operation
    /// </summary>
    /// <param name="monitoredMachine">null if not monitored</param>
    /// <returns></returns>
    TimeSpan? GetCycleDuration (IMonitoredMachine monitoredMachine)
    {
      if ((null != m_manufacturingOrder) && (m_manufacturingOrder.CycleDuration.HasValue)) {
        return m_manufacturingOrder.CycleDuration.Value;
      }

      return m_operation.GetStandardCycleDuration (monitoredMachine);
    }

    /// <summary>
    /// Is the cycle a long cycle ?
    /// 
    /// If the cycle duration is not defined, let's consider it is not
    /// </summary>
    /// <param name="cycleDuration"></param>
    /// <returns></returns>
    bool IsLongCycle (TimeSpan? cycleDuration)
    {
      if (!cycleDuration.HasValue) {
        return false;
      }

      TimeSpan longCycleMinDuration = Lemoine.Info.ConfigSet
        .LoadAndGet<TimeSpan> (LONG_CYCLE_MIN_DURATION_KEY, LONG_CYCLE_MIN_DURATION_DEFAULT);
      return longCycleMinDuration <= cycleDuration.Value;
    }

    /// <summary>
    /// Get the number of pieces made by one cycle
    /// </summary>
    /// <returns></returns>
    int GetNbPiecesByCycle ()
    {
      InitializeIntermediateWorkPieces ();

      // - Get the number of pieces made by one cycle
      int nbPiecesByCycle;
      nbPiecesByCycle = m_operation.IntermediateWorkPieces
        .Aggregate (0, (total, next) => total + next.OperationQuantity);
      if (0 == nbPiecesByCycle) {
        log.Fatal ($"GetNbPiecesByCycle: the number of pieces by cycle is 0 for operation {((IDataWithId)m_operation).Id}, which can't be => correct it to 1");
        nbPiecesByCycle = 1;
      }
      return nbPiecesByCycle;
    }

    void InitializeIntermediateWorkPieces ()
    {
      if (!ModelDAOHelper.DAOFactory.IsInitialized (m_operation)
        || !ModelDAOHelper.DAOFactory.IsInitialized (m_operation.IntermediateWorkPieces)) {
        try {
          using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (var transaction = session.BeginReadOnlyTransaction ("Business.OperationCycleProperties.IntermediateWorkPieces")) {
              // Note: with asynchronous calls, it is not possible to use Lock() because you can get the exception
              // LazyInitializationException / Illegally attempted to associate a proxy with two open sessions
              m_operation = ModelDAOHelper.DAOFactory.OperationDAO.FindById (((IDataWithId)m_operation).Id);
              ModelDAOHelper.DAOFactory.Initialize (m_operation.IntermediateWorkPieces);
            }
          }
        }
        catch (Exception ex) {
          log.Error ($"InitializeIntermediateWorkPieces: exception", ex);
          throw;
        }
      }

    }
  }

  /// <summary>
  /// Response of the business request OperationCycleProperties
  /// </summary>
  public sealed class OperationCyclePropertiesResponse
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cycleDuration"></param>
    /// <param name="isLongCycle"></param>
    /// <param name="nbPiecesByCycle"></param>
    internal OperationCyclePropertiesResponse (TimeSpan? cycleDuration, bool isLongCycle, int nbPiecesByCycle)
    {
      this.CycleDuration = cycleDuration;
      this.IsLongCycle = isLongCycle;
      this.NbPiecesByCycle = nbPiecesByCycle;
    }

    /// <summary>
    /// Cycle duration of the operation
    /// </summary>
    public TimeSpan? CycleDuration { get; private set; }

    /// <summary>
    /// Is the cycle a long cycle ?
    /// 
    /// If the cycle duration is not defined, let's consider it is not
    /// </summary>
    public bool IsLongCycle { get; private set; }

    /// <summary>
    /// Number of pieces made by one cycle
    /// </summary>
    public int NbPiecesByCycle { get; private set; }
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.Extensions.Business.Operation;
using System.Collections.Generic;
using Lemoine.ModelDAO;
using System.Threading.Tasks;
using Lemoine.Collections;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Given an intermediate work piece (or an operation) and a machine, return the  production Capacity,
  /// it means the number of parts a specific machine may produce at best per hour
  /// </summary>
  public sealed class ProductionCapacityPerHour
    : IRequest<double?>
  {
    readonly IMachine m_machine;
    readonly IIntermediateWorkPiece m_intermediateWorkPiece;
    readonly IOperation m_operation;

    static readonly ILog log = LogManager.GetLogger (typeof (ProductionCapacityPerHour).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="intermediateWorkPiece">not null</param>
    public ProductionCapacityPerHour (IMachine machine, IIntermediateWorkPiece intermediateWorkPiece)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != intermediateWorkPiece);

      m_machine = machine;
      m_intermediateWorkPiece = intermediateWorkPiece;
      m_operation = null;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="operation">not null</param>
    public ProductionCapacityPerHour (IMachine machine, IOperation operation)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != operation);

      m_machine = machine;
      m_intermediateWorkPiece = null;
      m_operation = operation;
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public double? Get ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetAsync: ProductionCapacityPerHour");
      }

      if (null != m_intermediateWorkPiece) {
        return GetForIntermediateWorkPiece ();
      }
      else if (null != m_operation) {
        return GetForOperation ();
      }
      else {
        Debug.Assert (false);
        throw new InvalidOperationException ("Unexpected");
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<double?> GetAsync ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetAsync: ProductionCapacityPerHour");
      }

      if (null != m_intermediateWorkPiece) {
        return await GetForIntermediateWorkPieceAsync ();
      }
      else if (null != m_operation) {
        return await GetForOperationAsync ();
      }
      else {
        Debug.Assert (false);
        throw new InvalidOperationException ("Unexpected");
      }
    }

    double? GetForIntermediateWorkPiece ()
    {
      Debug.Assert (null != m_intermediateWorkPiece);

      var productionCapacityExtensionsRequest = new Lemoine.Business.Extension.MachineExtensions<IProductionCapacityExtension> (m_machine, (ext, m) => ext.Initialize (m));
      var productionCapacityExtensions = (ServiceProvider.Get (productionCapacityExtensionsRequest))
        .OrderByDescending (ext => ext.Score);
      foreach (var extension in productionCapacityExtensions) {
        try {
          var productionCapacityPerHour = Task.Run (async () => await extension.GetCapacityPerHourAsync (m_intermediateWorkPiece)).Result;
          if (log.IsDebugEnabled) {
            log.Debug ($"GetForIntermediateWorkPiece: production Capacity per hour={productionCapacityPerHour} from extension {extension} for intermediateworkpiece {((IDataWithId)m_intermediateWorkPiece).Id}");
          }
          return productionCapacityPerHour;
        }
        catch (Exception ex) {
          if (log.IsInfoEnabled) {
            log.Info ($"GetForIntermediateWorkPiece: extension {extension} return an exception, try the next one", ex);
          }
        }
      }

      if (log.IsWarnEnabled) {
        log.Warn ($"GetForIntermediateWorkPiece: no extension returned a valid production Capacity => return null");
      }
      return null;
    }

    async Task<double?> GetForIntermediateWorkPieceAsync ()
    {
      Debug.Assert (null != m_intermediateWorkPiece);

      var productionCapacityExtensionsRequest = new Lemoine.Business.Extension.MachineExtensions<IProductionCapacityExtension> (m_machine, (ext, m) => ext.Initialize (m));
      var productionCapacityExtensions = (await ServiceProvider.GetAsync (productionCapacityExtensionsRequest))
        .OrderByDescending (ext => ext.Score);
      foreach (var extension in productionCapacityExtensions) {
        try {
          var productionCapacityPerHour = await extension.GetCapacityPerHourAsync (m_intermediateWorkPiece);
          if (log.IsDebugEnabled) {
            log.Debug ($"GetForIntermediateWorkPieceAsync: production Capacity per hour={productionCapacityPerHour} from extension {extension} for intermediateworkpiece {((IDataWithId)m_intermediateWorkPiece).Id}");
          }
          return productionCapacityPerHour;
        }
        catch (Exception ex) {
          if (log.IsInfoEnabled) {
            log.Info ($"GetForIntermediateWorkPieceAsync: extension {extension} return an exception, try the next one", ex);
          }
        }
      }

      if (log.IsWarnEnabled) {
        log.Warn ($"GetForIntermediateWorkPieceAsync: no extension returned a valid production Capacity => return null");
      }
      return null;
    }

    double? GetForOperation ()
    {
      Debug.Assert (null != m_operation);

      IEnumerable<IIntermediateWorkPiece> intermediateWorkPieces;
      if (ModelDAOHelper.DAOFactory.IsInitialized (m_operation.IntermediateWorkPieces)) {
        intermediateWorkPieces = m_operation.IntermediateWorkPieces;
      }
      else {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          intermediateWorkPieces = ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO
            .FindByOperation (m_operation);
        }
      }

      var capacities = intermediateWorkPieces
        .Select (x => GetByIntermediateWorkPiece (x))
        .Where (x => x.HasValue);
      if (!capacities.Any ()) {
        return null;
      }
      else {
        return capacities.Sum ();
      }
    }

    async System.Threading.Tasks.Task<double?> GetForOperationAsync ()
    {
      Debug.Assert (null != m_operation);

      IEnumerable<IIntermediateWorkPiece> intermediateWorkPieces;
      if (ModelDAOHelper.DAOFactory.IsInitialized (m_operation.IntermediateWorkPieces)) {
        intermediateWorkPieces = m_operation.IntermediateWorkPieces;
      }
      else {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          intermediateWorkPieces = ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO
            .FindByOperation (m_operation);
        }
      }

      IEnumerable<double?> Capacitys = await Task.WhenAll (intermediateWorkPieces
        .Select (async x => await GetByIntermediateWorkPieceAsync (x)));
      Capacitys = Capacitys.Where (x => x.HasValue);
      if (!Capacitys.Any ()) {
        return null;
      }
      else {
        return Capacitys.Sum ();
      }
    }

    double? GetByIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece)
    {
      var request = new ProductionCapacityPerHour (m_machine, intermediateWorkPiece);
      return Lemoine.Business.ServiceProvider
        .Get (request);
    }

    async System.Threading.Tasks.Task<double?> GetByIntermediateWorkPieceAsync (IIntermediateWorkPiece intermediateWorkPiece)
    {
      var request = new ProductionCapacityPerHour (m_machine, intermediateWorkPiece);
      return await Lemoine.Business.ServiceProvider
        .GetAsync (request);
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      if (null != m_intermediateWorkPiece) {
        return $"Business.Operation.ProductionCapacityPerHour.{m_machine.Id}.{((IDataWithId)m_intermediateWorkPiece).Id}";
      }
      else if (null != m_operation) {
        return $"Business.Operation.ProductionCapacityPerHour.{m_machine.Id}.op.{((IDataWithId)m_operation).Id}";
      }
      else {
        Debug.Assert (false);
        throw new InvalidOperationException ("Unexpected");
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (double? data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<double?> data)
    {
      return true;
    }
    #endregion // IRequest implementation
  }
}

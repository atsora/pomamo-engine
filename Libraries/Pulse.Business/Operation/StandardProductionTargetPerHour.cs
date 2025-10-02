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
  /// Given an intermediate work piece (or an operation) and a machine, return the standard production target,
  /// it means the number of parts a specific machine is supposed to produced in one hour
  /// </summary>
  public sealed class StandardProductionTargetPerHour
    : IRequest<double?>
  {
    readonly IMachine m_machine;
    readonly IIntermediateWorkPiece m_intermediateWorkPiece;
    readonly IOperation m_operation;

    static readonly ILog log = LogManager.GetLogger (typeof (StandardProductionTargetPerHour).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="intermediateWorkPiece">not null</param>
    public StandardProductionTargetPerHour (IMachine machine, IIntermediateWorkPiece intermediateWorkPiece)
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
    public StandardProductionTargetPerHour (IMachine machine, IOperation operation)
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
        log.Debug ($"Get: StandardProductionTargetPerHour");
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
        log.Debug ($"GetAsync: StandardProductionTargetPerHour");
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

      var productionTargetExtensionsRequest = new Lemoine.Business.Extension.MachineExtensions<IProductionTargetExtension> (m_machine, (ext, m) => ext.Initialize (m));
      var productionTargetExtensions = (ServiceProvider.Get (productionTargetExtensionsRequest))
        .OrderByDescending (ext => ext.Score);
      foreach (var extension in productionTargetExtensions) {
        try {
          var productionTargetPerHour = extension.GetTargetPerHour (m_intermediateWorkPiece);
          if (log.IsDebugEnabled) {
            log.Debug ($"GetForIntermediateWorkPiece: production target per hour={productionTargetPerHour} from extension {extension} for intermediateworkpiece {((IDataWithId)m_intermediateWorkPiece).Id}");
          }
          return productionTargetPerHour;
        }
        catch (Exception ex) {
          if (log.IsInfoEnabled) {
            log.Info ($"GetForIntermediateWorkPiece: extension {extension} return an exception, try the next one", ex);
          }
        }
      }

      if (log.IsWarnEnabled) {
        log.Warn ($"GetForIntermediateWorkPiece: no extension returned a valid production target for piece={((IDataWithId)m_intermediateWorkPiece).Id}=> return null");
      }
      return null;
    }

    async Task<double?> GetForIntermediateWorkPieceAsync ()
    {
      Debug.Assert (null != m_intermediateWorkPiece);

      var productionTargetExtensionsRequest = new Lemoine.Business.Extension.MachineExtensions<IProductionTargetExtension> (m_machine, (ext, m) => ext.Initialize (m));
      var productionTargetExtensions = (await ServiceProvider.GetAsync (productionTargetExtensionsRequest))
        .OrderByDescending (ext => ext.Score);
      foreach (var extension in productionTargetExtensions) {
        try {
          var productionTargetPerHour = await extension.GetTargetPerHourAsync (m_intermediateWorkPiece);
          if (log.IsDebugEnabled) {
            log.Debug ($"GetForIntermediateWorkPieceAsync: production target per hour={productionTargetPerHour} from extension {extension} for intermediateworkpiece {((IDataWithId)m_intermediateWorkPiece).Id}");
          }
          return productionTargetPerHour;
        }
        catch (Exception ex) {
          if (log.IsInfoEnabled) {
            log.Info ($"GetForIntermediateWorkPieceAsync: extension {extension} return an exception, try the next one", ex);
          }
        }
      }

      if (log.IsWarnEnabled) {
        log.Warn ($"GetForIntermediateWorkPieceAsync: no extension returned a valid production target for piece={((IDataWithId)m_intermediateWorkPiece).Id}=> return null");
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

      IEnumerable<double?> targets = intermediateWorkPieces
        .Select (x => GetByIntermediateWorkPiece (x))
        .Where (x => x.HasValue);
      if (!targets.Any ()) {
        return null;
      }
      else {
        return targets.Sum ();
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

      IEnumerable<double?> targets = await Task.WhenAll (intermediateWorkPieces
        .Select (async x => await GetByIntermediateWorkPieceAsync (x)));
      targets = targets.Where (x => x.HasValue);
      if (!targets.Any ()) {
        return null;
      }
      else {
        return targets.Sum ();
      }
    }

    double? GetByIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece)
    {
      var request = new StandardProductionTargetPerHour (m_machine, intermediateWorkPiece);
      return Lemoine.Business.ServiceProvider
        .Get (request);
    }

    async System.Threading.Tasks.Task<double?> GetByIntermediateWorkPieceAsync (IIntermediateWorkPiece intermediateWorkPiece)
    {
      var request = new StandardProductionTargetPerHour (m_machine, intermediateWorkPiece);
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
        return $"Business.Operation.StandardProductionTargetPerHour.{m_machine.Id}.{((IDataWithId)m_intermediateWorkPiece).Id}";
      }
      else if (null != m_operation) {
        return $"Business.Operation.StandardProductionTargetPerHour.{m_machine.Id}.op.{((IDataWithId)m_operation).Id}";
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

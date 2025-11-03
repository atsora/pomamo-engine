// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Business;
using Lemoine.Collections;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Request class to get ...
  /// </summary>
  public sealed class OperationIntermediateWorkPieces
    : IRequest<ICollection<IIntermediateWorkPiece>>
  {
    readonly IOperation m_operation;

    static readonly ILog log = LogManager.GetLogger (typeof (OperationIntermediateWorkPieces).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="operation">not null</param>
    public OperationIntermediateWorkPieces (IOperation operation)
    {
      Debug.Assert (null != operation);

      m_operation = operation;
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public ICollection<IIntermediateWorkPiece> Get ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: ...");
      }

      if (ModelDAOHelper.DAOFactory.IsInitialized (m_operation)
        && ModelDAOHelper.DAOFactory.IsInitialized (m_operation.IntermediateWorkPieces)) {
        return m_operation.IntermediateWorkPieces;
      }
      else {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var sessionOperation = ModelDAOHelper.DAOFactory.OperationDAO
            .FindById (((IDataWithId)m_operation).Id);
          ModelDAOHelper.DAOFactory.Initialize (sessionOperation.IntermediateWorkPieces);
          return sessionOperation.IntermediateWorkPieces.ToList ();
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<ICollection<IIntermediateWorkPiece>> GetAsync ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetAsync: ...");
      }

      if (ModelDAOHelper.DAOFactory.IsInitialized (m_operation)
        && ModelDAOHelper.DAOFactory.IsInitialized (m_operation.IntermediateWorkPieces)) {
        return m_operation.IntermediateWorkPieces;
      }
      else {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var sessionOperation = await ModelDAOHelper.DAOFactory.OperationDAO
            .FindByIdAsync (((IDataWithId)m_operation).Id);
          ModelDAOHelper.DAOFactory.Initialize (sessionOperation.IntermediateWorkPieces);
          return sessionOperation.IntermediateWorkPieces.ToList ();
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Operation.OperationIntermediateWorkPieces." + m_operation.Id;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (ICollection<IIntermediateWorkPiece> response)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<ICollection<IIntermediateWorkPiece>> data)
    {
      if (null == data.Value) {
        return true;
      }
      else {
        return true;
      }
    }
    #endregion // IRequest implementation
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lemoine.Collections;

namespace Lemoine.Business.Tool
{
  /// <summary>
  /// Request class to get the tools that are associated to an operation
  /// </summary>
  public sealed class OperationTools
    : IRequest<IEnumerable<string>>
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (OperationTools).FullName);

    #region Getters / Setters
    /// <summary>
    /// Operation in the request
    /// 
    /// not null
    /// </summary>
    IOperation Operation { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="operation">not null</param>
    public OperationTools (IOperation operation)
    {
      Debug.Assert (null != operation);

      this.Operation = operation;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>IEnumerable&lt;string&gt; not null</returns>
    public IEnumerable<string> Get ()
    {
      IOperation operation = this.Operation;
      Debug.Assert (null != operation);

      if (!ModelDAOHelper.DAOFactory.IsInitialized (operation)) {
        // sequence is lazy => get a new instance here of the object
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("Business.OperationTools.Operation")) {
            operation = ModelDAOHelper.DAOFactory.OperationDAO.FindById (((IDataWithId)operation).Id);
            ModelDAOHelper.DAOFactory.Initialize (operation.Sequences);
          }
        }
      }
      else if (!ModelDAOHelper.DAOFactory.IsInitialized (operation.Sequences)) {
        try {
          using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (var transaction = session.BeginReadOnlyTransaction ("Business.OperationTools.Sequences")) {
              ModelDAOHelper.DAOFactory.OperationDAO.Lock (operation);
              ModelDAOHelper.DAOFactory.Initialize (operation.Sequences);
            }
          }
        }
        catch (Exception ex) {
          if (Lemoine.Core.ExceptionManagement.ExceptionTest.IsStale (ex)) {
            log.WarnFormat ("Get: operation {0} is stale, try to reload it", ((IDataWithId)operation).Id);
            using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
              using (var transaction = session.BeginReadOnlyTransaction ("Business.OperationTools.Sequences.Stale")) {
                operation = ModelDAOHelper.DAOFactory.OperationDAO.FindById (((IDataWithId)operation).Id);
                ModelDAOHelper.DAOFactory.Initialize (operation.Sequences);
              }
            }
          }
          else {
            throw;
          }
        }
      }

      return operation.Sequences
        .Where (s => !string.IsNullOrEmpty (s.ToolNumber))
        .Select (s => s.ToolNumber)
        .Distinct ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<string>> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Tool.OperationTools." + ((IDataWithId<int>)Operation).Id;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<IEnumerable<string>> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (IEnumerable<string> data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }
    #endregion // IRequest implementation
  }
}

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
  /// Request class to get the sequences that are associated to an operation and a tool
  /// </summary>
  public sealed class OperationToolSequences
    : IRequest<IEnumerable<ISequence>>
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (OperationToolSequences).FullName);

    #region Getters / Setters
    /// <summary>
    /// Operation in the request
    /// 
    /// not null
    /// </summary>
    IOperation Operation { get; set; }

    /// <summary>
    /// Tool number
    /// 
    /// not empty and not null
    /// </summary>
    string ToolNumber { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="operation">not null</param>
    /// <param name="toolNumber">not empty and not null</param>
    public OperationToolSequences (IOperation operation, string toolNumber)
    {
      Debug.Assert (null != operation);
      Debug.Assert (!string.IsNullOrEmpty (toolNumber));

      this.Operation = operation;
      this.ToolNumber = toolNumber;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ISequence> Get ()
    {
      IOperation operation = this.Operation;
      Debug.Assert (null != operation);

      if (!ModelDAOHelper.DAOFactory.IsInitialized (operation)) {
        // sequence is lazy => get a new instance here of the object
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("Business.OperationToolSequences.Operation")) {
            operation = ModelDAOHelper.DAOFactory.OperationDAO.FindById (((IDataWithId)operation).Id);
            ModelDAOHelper.DAOFactory.Initialize (operation.Sequences);
          }
        }
      }
      else if (!ModelDAOHelper.DAOFactory.IsInitialized (operation.Sequences)) {
        try {
          using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (var transaction = session.BeginReadOnlyTransaction ("Business.OperationToolSequences.Sequences")) {
              ModelDAOHelper.DAOFactory.OperationDAO.Lock (operation);
              ModelDAOHelper.DAOFactory.Initialize (operation.Sequences);
            }
          }
        }
        catch (Exception ex) {
          if (Lemoine.Core.ExceptionManagement.ExceptionTest.IsStale (ex)) {
            log.WarnFormat ("Get: operation {0} is stale, try to reload it", ((IDataWithId)operation).Id);
            using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
              using (var transaction = session.BeginReadOnlyTransaction ("Business.OperationToolSequences.Sequences.Stale")) {
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
        .Where (s => this.ToolNumber.Equals (s.ToolNumber, StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<ISequence>> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Tool.OperationToolSequences." + ((IDataWithId<int>)Operation).Id + "." + this.ToolNumber;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<IEnumerable<ISequence>> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (IEnumerable<ISequence> data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }
    #endregion // IRequest implementation
  }
}

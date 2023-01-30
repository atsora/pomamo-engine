// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Business.MachineMode
{
  /// <summary>
  /// IsDescendantOrSelfOf
  /// </summary>
  public class IsDescendantOrSelfOf: IRequest<bool>
  {
    readonly ILog log = LogManager.GetLogger (typeof (IsDescendantOrSelfOf).FullName);

    #region Getters / Setters
    /// <summary>
    /// Ancestor machine mode to test
    /// </summary>
    int AncestorId { get; set; }

    /// <summary>
    /// Possible descendant to test
    /// </summary>
    int DescendantId { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="ancestor">not null</param>
    /// <param name="descendant">not null</param>
    public IsDescendantOrSelfOf (IMachineMode ancestor, IMachineMode descendant)
    {
      Debug.Assert (null != ancestor);
      Debug.Assert (null != descendant);

      this.AncestorId = ancestor.Id;
      this.DescendantId = descendant.Id;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="ancestorId"></param>
    /// <param name="descendantId"></param>
    public IsDescendantOrSelfOf (int ancestorId, int descendantId)
    {
      this.AncestorId = ancestorId;
      this.DescendantId = descendantId;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="ancestor">not null</param>
    /// <param name="descendantId"></param>
    public IsDescendantOrSelfOf (IMachineMode ancestor, int descendantId)
    {
      Debug.Assert (null != ancestor);

      this.AncestorId = ancestor.Id;
      this.DescendantId = descendantId;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="ancestorId"></param>
    /// <param name="descendant">not null</param>
    public IsDescendantOrSelfOf (int ancestorId, IMachineMode descendant)
    {
      Debug.Assert (null != descendant);

      this.AncestorId = ancestorId;
      this.DescendantId = descendant.Id;
    }
    #endregion // Constructors

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public bool Get ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: ancestor {this.AncestorId} descendant {this.DescendantId}");
      }

      if (this.AncestorId == this.DescendantId) {
        return true;
      }

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.MachineMode.IsDescendantOrSelf")) {
          var ancestor = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById (this.AncestorId);
          if (null == ancestor) {
            log.Error ($"Get: ancestor with id {this.AncestorId} does not exist");
            return false;
          }
          var descendant = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById (this.DescendantId);
          if (null == descendant) {
            log.Error ($"Get: descendant with id {this.DescendantId} does not exist");
            return false;
          }
          return descendant.IsDescendantOrSelfOf (ancestor);
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<bool> GetAsync ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetAsync: ancestor {this.AncestorId} descendant {this.DescendantId}");
      }

      if (this.AncestorId == this.DescendantId) {
        return true;
      }

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.MachineMode.IsDescendantOrSelf")) {
          var ancestor = await ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindByIdAsync (this.AncestorId);
          if (null == ancestor) {
            log.Error ($"GetAsync: ancestor with id {this.AncestorId} does not exist");
            return false;
          }
          var descendant = await ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindByIdAsync (this.DescendantId);
          if (null == descendant) {
            log.Error ($"GetAsync: descendant with id {this.DescendantId} does not exist");
            return false;
          }
          return descendant.IsDescendantOrSelfOf (ancestor);
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return $"Business.MachineMode.IsDescendantOrSelfOf.{this.AncestorId}.{this.DescendantId}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<bool> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (bool data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }
    #endregion // IRequest implementation
  }
}

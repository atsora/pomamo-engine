// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;

namespace Lemoine.WebService
{
  /// <summary>
  /// GetNonConformanceReasonList service
  /// </summary>
  public class GetNonConformanceReasonListService: GenericCachedService<Lemoine.DTO.GetNonConformanceReasonList>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GetNonConformanceReasonListService).FullName);

    #region Constructors
    /// <summary>
    /// GetNonConformanceReasonList is a cached service.
    /// </summary>
    public GetNonConformanceReasonListService () : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (Lemoine.DTO.GetNonConformanceReasonList request)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IList<INonConformanceReason> list = ModelDAOHelper.DAOFactory.NonConformanceReasonDAO.FindAll();
        return (new Lemoine.DTO.NonConformanceReasonDTOAssembler()).Assemble(list);
      }
    }
    
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// ReasonFindById Service.
  /// </summary>
  public class ReasonFindByIdService : GenericCachedService<Pulse.Web.WebDataAccess.ReasonFindById>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ReasonFindByIdService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ReasonFindByIdService () : base(Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(Pulse.Web.WebDataAccess.ReasonFindById request)
    {
      int reasonId = request.Id;
      
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IReason reason = ModelDAOHelper.DAOFactory.ReasonDAO
          .FindById (reasonId);
        if (null == reason) {
          return new ErrorDTO ("No reason with id " + reasonId,
                               ErrorStatus.WrongRequestParameter);
        }
        else {
          return new ReasonDTOAssembler ().Assemble (reason);
        }
      }
    }
    #endregion // Methods
  }
}

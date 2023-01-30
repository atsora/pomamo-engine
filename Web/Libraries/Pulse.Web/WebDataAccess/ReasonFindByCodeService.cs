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
  /// ReasonFindByCode Service.
  /// </summary>
  public class ReasonFindByCodeService : GenericCachedService<Pulse.Web.WebDataAccess.ReasonFindByCode>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ReasonFindByCodeService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ReasonFindByCodeService () : base(Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(Pulse.Web.WebDataAccess.ReasonFindByCode request)
    {
      string reasonCode = request.Code;
      
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IReason reason = ModelDAOHelper.DAOFactory.ReasonDAO
          .FindByCode (reasonCode);
        if (null == reason) {
          return new ErrorDTO (string.Format ("No reason with code {0}",
                                              reasonCode),
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

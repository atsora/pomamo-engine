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
  /// UserFindById Service.
  /// </summary>
  public class UserFindByIdService : GenericCachedService<Pulse.Web.WebDataAccess.UserFindById>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (UserFindByIdService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public UserFindByIdService () : base(Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(Pulse.Web.WebDataAccess.UserFindById request)
    {
      int userId = request.Id;
      
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IUser user = ModelDAOHelper.DAOFactory.UserDAO
          .FindById (userId);
        if (null == user) {
          return new ErrorDTO (string.Format ("No user with ID {0}",
                                              userId),
                               ErrorStatus.WrongRequestParameter);
        }
        else {
          return new UserDTOAssembler ().Assemble (user);
        }
      }
    }
    #endregion // Methods
  }
}

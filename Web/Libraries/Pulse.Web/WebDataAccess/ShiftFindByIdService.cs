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
  /// ShiftFindById Service.
  /// </summary>
  public class ShiftFindByIdService : GenericCachedService<Pulse.Web.WebDataAccess.ShiftFindById>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ShiftFindByIdService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ShiftFindByIdService () : base(Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(Pulse.Web.WebDataAccess.ShiftFindById request)
    {
      int id = request.Id;
      
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IShift shift = ModelDAOHelper.DAOFactory.ShiftDAO
          .FindById (id);
        if (null == shift) {
          return new ErrorDTO ("No shift with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }
        else {
          return new ShiftDTOAssembler ().Assemble (shift);
        }
      }
    }
    #endregion // Methods
  }
}

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
  /// GlobalModificationFindById Service.
  /// </summary>
  public class GlobalModificationFindByIdService : GenericCachedService<Pulse.Web.WebDataAccess.GlobalModificationFindById>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GlobalModificationFindByIdService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public GlobalModificationFindByIdService () : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(Pulse.Web.WebDataAccess.GlobalModificationFindById request)
    {
      long machineModificationId = request.Id;
      
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IGlobalModification machineModification = ModelDAOHelper.DAOFactory.GlobalModificationDAO
          .FindById (machineModificationId);
        if (null == machineModification) {
          return new ErrorDTO ("No global modification with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }
        else {
          return new Pulse.Web.WebDataAccess.GlobalModificationDTOAssembler ().Assemble (machineModification);
        }
      }
    }
    #endregion // Methods
  }
}

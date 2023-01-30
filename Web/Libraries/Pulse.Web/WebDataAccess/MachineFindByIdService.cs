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
  /// MachineFindById Service.
  /// </summary>
  public class MachineFindByIdService : GenericCachedService<Pulse.Web.WebDataAccess.MachineFindById>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MachineFindByIdService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public MachineFindByIdService () : base (Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (Pulse.Web.WebDataAccess.MachineFindById request)
    {
      int MachineId = request.Id;

      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IMachine Machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (MachineId);
        if (null == Machine) {
          return new ErrorDTO ("No Machine with id " + MachineId,
                               ErrorStatus.WrongRequestParameter);
        }
        else {
          return new MachineDTOAssembler ().Assemble (Machine);
        }
      }
    }
    #endregion // Methods
  }
}

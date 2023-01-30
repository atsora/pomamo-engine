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
  /// ComputerFindById Service.
  /// </summary>
  public class ComputerGetAlertService : GenericCachedService<Pulse.Web.WebDataAccess.ComputerGetAlert>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ComputerGetAlertService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ComputerGetAlertService () : base (Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (Pulse.Web.WebDataAccess.ComputerGetAlert request)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IComputer alert = ModelDAOHelper.DAOFactory.ComputerDAO
          .GetAlert ();
        if (null == alert) {
          return new ErrorDTO ("No Alert",
                               ErrorStatus.NotApplicable); // Or MissingConfiguration if it makes sense to always define such a computer ?
        }
        else {
          return new ComputerDTOAssembler ().Assemble (alert);
        }
      }
    }
    #endregion // Methods
  }
}

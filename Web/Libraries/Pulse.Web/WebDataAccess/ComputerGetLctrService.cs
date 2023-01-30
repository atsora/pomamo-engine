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
using Pulse.Web.WebDataAccess;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// ComputerFindById Service.
  /// </summary>
  public class ComputerGetLctrService : GenericCachedService<Pulse.Web.WebDataAccess.ComputerGetLctr>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ComputerGetLctrService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ComputerGetLctrService () : base(Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(Pulse.Web.WebDataAccess.ComputerGetLctr request)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IComputer lctr = ModelDAOHelper.DAOFactory.ComputerDAO
          .GetLctr ();
        if (null == lctr) {
          return new ErrorDTO ("No lctr",
                               ErrorStatus.MissingConfiguration);
        }
        else {
          return new ComputerDTOAssembler ().Assemble (lctr);
        }
      }
    }
    #endregion // Methods
  }
}

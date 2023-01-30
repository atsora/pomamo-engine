// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;
using Lemoine.Web;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// ComputerFindById Service.
  /// </summary>
  public class ComputerGetLpostsService : GenericCachedService<Pulse.Web.WebDataAccess.ComputerGetLposts>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ComputerGetLctrService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ComputerGetLpostsService () : base(Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(Pulse.Web.WebDataAccess.ComputerGetLposts request)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IList<IComputer> lposts = ModelDAOHelper.DAOFactory.ComputerDAO
          .GetLposts ();
        return new ComputerDTOAssembler ().Assemble (lposts);
      }
    }
    #endregion // Methods
  }
}

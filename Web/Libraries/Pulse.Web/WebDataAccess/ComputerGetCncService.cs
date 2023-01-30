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
  public class ComputerGetCncService : GenericCachedService<Pulse.Web.WebDataAccess.ComputerGetCnc>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ComputerGetLctrService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ComputerGetCncService () : base(Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(Pulse.Web.WebDataAccess.ComputerGetCnc request)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IList<IComputer> cncs = ModelDAOHelper.DAOFactory.ComputerDAO
          .GetCnc ();
        return new ComputerDTOAssembler ().Assemble (cncs);
      }
    }
    #endregion // Methods
  }
}

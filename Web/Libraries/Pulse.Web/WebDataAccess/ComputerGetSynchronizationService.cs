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
  public class ComputerGetSynchronizationService : GenericCachedService<Pulse.Web.WebDataAccess.ComputerGetSynchronization>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ComputerGetSynchronizationService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ComputerGetSynchronizationService () : base (Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (Pulse.Web.WebDataAccess.ComputerGetSynchronization request)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IComputer synchronization = ModelDAOHelper.DAOFactory.ComputerDAO
          .GetSynchronization ();
        if (null == synchronization) {
          return new ErrorDTO ("No synchronization",
                               ErrorStatus.NotApplicable);
        }
        else {
          return new ComputerDTOAssembler ().Assemble (synchronization);
        }
      }
    }
    #endregion // Methods
  }
}

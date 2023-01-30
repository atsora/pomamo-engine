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
  public class ComputerGetAutoReasonService : GenericCachedService<Pulse.Web.WebDataAccess.ComputerGetAutoReason>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ComputerGetAutoReasonService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ComputerGetAutoReasonService () : base (Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (Pulse.Web.WebDataAccess.ComputerGetAutoReason request)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IComputer autoReason = ModelDAOHelper.DAOFactory.ComputerDAO
          .GetAutoReason ();
        if (null == autoReason) {
          return new ErrorDTO ("No AutoReason",
                               ErrorStatus.NotApplicable); // Or MissingConfiguration if it makes sense to always define such a computer ?
        }
        else {
          return new ComputerDTOAssembler ().Assemble (autoReason);
        }
      }
    }
    #endregion // Methods
  }
}

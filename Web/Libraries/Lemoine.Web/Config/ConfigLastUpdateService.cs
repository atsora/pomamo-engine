// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Lemoine.Extensions.Web.Responses;

namespace Lemoine.Web.Config
{
  /// <summary>
  /// Description of ConfigLastUpdateService
  /// </summary>
  public class ConfigLastUpdateService
    : GenericCachedService<ConfigLastUpdateRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ConfigLastUpdateService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ConfigLastUpdateService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentLong)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(ConfigLastUpdateRequestDTO request)
    {
      // Get the new config update date/time
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.Config.LastUpdate"))
      {
        var applicationState = ModelDAOHelper.DAOFactory.ApplicationStateDAO
          .GetNoCache (ApplicationStateKey.ConfigUpdate.ToKey ());
        if (null == applicationState) {
          log.ErrorFormat ("GetWithoutCache: " +
                           "no application state is defined for {0}",
                           ApplicationStateKey.ConfigUpdate.ToKey ());
          return new ErrorDTO ("No application state is set for " + ApplicationStateKey.ConfigUpdate.ToKey (),
                               ErrorStatus.NotApplicable);
        }
        else {
          var response = new ConfigLastUpdateResponseDTO ();
          response.UpdateDateTime = ConvertDTO.DateTimeUtcToIsoStringMs ((DateTime)applicationState.Value);
          return response;
        }
      }
    }
    #endregion // Methods
  }
}

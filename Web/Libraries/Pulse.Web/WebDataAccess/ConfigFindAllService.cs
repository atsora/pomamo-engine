// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Web;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// 
  /// </summary>
  public class ConfigFindAllService
    : GenericCachedService<ConfigFindAll>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ConfigFindAllService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ConfigFindAllService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (ConfigFindAll request)
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var configs = ModelDAOHelper.DAOFactory.ConfigDAO
          .FindAll ();
        return new ConfigDTOAssembler ().Assemble (configs);
      }
    }
    #endregion // Methods
  }
}

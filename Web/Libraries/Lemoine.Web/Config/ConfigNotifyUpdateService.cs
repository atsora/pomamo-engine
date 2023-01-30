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
using System.Threading.Tasks;

namespace Lemoine.Web.Config
{
  /// <summary>
  /// Description of PulseVersionsService
  /// </summary>
  public class ConfigNotifyUpdateService
    : GenericNoCacheService<ConfigNotifyUpdateRequestDTO>
  {
    static readonly string BROADCAST_KEY = "Web.Broadcast";
    static readonly bool BROADCAST_DEFAULT = true;

    static readonly ILog log = LogManager.GetLogger (typeof (ConfigNotifyUpdateService).FullName);

#if !NSERVICEKIT
    readonly
#endif // NSERVICEKIT
    Lemoine.Core.Cache.ICacheClient m_cacheClient;

    #region Constructors
#if NSERVICEKIT
    /// <summary>
    /// 
    /// </summary>
    public ConfigNotifyUpdateService ()
    {
    }
#else // !NSERVICEKIT
    /// <summary>
    /// 
    /// </summary>
    public ConfigNotifyUpdateService (Lemoine.Core.Cache.ICacheClient cacheClient)
    {
      m_cacheClient = cacheClient;
    }
#endif // NSERVICEKIT
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override async Task<object> Get (ConfigNotifyUpdateRequestDTO request)
    {
#if NSERVICEKIT
      if (!(this.NServiceKitCacheClient is Lemoine.Core.Cache.ICacheClient)) {
        log.FatalFormat ("GetWithoutCache: " +
                         "cache client is not a Lemoine.Core.Cache.ICacheClient");
        return new ErrorDTO ("Cache client not a Lemoine.Core.Cache.ICacheClient", ErrorStatus.UnexpectedError);
      }
      m_cacheClient = this.NServiceKitCacheClient as Lemoine.Core.Cache.ICacheClient;
      Debug.Assert (null != m_cacheClient);
#endif // NSERVICEKIT

      // Flush the cache
      m_cacheClient.FlushAll ();

      // Store the new config update date/time
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ("Web.Config.NotifyUpdate.configupdate")) {
        ModelDAOHelper.DAOFactory.ApplicationStateDAO.Update (ApplicationStateKey.ConfigUpdate.ToKey (), DateTime.UtcNow);

        transaction.Commit ();
      }

      // Broadcast it to the other web services
      if (Lemoine.Info.ConfigSet.LoadAndGet<bool> (BROADCAST_KEY, BROADCAST_DEFAULT)) {
        IList<IComputer> webComputers;
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.Config.NotifyUpdate.broadcast")) {
          webComputers = await ModelDAOHelper.DAOFactory.ComputerDAO.GetWebAsync ();
        }
        if (1 < webComputers.Count) {
          await Lemoine.WebClient.Query
            .BroadcastAsync (webComputers.Select (c => new Tuple<string, string, string> (c.Name, c.Address, c.WebServiceUrl)), "/Cache/Flush");
        }
      }

      return new OkDTO ("");
    }
    #endregion // Methods
  }
}

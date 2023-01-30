// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Business.Cache;
using Lemoine.Extensions.Web.Responses;
using System.Threading.Tasks;
using Lemoine.Web;

namespace Pulse.Web.Cache
{
  /// <summary>
  /// Description of ClearDomainService
  /// </summary>
  public class ClearDomainService
    : GenericNoCacheService<ClearDomainRequestDTO>
  {
    static readonly string BROADCAST_KEY = "Web.Broadcast";
    static readonly bool BROADCAST_DEFAULT = true;

    static readonly ILog log = LogManager.GetLogger (typeof (ClearDomainService).FullName);

#if !NSERVICEKIT
    readonly
#endif // NSERVICEKIT
    Lemoine.Core.Cache.ICacheClient m_cacheClient;

    #region Constructors
#if NSERVICEKIT
    /// <summary>
    /// 
    /// </summary>
    public ClearDomainService ()
    {
    }
#else // !NSERVICEKIT
    /// <summary>
    /// 
    /// </summary>
    public ClearDomainService (Lemoine.Core.Cache.ICacheClient cacheClient)
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
    public override async Task<object> Get (ClearDomainRequestDTO request)
    {
      if (string.IsNullOrEmpty (request.Domain)) {
        log.ErrorFormat ("GetWithoutCache: " +
                         "empty domain");
        return new ErrorDTO ("empty domain", ErrorStatus.WrongRequestParameter);
      }

#if NSERVICEKIT
      if (!(this.NServiceKitCacheClient is Lemoine.Core.Cache.ICacheClient)) {
        log.FatalFormat ("GetWithoutCache: " +
                         "cache client is not a Lemoine.Core.Cache.ICacheClient");
        return new ErrorDTO ("Cache client not a Lemoine.Core.Cache.ICacheClient", ErrorStatus.UnexpectedError);
      }
      m_cacheClient = this.NServiceKitCacheClient as Lemoine.Core.Cache.ICacheClient;
      Debug.Assert (null != m_cacheClient);
#endif // NSERVICEKIT

      try {
        var task = ClearDomainAsync (m_cacheClient, request.Domain, request.Broadcast, request.Wait);
        if (request.Wait) {
          await task;
        }
      }
      catch (Exception ex) {
        log.Error ("GetWithoutCache: " +
                   "exception in ClearDomain", ex);
        return new ErrorDTO ("exception", ErrorStatus.TransientProcessError);
      }

      return new OkDTO ("");
    }
    #endregion // Methods

    /// <summary>
    /// Clear the cache domain asynchronously
    /// </summary>
    /// <param name="cacheClient"></param>
    /// <param name="domain"></param>
    /// <param name="broadcast"></param>
    /// <param name="rethrow">rethrow any exception</param>
    /// <returns></returns>
    public static async System.Threading.Tasks.Task ClearDomainAsync (Lemoine.Core.Cache.ICacheClient cacheClient, string domain, bool broadcast, bool rethrow = false)
    {
      try {
        var cleared = await cacheClient.ClearDomainAsync (domain);
        if (!cleared) {
          return;
        }

        // Broadcast it to the other web services
        if (broadcast
            && Lemoine.Info.ConfigSet.LoadAndGet<bool> (BROADCAST_KEY, BROADCAST_DEFAULT)) {
          IList<IComputer> webComputers;
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.Cache.ClearDomain.broadcast")) {
            webComputers = await ModelDAOHelper.DAOFactory.ComputerDAO.GetWebAsync ();
          }
          if (1 < webComputers.Count) {
            var request = $"/Cache/ClearDomain/{domain}?Broadcast=false";
            await Lemoine.WebClient.Query
              .BroadcastAsync (webComputers.Select (c => new Tuple<string, string, string> (c.Name, c.Address, c.WebServiceUrl)), request);
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"ClearDomainAsync: exception wait={rethrow}", ex);
        if (rethrow) {
          throw;
        }
      }
    }

    /// <summary>
    /// Clear the cache domain
    /// </summary>
    /// <param name="cacheClient"></param>
    /// <param name="domain"></param>
    /// <param name="broadcast"></param>
    /// <param name="wait"></param>
    public static void ClearDomain (Lemoine.Core.Cache.ICacheClient cacheClient, string domain, bool broadcast, bool wait = false)
    {
      var cleared = cacheClient.ClearDomain (domain);
      if (!cleared) {
        return;
      }

      // Broadcast it to the other web services
      if (broadcast
          && Lemoine.Info.ConfigSet.LoadAndGet<bool> (BROADCAST_KEY, BROADCAST_DEFAULT)) {
        IList<IComputer> webComputers;
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.Cache.ClearDomain.broadcast")) {
          webComputers = ModelDAOHelper.DAOFactory.ComputerDAO.GetWeb ();
        }
        if (1 < webComputers.Count) {
          var request = $"/Cache/ClearDomain/{domain}?Broadcast=false";
          var task = Lemoine.WebClient.Query
            .BroadcastAsync (webComputers.Select (c => new Tuple<string, string, string> (c.Name, c.Address, c.WebServiceUrl)), request, rethrow: wait);
          if (wait) {
            task.Wait ();
          }
        }
      }
    }
  }
}

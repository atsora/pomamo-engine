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
using Lemoine.Extensions.Web.Responses;
using System.Threading.Tasks;
using Lemoine.Web;

namespace Pulse.Web.Cache
{
  /// <summary>
  /// Description of FlushCacheService
  /// </summary>
  public class ClearPulseInfoService
    : GenericNoCacheService<ClearPulseInfoRequestDTO>
  {
    static readonly string BROADCAST_KEY = "Web.Broadcast";
    static readonly bool BROADCAST_DEFAULT = true;

    static readonly ILog log = LogManager.GetLogger(typeof (ClearPulseInfoService).FullName);

#if !NSERVICEKIT
    readonly
#endif // NSERVICEKIT
    Lemoine.Core.Cache.ICacheClient m_cacheClient;

    #region Constructors
#if NSERVICEKIT
    /// <summary>
    /// 
    /// </summary>
    public ClearPulseInfoService ()
    {
    }
#else // !NSERVICEKIT
    /// <summary>
    /// 
    /// </summary>
    public ClearPulseInfoService (Lemoine.Core.Cache.ICacheClient cacheClient)
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
    public override async Task<object> Get (ClearPulseInfoRequestDTO request)
    {
      Lemoine.Info.PulseInfo.Reload ();

#if NSERVICEKIT
      if (!(this.NServiceKitCacheClient is Lemoine.Core.Cache.ICacheClient)) {
        log.FatalFormat ("GetWithoutCache: " +
                         "cache client is not a Lemoine.Core.Cache.ICacheClient");
        return new ErrorDTO ("Cache client not a Lemoine.Core.Cache.ICacheClient", ErrorStatus.UnexpectedError);
      }
      m_cacheClient = this.NServiceKitCacheClient as Lemoine.Core.Cache.ICacheClient;
      Debug.Assert (null != m_cacheClient);
#endif // NSERVICEKIT

      if (request.Wait) {
        await ClearDomainService.ClearDomainAsync (m_cacheClient, "PulseInfo", false);
      }
      else {
        _ = ClearDomainService.ClearDomainAsync (m_cacheClient, "PulseInfo", false);
      }

      // Broadcast it to the other web services
      if (Lemoine.Info.ConfigSet.LoadAndGet<bool> (BROADCAST_KEY, BROADCAST_DEFAULT)) {
        IList<IComputer> webComputers;
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.Cache.ClearPulseInfo.broadcast"))
        {
          webComputers = ModelDAOHelper.DAOFactory.ComputerDAO.GetWeb ();
        }
        if (1 < webComputers.Count) {
          var requestString = "/Cache/ClearPulseInfo?Broadcast=false";
          var task = Lemoine.WebClient.Query
            .BroadcastAsync (webComputers.Select (c => new Tuple<string, string, string> (c.Name, c.Address, c.WebServiceUrl)), requestString, rethrow: request.Wait);
          ;
          if (request.Wait) {
            await task;
          }
        }
      }
      
      return new OkDTO ("PulseInfo cache cleared");
    }
#endregion // Methods
  }
}

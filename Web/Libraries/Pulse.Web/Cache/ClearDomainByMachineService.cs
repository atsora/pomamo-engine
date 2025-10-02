// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
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
  /// Description of ClearDomainByMachineService
  /// </summary>
  public class ClearDomainByMachineService
    : GenericNoCacheService<ClearDomainByMachineRequestDTO>
  {
    static readonly string BROADCAST_KEY = "Web.Broadcast";
    static readonly bool BROADCAST_DEFAULT = true;

    static readonly ILog log = LogManager.GetLogger(typeof (ClearDomainByMachineService).FullName);

    readonly Lemoine.Core.Cache.ICacheClient m_cacheClient;

    /// <summary>
    /// 
    /// </summary>
    public ClearDomainByMachineService (Lemoine.Core.Cache.ICacheClient cacheClient)
    {
      m_cacheClient = cacheClient;
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override async Task<object> Get (ClearDomainByMachineRequestDTO request)
    {
      if (string.IsNullOrEmpty (request.Domain)) {
        log.Error ("GetWithoutCache: empty domain");
        return new ErrorDTO ("empty domain", ErrorStatus.WrongRequestParameter);
      }

      try {
        var task = ClearDomainAsync (m_cacheClient, request.Domain, request.MachineId, request.Broadcast, rethrow: request.Wait);
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

    /// <summary>
    /// Clear the cache domain
    /// </summary>
    /// <param name="cacheClient"></param>
    /// <param name="domain"></param>
    /// <param name="machineId"></param>
    /// <param name="broadcast"></param>
    /// <param name="rethrow"></param>
    public static async Task ClearDomainAsync (Lemoine.Core.Cache.ICacheClient cacheClient, string domain, int machineId, bool broadcast, bool rethrow = true)
    {
      try {
        var cleared = await cacheClient.ClearDomainByMachineAsync (domain, machineId);
        if (!cleared) {
          return;
        }

        // Broadcast it to the other web services
        if (broadcast
            && Lemoine.Info.ConfigSet.LoadAndGet<bool> (BROADCAST_KEY, BROADCAST_DEFAULT)) {
          IList<IComputer> webComputers;
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.Cache.ClearDomainByMachine.broadcast")) {
            webComputers = ModelDAOHelper.DAOFactory.ComputerDAO.GetWeb ();
          }
          if (1 < webComputers.Count) {
            var request = $"/Cache/ClearDomainByMachine/{domain}/{machineId}?Broadcast=false";
            await Lemoine.WebClient.Query
              .BroadcastAsync (webComputers.Select (c => new Tuple<string, string, string> (c.Name, c.Address, c.WebServiceUrl)), request);
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"ClearDomainAsync: exception rethrow={rethrow}", ex);
        if (rethrow) {
          throw;
        }
      }
    }
  }
}

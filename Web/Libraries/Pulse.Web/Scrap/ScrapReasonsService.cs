// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.ModelDAO;
using Lemoine.Web;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Scrap
{
  /// <summary>
  /// Non conformance reasons that can be used to declare scrap on a machine
  ///
  /// Every reason applies to every machine for the moment.
  /// The reasons are sorted by display priority (the ones without a priority last), then by display
  /// </summary>
  public class ScrapReasonsService
    : GenericCachedService<ScrapReasonsRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ScrapReasonsService).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ScrapReasonsService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override async Task<object> Get (ScrapReasonsRequestDTO request)
    {
      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("Web.Scrap.ScrapReasons")) {
            var machine = await ModelDAOHelper.DAOFactory.MonitoredMachineDAO
              .FindByIdAsync (request.MachineId);
            if (machine is null) {
              log.Error ($"Get: unknown machine with ID {request.MachineId}");
              return new ErrorDTO ("No machine with the specified ID",
                                   ErrorStatus.WrongRequestParameter);
            }

            var reasons = await ModelDAOHelper.DAOFactory.NonConformanceReasonDAO
              .FindAllAsync ();
            var assembler = new NonConformanceReasonDTOAssembler ();
            return new ScrapReasonsResponseDTO {
              Reasons = reasons
                .OrderBy (x => x.DisplayPriority.HasValue ? 0 : 1)
                .ThenBy (x => x.DisplayPriority ?? 0.0)
                .ThenBy (x => x.Display, StringComparer.CurrentCultureIgnoreCase)
                .Select (x => assembler.Assemble (x))
                .ToList ()
            };
          }
        }
      }
      catch (Exception ex) {
        log.Error ("Get: exception", ex);
        throw;
      }
    }
  }
}

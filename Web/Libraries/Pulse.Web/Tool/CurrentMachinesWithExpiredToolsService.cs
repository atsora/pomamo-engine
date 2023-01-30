// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Business.Tool;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Web;
using Pulse.Extensions.Web.Responses;

namespace Pulse.Web.Tool
{
  /// <summary>
  /// Description of CurrentMachinesWithExpiredToolsService.
  /// </summary>
  public class CurrentMachinesWithExpiredToolsService : GenericCachedService<CurrentMachinesWithExpiredToolsRequestDTO>
  {
    static readonly string DEFAULT_MAX_REMAINING_DURATION_KEY = "Web.Tool.CurrentMachinesWithExpiredTools.DefaultMaxRemainingDuration";
    static readonly TimeSpan DEFAULT_MAX_REMAINING_DURATION_DEFAULT = TimeSpan.FromHours (2);
    
    static readonly ILog log = LogManager.GetLogger(typeof (CurrentMachinesWithExpiredToolsService).FullName);
    
    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public CurrentMachinesWithExpiredToolsService () : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(CurrentMachinesWithExpiredToolsRequestDTO request)
    {
      TimeSpan maxRemainingDuration;
      if (request.MaxRemainingDuration.HasValue) {
        maxRemainingDuration = TimeSpan.FromSeconds (request.MaxRemainingDuration.Value);
      }
      else { // Default value: 2 hours
        maxRemainingDuration = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (DEFAULT_MAX_REMAINING_DURATION_KEY,
                                                                            DEFAULT_MAX_REMAINING_DURATION_DEFAULT);
      }

      var machinesWithExpiringToolsRequest = new MachinesWithExpiringTools (maxRemainingDuration);
      var machinesWithExpiringTools = Lemoine.Business.ServiceProvider
        .Get<MachinesWithExpiringToolsResponse> (machinesWithExpiringToolsRequest);

      IList<MachineDTO> result = new List<MachineDTO> ();
      foreach (var machine in machinesWithExpiringTools.Machines) {
        result.Add (new MachineDTOAssembler ().Assemble (machine));
      }
      
      return result;
    }
    #endregion // Methods
  }
}

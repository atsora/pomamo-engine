// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;
using System.Diagnostics;

using Lemoine.Core.Log;

using NServiceKit.ServiceInterface; // Define GetCacheClient

using Lemoine.Model;
using Lemoine.ModelDAO;


namespace Lemoine.WebService
{
  
  /// <summary>
  /// Web Services for Date Time
  /// </summary>
  public class DateTimeService : NServiceKit.ServiceInterface.Service
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DateTimeService).FullName);
    
    #region Methods
    
    /// <summary>
    /// Response to GET request for GetRangeAround
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Lemoine.DTO.GetRangeAround request) {
      return new GetRangeAroundService().Get(this.GetCacheClient(),
                                             base.RequestContext,
                                             base.Request,
                                             request);
    }

    /// <summary>
    /// Response to GET request for GetLastShift
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Lemoine.DTO.GetLastShift request)
    {
      return new GetLastShiftService ()
        .Get (this.GetCacheClient(),
              base.RequestContext,
              base.Request,
              request);
    }
    
    /// <summary>
    /// Response to GET request for GetListOfShiftSlot
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Lemoine.DTO.GetListOfShiftSlot request) {
      return new GetListOfShiftSlotService().Get(this.GetCacheClient(),
                                                 base.RequestContext,
                                                 base.Request,
                                                 request);
    }
    #endregion // Methods
  }
}
#endif // NSERVICEKIT

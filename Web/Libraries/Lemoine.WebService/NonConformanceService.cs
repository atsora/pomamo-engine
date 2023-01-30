// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;
using Lemoine.DTO;
using Lemoine.Core.Log;

using NServiceKit.ServiceInterface;

using Lemoine.Collections;
using Lemoine.Model;
using Lemoine.ModelDAO;


namespace Lemoine.WebService
{
  /// <summary>
  /// Web service about Nonconformance.
  /// </summary>
  public class NonConformanceService : Service
  {

    static readonly ILog log = LogManager.GetLogger(typeof (NonConformanceService).FullName);

#region Methods
    
#region GetNonConformanceReasonList
    /// <summary>
    /// Response to GET request for GetNonConformanceReasonList
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Lemoine.DTO.GetNonConformanceReasonList request) {
      return new GetNonConformanceReasonListService().Get(this.GetCacheClient(),
                                                          base.RequestContext,
                                                          base.Request,
                                                          request);
    }
#endregion //GetNonConformanceReason
    
    
#region SaveNonConformanceReport
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Lemoine.DTO.SaveNonConformanceReport request) {
      return new SaveNonConformanceReportService().Get(base.RequestContext,
                                                       base.Request,
                                                       request);
    }
#endregion //SaveNonConformanceReport

  }
    
#endregion // Methods
}
#endif // NSERVICEKIT
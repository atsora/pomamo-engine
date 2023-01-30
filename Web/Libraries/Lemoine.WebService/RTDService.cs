// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Lemoine.DTO;
using Lemoine.Core.Log;
using NServiceKit.ServiceHost;
using NServiceKit.ServiceInterface;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System.ComponentModel;



namespace Lemoine.WebService
{
  
  /// <summary>
  /// Web Services for RTD
  /// </summary>
  // [EnableCors(allowedMethods:"GET,POST,OPTIONS")] // does not compile prior 4.0 but not necessary it seems
  public class RTDService : Service
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RTDService).FullName);
    
    #region Methods
    
    #region GetMachineTree
    /// <summary>
    /// Response to GET request for GetMachineTree
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Lemoine.DTO.GetMachineTree request) {
      return new GetMachineTreeService().Get(this.GetCacheClient(),
                                             base.RequestContext,
                                             base.Request,
                                             request);
    }
    #endregion
    
    #endregion // Methods
  }
}
#endif // NSERVICEKIT

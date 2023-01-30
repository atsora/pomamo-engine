// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Lemoine.Core.Log;
using NServiceKit.ServiceInterface;

using Lemoine.DTO;
using Lemoine.Collections;
using Lemoine.Model;
using Lemoine.ModelDAO;

using System.IO;
using System.Runtime.Serialization;

using System.Web;


namespace Lemoine.WebService
{
  
  /// <summary>
  /// Web Service for UAT
  /// </summary>
  public class UATService : Service
  {

    static readonly ILog log = LogManager.GetLogger(typeof (UATService).FullName);
    
#region Methods
    
#region GetLastMachineStatusV2
    /// <summary>
    /// Response to GET request for GetLastMachineStatusV2
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Lemoine.DTO.GetLastMachineStatusV2 request) {
      return new GetLastMachineStatusV2Service().Get(this.GetCacheClient(),
                                                     base.RequestContext,
                                                     base.Request,
                                                     request);
    }
#endregion
    
#region GetMachineObservationStateListV2
    /// <summary>
    /// Response to GET request for GetMachineObservationStateListV2
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Lemoine.DTO.GetMachineObservationStateListV2 request) {
      return new GetMachineObservationStateListV2Service().Get(this.GetCacheClient(),
                                                               base.RequestContext,
                                                               base.Request,
                                                               request);
    }
#endregion
    
#region GetMachineObservationStateSelection
    /// <summary>
    /// Response to GET request for GetMachineObservationStateSelection
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Lemoine.DTO.GetMachineObservationStateSelection request) {
      return new GetMachineObservationStateSelectionService().Get(this.GetCacheClient(),
                                                                  base.RequestContext,
                                                                  base.Request,
                                                                  request);
    }
#endregion
    
#region SaveMachineObservationStateV2
    /// <summary>
    /// Response to GET request for SaveMachineObservationStateV2
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Lemoine.DTO.SaveMachineObservationStateV2 request) {
      return new SaveMachineObservationStateV2Service().Get(base.RequestContext,
                                                            base.Request,
                                                            request);
    }
#endregion
    
#region GetLastCycleWithSerialNumberV2
    /// <summary>
    /// Response to GET request for GetLastCycleWithSerialNumberV2
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Lemoine.DTO.GetLastCycleWithSerialNumberV2 request) {
      return new GetLastCycleWithSerialNumberV2Service().Get(this.GetCacheClient(),
                                                             base.RequestContext,
                                                             base.Request,
                                                             request);
    }
#endregion
    
#region GetCyclesWithWorkInformationsInPeriodV2
    /// <summary>
    /// Response to GET request for GetCyclesWithWorkInformationsInPeriodV2
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Lemoine.DTO.GetCyclesWithWorkInformationsInPeriodV2 request) {
      return new GetCyclesWithWorkInformationsInPeriodV2Service().Get(this.GetCacheClient(),
                                                                      base.RequestContext,
                                                                      base.Request,
                                                                      request);
    }
#endregion
    
#region SaveSerialNumberV5
    /// <summary>
    /// Response to GET request for SaveSerialNumberV5
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Lemoine.DTO.SaveSerialNumberV5 request)
    {
      return new SaveSerialNumberV5Service().Get(base.RequestContext,
                                                 base.Request,
                                                 request);
    }
#endregion
    
#region GetLastWorkInformationV3
    /// <summary>
    /// Response to GET request for GetLastWorkInformationV3
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Lemoine.DTO.GetLastWorkInformationV3 request) {
      return new GetLastWorkInformationV3Service().Get(this.GetCacheClient(),
                                                       base.RequestContext,
                                                       base.Request,
                                                       request);
    }
#endregion
    
#region GetListOfOperationSlotV2
    /// <summary>
    /// Response to GET request for GetListOfOperationSlotV2
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Lemoine.DTO.GetListOfOperationSlotV2 request) {
      return new GetListOfOperationSlotV2Service().Get(this.GetCacheClient(),
                                                       base.RequestContext,
                                                       base.Request,
                                                       request);
    }
#endregion
    
#region GetOperationCycles
    /// <summary>
    /// Response to GET request for GetOperationCycles
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Lemoine.DTO.GetOperationCycles request) {
      return new GetOperationCyclesService().Get(this.GetCacheClient(),
                                                 base.RequestContext,
                                                 base.Request,
                                                 request);
    }
#endregion
    
#endregion // Methods
  }
}
#endif // NSERVICEKIT
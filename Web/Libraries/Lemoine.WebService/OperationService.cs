// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;
using Lemoine.DTO;
using Lemoine.Core.Log;

using NServiceKit.ServiceInterface;

namespace Lemoine.WebService
{
  /// <summary>
  /// Description of OperationService.
  /// </summary>
  public class OperationService : Service
  {

    #region Methods

    #region GetOperationCycleDeliverablePieceWithWorkInformation
    /// <summary>
    /// Response to GET request for GetOperationCycleDeliverablePiece
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(Lemoine.DTO.GetOperationCycleDeliverablePieceWithWorkInformation request) {
      return new GetOperationCycleDeliverablePieceWithWorkInformationService().Get(this.GetCacheClient(),
                                                   base.RequestContext,
                                                   base.Request,
                                                   request);
    }    
    #endregion //GetOperationCycleDeliverablePieceWithWorkInformation
    
    #endregion // Methods
    
    
  }
}
#endif // NSERVICEKIT
// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Net;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Modification
{
  /// <summary>
  /// Request DTO for PendingModifications
  /// </summary>
  [Route("/PendingModifications/", "GET", Notes="To be used with ?RevisionId= or ?ModificationId=")]
  [Route("/PendingModifications/Get/{RevisionId}", "GET")]
  [Route("/Modification/Pending/", "GET", Notes="To be used with ?RevisionId= or ?ModificationId=")]
  [Route("/Modification/Pending/Get/{RevisionId}", "GET")]
  [Route("/GetPendingModificationsFromRevision/", "GET")] // To keep some compatibility and remove the old web service
  [Route("/GetPendingModificationsFromRevision/Get/{RevisionId}", "GET")] // To keep some compatibility and remove the old web service
  public class PendingModificationsRequestDTO: IReturn<PendingModificationsResponseDTO>
  {
    /// <summary>
    /// Id of requested revision
    /// </summary>
    [ApiMember(Name="RevisionId", Description="Revision Id", ParameterType="path", DataType="int", IsRequired=false)]
    public int RevisionId { get; set; }
    
    /// <summary>
    /// Id of requested modification
    /// </summary>
    [ApiMember(Name="ModificationId", Description="Modification Id", ParameterType="path", DataType="int", IsRequired=false)]
    public int ModificationId { get; set; }
    
    /// <summary>
    /// Alternative revision ID (not documented because deprecated)
    /// </summary>
    public int Id { get; set; }
  }  
  
}


// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.Core.Log;
using Pulse.Extensions.Web.Responses;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Reponse DTO for ReasonSave service
  /// </summary>
  [Api("ReasonSave Response DTO")]
  public class ReasonSaveResponseDTO
  {
    /// <summary>
    /// Revision
    /// </summary>
    public RevisionDTO Revision { get; set; }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="revision"></param>
    public ReasonSaveResponseDTO (IRevision revision)
    {
      this.Revision = new RevisionDTO (revision);
    }
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT

namespace Pulse.Web.Modification
{
  /// <summary>
  /// Response DTO for PendingModifications
  /// </summary>
  [Api("PendingModifications Response DTO")]
  public class PendingModificationsResponseDTO
  {
    /// <summary>
    /// Number of pending modifications
    /// </summary>
    public double Number { get; set; }    
    
    /// <summary>
    /// Id of requested revision
    /// </summary>
    public int RevisionId { get; set; }
  }
}

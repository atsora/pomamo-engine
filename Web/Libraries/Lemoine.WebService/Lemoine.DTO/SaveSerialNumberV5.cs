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
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Request DTO for SaveSerialNumber.
  /// </summary>
  [Route("/SaveSerialNumberV5/", "GET")]
  [Route("/SaveSerialNumberV5/{MachineId}/{DateTime}/{IsBegin}/{SerialNumber}", "GET")]

  public class SaveSerialNumberV5
  {
    /// <summary>
    /// Id of the Monitored Machine
    /// </summary>
    public int MachineId { get; set; }
    
    /// <summary>
    /// Begin or end date time as offset in ISO format
    /// </summary>
    public string DateTime { get; set; }
    
    /// <summary>
    /// Is DateTime a begin or end ?
    /// </summary>
    public bool IsBegin { get; set; }
    
    /// <summary>
    /// Serial Number
    /// </summary>
    public string SerialNumber { get; set; }
  }
}

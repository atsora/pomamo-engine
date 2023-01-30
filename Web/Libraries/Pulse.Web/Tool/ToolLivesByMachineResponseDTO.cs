// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Model;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Tool
{
  /// <summary>
  /// Response DTO for one item
  /// </summary>
  [Api ("Response DTO for /ToolLivesByMachine service")]
  public class ToolLivesByMachineResponseDTO
  {
    /// <summary>
    /// Constructor (default values)
    /// </summary>
    public ToolLivesByMachineResponseDTO ()
    {
      this.Tools = new List<ToolPropertyDTO> ();
    }

    /// <summary>
    /// Current operation of the machine
    /// </summary>
    public OperationDTO Operation { get; set; }

    /// <summary>
    /// Details on each tool
    /// 
    /// First expiring tools are returned first
    /// </summary>
    public IList<ToolPropertyDTO> Tools { get; set; }

    /// <summary>
    /// UTC Date/time of the response in ISO format 
    /// </summary>
    public string DateTime { get; set; }

    #region Update methods
    /// <summary>
    /// Specify an operation in a ToolLivesByMachineResponseDTO
    /// </summary>
    /// <param name="operation">nullable</param>
    /// <returns></returns>
    internal void UpdateOperation (IOperation operation)
    {
      this.Operation = new OperationDTOAssembler ().AssembleLong (operation);
    }
    #endregion // Update methods
  }

  /// <summary>
  /// Property of a tool that will reach a limit/warning
  /// </summary>
  public class ToolPropertyDTO
  {
    /// <summary>
    /// Does this tool corresponds to a tool group instead ?
    /// </summary>
    public bool Group { get; set; }

    /// <summary>
    /// Display of ToolPosition
    /// </summary>
    public string Display { get; set; }

    /// <summary>
    /// Is it the active sister tool
    /// 
    /// (for individual tools only)
    /// </summary>
    public bool ActiveSisterTool { get; set; }

    /// <summary>
    /// Are there still valid sister tools after it ?
    /// 
    /// (for individual tools part of a group only)
    /// 
    /// false by default if not part of a tool group
    /// </summary>
    public bool ValidSisterTools { get; set; }

    /// <summary>
    /// Remaining number of cycles before expiration
    /// </summary>
    public double? RemainingCycles { get; set; }

    /// <summary>
    /// Estimated expiration date/time if not expired yet
    /// 
    /// If already expired, past expiration date/time (not possible yet, to complete later)
    /// </summary>
    public string ExpirationDateTime { get; set; }

    /// <summary>
    /// Estimated expiration date/time range in case no expiration date/time could be determined precisely
    /// </summary>
    public string ExpirationDateTimeRange { get; set; }

    /// <summary>
    /// The tool is expired
    /// </summary>
    public bool Expired { get; set; }

    /// <summary>
    /// The tool is is in warning
    /// </summary>
    public bool Warning { get; set; }
  }
}
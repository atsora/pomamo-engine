// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Signal
{
  public class SignalMessageDTO
  {
    /// <summary>
    /// Message
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Background color
    /// </summary>
    public string BgColor { get; set; }

    /// <summary>
    /// Foreground color
    /// </summary>
    public string FgColor { get; set; }
  }

  /// <summary>
  /// Response DTO for Signal
  /// </summary>
  [Api ("Signal Response DTO")]
  public class SignalResponseDTO
  {
    /// <summary>
    /// Messages
    /// 
    /// Not null
    /// </summary>
    public List<SignalMessageDTO> Messages { get; set; } = new List<SignalMessageDTO> ();

    /// <summary>
    /// Add a message to the response
    /// </summary>
    /// <param name="message">The message content</param>
    /// <param name="bgColor">The background color</param>
    /// <param name="fgColor">The foreground color</param>
    public void AddMessage (string message, string bgColor, string fgColor)
    {
      this.Messages.Add (new SignalMessageDTO () { Message = message, BgColor = bgColor, FgColor = fgColor });
    }
  }
}

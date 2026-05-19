// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Pulse.Extensions.Web
{
  /// <summary>
  /// Signal to broadcast
  /// </summary>
  public class Signal
  {
    /// <summary>
    /// Message to display
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Color that is associated to the message (for example, #FFFFFF)
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message">not null</param>
    /// <param name="color">not null</param>
    Signal (string message, string color)
    {
      Debug.Assert (null != message);
      Debug.Assert (null != color);

      this.Message = message;
      this.Color = color;
    }
  }
}

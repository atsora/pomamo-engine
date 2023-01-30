// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Model;

namespace Pulse.Web.CncValue
{
  /// <summary>
  /// Stack light DTO
  /// </summary>
  [Api ("StackLight Response DTO")]
  public class StackLightDTO
  {
    /// <summary>
    /// Lights
    /// </summary>
    public List<StackSingleLightDTO> Lights { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="stackLight"></param>
    public StackLightDTO (StackLight stackLight)
    {
      this.Lights = new List<StackSingleLightDTO> ();
      AddSingleLight (stackLight, StackLightColor.Red);
      AddSingleLight (stackLight, StackLightColor.Yellow);
      AddSingleLight (stackLight, StackLightColor.Green);
      AddSingleLight (stackLight, StackLightColor.Blue);
      AddSingleLight (stackLight, StackLightColor.White);
    }

    void AddSingleLight (StackLight stackLight, StackLightColor color)
    {
      var status = stackLight.GetStatus (color);
      if (!status.Equals (StackLightStatus.NotAcquired)) {
        this.Lights.Add (new StackSingleLightDTO (color, status));
      }
    }
  }

  /// <summary>
  /// DTO for a single light
  /// </summary>
  public class StackSingleLightDTO
  {
    /// <summary>
    /// Color: Red / Yellow / Green
    /// </summary>
    public string Color { get; set; }

    /// <summary>
    /// Status: on / flashing / off
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="color"></param>
    /// <param name="status"></param>
    internal StackSingleLightDTO (StackLightColor color, StackLightStatus status)
    {
      this.Color = color.ToString ();
      this.Status = status.ToString ();
    }
  }
}

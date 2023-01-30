// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// Active or coming event (cycle completion, active stop, coming stop, coming cycle end)
  /// </summary>
  public class EventDTO
  {
    /// <summary>
    /// Message to display above the date/time
    /// 
    /// It must be short. It should be limited to about 15 characters
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Long text with a full description of the event
    /// 
    /// It can be longer than Message and give some additional information on the event
    /// 
    /// It is optional
    /// </summary>
    public string LongText { get; set; }

    /// <summary>
    /// Estimated date/time for coming events
    /// Event start date/time for active events
    /// </summary>
    public string DateTime { get; set; }

    /// <summary>
    /// Event severity
    /// </summary>
    public SeverityDTO Severity { get; set; }

    /// <summary>
    /// Pause the countdown when the machine is not running 
    /// during a machining sequence
    /// </summary>
    public bool PauseWhenMachiningNotRunning { get; set; }

    /// <summary>
    /// Can the event be possibly late ? Meaning the returned duration can be negative ?
    /// </summary>
    public bool PossiblyLate { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="ev">not null</param>
    internal EventDTO (Pulse.Extensions.Web.Event ev)
    {
      Debug.Assert (null != ev);

      this.Message = ev.Message;
      this.LongText = ev.LongText;
      this.DateTime = ConvertDTO.DateTimeUtcToIsoString (ev.DateTime);
      this.Severity = new SeverityDTO (ev.Severity);
      this.PauseWhenMachiningNotRunning = ev.PauseWhenMachiningNotRunning;
      this.PossiblyLate = ev.PossiblyLate;
    }
  }
}

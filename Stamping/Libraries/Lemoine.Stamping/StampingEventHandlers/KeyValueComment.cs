// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Stamping.StampingEventHandlers
{
  /// <summary>
  /// KeyValueComment
  /// </summary>
  public class KeyValueComment : IStampingEventHandler
  {
    readonly ILog log = LogManager.GetLogger (typeof (KeyValueComment).FullName);

    readonly StampingData m_stampingData;

    /// <summary>
    /// Constructor
    /// </summary>
    public KeyValueComment (IStamper stamper, StampingData stampingData)
    {
      this.Stamper = stamper;
      m_stampingData = stampingData;
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public IStampingEventHandler? Next { get; set; }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public IStamper Stamper { get; }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void NotifyNewBlock (bool edit, int level)
    {
      this.Next?.NotifyNewBlock (edit, level);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetComment (string message)
    {
      var split = message.Split ('=', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
      if (2 == split.Length) {
        if (log.IsDebugEnabled) {
          log.Debug ($"SetComment: {split[0]}={split[1]}");
        }
        switch (split[0]) {
        case "[CADNAME]" or "[MODEL]":
          SetString ("CadName", split[1]);
          break;
        case "[STOCK]":
          SetDouble ("Stock", split[1]);
          break;
        case "[DEPTH]":
          SetDouble ("Depth", split[1]);
          break;
        case "[WIDTH]":
          SetDouble ("Width", split[1]);
          break;
        case "[TOLERANCE]":
          SetDouble ("Tolerance", split[1]);
          break;
        case "[RPM]":
          SetDouble ("ProgrammedSpindleSpeed", split[1]);
          break;
        case "[FEEDRATE]":
          SetDouble ("ProgrammedFeedrate", split[1]);
          break;
        case "[UNIT]":
          // TODO: Do something ?
          break;
        case "[STRATEGY]":
          SetString ("Strategy", split[1]);
          break;
        case "[MOLDNAME]" or "[PROJECT]":
          SetString ("ProjectName", split[1]);
          break;
        case "[COMPONENTNAME]" or "[PART]":
          SetString ("ComponentName", split[1]);
          break;
        case "[OPTYPE]":
          // TODO: Do something ?
          break;
        case "[TOOLCODE]":
          SetString ("ToolCode", split[1]);
          break;
        case "[TOOLNAME]":
          SetString ("ToolName", split[1]);
          break;
        case "[TOOLDIA]":
          SetDouble ("ToolDiameter", split[1]);
          break;
        case "[TOOLRAD]":
          SetDouble ("ToolRadius", split[1]);
          break;
        case "[TOOLMINLENGTH]":
          SetDouble ("MinimumToolLength", split[1]);
          break;
        case "[HOURS]":
          if (split[1].Contains (":")) {
            SetTimeSpanString ("SequenceDuration", split[1]);
          }
          break;
        // TODO: to complete: ComponentTypeCode, ...
        default:
          break;
        }
      }
      this.Next?.SetComment (message);
    }

    void SetString (string key, string value)
    {
      m_stampingData.Add (key, value);
    }

    void SetDouble (string key, string value)
    {
      if (double.TryParse (value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double d)) {
        m_stampingData.Add (key, d);
      }
      else {
        log.Error ($"SetDouble: {key}={value} is not a double");
      }
    }

    void SetTimeSpanString (string key, string value)
    {
      if (string.IsNullOrEmpty (value)) {
        log.Error ($"SetTimeSpanString: empty value for key={key}");
        return;
      }

      if (TimeSpan.TryParse (value, out TimeSpan d)) {
        m_stampingData.Add (key, d);
      }
      else {
        log.Error ($"SetTimeSpanString: {key}={value} is not a TimeSpan");
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetData (string key, object v)
    {
      this.Next?.SetData (key, v);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetMachiningTime (TimeSpan duration)
    {
      this.Next?.SetMachiningTime (duration);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetNextToolNumber (string toolNumber)
    {
      this.Next?.SetNextToolNumber (toolNumber);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartCycle ()
    {
      this.Next?.StartCycle ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartSequence (SequenceKind sequenceKind)
    {
      this.Next?.StartSequence (sequenceKind);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StopCycle ()
    {
      this.Next?.StopCycle ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartProgram (bool edit, int level)
    {
      this.Next?.StartProgram (edit, level);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void EndProgram (bool edit, int level, bool endOfFile)
    {
      this.Next?.EndProgram (edit, level, endOfFile);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void ResumeProgram (bool edit, int level)
    {
      this.Next?.ResumeProgram (edit, level);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SuspendProgram (bool optional = false, string details = "")
    {
      this.Next?.SuspendProgram (optional, details);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void TriggerMachining ()
    {
      this.Next?.TriggerMachining ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void TriggerToolChange (string toolNumber = "")
    {
      this.Next?.TriggerToolChange (toolNumber);
    }
  }
}

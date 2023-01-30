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
  public class CamSystemKeywordComment : IStampingEventHandler
  {
    readonly ILog log = LogManager.GetLogger (typeof (CamSystemKeywordComment).FullName);

    readonly StampingData m_stampingData;
    readonly ICamSystemKeywordsGetter m_camSystemKeywordsGetter;

    /// <summary>
    /// Constructor
    /// </summary>
    public CamSystemKeywordComment (IStamper stamper, StampingData stampingData, ICamSystemKeywordsGetter camSystemKeywordsGetter)
    {
      this.Stamper = stamper;
      m_stampingData = stampingData;
      m_camSystemKeywordsGetter = camSystemKeywordsGetter;
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
        if ((3 <= split[0].Length) && split[0].StartsWith ("[") && split[0].EndsWith ("]")) {
          var k = split[0].Substring (1, split[0].Length - 2);
          Debug.Assert (!string.IsNullOrEmpty (k));
          var camSystem = m_stampingData.CamSystem;
          if (k.Equals ("CAMSYSTEM", StringComparison.CurrentCultureIgnoreCase)) {
            if (camSystem is not null) {
              log.Warn ($"SetComment: CamSystem was already set");
            }
            m_stampingData.CamSystem = split[1];
          }
          else if (k.Equals (m_camSystemKeywordsGetter.GetCadName (camSystem), StringComparison.InvariantCultureIgnoreCase)) {
            SetString ("CadName", split[1]);
          }
          else if (k.Equals (m_camSystemKeywordsGetter.GetProjectName (camSystem), StringComparison.InvariantCultureIgnoreCase)) {
            SetString ("ProjectName", split[1]);
          }
          else if (k.Equals (m_camSystemKeywordsGetter.GetComponentName (camSystem), StringComparison.InvariantCultureIgnoreCase)) {
            SetString ("ComponentName", split[1]);
          }
          else if (k.Equals (m_camSystemKeywordsGetter.GetStock (camSystem), StringComparison.InvariantCultureIgnoreCase)) {
            SetDouble ("Stock", split[1]);
          }
          else if (k.Equals (m_camSystemKeywordsGetter.GetDepth (camSystem), StringComparison.InvariantCultureIgnoreCase)) {
            SetDouble ("Depth", split[1]);
          }
          else if (k.Equals (m_camSystemKeywordsGetter.GetWidth (camSystem), StringComparison.InvariantCultureIgnoreCase)) {
            SetDouble ("Width", split[1]);
          }
          else if (k.Equals (m_camSystemKeywordsGetter.GetTolerance (camSystem), StringComparison.InvariantCultureIgnoreCase)) {
            SetDouble ("Tolerance", split[1]);
          }
          else if (k.Equals (m_camSystemKeywordsGetter.GetProgrammedFeedrate (camSystem), StringComparison.InvariantCultureIgnoreCase)) {
            SetDouble ("ProgrammedFeedrate", split[1]);
          }
          else if (k.Equals (m_camSystemKeywordsGetter.GetProgrammedSpindleSpeed (camSystem), StringComparison.InvariantCultureIgnoreCase)) {
            SetDouble ("ProgrammedSpindleSpeed", split[1]);
          }
          else if (k.Equals (m_camSystemKeywordsGetter.GetStrategy (camSystem), StringComparison.InvariantCultureIgnoreCase)) {
            SetString ("Strategy", split[1]);
          }
          else if (k.Equals (m_camSystemKeywordsGetter.GetToolCode (camSystem), StringComparison.InvariantCultureIgnoreCase)) {
            SetString ("ToolCode", split[1]);
          }
          else if (k.Equals (m_camSystemKeywordsGetter.GetToolName (camSystem), StringComparison.InvariantCultureIgnoreCase)) {
            SetString ("ToolName", split[1]);
          }
          else if (k.Equals (m_camSystemKeywordsGetter.GetToolDiameter (camSystem), StringComparison.InvariantCultureIgnoreCase)) {
            SetDouble ("ToolDiameter", split[1]);
          }
          else if (k.Equals (m_camSystemKeywordsGetter.GetToolRadius (camSystem), StringComparison.InvariantCultureIgnoreCase)) {
            SetDouble ("ToolRadius", split[1]);
          }
          else if (k.Equals (m_camSystemKeywordsGetter.GetMinimumToolLength (camSystem), StringComparison.InvariantCultureIgnoreCase)) {
            SetDouble ("MinimumToolLength", split[1]);
          }
          else if (k.Equals (m_camSystemKeywordsGetter.GetOperationDurationHHMMSS (camSystem), StringComparison.InvariantCultureIgnoreCase)) {
            SetDurationHHMMSS ("OperationDuration", split[1]);
          }
          else if (k.Equals (m_camSystemKeywordsGetter.GetSequenceDurationHHMMSS (camSystem), StringComparison.InvariantCultureIgnoreCase)) {
            SetDurationHHMMSS ("SequenceDuration", split[1]);
          }
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

    void SetDurationHHMMSS (string key, string value)
    {
      if (string.IsNullOrEmpty (value)) {
        log.Error ($"SetDurationHHMMSS: empty timeSpan value for key={key}");
        return;
      }

      if (TimeSpan.TryParse (value, out TimeSpan d)) {
        m_stampingData.Add (key, d);
      }
      else {
        log.Error ($"SetDurationHHMMSS: {key}={value} is not a TimeSpan");
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

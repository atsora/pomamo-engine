// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Stamping.StampingEventHandlers
{
  public class ConsecutiveCommentsConfig
  {
    /// <summary>
    /// Regex for the first comment
    /// </summary>
    public string Regex1 { get; set; } = "";

    /// <summary>
    /// Regex for the second comment
    /// </summary>
    public string Regex2 { get; set; } = "";
  }

  /// <summary>
  /// Extract data from regex
  /// </summary>
  public class ConsecutiveComments : IStampingEventHandler
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConsecutiveComments).FullName);

    readonly StampingData m_stampingData;
    readonly Regex m_regex1;
    readonly Regex m_regex2;

    string? m_firstComment = null;
    bool m_newBlockAfterFirstComment = false;

    /// <summary>
    /// Constructor
    /// </summary>
    public ConsecutiveComments (IStamper stamper, StampingData stampingData, ConsecutiveCommentsConfig config)
    {
      this.Stamper = stamper;
      m_stampingData = stampingData;
      m_regex1 = new Regex (config.Regex1, RegexOptions.Compiled);
      m_regex2 = new Regex (config.Regex2, RegexOptions.Compiled);
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
      if (!m_newBlockAfterFirstComment && (m_firstComment is not null)) {
        m_newBlockAfterFirstComment = true;
      }
      else {
        m_newBlockAfterFirstComment = false;
        m_firstComment = null;
      }
      this.Next?.NotifyNewBlock (edit, level);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="message"></param>
    public void SetComment (string message)
    {
      if (m_firstComment is null) {
        if (m_regex1.IsMatch (message)) {
          m_firstComment = message;
          m_newBlockAfterFirstComment = false;
        }
      }
      else { // m_firstComment is not null
        var match2 = m_regex2.Match (message);
        if (match2.Success) {
          if (!ParseComment (m_firstComment, m_regex1) || !ParseComment (message, match2)) {
            log.Fatal ($"SetComment: unexpected, one of the two comments does not match one of the regex: {m_firstComment}/{message} {m_regex1}/{m_regex2}");
          }
        }
        m_firstComment = null;
      }
      this.Next?.SetComment (message);
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

    bool ParseComment (string comment, Regex regex)
    {
      var match = regex.Match (comment);
      return ParseComment (comment, match);
    }

    bool ParseComment (string comment, Match? match)
    {
      if (match?.Success ?? false) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ParseComment: {comment} matches regex");
        }
        foreach (Group group in match.Groups) {
          if (!int.TryParse (group.Name, out _)) {
            if (log.IsInfoEnabled) {
              log.Info ($"ParseComment: add {group.Name}={group.Value}");
            }
            m_stampingData.Add (group.Name, group.Value);
          }
        }
        return true;
      }
      else {
        return false;
      }
    }
  }
}

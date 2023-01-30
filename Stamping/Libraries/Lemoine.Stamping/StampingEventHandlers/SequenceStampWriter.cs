// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Stamping.StampingEventHandlers
{
  /// <summary>
  /// Sequence stamp writer: use the stamping data "SequenceStampId" and the IStampLineCreator
  /// to add a new line with the sequence stamp
  /// </summary>
  public class SequenceStampWriter
    : IStampingEventHandler
  {
    readonly ILog log = LogManager.GetLogger (typeof (SequenceStampWriter).FullName);

    readonly ISequenceStampLineCreator m_stampLineCreator;
    readonly IMilestoneStampLineCreator? m_milestoneStampLineCreator;

    /// <summary>
    /// Constructor
    /// </summary>
    public SequenceStampWriter (IStamper stamper, StampingData stampingData, ISequenceStampLineCreator stampLineCreator, IMilestoneStampLineCreator? milestoneStampLineCreator = null)
    {
      this.Stamper = stamper;
      this.StampingData = stampingData;
      m_stampLineCreator = stampLineCreator;
      m_milestoneStampLineCreator = milestoneStampLineCreator;
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    StampingData StampingData { get; }

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
    /// <param name="comment"></param>
    public void SetComment (string comment)
    {
      this.Next?.SetComment (comment);
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
    /// <param name="duration"></param>
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
      if (this.StampingData.TryGet ("SequenceStampId", out int sequenceStampId)) {
        if (m_milestoneStampLineCreator is not null) {
          var milestoneLine = m_milestoneStampLineCreator.CreateResetMilestoneLine (sequenceStampId);
          if (!string.IsNullOrEmpty (milestoneLine)) {
            this.Stamper.AddLine (milestoneLine);
          }
        }
        var line = m_stampLineCreator.CreateSequenceStampLine (sequenceStampId);
        if (string.IsNullOrEmpty (line)) {
          log.Error ($"StartSequence: line is empty");
        }
        else {
          this.Stamper.AddLine (line);
        }
      }
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
      if (edit && (0 == level)) {
        if (endOfFile) {
          log.Warn ($"EndProgram: end of file reached, do not write the stamp");
        }
        else { // !endOfFile
          if (m_milestoneStampLineCreator is not null) {
            var milestoneLine = m_milestoneStampLineCreator.CreateResetMilestoneLine ();
            if (!string.IsNullOrEmpty (milestoneLine)) {
              this.Stamper.AddLine (milestoneLine);
            }
          }
          var line = m_stampLineCreator.CreateSequenceStampLine (0);
          if (string.IsNullOrEmpty (line)) {
            log.Error ($"StopProgram: line is empty");
          }
          else {
            this.Stamper.AddLine (line);
          }
        }
      }
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

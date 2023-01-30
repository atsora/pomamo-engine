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
  /// DataSetter
  /// </summary>
  public class DataSetter
    : IStampingEventHandler
  {
    readonly ILog log = LogManager.GetLogger (typeof (DataSetter).FullName);

    readonly StampingData m_stampingData;

    /// <summary>
    /// Constructor
    /// </summary>
    public DataSetter (IStamper stamper, StampingData stampingData)
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
    public void SetComment (string comment)
    {
      this.Next?.SetComment (comment);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    public void SetData (string key, object v)
    {
      m_stampingData.Add (key, v);
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
    /// <param name="toolNumber"></param>
    public void SetNextToolNumber (string toolNumber)
    {
      m_stampingData.Add ("NextToolNumber", toolNumber);
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
    /// <param name="toolNumber"></param>
    public void TriggerToolChange (string toolNumber = "")
    {
      if (!string.IsNullOrEmpty (toolNumber)) {
        m_stampingData.ToolNumber = toolNumber;
      }
      else {
        var nextToolNumber = m_stampingData.GetString ("NextToolNumber");
        if (string.IsNullOrEmpty (nextToolNumber)) {
          log.Error ($"TriggerToolChange: next tool number is not set");
          m_stampingData.ToolNumber = null;
        }
        else {
          m_stampingData.ToolNumber = nextToolNumber;
          this.SetNextToolNumber ("");
        }
      }
      this.Next?.TriggerToolChange (toolNumber);
    }

  }
}

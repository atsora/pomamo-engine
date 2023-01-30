// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Conversion;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Stamping.StampingEventHandlers
{
  /// <summary>
  /// Set the stamping values for a sequence
  /// </summary>
  public class StampingValues
    : IStampingEventHandler
  {
    readonly ILog log = LogManager.GetLogger (typeof (DataSetter).FullName);

    readonly StampingData m_stampingData;
    readonly IAutoConverter m_autoConverter;

    /// <summary>
    /// Constructor
    /// </summary>
    public StampingValues (IStamper stamper, StampingData stampingData, IAutoConverter autoConverter)
    {
      this.Stamper = stamper;
      m_stampingData = stampingData;
      m_autoConverter = autoConverter;
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
      var sequence = m_stampingData.Sequence;
      if (sequence is null) {
        log.Error ("StartSequence: no sequence was previously set");
      }
      else { // sequence is set and not null
        foreach (var keyValue in m_stampingData) {
          TrySetStampingValue (sequence, keyValue.Key, keyValue.Value);
        }
      }
      this.Next?.StartSequence (sequenceKind);
    }

    bool TrySetStampingValue (ISequence sequence, string key, object value)
    {
      var field = ModelDAOHelper.DAOFactory.FieldDAO.FindByCode (key);
      if (field is null) {
        if (log.IsDebugEnabled) {
          log.Debug ($"SetSetStampingValue: field with code={key} does not exist");
        }
        return false;
      }
      else { // field is not null
        try {
          var stampingValue = ModelDAOHelper.ModelFactory.CreateStampingValue (sequence, field);
          switch (field.Type) {
          case FieldType.String:
            stampingValue.String = value.ToString ();
            break;
          case FieldType.Int32:
            stampingValue.Int = m_autoConverter.ConvertAuto<int> (value);
            break;
          case FieldType.Double:
            stampingValue.Double = m_autoConverter.ConvertAuto<double> (value);
            break;
          case FieldType.Boolean:
            stampingValue.Int = m_autoConverter.ConvertAuto<bool> (value) ? 1 : 0;
            break;
          }
          ModelDAOHelper.DAOFactory.StampingValueDAO.MakePersistent (stampingValue);
          return true;
        }
        catch (Exception ex) {
          log.Error ($"TrySetStampingValue: error for {key}={value}", ex);
          return false;
        }
      }
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
      this.Next?.TriggerToolChange (toolNumber);
    }

  }
}

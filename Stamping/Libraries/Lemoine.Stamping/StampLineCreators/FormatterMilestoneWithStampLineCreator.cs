// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Stamping.StampLineCreators
{
  /// <summary>
  /// Create a line with a milestone as the factional part of the stamp
  /// 
  /// The milestone in minutes is the number of 0.0001
  /// </summary>
  public class FormatterMilestoneWithStampLineCreator : IMilestoneStampLineCreator
  {
    readonly ILog log = LogManager.GetLogger (typeof (FormatterMilestoneWithStampLineCreator).FullName);

    readonly IStampVariablesGetter m_stampVariablesGetter;
    readonly ILineFormatter m_lineFormatter;

    /// <summary>
    /// Multiplicator for the milestone in minutes
    /// 
    /// Default: 0.0001
    /// </summary>
    public double MilestoneMultiplicator { get; set; } = 0.0001;

    /// <summary>
    /// Maximum number of fractional digits to write into the program
    /// 
    /// Default: 5
    /// </summary>
    public int FractionalDigits { get; set; } = 5;

    /// <summary>
    /// Constructor
    /// </summary>
    public FormatterMilestoneWithStampLineCreator (IStampVariablesGetter stampVariablesGetter, ILineFormatter lineFormatter)
    {
      m_stampVariablesGetter = stampVariablesGetter;
      m_lineFormatter = lineFormatter;
    }

    /// <summary>
    /// <see cref="IMilestoneStampLineCreator"/>
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <param name="sequenceStamp"></param>
    /// <returns></returns>
    public string? CreateMilestoneStampLine (TimeSpan timeSpan, double? sequenceStamp = null)
    {
      if (sequenceStamp.HasValue) {
        var v = sequenceStamp.Value + Math.Round (this.MilestoneMultiplicator * timeSpan.TotalMinutes, this.FractionalDigits);
        return m_lineFormatter.CreateLineWithVariableCheck (m_stampVariablesGetter.SequenceStampVariable, v, "stampWithMilestone");
      }
      else { // !sequenceStamp.HasValue
        if (log.IsWarnEnabled) {
          log.Warn ($"CreateMilestoneStampLine: no active sequence stamp, do nothing for milestone with timeSpan={timeSpan}");
        }
        return null;
      }
    }

    /// <summary>
    /// <see cref="IMilestoneStampLineCreator"/>
    /// </summary>
    /// <returns></returns>
    public string? CreateResetMilestoneLine (double? sequenceStamp)
    {
      return null;
    }
  }
}

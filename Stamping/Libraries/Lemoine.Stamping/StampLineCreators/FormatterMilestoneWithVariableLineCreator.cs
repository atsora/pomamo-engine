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
  /// Create a line with a milestone using its own variable if defined
  /// 
  /// The milestone in minutes with a possible decimal part is set in the variable
  /// </summary>
  public class FormatterMilestoneWithVariableLineCreator : IMilestoneStampLineCreator
  {
    readonly ILog log = LogManager.GetLogger (typeof (FormatterMilestoneWithStampLineCreator).FullName);

    readonly IStampVariablesGetter m_stampVariablesGetter;
    readonly ILineFormatter m_lineFormatter;

    /// <summary>
    /// Maximum number of fractional digits to write into the program
    /// 
    /// Default: 5
    /// </summary>
    public int FractionalDigits { get; set; } = 5;


    /// <summary>
    /// Constructor
    /// </summary>
    public FormatterMilestoneWithVariableLineCreator (IStampVariablesGetter stampVariablesGetter, ILineFormatter lineFormatter)
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
      var milestoneVariable = m_stampVariablesGetter.MilestoneStampVariable;
      if (!string.IsNullOrEmpty (milestoneVariable)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"CreateMilestoneStampLine: milestone variable={milestoneVariable} timeSpan={timeSpan}");
        }
        return m_lineFormatter.CreateLineWithVariableCheck (milestoneVariable, Math.Round (timeSpan.TotalMinutes, this.FractionalDigits));
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"CreateMilestoneStampLine: no milestone variable, do nothing");
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
      var milestoneVariable = m_stampVariablesGetter.MilestoneStampVariable;
      if (!string.IsNullOrEmpty (milestoneVariable)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"CreateResetMilestoneLine: milestone variable={milestoneVariable}");
        }
        return m_lineFormatter.CreateLineWithVariableCheck (milestoneVariable, 0);
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"CreateResetMilestoneLine: no milestone variable, do nothing");
        }
        return null;
      }
    }
  }
}

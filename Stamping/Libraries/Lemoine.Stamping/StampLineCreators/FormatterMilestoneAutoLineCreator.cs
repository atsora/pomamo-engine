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
  /// If the milestone variable is set, use <see cref="FormatterMilestoneWithVariableLineCreator"/> else use <see cref="FormatterMilestoneWithStampLineCreator"/>
  /// </summary>
  public class FormatterMilestoneAutoLineCreator : IMilestoneStampLineCreator
  {
    readonly ILog log = LogManager.GetLogger (typeof (FormatterMilestoneAutoLineCreator).FullName);

    readonly IMilestoneStampLineCreator m_milestoneStampLineCreator;

    /// <summary>
    /// Maximum number of fractional digits to write into the program
    /// </summary>
    public int FractionalDigits
    {
      get => m_milestoneStampLineCreator.FractionalDigits;
      set { m_milestoneStampLineCreator.FractionalDigits = value; }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public FormatterMilestoneAutoLineCreator (IStampVariablesGetter stampVariablesGetter, ILineFormatter lineFormatter)
    {
      var milestoneVariable = stampVariablesGetter.MilestoneStampVariable;
      if (!string.IsNullOrEmpty (milestoneVariable)) { // The milestone variable is defined
        var milestoneStampLineCreator = new FormatterMilestoneWithVariableLineCreator (stampVariablesGetter, lineFormatter);
        m_milestoneStampLineCreator = milestoneStampLineCreator;
      }
      else { // No milestone variable
        var milestoneStampLineCreator = new FormatterMilestoneWithStampLineCreator (stampVariablesGetter, lineFormatter);
        m_milestoneStampLineCreator = milestoneStampLineCreator;
      }
    }

    /// <summary>
    /// <see cref="IMilestoneStampLineCreator"/>
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <param name="sequenceStamp"></param>
    /// <returns></returns>
    public string? CreateMilestoneStampLine (TimeSpan timeSpan, double? sequenceStamp = null)
    {
      return m_milestoneStampLineCreator.CreateMilestoneStampLine (timeSpan, sequenceStamp);
    }

    /// <summary>
    /// <see cref="IMilestoneStampLineCreator"/>
    /// </summary>
    /// <returns></returns>
    public string? CreateResetMilestoneLine (double? sequenceStamp)
    {
      return m_milestoneStampLineCreator.CreateResetMilestoneLine (sequenceStamp);
    }
  }
}

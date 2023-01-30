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
  /// Line creator implementations using a <see cref="ILineFormatter"/>
  /// </summary>
  public class FormatterStampLineCreators: ISequenceStampLineCreator, IStartCycleStampLineCreator, IStopCycleStampLineCreator
  {
    readonly ILog log = LogManager.GetLogger (typeof (FormatterStampLineCreators).FullName);

    readonly IStampVariablesGetter m_stampVariablesGetter;
    readonly ILineFormatter m_lineFormatter;

    /// <summary>
    /// Constructor
    /// </summary>
    public FormatterStampLineCreators (IStampVariablesGetter stampVariablesGetter, ILineFormatter lineFormatter)
    {
      m_stampVariablesGetter = stampVariablesGetter;
      m_lineFormatter = lineFormatter;
    }

    /// <summary>
    /// <see cref="ISequenceStampLineCreator"/>
    /// </summary>
    /// <param name="stamp"></param>
    /// <returns></returns>
    public string CreateSequenceStampLine (double stamp) => m_lineFormatter.CreateLineWithVariableCheck (m_stampVariablesGetter.SequenceStampVariable, stamp, "sequence");

    /// <summary>
    /// <see cref="IStartCycleStampLineCreator"/>
    /// </summary>
    /// <param name="stamp"></param>
    /// <returns></returns>
    public string CreateStartCycleStampLine (double stamp) => m_lineFormatter.CreateLineWithVariableCheck (m_stampVariablesGetter.StartCycleStampVariable, stamp, "startCycle");

    /// <summary>
    /// <see cref="IStopCycleStampLineCreator"/>
    /// </summary>
    /// <param name="stamp"></param>
    /// <returns></returns>
    public string CreateStopCycleStampLine (double stamp) => m_lineFormatter.CreateLineWithVariableCheck (m_stampVariablesGetter.StopCycleStampVariable, stamp, "stopCycle");
  }
}

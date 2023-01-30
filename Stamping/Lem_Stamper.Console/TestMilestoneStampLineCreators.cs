// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Stamping;

namespace Lem_Stamper.Console
{
  /// <summary>
  /// SequenceStampLineCreator for tests
  /// </summary>
  public class TestMilestoneStampLineCreators
    : IMilestoneStampLineCreator
  {
    readonly ILog log = LogManager.GetLogger (typeof (TestMilestoneStampLineCreators).FullName);

    /// <summary>
    /// <see cref="IMilestoneStampLineCreator"/>
    /// </summary>
    public int FractionalDigits { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public TestMilestoneStampLineCreators ()
    {
    }

    /// <summary>
    /// <see cref="IMilestoneStampLineCreator"/>
    /// </summary>
    /// <param name="sequenceTime"></param>
    /// <param name="stamp"></param>
    /// <returns></returns>
    public string? CreateMilestoneStampLine (TimeSpan sequenceTime, double? stamp)
    {
      var d = Math.Round (sequenceTime.TotalMinutes, this.FractionalDigits);
      return $"(Time={d})";
    }

    /// <summary>
    /// <see cref="IMilestoneStampLineCreator"/>
    /// </summary>
    /// <param name="sequenceStamp"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public string? CreateResetMilestoneLine (double? sequenceStamp = null)
    {
      return "(Time=0)";
    }
  }
}

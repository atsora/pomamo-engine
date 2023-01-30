// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Stamping;

namespace Lem_TestHeidenhainParser.Console
{
  /// <summary>
  /// SequenceStampLineCreator for tests
  /// </summary>
  public class TestMilestoneStampLineCreator
    : IMilestoneStampLineCreator
  {
    readonly ILog log = LogManager.GetLogger (typeof (TestMilestoneStampLineCreator).FullName);

    public int FractionalDigits { get; set; } = 5;

    /// <summary>
    /// Constructor
    /// </summary>
    public TestMilestoneStampLineCreator ()
    {
    }

    public string CreateMilestoneStampLine (TimeSpan sequenceTime, double? stamp)
    {
      var d = Math.Round (sequenceTime.TotalMinutes, this.FractionalDigits);
      return $"(Time={d})";
    }

    public string CreateResetMilestoneLine (double? sequenceStamp = null)
    {
      return null;
    }
  }
}

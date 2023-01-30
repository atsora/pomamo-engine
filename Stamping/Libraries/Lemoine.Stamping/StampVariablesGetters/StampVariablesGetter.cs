// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Stamping.StampVariablesGetters
{
  /// <summary>
  /// StampVariablesGetter
  /// </summary>
  public class StampVariablesGetter
    : IStampVariablesGetter
  {
    readonly ILog log = LogManager.GetLogger (typeof (StampVariablesGetter).FullName);

    /// <summary>
    /// Sequence stamp variable
    /// </summary>
    public string SequenceStampVariable { get; set; } = "";

    /// <summary>
    /// Start cycle stamp variable
    /// </summary>
    public string StartCycleStampVariable { get; set; } = "";

    /// <summary>
    /// Stop cycle stamp variable
    /// </summary>
    public string StopCycleStampVariable { get; set; } = "";

    /// <summary>
    /// Milestone stamp variable
    /// </summary>
    public string MilestoneStampVariable { get; set; } = "";

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public StampVariablesGetter ()
    {
    }
    #endregion // Constructors

  }
}

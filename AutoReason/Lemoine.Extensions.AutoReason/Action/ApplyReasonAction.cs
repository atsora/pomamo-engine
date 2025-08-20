// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.AutoReason.ActionableAutoReasonExtension;
using Lemoine.Model;

namespace Lemoine.Extensions.AutoReason.Action
{
  /// <summary>
  /// ApplyReasonAction
  /// </summary>
  public class ApplyReasonAction : IReasonAction
  {
    readonly ILog log = LogManager.GetLogger (typeof (ApplyReasonAction).FullName);

    readonly int m_commitNumber = 0;
    readonly IApplyReasonAutoReason m_autoReason;
    readonly IMachine m_machine;
    readonly IReason m_reason;
    readonly double m_reasonScore;
    readonly UtcDateTimeRange m_range;
    readonly string m_dynamic;
    readonly string m_details;
    readonly bool m_overwriteRequired = false;

    /// <summary>
    /// Constructor with the default machine, reason and reason score
    /// </summary>
    /// <param name="autoReason"></param>
    /// <param name="range"></param>
    /// <param name="dynamic"></param>
    /// <param name="details"></param>
    /// <param name="overwriteRequired"></param>
    public ApplyReasonAction (IApplyReasonAutoReason
      autoReason, UtcDateTimeRange range, string dynamic = "", string details = "", bool overwriteRequired = false, int commitNumber = 0)
      : this (autoReason, autoReason.Machine, autoReason.Reason, autoReason.ReasonScore, range, dynamic, details, overwriteRequired, commitNumber: commitNumber)
    {
    }

    /// <summary>
    /// Constructor with the default machine
    /// </summary>
    /// <param name="autoReason"></param>
    /// <param name="reason">not null</param>
    /// <param name="reasonScore"></param>
    /// <param name="range"></param>
    /// <param name="dynamic"></param>
    /// <param name="details"></param>
    /// <param name="overwriteRequired"></param>
    public ApplyReasonAction (IApplyReasonAutoReason
      autoReason, IReason reason, double reasonScore, UtcDateTimeRange range, string dynamic = "", string details = "", bool overwriteRequired = false, int commitNumber = 0)
      : this (autoReason, autoReason.Machine, reason, reasonScore, range, dynamic, details, overwriteRequired, commitNumber: commitNumber)
    {
    }

    /// <summary>
    /// Constructor with an alternate machine
    /// </summary>
    /// <param name="autoReason"></param>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="dynamic"></param>
    /// <param name="details"></param>
    /// <param name="overwriteRequired"></param>
    public ApplyReasonAction (IApplyReasonAutoReason
      autoReason, IMachine machine, UtcDateTimeRange range, string dynamic = "", string details = "", bool overwriteRequired = false, int commitNumber = 0)
      : this (autoReason, machine, autoReason.Reason, autoReason.ReasonScore, range, dynamic, details, overwriteRequired, commitNumber: commitNumber)
    {
    }

    /// <summary>
    /// Constructor with an alternate machine
    /// </summary>
    /// <param name="autoReason"></param>
    /// <param name="machine">not null</param>
    /// <param name="reason">not null</param>
    /// <param name="reasonScore"></param>
    /// <param name="range"></param>
    /// <param name="dynamic"></param>
    /// <param name="details"></param>
    /// <param name="overwriteRequired"></param>
    public ApplyReasonAction (IApplyReasonAutoReason
      autoReason, IMachine machine, IReason reason, double reasonScore, UtcDateTimeRange range, string dynamic = "", string details = "", bool overwriteRequired = false, int commitNumber = 0)
    {
      Debug.Assert (null != autoReason);
      Debug.Assert (null != machine);

      m_commitNumber = commitNumber;
      m_autoReason = autoReason;
      m_machine = machine;
      m_reason = reason;
      m_reasonScore = reasonScore;
      m_range = range;
      m_dynamic = dynamic;
      m_details = details;
      m_overwriteRequired = overwriteRequired;
    }

    /// <summary>
    /// <see cref="IAutoReasonAction.Name"/>
    /// </summary>
    public string Name => "ApplyReason";

    /// <summary>
    /// <see cref="IAutoReasonAction.CommitNumber"/>
    /// </summary>
    public int CommitNumber => m_commitNumber;

    /// <summary>
    /// 
    /// </summary>
    public void Run ()
    {
      m_autoReason.ApplyReason (m_machine, m_reason, m_reasonScore, m_range, m_dynamic, m_details, m_overwriteRequired);
    }
  }
}

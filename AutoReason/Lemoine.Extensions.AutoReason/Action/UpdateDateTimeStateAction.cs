// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.AutoReason.ActionableAutoReasonExtension;

namespace Lemoine.Extensions.AutoReason.Action
{
  /// <summary>
  /// Action to update the date/time state
  /// </summary>
  public class UpdateDateTimeStateAction: IStateAction
  {
    readonly ILog log = LogManager.GetLogger (typeof (UpdateDateTimeStateAction).FullName);

    readonly int m_commitNumber = 0;
    readonly IDateTimeStateAutoReason m_autoReason;
    readonly DateTime m_previousDateTime;
    readonly DateTime m_dateTime;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="autoReason">not null</param>
    /// <param name="dateTime"></param>
    public UpdateDateTimeStateAction (IDateTimeStateAutoReason autoReason, DateTime dateTime, int commitNumber = 0)
    {
      Debug.Assert (null != autoReason);

      m_commitNumber = commitNumber;
      m_autoReason = autoReason;
      m_previousDateTime = autoReason.DateTime;
      m_dateTime = dateTime;
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public string Name => "UpdateDateTime";

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public int CommitNumber => m_commitNumber;

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public void Run ()
    {
      m_autoReason.UpdateDateTime (m_dateTime);
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public void Reset ()
    {
      m_autoReason.ResetDateTime (m_previousDateTime);
    }

  }
}

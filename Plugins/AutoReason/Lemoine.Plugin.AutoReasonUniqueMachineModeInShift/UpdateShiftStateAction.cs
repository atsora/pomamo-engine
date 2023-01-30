// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Core.Log;
using Lemoine.Extensions.AutoReason;

namespace Lemoine.Plugin.AutoReasonUniqueMachineModeInShift
{
  /// <summary>
  /// UpdateShiftStateAction
  /// </summary>
  internal class UpdateShiftStateAction : IStateAction
  {
    readonly ILog log = LogManager.GetLogger (typeof (UpdateShiftStateAction).FullName);

    readonly AutoReasonExtension m_autoReason;
    readonly int m_previousId;
    readonly int m_id;
    readonly DateTime? m_previousDatetime;
    readonly DateTime? m_dateTime;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="autoReason">not null</param>
    /// <param name="dateTime"></param>
    public UpdateShiftStateAction (AutoReasonExtension autoReason, int previousShiftId, DateTime? startOfShift)
    {
      Debug.Assert (null != autoReason);

      m_autoReason = autoReason;
      m_previousId = autoReason.GetPreviousShiftId ();
      m_id = previousShiftId;
      m_previousDatetime = autoReason.GetStartOfShift ();
      m_dateTime = startOfShift;
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public string Name
    {
      get { return "UpdatePreviousShift"; }
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public void Run ()
    {
      m_autoReason.UpdatePreviousShiftId (m_id);
      m_autoReason.UpdateStartOfShift (m_dateTime);
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public void Reset ()
    {
      m_autoReason.ResetPreviousShiftId (m_previousId);
      m_autoReason.ResetStartOfShift (m_previousDatetime);
    }
  }
}

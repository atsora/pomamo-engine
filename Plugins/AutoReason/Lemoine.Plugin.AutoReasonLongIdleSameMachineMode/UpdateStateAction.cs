// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Core.Log;
using Lemoine.Extensions.AutoReason;

namespace Lemoine.Plugin.AutoReasonLongIdleSameMachineMode
{
  /// <summary>
  /// UpdateStateAction
  /// </summary>
  internal class UpdateStateAction : IStateAction
  {
    readonly ILog log = LogManager.GetLogger (typeof (UpdateStateAction).FullName);

    readonly AutoReasonExtension m_autoReason;
    readonly int m_oldPreviousId;
    readonly TimeSpan m_oldIdleDuration;
    readonly int m_newPreviousId;
    readonly TimeSpan m_newIdleDuration;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="autoReason">not null</param>
    /// <param name="dateTime"></param>
    public UpdateStateAction (AutoReasonExtension autoReason, int previousOperationId, TimeSpan idleDuration)
    {
      Debug.Assert (null != autoReason);
      m_autoReason = autoReason;

      // Old state of previous id and idle duration
      m_oldPreviousId = autoReason.GetPreviousMachineModeId ();
      m_oldIdleDuration = autoReason.GetIdleDuration ();

      // New state
      m_newPreviousId = previousOperationId;
      m_newIdleDuration = idleDuration;
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public string Name
    {
      get { return "UpdateState"; }
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public void Run ()
    {
      m_autoReason.UpdateState (m_newPreviousId, m_newIdleDuration);
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public void Reset ()
    {
      m_autoReason.ResetPreviousMachineModeId (m_oldPreviousId);
      m_autoReason.ResetIdleDuration (m_oldIdleDuration);
    }

  }
}

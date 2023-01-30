// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Core.Log;
using Lemoine.Extensions.AutoReason;

namespace Lemoine.Plugin.AutoReasonShiftEnd
{
  /// <summary>
  /// UpdatePreviousOperationIdAction
  /// </summary>
  internal class UpdatePreviousShiftIdAction : IStateAction
  {
    readonly ILog log = LogManager.GetLogger (typeof (UpdatePreviousShiftIdAction).FullName);

    readonly AutoReasonExtension m_autoReason;
    readonly int m_previousId;
    readonly int m_id;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="autoReason">not null</param>
    /// <param name="dateTime"></param>
    public UpdatePreviousShiftIdAction (AutoReasonExtension autoReason, int previousShiftId)
    {
      Debug.Assert (null != autoReason);

      m_autoReason = autoReason;
      m_previousId = autoReason.GetPreviousShiftId ();
      m_id = previousShiftId;
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public string Name
    {
      get { return "UpdatePreviousShiftId"; }
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public void Run ()
    {
      m_autoReason.UpdatePreviousShiftId (m_id);
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public void Reset ()
    {
      m_autoReason.ResetPreviousShiftId (m_previousId);
    }

  }
}

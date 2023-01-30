// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.AutoReason;

namespace Lemoine.Plugin.AutoReasonBetweenPrograms

{
  /// <summary>
  /// UpdatePreviousOperationIdAction
  /// </summary>
  internal class UpdatePreviousSlotAction : IStateAction
  {
    readonly ILog log = LogManager.GetLogger (typeof (UpdatePreviousSlotAction).FullName);

    readonly AutoReasonExtension m_autoReason;
    readonly int m_previousComponentId;
    readonly int m_componentId;
    readonly DateTime? m_previousEnd;
    readonly DateTime? m_end;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="autoReason">not null</param>
    /// <param name="dateTime"></param>
    public UpdatePreviousSlotAction (AutoReasonExtension autoReason, int componentId, DateTime end)
    {
      Debug.Assert (null != autoReason);

      m_autoReason = autoReason;
      autoReason.GetPreviousSlot (out m_previousComponentId, out m_previousEnd);
      m_componentId = componentId;
      m_end = end;
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public string Name
    {
      get { return "UpdatePreviousSlot"; }
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public void Run ()
    {
      m_autoReason.UpdatePreviousSlot (m_componentId, m_end);
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public void Reset ()
    {
      m_autoReason.ResetPreviousSlot (m_previousComponentId, m_previousEnd);
    }

  }
}

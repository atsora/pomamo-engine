// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Core.Log;
using Lemoine.Extensions.AutoReason;

namespace Lemoine.Plugin.AutoReasonBetweenComponents
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
    readonly string m_previousComponentDetails;
    readonly string m_componentDetails;
    readonly DateTime? m_previousEnd;
    readonly DateTime? m_end;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="autoReason"></param>
    /// <param name="componentId"></param>
    /// <param name="componentDetails"></param>
    /// <param name="end"></param>
    public UpdatePreviousSlotAction (AutoReasonExtension autoReason, int componentId, string componentDetails, DateTime end)
    {
      Debug.Assert (null != autoReason);

      m_autoReason = autoReason;
      autoReason.GetPreviousSlot (out m_previousComponentId, out m_previousComponentDetails, out m_previousEnd);
      m_componentId = componentId;
      m_componentDetails = componentDetails;
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
      m_autoReason.UpdatePreviousSlot (m_componentId, m_componentDetails, m_end);
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public void Reset ()
    {
      m_autoReason.ResetPreviousSlot (m_previousComponentId, m_previousComponentDetails, m_previousEnd);
    }
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.AutoReason;

namespace Lemoine.Plugin.AutoReasonToolChange
{
  /// <summary>
  /// UpdatePreviousOperationIdAction
  /// </summary>
  internal class UpdateLastToolChangedEventIdAction : IStateAction
  {
    readonly ILog log = LogManager.GetLogger (typeof (UpdateLastToolChangedEventIdAction).FullName);

    readonly AutoReasonExtension m_autoReason;
    readonly int? m_previousId;
    readonly int m_id;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="autoReason">not null</param>
    /// <param name="dateTime"></param>
    public UpdateLastToolChangedEventIdAction (AutoReasonExtension autoReason, int lastToolChangedEventId)
    {
      Debug.Assert (null != autoReason);

      m_autoReason = autoReason;
      m_previousId = autoReason.GetLastToolChangedEventId ();
      m_id = lastToolChangedEventId;
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public string Name
    {
      get { return "UpdateLastToolChangedEventId"; }
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public void Run ()
    {
      m_autoReason.UpdateLastToolChangedEventId (m_id);
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public void Reset ()
    {
      m_autoReason.ResetLastToolChangedEventId (m_previousId);
    }

  }
}

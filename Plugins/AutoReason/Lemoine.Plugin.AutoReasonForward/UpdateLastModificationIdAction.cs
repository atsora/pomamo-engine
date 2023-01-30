// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.AutoReason;

namespace Lemoine.Plugin.AutoReasonForward
{
  /// <summary>
  /// UpdateLastModificationIdAction
  /// </summary>
  internal class UpdateLastModificationIdAction : IStateAction
  {
    readonly ILog log = LogManager.GetLogger (typeof (UpdateLastModificationIdAction).FullName);

    readonly AutoReasonExtension m_autoReason;
    readonly long m_previousId;
    readonly long m_id;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="autoReason">not null</param>
    /// <param name="lastModificationId"></param>
    public UpdateLastModificationIdAction (AutoReasonExtension autoReason, long lastModificationId)
    {
      Debug.Assert (null != autoReason);

      m_autoReason = autoReason;
      m_previousId = autoReason.GetLastModificationId ();
      m_id = lastModificationId;
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public string Name
    {
      get { return "UpdateLastModificationId"; }
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public void Run ()
    {
      m_autoReason.UpdateLastModificationId (m_id);
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public void Reset ()
    {
      m_autoReason.ResetLastModificationId (m_previousId);
    }

  }
}

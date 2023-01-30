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
  /// Action to update the date/time state
  /// </summary>
  public class UpdateMachineModuleDateTimeStateAction : IStateAction
  {
    readonly ILog log = LogManager.GetLogger (typeof (UpdateMachineModuleDateTimeStateAction).FullName);

    readonly IMachineModule m_machineModule;
    readonly IMachineModuleDateTimeStateAutoReason m_autoReason;
    readonly DateTime m_previousDateTime;
    readonly DateTime m_dateTime;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="autoReason">not null</param>
    /// <param name="machineModule">not null</param>
    /// <param name="dateTime"></param>
    public UpdateMachineModuleDateTimeStateAction (IMachineModuleDateTimeStateAutoReason autoReason, IMachineModule machineModule, DateTime dateTime)
    {
      Debug.Assert (null != autoReason);

      m_autoReason = autoReason;
      m_machineModule = machineModule;
      m_previousDateTime = autoReason.GetDateTime (machineModule);
      m_dateTime = dateTime;
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public string Name
    {
      get { return "UpdateDateTime"; }
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public void Run ()
    {
      m_autoReason.UpdateDateTime (m_machineModule, m_dateTime);
    }

    /// <summary>
    /// <see cref="IStateAction"/>
    /// </summary>
    public void Reset ()
    {
      m_autoReason.ResetDateTime (m_machineModule, m_previousDateTime);
    }

  }
}

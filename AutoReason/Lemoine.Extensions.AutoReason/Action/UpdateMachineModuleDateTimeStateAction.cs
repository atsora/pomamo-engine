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
using Lemoine.Model;

namespace Lemoine.Extensions.AutoReason.Action
{
  /// <summary>
  /// Action to update the date/time state
  /// </summary>
  public class UpdateMachineModuleDateTimeStateAction : IStateAction
  {
    readonly ILog log = LogManager.GetLogger (typeof (UpdateMachineModuleDateTimeStateAction).FullName);

    readonly int m_commitNumber = 0;
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
    public UpdateMachineModuleDateTimeStateAction (IMachineModuleDateTimeStateAutoReason autoReason, IMachineModule machineModule, DateTime dateTime, int commitNumber = 0)
    {
      Debug.Assert (null != autoReason);

      m_autoReason = autoReason;
      m_machineModule = machineModule;
      m_previousDateTime = autoReason.GetDateTime (machineModule);
      m_dateTime = dateTime;
      m_commitNumber = commitNumber;
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

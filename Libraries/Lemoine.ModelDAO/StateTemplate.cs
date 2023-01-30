// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Default MachineStateTemplate Ids
  /// </summary>
  public enum StateTemplate
  {
    /// <summary>
    /// Machine ON with operator (attended)
    /// </summary>
    Attended = 1,
    /// <summary>
    /// Machine ON without operator
    /// </summary>
    Unattended = 2,
    /// <summary>
    /// Machine ON with operator (on-site)
    /// </summary>
    OnSite = 3,
    /// <summary>
    /// Machine ON with on call operator (off-site)
    /// </summary>
    OnCall = 4,
    /// <summary>
    /// Machine OFF
    /// </summary>
    Off = 5,
    /// <summary>
    /// Set-up
    /// </summary>
    SetUp = 7,
    /// <summary>
    /// Quality check
    /// </summary>
    QualityCheck = 8,
    /// <summary>
    /// Production
    /// </summary>
    Production = 9,
    /// <summary>
    /// Maintenance
    /// </summary>
    Maintenance = 10
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration;
using Lemoine.Model;
using System;

using System.Collections.Generic;

namespace Pulse.Extensions.Configuration
{
  /// <summary>
  /// Interface for configurations with a machine filter
  /// </summary>
  public interface IConfigurationWithMachineFilter : IConfiguration
  {
    /// <summary>
    /// Machine filter Id
    /// </summary>
    int MachineFilterId { get; set; }
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Interfaces
{
  /// <summary>
  /// 
  /// </summary>
  public interface IPluginInstance
  {
    /// <summary>
    /// Instance ID
    /// </summary>
    int InstanceId { get; }

    /// <summary>
    /// Instance name
    /// </summary>
    string InstanceName { get; }

    /// <summary>
    /// Is the plugin instance active ?
    /// </summary>
    bool InstanceActive { get; }

    /// <summary>
    /// Instance parameters
    /// </summary>
    string InstanceParameters { get; }

    /// <summary>
    /// Package identifying name
    /// </summary>
    string PackageIdentifyingName { get; }
  }
}

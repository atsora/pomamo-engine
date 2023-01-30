// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Pulse.Extensions.Database
{
  /// <summary>
  /// Base interface for all extensions that are initialized first with a machine in parameter
  /// </summary>
  public interface IInitializedByMachineExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialize the extension
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns>false: the extension is not active</returns>
    bool Initialize (IMachine machine);
  }
}

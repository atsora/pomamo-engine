// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Lemoine.Extensions.Interfaces
{
  /// <summary>
  /// Interface to implement for classes that return list of possible plugin directories
  /// </summary>
  public interface IPluginDirectories
  {
    /// <summary>
    /// Get the plugin directories
    /// </summary>
    /// <param name="pluginUserDirectoryActive"></param>
    /// <param name="pluginNames">nullable</param>
    /// <param name="checkedThread">nullable</param>
    /// <param name="cancellationToken">Optional</param>
    /// <returns></returns>
    IEnumerable<DirectoryInfo> GetDirectories (bool pluginUserDirectoryActive, IEnumerable<string> pluginNames = null, Lemoine.Threading.IChecked checkedThread = null, CancellationToken? cancellationToken = null);
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;

namespace Lemoine.Extensions.Interfaces
{
  /// <summary>
  /// Interface for an extensions loader
  /// </summary>
  public interface IExtensionsLoader
  {
    /// <summary>
    /// Associated extensions provider
    /// 
    /// null if not applicable
    /// </summary>
    IExtensionsProvider ExtensionsProvider { get; }

    /// <summary>
    /// Load the extensions asynchronously
    /// </summary>
    /// <param name="cancellationToken">Optional</param>
    /// <returns></returns>
    System.Threading.Tasks.Task LoadExtensionsAsync (CancellationToken? cancellationToken = null);

    /// <summary>
    /// Load the extensions synchronously
    /// </summary>
    /// <returns></returns>
    void LoadExtensions ();
  }
}

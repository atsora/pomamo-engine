// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System.Threading.Tasks;

namespace Lemoine.Hosting.AsyncInitialization
{
  /// <summary>
  /// Represents a type that performs async initialization.
  /// </summary>
  public interface IAsyncInitializer
  {
    /// <summary>
    /// Performs async initialization.
    /// </summary>
    /// <returns>A task that represents the initialization completion.</returns>
    Task InitializeAsync ();
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Asynchronous commit options
  /// </summary>
  public enum SynchronousCommit {
    /// <summary>
    /// Default
    /// </summary>
    Default,
    /// <summary>
    /// Off
    /// </summary>
    Off,
    /// <summary>
    /// On
    /// </summary>
    On,
    /// <summary>
    /// Local
    /// </summary>
    Local
  };
}

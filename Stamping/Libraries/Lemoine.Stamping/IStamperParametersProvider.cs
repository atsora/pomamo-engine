// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Stamping
{
  /// <summary>
  /// Parameters to provide to a <see cref="IStamper">
  /// </summary>
  public interface IStamperParametersProvider
  {
    /// <summary>
    /// Input file path
    /// </summary>
    string InputFilePath { get; }

    /// <summary>
    /// Output file path
    /// </summary>
    string OutputFilePath { get; }
  }
}

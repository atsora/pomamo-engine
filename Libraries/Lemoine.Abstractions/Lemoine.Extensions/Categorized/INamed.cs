// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Extension.Categorized
{
  /// <summary>
  /// Interface for named extensions
  /// </summary>
  public interface INamed
  {
    /// <summary>
    /// Name
    /// </summary>
    string Name { get; }
  }
}

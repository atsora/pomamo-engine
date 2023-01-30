// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Collections
{
  /// <summary>
  /// Name of the collection
  /// </summary>
  public interface INamedCollection
  {
    /// <summary>
    /// Name of the collection
    /// </summary>
    string Name { get; }
  }
}

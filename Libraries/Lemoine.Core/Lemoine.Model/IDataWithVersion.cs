// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Interface for all the models that have a Version property
  /// to manage the concurrent accesses
  /// </summary>
  public interface IDataWithVersion
  {
    /// <summary>
    /// Version
    /// </summary>
    int Version { get; }
  }
}

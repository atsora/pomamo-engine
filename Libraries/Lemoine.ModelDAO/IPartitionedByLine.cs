// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Interface for all the models that may be partioned by line
  /// </summary>
  public interface IPartitionedByLine
  {
    /// <summary>
    /// Associated line
    /// 
    /// Can't be null
    /// </summary>
    ILine Line { get; }
  }
}

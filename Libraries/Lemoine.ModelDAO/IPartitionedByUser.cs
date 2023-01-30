// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Interface for all the models that may be partioned by user
  /// </summary>
  public interface IPartitionedByUser
  {
    /// <summary>
    /// Associated user
    /// 
    /// Can't be null
    /// </summary>
    IUser User { get; }
  }
}

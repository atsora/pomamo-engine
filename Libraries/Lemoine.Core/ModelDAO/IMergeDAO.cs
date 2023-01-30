// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Description of IMergeDAO.
  /// </summary>
  public interface IMergeDAO<I>
  {
    /// <summary>
    /// Merge one old item into a new one
    /// 
    /// This returns the merged item
    /// </summary>
    /// <param name="oldItem"></param>
    /// <param name="newItem"></param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    I Merge (I oldItem,
             I newItem,
             ConflictResolution conflictResolution);
  }
}

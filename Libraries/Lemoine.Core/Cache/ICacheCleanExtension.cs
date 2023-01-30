// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Core.Cache
{
  /// <summary>
  /// Description of ICacheCleanExtension.
  /// </summary>
  public interface ICacheCleanExtension
  {
    /// <summary>
    /// Clean the cache
    /// 
    /// Note we accept from time to time some keys are not cleaned because of the concurrent accesses
    /// </summary>
    void CleanCache ();
  }
}

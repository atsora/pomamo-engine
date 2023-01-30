// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Core.Cache
{
  /// <summary>
  /// Description of ICacheWithCleanExtension.
  /// </summary>
  public interface ICacheClientWithCleanExtension
    : ICacheClient
    , ICacheCleanExtension
  {
  }
}

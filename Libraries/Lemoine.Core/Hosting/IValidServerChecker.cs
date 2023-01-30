// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Core.Hosting
{
  /// <summary>
  /// Interface to implement to check a server is the right server for a specific service
  /// </summary>
  public interface IValidServerChecker
  {
    /// <summary>
    /// Is the local server the valid server for this specific service ?
    /// </summary>
    /// <returns></returns>
    bool IsValidServerForService ();
  }
}

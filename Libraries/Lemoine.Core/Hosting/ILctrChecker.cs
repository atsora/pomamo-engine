// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Core.Hosting
{
  /// <summary>
  /// Interface to implement to test if a server is lctr
  /// </summary>
  public interface ILctrChecker
  {
    /// <summary>
    /// Is the local server Lctr ?
    /// </summary>
    /// <returns></returns>
    bool IsLctr ();
  }
}

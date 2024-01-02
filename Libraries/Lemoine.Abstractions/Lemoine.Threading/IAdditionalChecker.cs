// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Threading
{
  /// <summary>
  /// Interface for an additional checker in checking thread
  /// </summary>
  public interface IAdditionalChecker
  {
    /// <summary>
    /// Early initialization
    /// </summary>
    void Initialize ();

    /// <summary>
    /// If false is returned, the application is restarted
    /// </summary>
    /// <returns></returns>
    bool Check ();
  }
}

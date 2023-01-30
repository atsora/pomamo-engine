// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Core.Hosting
{
  /// <summary>
  /// Collection of application initializers
  /// </summary>
  public interface IApplicationInitializerCollection : IApplicationInitializer
  {
    /// <summary>
    /// Add a new application initializer
    /// </summary>
    /// <param name="application"></param>
    /// <returns></returns>
    IApplicationInitializerCollection Add (IApplicationInitializer application);
  }
}

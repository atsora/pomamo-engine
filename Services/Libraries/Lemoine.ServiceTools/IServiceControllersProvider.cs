// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.ServiceTools
{
  /// <summary>
  /// Provides a set of <see cref="IServiceController"/> services to check
  /// </summary>
  public interface IServiceControllersProvider
  {
    /// <summary>
    /// Get the set of <see cref="IServiceController"/> to check
    /// </summary>
    /// <returns></returns>
    IEnumerable<IServiceController> GetServiceControllers ();
  }
}

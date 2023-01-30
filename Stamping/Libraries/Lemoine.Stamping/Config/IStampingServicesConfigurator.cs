// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Lemoine.Stamping.Config
{
  /// <summary>
  /// Interface to implement to add or configure new services
  /// </summary>
  public interface IStampingServicesConfigurator
  {
    /// <summary>
    /// Configure
    /// </summary>
    /// <param name="services"></param>
    void Configure (IServiceCollection services);
  }
}

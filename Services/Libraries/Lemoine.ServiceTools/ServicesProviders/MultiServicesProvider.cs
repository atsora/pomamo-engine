// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.ServiceTools.ServicesProviders
{
  /// <summary>
  /// MultiServicesProvider
  /// </summary>
  public class MultiServicesProvider: IServiceControllersProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (MultiServicesProvider).FullName);

    readonly IEnumerable<IServiceControllersProvider> m_serviceControllersProviders;

    /// <summary>
    /// Constructor
    /// </summary>
    public MultiServicesProvider (params IServiceControllersProvider[] serviceControllersProviders)
    {
      m_serviceControllersProviders = serviceControllersProviders;
    }

    /// <summary>
    /// <see cref="IServiceControllersProvider"/>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IEnumerable<IServiceController> GetServiceControllers ()
    {
      return m_serviceControllersProviders.SelectMany (x => x.GetServiceControllers ());
    }
  }
}

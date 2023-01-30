// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions;

namespace Pulse.Extensions.Web
{
  /// <summary>
  /// ExtensionInterfaceProvider
  /// </summary>
  public class ExtensionInterfaceProvider : IExtensionInterfaceProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (ExtensionInterfaceProvider).FullName);

    /// <summary>
    /// <see cref="IExtensionInterfaceProvider"/>
    /// </summary>
    public void Load ()
    {
    }

    /// <summary>
    /// Return the interface providers that must be loaded
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<IExtensionInterfaceProvider> GetInterfaceProviders ()
    {
      return new List<IExtensionInterfaceProvider> {
        new Lemoine.Extensions.Database.ExtensionInterfaceProvider (),
        new Lemoine.Extensions.Business.ExtensionInterfaceProvider (),
        new Pulse.Extensions.Database.ExtensionInterfaceProvider (),
        new Pulse.Extensions.Business.ExtensionInterfaceProvider (),
        new ExtensionInterfaceProvider (),
      };
    }
  }
}

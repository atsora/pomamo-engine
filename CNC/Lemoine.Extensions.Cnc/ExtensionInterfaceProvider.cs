// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Extensions.Cnc
{
  /// <summary>
  /// ExtensionInterfaceProvider
  /// </summary>
  public class ExtensionInterfaceProvider : IExtensionInterfaceProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (ExtensionInterfaceProvider).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ExtensionInterfaceProvider ()
    {
    }

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
        new Lemoine.Extensions.Business.ExtensionInterfaceProvider (), // Includes Lemoine.Extensions.Database
        new Lemoine.Extensions.Database.ExtensionInterfaceProvider (),
        new ExtensionInterfaceProvider (),
      };
    }
  }
}

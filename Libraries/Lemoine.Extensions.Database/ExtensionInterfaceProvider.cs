// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Extensions.Database
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

  }
}

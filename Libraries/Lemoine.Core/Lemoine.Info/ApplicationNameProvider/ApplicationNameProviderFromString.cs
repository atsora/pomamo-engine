// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Info.ApplicationNameProvider
{
  /// <summary>
  /// ApplicationNameProviderFromString
  /// </summary>
  public class ApplicationNameProviderFromString: IApplicationNameProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (ApplicationNameProviderFromString).FullName);

    readonly string m_applicationName;

    /// <summary>
    /// Constructor
    /// </summary>
    public ApplicationNameProviderFromString (string applicationName)
    {
      m_applicationName = applicationName;
    }

    /// <summary>
    /// <see cref="IApplicationNameProvider"/>
    /// </summary>
    public string ApplicationName => m_applicationName;
  }
}

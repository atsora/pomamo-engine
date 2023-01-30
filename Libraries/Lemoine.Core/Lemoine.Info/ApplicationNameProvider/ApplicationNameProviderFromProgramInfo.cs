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
  /// ApplicationNameFromProgramInfo
  /// </summary>
  public class ApplicationNameProviderFromProgramInfo: IApplicationNameProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (ApplicationNameProviderFromProgramInfo).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ApplicationNameProviderFromProgramInfo ()
    {
    }

    /// <summary>
    /// <see cref="IApplicationNameProvider"/>
    /// </summary>
    public string ApplicationName => ProgramInfo.Name;
  }
}

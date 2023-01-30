// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Stamping.DataInterpreters
{
  /// <summary>
  /// CadName regex interpreter
  /// </summary>
  public class CadNameRegexInterpreter: RegexFallbackInterpreter, IDataInterpreter
  {
    readonly ILog log = LogManager.GetLogger (typeof (CadNameRegexInterpreter).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CadNameRegexInterpreter ()
      : base ("CadName")
    {
    }

  }
}

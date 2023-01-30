// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Extensions.Analysis.StateMachine
{
  /// <summary>
  /// Class to catch the interruption of an analysis
  /// </summary>
  public class InterruptException : Exception
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    public InterruptException (string message)
      : base (message)
    { }
  }
}

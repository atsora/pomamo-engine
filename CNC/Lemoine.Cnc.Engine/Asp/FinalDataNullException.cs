// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.CncEngine.Asp
{
  /// <summary>
  /// FinalDataNullException
  /// </summary>
  public class FinalDataNullException: Exception
  {
    readonly ILog log = LogManager.GetLogger (typeof (FinalDataNullException).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public FinalDataNullException ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    public FinalDataNullException (string message)
      : base (message)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public FinalDataNullException (string message, Exception inner)
      : base (message, inner)
    {
    }
  }
}

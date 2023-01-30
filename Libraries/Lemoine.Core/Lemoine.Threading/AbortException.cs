// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Threading
{
  /// <summary>
  /// Exception to raise when a process or thread must be aborted
  /// </summary>
  public class AbortException : Exception
  {
    readonly ILog log = LogManager.GetLogger (typeof (AbortException).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public AbortException ()
      : base ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    public AbortException (string message)
      : base (message)
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public AbortException (string message, Exception innerException)
      : base (message, innerException)
    { 
    }
    #endregion // Constructors
  }
}

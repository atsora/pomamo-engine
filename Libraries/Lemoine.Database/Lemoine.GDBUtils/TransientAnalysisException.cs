// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.GDBUtils
{
  /// <summary>
  /// Description of TransientAnalysisException.
  /// </summary>
  public class TransientAnalysisException: Exception
  {
    static readonly ILog log = LogManager.GetLogger(typeof (TransientAnalysisException).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public TransientAnalysisException ()
      : base ()
    {
    }

    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="message"></param>
    public TransientAnalysisException (string message)
      : base (message)
    {
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
  }
}

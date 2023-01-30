// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Exception to use to interrupt an analysis before its end.
  /// 
  /// Note that the database is still coherent and you can just commit the transaction in case this transaction is raised
  /// </summary>
  public class InterruptedAnalysis: Exception
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (InterruptedAnalysis).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public InterruptedAnalysis (string message)
      : base (message)
    {
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
  }
}

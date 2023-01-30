// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Web.FileRepo
{
  /// <summary>
  /// FileRepoWebException
  /// </summary>
  public class FileRepoWebException: Exception
  {
    readonly ILog log = LogManager.GetLogger (typeof (FileRepoWebException).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public FileRepoWebException (Exception inner)
      : base ("FileRepoWeb exception", inner)
    {
    }
    #endregion // Constructors

  }
}

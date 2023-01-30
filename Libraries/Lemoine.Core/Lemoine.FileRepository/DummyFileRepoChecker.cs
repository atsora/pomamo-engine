// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.FileRepository
{
  /// <summary>
  /// DummyFileRepoChecker
  /// </summary>
  public class DummyFileRepoChecker: IFileRepoChecker
  {
    readonly ILog log = LogManager.GetLogger (typeof (DummyFileRepoChecker).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    public bool Check ()
    {
      return true;
    }

    public void Initialize ()
    {
      return;
    }
  }
}

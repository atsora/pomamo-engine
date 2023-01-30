// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Threading;

namespace Lemoine.Business.Config
{
  /// <summary>
  /// Dummy implementation of <see cref="IConfigUpdateChecker"/> that does not nothing special
  /// </summary>
  public class DummyConfigUpdateChecker: IConfigUpdateChecker
  {
    readonly ILog log = LogManager.GetLogger (typeof (DummyConfigUpdateChecker).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public DummyConfigUpdateChecker ()
    {
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="IAdditionalChecker"/>
    /// </summary>
    /// <returns></returns>
    public bool Check ()
    {
      return true;
    }

    /// <summary>
    /// <see cref="IAdditionalChecker"/>
    /// </summary>
    public void Initialize ()
    {
      return;
    }

    public bool CheckNoConfigUpdate ()
    {
      return true;
    }

  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Core.Hosting.LctrChecker
{
  /// <summary>
  /// <see cref="ILctrChecker"/> implementation to force the local computer is lctr
  /// </summary>
  public class ForceLctr: ILctrChecker
  {
    readonly ILog log = LogManager.GetLogger (typeof (ForceLctr).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ForceLctr ()
    {
    }

    /// <summary>
    /// <see cref="ILctrChecker"/>
    /// </summary>
    /// <returns></returns>
    public bool IsLctr ()
    {
      return true;
    }
  }
}

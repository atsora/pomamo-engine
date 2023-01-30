// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Extensions.Web.Attributes
{
  /// <summary>
  /// AllowAnonynousAttribute
  /// </summary>
  [AttributeUsage (AttributeTargets.Class, AllowMultiple = false)]
  public class AllowAnonymousAttribute : Attribute
  {
    readonly ILog log = LogManager.GetLogger (typeof (AllowAnonymousAttribute).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AllowAnonymousAttribute ()
    {
    }
  }
}

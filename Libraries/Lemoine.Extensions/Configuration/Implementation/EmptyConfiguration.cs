// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Extensions.Configuration.Implementation
{
  /// <summary>
  /// Empty configuration
  /// </summary>
  public class EmptyConfiguration: IConfiguration
  {
    readonly ILog log = LogManager.GetLogger (typeof (EmptyConfiguration).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public EmptyConfiguration ()
    {
    }

    /// <summary>
    /// <see cref="IConfiguration"/>
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public bool IsValid (out IEnumerable<string> errors)
    {
      errors = new List<string> ();
      return true;
    }
    #endregion // Constructors

  }
}

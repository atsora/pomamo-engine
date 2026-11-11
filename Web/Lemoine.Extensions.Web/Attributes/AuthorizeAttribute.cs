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
  /// AuthorizeAttribute
  /// </summary>
  [AttributeUsage (AttributeTargets.Class, AllowMultiple = true)]
  public class AuthorizeAttribute : Attribute
  {
    readonly ILog log = LogManager.GetLogger (typeof (AuthorizeAttribute).FullName);

    /// <summary>
    /// Gets or sets the policy to apply to authorize an access to the resource
    /// </summary>
    public string Policy { get; set; }

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public AuthorizeAttribute ()
    {
    }
    #endregion // Constructors

  }
}

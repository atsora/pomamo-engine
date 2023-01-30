// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;

namespace Lemoine.Plugin.AutoReasonBetweenPrograms
{
  public sealed class Configuration
    : Lemoine.Extensions.AutoReason.AutoReasonConfiguration
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration ()
    {
    }
    #endregion // Constructors
  }
}

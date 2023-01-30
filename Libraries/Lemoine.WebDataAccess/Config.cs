// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Config
  /// </summary>
  public class Config: IConfig
  {
    readonly ILog log = LogManager.GetLogger (typeof (Config).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Config ()
    {
    }
    #endregion // Constructors

    #region IConfig implementation
    public string Description
    {
      get; set;
    }

    public int Id
    {
      get; set;
    }

    public string Key
    {
      get; set;
    }

    public object Value
    {
      get; set;
    }

    public bool Active
    {
      get; set;
    }

    public int Version
    {
      get; set;
    }
    #endregion // IConfig implementation
  }
}

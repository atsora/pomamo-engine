// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Core.Log;

namespace Lemoine.Extensions
{
  /// <summary>
  /// Abstract class for extensions that are not configurable
  /// </summary>
  public abstract class NotConfigurableExtension
    : IExtension
  {
    #region Getters / Setters
    #endregion // Getters / Setters

    static readonly ILog log = LogManager.GetLogger (typeof (NotConfigurableExtension).FullName);

    #region IExtension implementation
    /// <summary>
    /// <see cref="IExtension"/>
    /// </summary>
    public bool UniqueInstance
    {
      get { return true; }
    }
    #endregion // IExtension implementation
  }
}

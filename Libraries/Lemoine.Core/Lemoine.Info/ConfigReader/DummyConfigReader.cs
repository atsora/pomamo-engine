// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// DummyConfigReader
  /// </summary>
  public class DummyConfigReader: IGenericConfigReader
  {
    readonly ILog log = LogManager.GetLogger (typeof (DummyConfigReader).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public DummyConfigReader ()
    {
    }
    
    /// <summary>
    /// <see cref="IGenericConfigReader"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T> (string key)
    {
      throw new ConfigKeyNotFoundException (key);
    }
    #endregion // Constructors

  }
}

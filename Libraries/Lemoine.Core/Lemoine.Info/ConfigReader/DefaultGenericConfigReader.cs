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
  /// Base config reader class to set default values
  /// </summary>
  public abstract class DefaultGenericConfigReader : IGenericConfigReader
  {
    readonly ILog log = LogManager.GetLogger (typeof (DefaultGenericConfigReader).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public DefaultGenericConfigReader ()
    {
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="IGenericConfigReader"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T> (string key)
    {
      return Convert<T> (Get (key));
    }

    /// <summary>
    /// Method to implement in sub-classes
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    protected abstract object Get (string key);

    T Convert<T> (object v)
    {
      if (v is T) {
        return (T)v;
      }
      else {
        log.Error ($"Convert: {v} is not of type {typeof (T)}");
        throw new InvalidCastException ();
      }
    }
  }
}

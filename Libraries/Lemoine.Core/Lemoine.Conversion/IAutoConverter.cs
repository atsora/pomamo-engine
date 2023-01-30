// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Conversion
{
  /// <summary>
  /// Interface to define an auto-convert method
  /// </summary>
  public interface IAutoConverter
  {
    /// <summary>
    /// Try to automatically convert an object to a specific type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="x"></param>
    /// <returns></returns>
    T ConvertAuto<T> (object x);

    /// <summary>
    /// Try to automatically convert an object to a specific type
    /// </summary>
    /// <param name="x"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    object ConvertAuto (object x, Type t);

    /// <summary>
    /// Check if a specific value is compatible with a specific type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="x"></param>
    /// <returns></returns>
    bool IsCompatible<T> (object x);

    /// <summary>
    /// Check if a specific value is compatible with a specific type
    /// </summary>
    /// <param name="x"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    bool IsCompatible (object x, Type t);
  }
}

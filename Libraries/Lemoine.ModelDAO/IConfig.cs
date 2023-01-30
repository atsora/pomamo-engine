// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table Config
  /// </summary>
  public interface IConfig: IVersionable
  {
    /// <summary>
    /// Config key
    /// 
    /// Use the '.' separator to structure the key
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Config description
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Config value, a serializable object
    /// </summary>
    object Value { get; set; }

    /// <summary>
    /// Active property
    /// </summary>
    bool Active { get; set; }
  }
}

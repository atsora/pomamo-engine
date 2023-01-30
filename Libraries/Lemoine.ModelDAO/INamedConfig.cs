// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table namedconfig
  /// </summary>
  public interface INamedConfig: IVersionable
  {
    /// <summary>
    /// Name of a set of config
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Config key
    /// 
    /// Use the '.' separator to structure the key
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Config value, a serializable object
    /// </summary>
    object Value { get; set; }
  }
}

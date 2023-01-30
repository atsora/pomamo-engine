// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Log levels
  /// </summary>
  public enum LogLevel {
    /// <summary>
    /// DEBUG log level
    /// </summary>
    DEBUG=0,
    /// <summary>
    /// INFO log level
    /// </summary>
    INFO=1,
    /// <summary>
    /// NOTICE log level
    /// </summary>
    NOTICE=2,
    /// <summary>
    /// WARN log level
    /// </summary>
    WARN=3,
    /// <summary>
    /// ERROR log level
    /// </summary>
    ERROR=4,
    /// <summary>
    /// CRIT log level
    /// </summary>
    CRIT=5
  };
  
  /// <summary>
  /// Log model
  /// </summary>
  public interface IBaseLog: Lemoine.Collections.IDataWithId, ISerializableModel
  {
    /// <summary>
    /// Log UTC date/time
    /// </summary>
    DateTime DateTime { get; set; }
    
    /// <summary>
    /// Log level
    /// </summary>
    LogLevel Level { get; set; }
    
    /// <summary>
    /// Log message
    /// </summary>
    string Message { get; set; }
    
    /// <summary>
    /// Module in which the log was recorded
    /// </summary>
    string Module { get; set; }
  }
}

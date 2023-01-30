// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.Core.ExceptionManagement
{
  /// <summary>
  /// Details on a database exception
  /// </summary>
  public interface IDatabaseExceptionDetails
  {
    #region Getters / Setters
    /// <summary>
    /// Database exception source
    /// </summary>
    Exception DatabaseException { get; }
    
    /// <summary>
    /// Base message
    /// </summary>
    string BaseMessage { get; }
    
    /// <summary>
    /// Code
    /// </summary>
    string Code { get; }
    
    /// <summary>
    /// Details
    /// </summary>
    string Detail { get; }
    
    /// <summary>
    /// Error SQL
    /// </summary>
    string ErrorSql { get; }
    
    /// <summary>
    /// File
    /// </summary>
    string File { get; }
    
    /// <summary>
    /// Hint
    /// </summary>
    string Hint { get; }
    
    /// <summary>
    /// Line
    /// </summary>
    string Line { get; }
    
    /// <summary>
    /// Position
    /// </summary>
    string Position { get; }
    
    /// <summary>
    /// Routine
    /// </summary>
    string Routine { get; }
    
    /// <summary>
    /// Severity
    /// </summary>
    string Severity { get; }
    
    /// <summary>
    /// Where
    /// </summary>
    string Where { get; }
    #endregion // Getters / Setters
    
    /// <summary>
    /// <see cref="Object.ToString()"></see>
    /// </summary>
    /// <returns></returns>
    string ToString();
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.DataRepository
{
  /// <summary>
  /// Raised when an error was found in the schema
  /// </summary>
  public class SchemaException : RepositoryException
  {
    /// <summary>
    /// <see cref="RepositoryException"/>
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public SchemaException (string message)
      : base (message)
    {
    }

    /// <summary>
    /// <see cref="RepositoryException"/>
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException"></param>
    public SchemaException (string message, Exception innerException)
      : base (message, innerException)
    {
    }
  }

}

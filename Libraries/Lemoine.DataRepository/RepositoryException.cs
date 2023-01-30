// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DataRepository
{
  /// <summary>
  /// Generic repository exception
  /// </summary>
  public class RepositoryException : Exception
  {
    /// <summary>
    /// Initializes a new instance of the RepositoryException class.
    /// <see cref="Exception">Exception constructor</see>
    /// </summary>
    public RepositoryException () : base ()
    {
    }

    /// <summary>
    /// Initializes a new instance of the RepositoryException class with a specified error message.
    /// <see cref="Exception">Exception constructor</see>
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RepositoryException (string message) : base (message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the RepositoryException class with a specified error message.
    /// <see cref="Exception">Exception constructor</see>
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The inner exception.</param>
    public RepositoryException (string message, Exception innerException)
      : base (message, innerException)
    {
    }
  }

  /// <summary>
  /// Validation exception
  /// </summary>
  public class ValidationException : RepositoryException
  {
    /// <summary>
    /// Initializes a new instance of the ValidationException class.
    /// <see cref="Exception">Exception constructor</see>
    /// </summary>
    public ValidationException () : base ()
    {
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class with a specified error message.
    /// <see cref="Exception">Exception constructor</see>
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ValidationException (string message) : base (message)
    {
    }
  }
}

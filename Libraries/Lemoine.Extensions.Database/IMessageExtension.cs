// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Database
{
  /// <summary>
  /// Extension to process the messages from the database
  /// </summary>
  public interface IMessageExtension : IExtension
  {
    /// <summary>
    /// Process the message
    /// </summary>
    /// <param name="message"></param>
    void ProcessMessage (string message);

    /// <summary>
    /// Process the message asynchronously
    /// </summary>
    /// <param name="message"></param>
    System.Threading.Tasks.Task ProcessMessageAsync (string message);
  }
}

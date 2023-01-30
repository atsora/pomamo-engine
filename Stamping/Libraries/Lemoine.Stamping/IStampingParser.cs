// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Lemoine.Stamping
{
  /// <summary>
  /// Interface to implement to parse a file to stamp
  /// </summary>
  public interface IStampingParser
  {
    /// <summary>
    /// Line feed to consider when a new line is added
    /// </summary>
    StamperLineFeed LineFeed { get; }

    /// <summary>
    /// Parse
    /// </summary>
    /// <param name="stamper"></param>
    /// <param name="stampingEventHandler"></param>
    /// <param name="stampingData"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The whole file was parsed</returns>
    Task<bool> ParseAsync (IStamper stamper, IStampingEventHandler stampingEventHandler, StampingData stampingData, CancellationToken cancellationToken = default);
  }
}

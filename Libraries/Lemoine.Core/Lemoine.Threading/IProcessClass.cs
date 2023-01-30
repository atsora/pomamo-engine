// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using Lemoine.Core.Log;

namespace Lemoine.Threading
{
  /// <summary>
  /// Interface to a class whose main method is called in a process
  /// </summary>
  public interface IProcessClass
  {
    /// <summary>
    /// Time after which the process kills itself (null: not set)
    /// </summary>
    TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Not responding timeout (null: unknown / not set)
    /// </summary>
    TimeSpan? NotRespondingTimeout { get; set; }
    
    /// <summary>
    /// Time to sleep before restarting a malfunctioning thread (null: unknown / not set)
    /// </summary>
    TimeSpan? SleepBeforeRestart { get; set; }

    /// <summary>
    /// Use a stamp file to check a process
    /// </summary>
    bool UseStampFile { get; set; }
    
    /// <summary>
    /// Parent process ID
    /// </summary>
    int ParentProcessId { get; set; }

    /// <summary>
    /// Return the full stamp file path to use to check a process
    /// </summary>
    /// <returns></returns>
    string GetStampFilePath ();

    /// <summary>
    /// Start the process
    /// 
    /// Optionally, a specific file is used
    /// to monitor if the process is still working
    /// 
    /// If ParentProcessId is not 0,
    /// the program exists in case the corresponding process
    /// is not running any more
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="apartmentState">not used</param>
    void Start (CancellationToken? cancellationToken = null, ApartmentState apartmentState = ApartmentState.MTA);

    /// <summary>
    /// Get the logger
    /// </summary>
    /// <returns></returns>
    ILog GetLogger ();
  }
}

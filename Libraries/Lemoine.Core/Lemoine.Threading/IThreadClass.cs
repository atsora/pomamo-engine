// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using Lemoine.Core.Log;

namespace Lemoine.Threading
{
  /// <summary>
  /// Interface to a class whose main method is called in a thread
  /// </summary>
  public interface IThreadClass
  {
    /// <summary>
    /// Time after which the thread kills itself.
    /// 
    /// This is safer to make it thread safe although this is not necessary
    /// if Timeout is only set before it is run
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
    /// Latest execution date/time of the ProcessTasks method
    /// 
    /// Warning: this getter / setter must be thread safe !
    /// </summary>
    DateTime LastExecution { get; set; }

    /// <summary>
    /// Was the thread completed but in error (an exception was raised) ?
    /// </summary>
    bool Error { get; }

    /// <summary>
    /// Is Exit requested ?
    /// </summary>
    bool ExitRequested { get; }

    /// <summary>
    /// Associated thread
    /// </summary>
    System.Threading.Thread Thread { get; }
    
    /// <summary>
    /// Was the thread started ?
    /// </summary>
    bool Started { get; }

    /// <summary>
    /// Start the thread
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="apartmentState"></param>
    void Start (CancellationToken? cancellationToken = null, ApartmentState apartmentState = ApartmentState.MTA);

    /// <summary>
    /// Try to restart the thread (cancel it and start again)
    /// </summary>
    /// <param name="timeout">Timeout for Cancel</param>
    /// <returns>success</returns>
    bool Restart (TimeSpan? timeout = null);

#if NETSTANDARD
    /// <summary>
    /// Run asynchronously
    /// </summary>
    /// <param name="cancellationToken">Optional</param>
    /// <returns></returns>
    System.Threading.Tasks.Task RunAsync (CancellationToken? cancellationToken = null);
#endif // NETSTANDARD

    /// <summary>
    /// Check if the check tread must be in pause ? 
    /// </summary>
    /// <returns></returns>
    bool IsCheckInPause ();

    /// <summary>
    /// Cancel the thread execution. This is not too aggressive, try to close all the threads propely
    /// </summary>
    /// <param name="timeout">Time to wait the thread completion after the thread is cancelled. If it is not set, the Timeout property is considered</param>
    /// <returns>success</returns>
    /// <exception cref="Exception">Exception raised by the Join or Cancel method</exception>
    bool Cancel (TimeSpan? timeout = null);

    /// <summary>
    /// Interrupt the thread. Same as Cancel with no return code and no exception is raised
    /// 
    /// There is no guarantee anything was done
    /// </summary>
    void Interrupt ();

    /// <summary>
    /// Like Interrupt, but potentially more aggressive (at least on .NET Framework)
    /// </summary>
    /// <param name="tryCancelFirst">Try to cancel the thread first</param>
    /// <returns>Success</returns>
    bool Abort (bool tryCancelFirst = true);

    /// <summary>
    /// Get the logger
    /// </summary>
    /// <returns></returns>
    ILog GetLogger ();
  }
}

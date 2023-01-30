// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Lemoine.Core.Log;

namespace Lemoine.Core
{
  /// <summary>
  /// Utilities
  /// </summary>
  public static class Environment
  {
    static ILog log = LogManager.GetLogger (typeof (Environment).FullName);

    /// <summary>
    /// Request a stop of the service with a specified message and the exception that was the cause of the exit request
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    public static void LogAndForceExit (Exception ex, ILog logger)
    {
      LogManager.SetApplicationStopping ();

      try {
        ILog localLogger = logger ?? log;
        localLogger.Fatal ("Exit: exit was requested", ex);
      }
      catch (Exception) { }

      ForceExit ();
    }

    /// <summary>
    /// Request a stop of the service with a specified message and the exception that was the cause of the exit request
    /// </summary>
    /// <param name="logger"></param>
    public static void LogAndForceExit (ILog logger)
    {
      LogManager.SetApplicationStopping ();

      try {
        ILog localLogger = logger ?? log;
        localLogger.FatalFormat ("Exit: exit was requested by {0}",
                                 System.Environment.StackTrace);
      }
      catch (Exception) { }

      ForceExit ();
    }

    /// <summary>
    /// Request a stop of the service
    /// 
    /// Use a default logger here
    /// </summary>
    public static void LogAndForceExit ()
    {
      LogAndForceExit (null);
    }

    /// <summary>
    /// Force to exit
    /// </summary>
    public static void ForceExit ()
    {
      LogManager.SetApplicationStopping ();

      try {
        LogManager.Shutdown ();
      }
      catch (Exception) { }

      // Note: Process.GetCurrentProcess().Kill() is more aggressive than System.Environment.Exit (1)
      // It is ok because the web service is normally run by the System user
      try {
        Process.GetCurrentProcess ().Kill ();
      }
      finally {
        // Run also in addition Exit(1) just in case
        System.Environment.Exit (1);
      }
    }

  }
}

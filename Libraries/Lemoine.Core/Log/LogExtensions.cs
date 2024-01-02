// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Core.Log
{
  /// <summary>
  /// Extensions to log4net
  /// </summary>
  public static class LogExtensions
  {
    /// <summary>
    /// Log an exception guessing the log level from the exception severity
    /// </summary>
    public static void Exception (this ILog logger, Exception ex, string message = null)
    {
      if (Lemoine.Core.ExceptionManagement.ExceptionTest.RequiresExit (ex)) { // Fatal
        if (logger.IsFatalEnabled) {
          if (string.IsNullOrEmpty (message)) {
            logger.Fatal ("the exception requires to exit", ex);
          }
          else {
            logger.Fatal ($"{message}: the exception requires to exit", ex);
          }
        }
      }
      else if (Lemoine.Core.ExceptionManagement.ExceptionTest.IsTransactionSerializationFailure (ex)
        || Lemoine.Core.ExceptionManagement.ExceptionTest.IsTransactionAborted (ex)) {
        if (logger.IsInfoEnabled) {
          if (string.IsNullOrEmpty (message)) {
            logger.Info ("aborted transaction (serialization failure or not)", ex);
          }
          else {
            logger.Info ($"{message}: aborted transaction (serialization failure or not)", ex);
          }
        }
      }
      else if (Lemoine.Core.ExceptionManagement.ExceptionTest.IsTemporary (ex)
        || Lemoine.Core.ExceptionManagement.ExceptionTest.IsTemporaryWithDelay (ex)
        || Lemoine.Core.ExceptionManagement.ExceptionTest.IsStale (ex)
        || Lemoine.Core.ExceptionManagement.ExceptionTest.IsTimeoutFailure (ex)) { // Warn
        if (logger.IsWarnEnabled) {
          if (string.IsNullOrEmpty (message)) {
            logger.Warn ("temporary / stale / timeout error", ex);
          }
          else {
            logger.Warn ($"{message}: temporary", ex);
          }
        }
      }
      else if (Lemoine.Core.ExceptionManagement.ExceptionTest.IsNotError (ex)) { // Info
        if (logger.IsInfoEnabled) {
          if (string.IsNullOrEmpty (message)) {
            logger.Info ("not  a real error", ex);
          }
          else {
            logger.Info ($"{message}: not a real error", ex);
          }
        }
      }
      else { // Error
        if (logger.IsErrorEnabled) {
          if (string.IsNullOrEmpty (message)) {
            logger.Error ("exception", ex);
          }
          else {
            logger.Error (message, ex);
          }
        }
      }
    }

    /// <summary>
    /// Parse a log level
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public static Level ParseLevel (string level)
    {
      return (Level)Enum.Parse (typeof(Level), level);
    }

    #region log4net extensions
    /// <summary>
    /// Add a log method that takes a level in argument
    /// </summary>
    /// <param name="log"></param>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static bool Log (this log4net.ILog log, log4net.Core.Level level, string message)
    {
      return Log (log, level, message, null);
    }

    /// <summary>
    /// Add a log method that takes a level in argument
    /// </summary>
    /// <param name="log"></param>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    public static bool Log (this log4net.ILog log, log4net.Core.Level level, string message, Exception exception)
    {
      var logger = log.Logger;
      if (logger.IsEnabledFor (level)) {
        logger.Log (logger.GetType (), level, message, exception);
        return true;
      }

      return false;
    }

    /// <summary>
    /// Add a LogFormat method that takes a level in argument
    /// </summary>
    /// <param name="log"></param>
    /// <param name="level"></param>
    /// <param name="messageFormat"></param>
    /// <param name="messageArguments"></param>
    /// <returns></returns>
    public static bool LogFormat (this log4net.ILog log, log4net.Core.Level level, string messageFormat, params object[] messageArguments)
    {
      var logger = log.Logger;
      if (logger.IsEnabledFor (level)) {
        var message = string.Format (messageFormat, messageArguments);
        logger.Log (logger.GetType (), level, message, null);

        return true;
      }

      return false;
    }

    /// <summary>
    /// Parse a log level
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public static log4net.Core.Level ParseLog4netLevel (string level)
    {
      switch (level) {
        case "Trace":
          return log4net.Core.Level.Trace;
        case "Debug":
          return log4net.Core.Level.Debug;
        case "Finest":
          return log4net.Core.Level.Finest;
        case "Finer":
          return log4net.Core.Level.Finer;
        case "Fine":
          return log4net.Core.Level.Fine;
        case "Verbose":
          return log4net.Core.Level.Verbose;
        case "Info":
          return log4net.Core.Level.Info;
        case "Notice":
          return log4net.Core.Level.Notice;
        case "Warn":
          return log4net.Core.Level.Warn;
        case "Error":
          return log4net.Core.Level.Error;
        case "Severe":
          return log4net.Core.Level.Severe;
        case "Fatal":
          return log4net.Core.Level.Fatal;
        case "Alert":
          return log4net.Core.Level.Alert;
        case "Critical":
          return log4net.Core.Level.Critical;
        case "Emergency":
          return log4net.Core.Level.Emergency;
        default:
          throw new ArgumentException ("Invalid level " + level, "level");
      }
    }
    #endregion // log4net extensions
  }
}

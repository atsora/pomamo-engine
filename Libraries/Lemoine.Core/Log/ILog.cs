// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#region Apache License
//
// Licensed to the Apache Software Foundation (ASF) under one or more 
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership. 
// The ASF licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with 
// the License. You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// The initial file that is drawn from the log4net library
// was modified for Pomamo
#endregion

using System;

namespace Lemoine.Core.Log
{
  /// <summary>
  /// Levels
  /// </summary>
  public enum Level
  {
    /// <summary>
    /// Trace
    /// </summary>
    Trace,
    /// <summary>
    /// Debug
    /// </summary>
    Debug,
    //    Finest,
    //    Finer,
    //    Fine,
    //    Verbose,
    /// <summary>
    /// Info
    /// </summary>
    Info,
    /// <summary>
    /// Notive
    /// </summary>
    Notice,
    /// <summary>
    /// Warn
    /// </summary>
    Warn,
    /// <summary>
    /// Error
    /// </summary>
    Error,
    //    Severe,
    /// <summary>
    /// Fatal
    /// </summary>
    Fatal,
    //    Alert,
    //    Critical,
    //    Emergency,
  }

  /// <summary>
  /// ILog interface for PULSE, similar to the log4net ILog interface
  /// 
  /// log4net documentation:
  /// The ILog interface is use by application to log messages into
  /// the log4net framework.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Use the LogManager to obtain logger instances
  /// that implement this interface. The LogManager.GetLogger(Assembly,Type)
  /// static method is used to get logger instances.
  /// </para>
  /// <para>
  /// This class contains methods for logging at different levels and also
  /// has properties for determining if those logging levels are
  /// enabled in the current configuration.
  /// </para>
  /// <para>
  /// This interface can be implemented in different ways. This documentation
  /// specifies reasonable behavior that a caller can expect from the actual
  /// implementation, however different implementations reserve the right to
  /// do things differently.
  /// </para>
  /// </remarks>
  /// <example>Simple example of logging messages
  /// <code lang="C#">
  /// ILog log = LogManager.GetLogger("application-log");
  /// 
  /// log.Info("Application Start");
  /// log.Debug("This is a debug message");
  /// 
  /// if (log.IsDebugEnabled)
  /// {
  ///		log.Debug("This is another debug message");
  /// }
  /// </code>
  /// </example>
  /// <author>Nicko Cadell</author>
  /// <author>Gert Driesen</author>
  public interface ILog
  {
    #region log4net interface
    /// <overloads>Log a message object with the <see cref="Level.Debug"/> level.</overloads>
    /// <summary>
    /// Log a message object with the <see cref="Level.Debug"/> level.
    /// </summary>
    /// <param name="message">The message object to log.</param>
    /// <remarks>
    /// <para>
    /// This method first checks if this logger is <c>DEBUG</c>
    /// enabled by comparing the level of this logger with the 
    /// <see cref="Level.Debug"/> level. If this logger is
    /// <c>DEBUG</c> enabled, then it converts the message object
    /// (passed as parameter) to a string by invoking the appropriate
    /// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
    /// proceeds to call all the registered appenders in this logger 
    /// and also higher in the hierarchy depending on the value of 
    /// the additivity flag.
    /// </para>
    /// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
    /// to this method will print the name of the <see cref="Exception"/> 
    /// but no stack trace. To print a stack trace use the 
    /// <see cref="Debug(object,Exception)"/> form instead.
    /// </para>
    /// </remarks>
    /// <seealso cref="Debug(object,Exception)"/>
    /// <seealso cref="IsDebugEnabled"/>
    void Debug (object message);

    /// <summary>
    /// Log a message object with the <see cref="Level.Debug"/> level including
    /// the stack trace of the <see cref="Exception"/> passed
    /// as a parameter.
    /// </summary>
    /// <param name="message">The message object to log.</param>
    /// <param name="exception">The exception to log, including its stack trace.</param>
    /// <remarks>
    /// <para>
    /// See the <see cref="Debug(object)"/> form for more detailed information.
    /// </para>
    /// </remarks>
    /// <seealso cref="Debug(object)"/>
    /// <seealso cref="IsDebugEnabled"/>
    void Debug (object message, Exception exception);

    /// <overloads>Log a formatted string with the <see cref="Level.Debug"/> level.</overloads>
    /// <summary>
    /// Logs a formatted message string with the <see cref="Level.Debug"/> level.
    /// </summary>
    /// <param name="format">A String containing zero or more format items</param>
    /// <param name="args">An Object array containing zero or more objects to format</param>
    /// <remarks>
    /// <para>
    /// The message is formatted using the <c>String.Format</c> method. See
    /// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
    /// of the formatting.
    /// </para>
    /// <para>
    /// This method does not take an <see cref="Exception"/> object to include in the
    /// log event. To pass an <see cref="Exception"/> use one of the <see cref="Debug(object,Exception)"/>
    /// methods instead.
    /// </para>
    /// </remarks>
    /// <seealso cref="Debug(object)"/>
    /// <seealso cref="IsDebugEnabled"/>
    void DebugFormat (string format, params object[] args);

    /// <overloads>Log a message object with the <see cref="Level.Info"/> level.</overloads>
    /// <summary>
    /// Logs a message object with the <see cref="Level.Info"/> level.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method first checks if this logger is <c>INFO</c>
    /// enabled by comparing the level of this logger with the 
    /// <see cref="Level.Info"/> level. If this logger is
    /// <c>INFO</c> enabled, then it converts the message object
    /// (passed as parameter) to a string by invoking the appropriate
    /// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
    /// proceeds to call all the registered appenders in this logger 
    /// and also higher in the hierarchy depending on the value of the 
    /// additivity flag.
    /// </para>
    /// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
    /// to this method will print the name of the <see cref="Exception"/> 
    /// but no stack trace. To print a stack trace use the 
    /// <see cref="Info(object,Exception)"/> form instead.
    /// </para>
    /// </remarks>
    /// <param name="message">The message object to log.</param>
    /// <seealso cref="Info(object,Exception)"/>
    /// <seealso cref="IsInfoEnabled"/>
    void Info (object message);

    /// <summary>
    /// Logs a message object with the <c>INFO</c> level including
    /// the stack trace of the <see cref="Exception"/> passed
    /// as a parameter.
    /// </summary>
    /// <param name="message">The message object to log.</param>
    /// <param name="exception">The exception to log, including its stack trace.</param>
    /// <remarks>
    /// <para>
    /// See the <see cref="Info(object)"/> form for more detailed information.
    /// </para>
    /// </remarks>
    /// <seealso cref="Info(object)"/>
    /// <seealso cref="IsInfoEnabled"/>
    void Info (object message, Exception exception);

    /// <overloads>Log a formatted message string with the <see cref="Level.Info"/> level.</overloads>
    /// <summary>
    /// Logs a formatted message string with the <see cref="Level.Info"/> level.
    /// </summary>
    /// <param name="format">A String containing zero or more format items</param>
    /// <param name="args">An Object array containing zero or more objects to format</param>
    /// <remarks>
    /// <para>
    /// The message is formatted using the <c>String.Format</c> method. See
    /// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
    /// of the formatting.
    /// </para>
    /// <para>
    /// This method does not take an <see cref="Exception"/> object to include in the
    /// log event. To pass an <see cref="Exception"/> use one of the <see cref="Info(object)"/>
    /// methods instead.
    /// </para>
    /// </remarks>
    /// <seealso cref="Info(object,Exception)"/>
    /// <seealso cref="IsInfoEnabled"/>
    void InfoFormat (string format, params object[] args);

    /// <overloads>Log a message object with the <see cref="Level.Warn"/> level.</overloads>
    /// <summary>
    /// Log a message object with the <see cref="Level.Warn"/> level.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method first checks if this logger is <c>WARN</c>
    /// enabled by comparing the level of this logger with the 
    /// <see cref="Level.Warn"/> level. If this logger is
    /// <c>WARN</c> enabled, then it converts the message object
    /// (passed as parameter) to a string by invoking the appropriate
    /// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
    /// proceeds to call all the registered appenders in this logger 
    /// and also higher in the hierarchy depending on the value of the 
    /// additivity flag.
    /// </para>
    /// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
    /// to this method will print the name of the <see cref="Exception"/> 
    /// but no stack trace. To print a stack trace use the 
    /// <see cref="Warn(object,Exception)"/> form instead.
    /// </para>
    /// </remarks>
    /// <param name="message">The message object to log.</param>
    /// <seealso cref="Warn(object,Exception)"/>
    /// <seealso cref="IsWarnEnabled"/>
    void Warn (object message);

    /// <summary>
    /// Log a message object with the <see cref="Level.Warn"/> level including
    /// the stack trace of the <see cref="Exception"/> passed
    /// as a parameter.
    /// </summary>
    /// <param name="message">The message object to log.</param>
    /// <param name="exception">The exception to log, including its stack trace.</param>
    /// <remarks>
    /// <para>
    /// See the <see cref="Warn(object)"/> form for more detailed information.
    /// </para>
    /// </remarks>
    /// <seealso cref="Warn(object)"/>
    /// <seealso cref="IsWarnEnabled"/>
    void Warn (object message, Exception exception);

    /// <overloads>Log a formatted message string with the <see cref="Level.Warn"/> level.</overloads>
    /// <summary>
    /// Logs a formatted message string with the <see cref="Level.Warn"/> level.
    /// </summary>
    /// <param name="format">A String containing zero or more format items</param>
    /// <param name="args">An Object array containing zero or more objects to format</param>
    /// <remarks>
    /// <para>
    /// The message is formatted using the <c>String.Format</c> method. See
    /// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
    /// of the formatting.
    /// </para>
    /// <para>
    /// This method does not take an <see cref="Exception"/> object to include in the
    /// log event. To pass an <see cref="Exception"/> use one of the <see cref="Warn(object)"/>
    /// methods instead.
    /// </para>
    /// </remarks>
    /// <seealso cref="Warn(object,Exception)"/>
    /// <seealso cref="IsWarnEnabled"/>
    void WarnFormat (string format, params object[] args);

    /// <overloads>Log a message object with the <see cref="Level.Error"/> level.</overloads>
    /// <summary>
    /// Logs a message object with the <see cref="Level.Error"/> level.
    /// </summary>
    /// <param name="message">The message object to log.</param>
    /// <remarks>
    /// <para>
    /// This method first checks if this logger is <c>ERROR</c>
    /// enabled by comparing the level of this logger with the 
    /// <see cref="Level.Error"/> level. If this logger is
    /// <c>ERROR</c> enabled, then it converts the message object
    /// (passed as parameter) to a string by invoking the appropriate
    /// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
    /// proceeds to call all the registered appenders in this logger 
    /// and also higher in the hierarchy depending on the value of the 
    /// additivity flag.
    /// </para>
    /// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
    /// to this method will print the name of the <see cref="Exception"/> 
    /// but no stack trace. To print a stack trace use the 
    /// <see cref="Error(object,Exception)"/> form instead.
    /// </para>
    /// </remarks>
    /// <seealso cref="Error(object,Exception)"/>
    /// <seealso cref="IsErrorEnabled"/>
    void Error (object message);

    /// <summary>
    /// Log a message object with the <see cref="Level.Error"/> level including
    /// the stack trace of the <see cref="Exception"/> passed
    /// as a parameter.
    /// </summary>
    /// <param name="message">The message object to log.</param>
    /// <param name="exception">The exception to log, including its stack trace.</param>
    /// <remarks>
    /// <para>
    /// See the <see cref="Error(object)"/> form for more detailed information.
    /// </para>
    /// </remarks>
    /// <seealso cref="Error(object)"/>
    /// <seealso cref="IsErrorEnabled"/>
    void Error (object message, Exception exception);

    /// <overloads>Log a formatted message string with the <see cref="Level.Error"/> level.</overloads>
    /// <summary>
    /// Logs a formatted message string with the <see cref="Level.Error"/> level.
    /// </summary>
    /// <param name="format">A String containing zero or more format items</param>
    /// <param name="args">An Object array containing zero or more objects to format</param>
    /// <remarks>
    /// <para>
    /// The message is formatted using the <c>String.Format</c> method. See
    /// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
    /// of the formatting.
    /// </para>
    /// <para>
    /// This method does not take an <see cref="Exception"/> object to include in the
    /// log event. To pass an <see cref="Exception"/> use one of the <see cref="Error(object)"/>
    /// methods instead.
    /// </para>
    /// </remarks>
    /// <seealso cref="Error(object,Exception)"/>
    /// <seealso cref="IsErrorEnabled"/>
    void ErrorFormat (string format, params object[] args);

    /// <overloads>Log a message object with the <see cref="Level.Fatal"/> level.</overloads>
    /// <summary>
    /// Log a message object with the <see cref="Level.Fatal"/> level.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method first checks if this logger is <c>FATAL</c>
    /// enabled by comparing the level of this logger with the 
    /// <see cref="Level.Fatal"/> level. If this logger is
    /// <c>FATAL</c> enabled, then it converts the message object
    /// (passed as parameter) to a string by invoking the appropriate
    /// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
    /// proceeds to call all the registered appenders in this logger 
    /// and also higher in the hierarchy depending on the value of the 
    /// additivity flag.
    /// </para>
    /// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
    /// to this method will print the name of the <see cref="Exception"/> 
    /// but no stack trace. To print a stack trace use the 
    /// <see cref="Fatal(object,Exception)"/> form instead.
    /// </para>
    /// </remarks>
    /// <param name="message">The message object to log.</param>
    /// <seealso cref="Fatal(object,Exception)"/>
    /// <seealso cref="IsFatalEnabled"/>
    void Fatal (object message);

    /// <summary>
    /// Log a message object with the <see cref="Level.Fatal"/> level including
    /// the stack trace of the <see cref="Exception"/> passed
    /// as a parameter.
    /// </summary>
    /// <param name="message">The message object to log.</param>
    /// <param name="exception">The exception to log, including its stack trace.</param>
    /// <remarks>
    /// <para>
    /// See the <see cref="Fatal(object)"/> form for more detailed information.
    /// </para>
    /// </remarks>
    /// <seealso cref="Fatal(object)"/>
    /// <seealso cref="IsFatalEnabled"/>
    void Fatal (object message, Exception exception);

    /// <overloads>Log a formatted message string with the <see cref="Level.Fatal"/> level.</overloads>
    /// <summary>
    /// Logs a formatted message string with the <see cref="Level.Fatal"/> level.
    /// </summary>
    /// <param name="format">A String containing zero or more format items</param>
    /// <param name="args">An Object array containing zero or more objects to format</param>
    /// <remarks>
    /// <para>
    /// The message is formatted using the <c>String.Format</c> method. See
    /// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
    /// of the formatting.
    /// </para>
    /// <para>
    /// This method does not take an <see cref="Exception"/> object to include in the
    /// log event. To pass an <see cref="Exception"/> use one of the <see cref="Fatal(object)"/>
    /// methods instead.
    /// </para>
    /// </remarks>
    /// <seealso cref="Fatal(object,Exception)"/>
    /// <seealso cref="IsFatalEnabled"/>
    void FatalFormat (string format, params object[] args);

    /// <summary>
    /// Checks if this logger is enabled for the <see cref="Level.Debug"/> level.
    /// </summary>
    /// <value>
    /// <c>true</c> if this logger is enabled for <see cref="Level.Debug"/> events, <c>false</c> otherwise.
    /// </value>
    /// <remarks>
    /// <para>
    /// This function is intended to lessen the computational cost of
    /// disabled log debug statements.
    /// </para>
    /// <para> For some ILog interface <c>log</c>, when you write:</para>
    /// <code lang="C#">
    /// log.Debug("This is entry number: " + i );
    /// </code>
    /// <para>
    /// You incur the cost constructing the message, string construction and concatenation in
    /// this case, regardless of whether the message is logged or not.
    /// </para>
    /// <para>
    /// If you are worried about speed (who isn't), then you should write:
    /// </para>
    /// <code lang="C#">
    /// if (log.IsDebugEnabled)
    /// { 
    ///     log.Debug("This is entry number: " + i );
    /// }
    /// </code>
    /// <para>
    /// This way you will not incur the cost of parameter
    /// construction if debugging is disabled for <c>log</c>. On
    /// the other hand, if the <c>log</c> is debug enabled, you
    /// will incur the cost of evaluating whether the logger is debug
    /// enabled twice. Once in <see cref="IsDebugEnabled"/> and once in
    /// the <see cref="Debug(object)"/>.  This is an insignificant overhead
    /// since evaluating a logger takes about 1% of the time it
    /// takes to actually log. This is the preferred style of logging.
    /// </para>
    /// <para>Alternatively if your logger is available statically then the is debug
    /// enabled state can be stored in a static variable like this:
    /// </para>
    /// <code lang="C#">
    /// private static readonly bool isDebugEnabled = log.IsDebugEnabled;
    /// </code>
    /// <para>
    /// Then when you come to log you can write:
    /// </para>
    /// <code lang="C#">
    /// if (isDebugEnabled)
    /// { 
    ///     log.Debug("This is entry number: " + i );
    /// }
    /// </code>
    /// <para>
    /// This way the debug enabled state is only queried once
    /// when the class is loaded. Using a <c>private static readonly</c>
    /// variable is the most efficient because it is a run time constant
    /// and can be heavily optimized by the JIT compiler.
    /// </para>
    /// <para>
    /// Of course if you use a static readonly variable to
    /// hold the enabled state of the logger then you cannot
    /// change the enabled state at runtime to vary the logging
    /// that is produced. You have to decide if you need absolute
    /// speed or runtime flexibility.
    /// </para>
    /// </remarks>
    /// <seealso cref="Debug(object)"/>
    bool IsDebugEnabled { get; }

    /// <summary>
    /// Checks if this logger is enabled for the <see cref="Level.Info"/> level.
    /// </summary>
    /// <value>
    /// <c>true</c> if this logger is enabled for <see cref="Level.Info"/> events, <c>false</c> otherwise.
    /// </value>
    /// <remarks>
    /// For more information see <see cref="ILog.IsDebugEnabled"/>.
    /// </remarks>
    /// <seealso cref="Info(object)"/>
    /// <seealso cref="ILog.IsDebugEnabled"/>
    bool IsInfoEnabled { get; }

    /// <summary>
    /// Checks if this logger is enabled for the <see cref="Level.Warn"/> level.
    /// </summary>
    /// <value>
    /// <c>true</c> if this logger is enabled for <see cref="Level.Warn"/> events, <c>false</c> otherwise.
    /// </value>
    /// <remarks>
    /// For more information see <see cref="ILog.IsDebugEnabled"/>.
    /// </remarks>
    /// <seealso cref="Warn(object)"/>
    /// <seealso cref="ILog.IsDebugEnabled"/>
    bool IsWarnEnabled { get; }

    /// <summary>
    /// Checks if this logger is enabled for the <see cref="Level.Error"/> level.
    /// </summary>
    /// <value>
    /// <c>true</c> if this logger is enabled for <see cref="Level.Error"/> events, <c>false</c> otherwise.
    /// </value>
    /// <remarks>
    /// For more information see <see cref="ILog.IsDebugEnabled"/>.
    /// </remarks>
    /// <seealso cref="Error(object)"/>
    /// <seealso cref="ILog.IsDebugEnabled"/>
    bool IsErrorEnabled { get; }

    /// <summary>
    /// Checks if this logger is enabled for the <see cref="Level.Fatal"/> level.
    /// </summary>
    /// <value>
    /// <c>true</c> if this logger is enabled for <see cref="Level.Fatal"/> events, <c>false</c> otherwise.
    /// </value>
    /// <remarks>
    /// For more information see <see cref="ILog.IsDebugEnabled"/>.
    /// </remarks>
    /// <seealso cref="Fatal(object)"/>
    /// <seealso cref="ILog.IsDebugEnabled"/>
    bool IsFatalEnabled { get; }
    #endregion // log4net interface

    #region Additional message levels
    /// <overloads>Log a message object with the <see cref="Level.Trace"/> level.</overloads>
    /// <summary>
    /// Log a message object with the <see cref="Level.Trace"/> level.
    /// </summary>
    /// <param name="message">The message object to log.</param>
    /// <seealso cref="IsTraceEnabled"/>
    void Trace (object message);

    /// <summary>
    /// Checks if this logger is enabled for the <see cref="Level.Trace"/> level.
    /// </summary>
    /// <value>
    /// <c>true</c> if this logger is enabled for <see cref="Level.Trace"/> events, <c>false</c> otherwise.
    /// </value>
    /// <seealso cref="Trace(object)"/>
    bool IsTraceEnabled { get; }
    #endregion // Additional message levels

    #region Pomamo extensions
    /// <summary>
    /// Name of the logger
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Add a log method that takes a level in argument
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    bool Log (Level level, string message);

    /// <summary>
    /// Add a log method that takes a level in argument
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    bool Log (Level level, string message, Exception exception);

    /// <summary>
    /// Add a LogFormat method that takes a level in argument
    /// </summary>
    /// <param name="level"></param>
    /// <param name="messageFormat"></param>
    /// <param name="messageArguments"></param>
    /// <returns></returns>
    bool LogFormat (Level level, string messageFormat, params object[] messageArguments);

    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="state">The identifier for the scope.</param>
    /// <returns>
    /// An IDisposable that ends the logical operation scope on dispose.
    /// </returns>
    IDisposable BeginScope<TState> (TState state);
    #endregion // Pomamo extensions
  }
}

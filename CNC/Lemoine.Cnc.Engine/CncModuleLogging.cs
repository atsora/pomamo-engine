// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

#if !NET40

using System;
using System.Collections.Concurrent;
using System.Reflection;
using Lemoine.Core.Log;
using Microsoft.Extensions.Logging;

namespace Lemoine.Cnc
{
  /// <summary>
  /// Give the cnc modules that log with Microsoft.Extensions.Logging a way to reach the logs of the
  /// acquisition.
  ///
  /// A cnc module is created by reflection, without any dependency injection, so a logger factory
  /// can not be injected in it. A module that needs one exposes instead a public static
  /// LoggerFactory property, and this class sets it once, with a factory that forwards to
  /// <see cref="Lemoine.Core.Log"/>: the category name is kept, so the log4net configuration of the
  /// acquisition applies to those logs as it does to the other ones.
  ///
  /// This is what Lemoine.Cnc.OpcUaClient relies on. It logs with Microsoft.Extensions.Logging
  /// rather than with log4net, because its license, the GPL-2.0, does not suit the Apache-2.0 of
  /// log4net. This class is what makes its logs visible when an acquisition loads it directly.
  ///
  /// A module that does not expose such a property is simply ignored.
  /// </summary>
  public static class CncModuleLogging
  {
    /// <summary>
    /// Name of the public static property a module exposes to receive the logger factory
    /// </summary>
    static readonly string LOGGER_FACTORY_PROPERTY = "LoggerFactory";

    static readonly ILog log = LogManager.GetLogger (typeof (CncModuleLogging).FullName);

    static readonly ConcurrentDictionary<Assembly, bool> s_processedAssemblies = new ConcurrentDictionary<Assembly, bool> ();
    static readonly Lazy<ILoggerFactory> s_loggerFactory = new Lazy<ILoggerFactory> (CreateLoggerFactory);

    static ILoggerFactory CreateLoggerFactory ()
    {
      var loggerFactory = new Microsoft.Extensions.Logging.LoggerFactory ();
      loggerFactory.AddProvider (new Lemoine.Core.Extensions.Logging.LoggerProvider ());
      return loggerFactory;
    }

    /// <summary>
    /// Set the logger factory of the assembly of a module, if it exposes one and if it was not
    /// set already
    /// </summary>
    /// <param name="module">nullable</param>
    public static void SetLoggerFactory (object module)
    {
      if (module is null) {
        return;
      }

      var assembly = module.GetType ().Assembly;
      if (!s_processedAssemblies.TryAdd (assembly, true)) {
        return;
      }

      try {
        var property = GetLoggerFactoryProperty (assembly);
        if (property is null) {
          if (log.IsDebugEnabled) {
            log.Debug ($"SetLoggerFactory: no {LOGGER_FACTORY_PROPERTY} property in {assembly.GetName ().Name}, nothing to do");
          }
          return;
        }
        property.SetValue (null, s_loggerFactory.Value);
        log.Info ($"SetLoggerFactory: the logs of {property.DeclaringType?.FullName} are now forwarded to the acquisition");
      }
      catch (Exception ex) {
        log.Error ($"SetLoggerFactory: the logger factory of {assembly.GetName ().Name} could not be set, its logs are lost", ex);
      }
    }

    static PropertyInfo GetLoggerFactoryProperty (Assembly assembly)
    {
      foreach (var type in assembly.GetExportedTypes ()) {
        var property = type.GetProperty (LOGGER_FACTORY_PROPERTY, BindingFlags.Public | BindingFlags.Static);
        if (property is not null
          && property.CanWrite
          && typeof (ILoggerFactory).IsAssignableFrom (property.PropertyType)) {
          return property;
        }
      }
      return null;
    }
  }
}

#endif // !NET40

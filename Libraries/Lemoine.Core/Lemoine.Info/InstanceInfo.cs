// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using Lemoine.Core.Log;

namespace Lemoine.Info
{
  /// <summary>
  /// Information on the installed instance of the product.
  ///
  /// A dev instance (for example a beta) can be installed next to the normal instance.
  /// It is activated by the config key dev=true, usually set in the file instance.options
  /// of the installation directory, so that it only applies to the programs of this installation.
  /// It is the default in a development branch of the next major release (NEXT_MAJOR).
  /// The resources the two instances would share are then suffixed by -dev:
  /// <item>the registry key, which is considered first, before the normal one</item>
  /// <item>the common and local configuration directories (ProgramData, LocalAppData)</item>
  /// <item>the Windows service names</item>
  /// <item>the DSN name of the database connection, if a connection with this suffix is defined.
  /// The unit test connection never falls back to the one without suffix</item>
  /// </summary>
  public static class InstanceInfo
  {
    /// <summary>
    /// Config key to activate the dev instance
    /// </summary>
    public static readonly string DEV_KEY = "dev";

    /// <summary>
    /// Default value of the config key dev
    ///
    /// true in a development branch of the next major release (NEXT_MAJOR, see Directory.Build.props),
    /// whose programs and unit tests must not share the resources of the current release,
    /// the database first. It may be overridden by dev=false
    /// </summary>
    public static readonly bool DEV_DEFAULT =
#if NEXT_MAJOR
      true
#else
      false
#endif
      ;

    /// <summary>
    /// Suffix of the resources of a dev instance
    /// </summary>
    public static readonly string DEV_SUFFIX = "-dev";

    /// <summary>
    /// Name of the options file, in the directory of the program, that is common to all the programs of an installation
    /// </summary>
    public static readonly string INSTANCE_OPTIONS_FILE_NAME = "instance.options";

    static readonly ILog log = LogManager.GetLogger (typeof (InstanceInfo).FullName);

    /// <summary>
    /// Is the program part of a dev instance?
    /// </summary>
    public static bool IsDev => Lemoine.Info.ConfigSet.LoadAndGet<bool> (DEV_KEY, DEV_DEFAULT);

    /// <summary>
    /// Suffix of the resources of this instance: -dev for a dev instance, else an empty string
    /// </summary>
    public static string Suffix => IsDev ? DEV_SUFFIX : "";

    /// <summary>
    /// Path of the options file that is common to all the programs of the installation directory
    ///
    /// null if the directory of the program is not known
    /// </summary>
    public static string InstanceOptionsPath
    {
      get {
        var directory = ProgramInfo.AbsoluteDirectory;
        if (directory is null) {
          log.Error ($"InstanceOptionsPath: the program directory is not known");
          return null;
        }
        return Path.Combine (directory, INSTANCE_OPTIONS_FILE_NAME);
      }
    }

    /// <summary>
    /// Name of a Windows service of the product in this instance
    /// </summary>
    /// <param name="baseServiceName">Service name in the normal instance, not null or empty</param>
    /// <returns></returns>
    public static string GetServiceName (string baseServiceName)
    {
      if (string.IsNullOrEmpty (baseServiceName)) {
        log.Fatal ($"GetServiceName: empty service name");
        throw new ArgumentNullException (nameof (baseServiceName));
      }
      return baseServiceName + Suffix;
    }
  }
}

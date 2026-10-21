// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.Info
{

  /// <summary>
  /// Define some constant values
  /// </summary>
  public static class Constants
  {
    /// <summary>
    /// Default database name
    /// </summary>
    public static string DEFAULT_DATABASE_NAME =>
#if ATSORA
      "atsora"
#elif LEMOINE
      "LemoineGDB"
#else
      "Pomamo"
#endif
      ;

    /// <summary>
    /// Default database name for the unit tests,
    /// used when the DbConnection.{DEFAULT_DSN_UNIT_TEST_NAME} config key is not set
    /// or does not specify any database.
    ///
    /// It is suffixed by -dev in a dev instance (see <see cref="InstanceInfo"/>),
    /// for example in a development branch of the next major release
    /// </summary>
    public static string DEFAULT_DATABASE_UNIT_TEST_NAME =>
#if ATSORA
      "atsoraunittests"
#else
      "pomamounittests"
#endif
      ;

    /// <summary>
    /// DSN name of the unit tests.
    ///
    /// The connection parameters are read in the DbConnection.UnitTests config key,
    /// or DbConnection.UnitTests-dev in a dev instance (see <see cref="InstanceInfo"/>)
    /// </summary>
    public static string DEFAULT_DSN_UNIT_TEST_NAME => "UnitTests";

    /// <summary>
    /// Default database user
    /// </summary>
    public static string DEFAULT_DATABASE_USER =>
#if ATSORA
      "atsoraapp"
#elif LEMOINE
      "LemoineUser"
#else
      "pomamoapp"
#endif
      ;

    /// <summary>
    /// Default database password
    /// </summary>
    public static string DEFAULT_DATABASE_PASSWORD =>
#if ATSORA
      "DatabasePassword"
#elif LEMOINE
      "DatabasePassword"
#else
      "DatabasePassword"
#endif
      ;

    /// <summary>
    /// Default DSN Name
    /// </summary>
    public static string DEFAULT_DSN_NAME =>
#if ATSORA
      "Atsora"
#elif LEMOINE
      "LemoineGDB"
#else
      "Pomamo"
#endif
      ;
  }
}

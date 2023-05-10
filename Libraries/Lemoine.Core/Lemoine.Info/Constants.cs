/* -*- c# -*- ******************************************************************
 * Copyright (c) 2023 Nicolas Relange. All Rights Reserved.
 */

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
    /// Default database name for the unit tests
    /// </summary>
    public static string DEFAULT_DATABASE_UNIT_TEST_NAME => "LemoineUnitTests";

    /// <summary>
    /// Default database user
    /// </summary>
    public static string DEFAULT_DATABASE_USER =>
#if ATSORA
      "AtsoraApp"
#elif LEMOINE
      "LemoineUser"
#else
      "PomamoApp"
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

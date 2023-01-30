// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 141: add a method to convert a UTC date/time to a day
  /// 
  /// Deprecated since sfkcfgs does not exist any more
  /// ConvertUtcToDay is not used any more also
  /// </summary>
  [Migration(141)]
  public class AddConvertUtcToDay: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddConvertUtcToDay).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"DROP FUNCTION IF EXISTS ConvertUtcToDay (datetime timestamp without time zone) RESTRICT");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DROP FUNCTION IF EXISTS ConvertUtcToDay (datetime timestamp without time zone) RESTRICT");
    }
  }
}

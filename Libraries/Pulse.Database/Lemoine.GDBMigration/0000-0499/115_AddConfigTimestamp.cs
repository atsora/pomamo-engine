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
  /// Migration 115: Add a timestamp column to the config table
  /// </summary>
  [Migration(115)]
  public class AddConfigTimestamp: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddConfigTimestamp).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.CONFIG,
                          new Column (TableName.CONFIG + "timestamp", DbType.DateTime, ColumnProperty.NotNull, "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"));
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION config_timestamp_update () RETURNS TRIGGER AS $$
BEGIN
  NEW.configtimestamp=CURRENT_TIMESTAMP AT TIME ZONE 'GMT';
  RETURN NEW;
END
$$ LANGUAGE plpgsql");
      Database.ExecuteNonQuery (@"CREATE TRIGGER config_insert_update BEFORE INSERT OR UPDATE 
ON config
FOR EACH ROW 
EXECUTE PROCEDURE config_timestamp_update ();");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.CONFIG,
                             TableName.CONFIG + "timestamp");
      Database.ExecuteNonQuery ("DROP FUNCTION IF EXISTS config_timestamp_update () CASCADE");
    }
  }
}

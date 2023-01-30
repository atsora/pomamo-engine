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
  /// Migration 212:
  /// </summary>
  [Migration(212)]
  public class AddNamedConfigTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddNamedConfigTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.NAMED_CONFIG,
                         new Column (TableName.NAMED_CONFIG + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.NAMED_CONFIG + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.NAMED_CONFIG + "timestamp", DbType.DateTime, ColumnProperty.NotNull, "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"),
                         new Column ("namedconfigname", DbType.String, ColumnProperty.NotNull),
                         new Column ("namedconfigkey", DbType.String, ColumnProperty.NotNull),
                         new Column ("namedconfigvalue", DbType.String, ColumnProperty.NotNull));
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION namedconfig_timestamp_update () RETURNS TRIGGER AS $$
BEGIN
  NEW.namedconfigtimestamp=CURRENT_TIMESTAMP AT TIME ZONE 'GMT';
  RETURN NEW;
END
$$ LANGUAGE plpgsql");
      Database.ExecuteNonQuery (@"CREATE TRIGGER namedconfig_insert_update BEFORE INSERT OR UPDATE
ON namedconfig
FOR EACH ROW
EXECUTE PROCEDURE namedconfig_timestamp_update ();");
      AddUniqueIndex (TableName.NAMED_CONFIG,
                      "namedconfigname", "namedconfigkey");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.NAMED_CONFIG);
    }
  }
}

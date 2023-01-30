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
  /// Migration 145: add a constraint on the color column of machine mode and make it not null
  /// </summary>
  [Migration(145)]
  public class MachineModeColorConstraint: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineModeColorConstraint).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"UPDATE machinemode 
SET machinemodecolor='#808080'
WHERE machinemodecolor IS NULL");
      // Warning: ChangeColumn changes the order in which the colunns are ordered
      // Migration 147 fixes this
      // Do not use the code below then:
/*    Database.ChangeColumn (TableName.MACHINE_MODE,
                             new Column (TableName.MACHINE_MODE + "color", DbType.String, 7, ColumnProperty.NotNull, "'#808080'"));*/
      // And use this instead:
      Database.ExecuteNonQuery (@"ALTER TABLE machinemode ALTER COLUMN machinemodecolor SET NOT NULL;");
      Database.ExecuteNonQuery (@"ALTER TABLE machinemode ALTER COLUMN machinemodecolor SET DEFAULT '#808080'::character varying;
");
      AddConstraintColor (TableName.MACHINE_MODE,
                          TableName.MACHINE_MODE + "color");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveConstraintColor (TableName.MACHINE_MODE,
                             TableName.MACHINE_MODE + "color");
      Database.ExecuteNonQuery (@"ALTER TABLE machinemode ALTER COLUMN machinemodecolor DROP NOT NULL;");
      Database.ExecuteNonQuery (@"ALTER TABLE machinemode ALTER COLUMN machinemodecolor DROP DEFAULT;");
    }
  }
}

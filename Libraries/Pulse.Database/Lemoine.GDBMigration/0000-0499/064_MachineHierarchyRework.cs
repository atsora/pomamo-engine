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
  /// Migration 064: Machine Hierarchy Rework
  /// 
  /// Add the following tables:
  /// <item>Department</item>
  /// <item>Company</item>
  /// <item>Category</item>
  /// <item>Sub-Category</item>
  /// </summary>
  [Migration(64)]
  public class MachineHierarchyRework: MachineHierarchyMigration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineHierarchyRework).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddTable (TableName.DEPARTMENT);
      AddTable (TableName.COMPANY);
      AddTable (TableName.MACHINE_CATEGORY);
      AddTable (TableName.MACHINE_SUB_CATEGORY);
      AddShopFloorDisplay ();
      
      Database.ExecuteNonQuery (@"ALTER TABLE machine 
ALTER COLUMN machinedisplaypriority
DROP NOT NULL");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"ALTER TABLE machine 
ALTER COLUMN machinedisplaypriority
SET NOT NULL");
      
      Remove (TableName.SHOP_FLOOR_DISPLAY);
      Remove (TableName.MACHINE_SUB_CATEGORY);
      Remove (TableName.MACHINE_CATEGORY);
      Remove (TableName.COMPANY);
      Remove (TableName.DEPARTMENT);
    }
    
    void AddTable (string tableName)
    {
      AddMachineHierarchyTable (tableName);
    }
    
    void Remove (string tableName)
    {
      RemoveMachineHierarchyTable (tableName);
    }
    
    void AddShopFloorDisplay ()
    {
      string tableName = TableName.SHOP_FLOOR_DISPLAY;
      Database.AddTable (tableName,
                         new Column (tableName + "Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (tableName + "Version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (tableName + "Name", DbType.String),
                         new Column (tableName + "Code", DbType.String));
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {0}name
SET DATA TYPE CITEXT;",
                                               tableName));
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {0}code
SET DATA TYPE CITEXT;",
                                               tableName));
      Database.AddCheckConstraint (tableName + "_name_code",
                                   tableName,
                                   string.Format ("(({0}name IS NOT NULL) OR ({0}code IS NOT NULL))",
                                                  tableName));
      AddUniqueIndexCondition (tableName,
                               tableName + "name IS NOT NULL",
                               tableName + "name");
      AddUniqueIndexCondition (tableName,
                               tableName + "code IS NOT NULL",
                               tableName + "code");
      
      // Add the foreign key to machine
      Database.AddColumn (TableName.MACHINE,
                          tableName + "Id", DbType.Int32);
      Database.GenerateForeignKey (TableName.MACHINE, tableName + "Id",
                                   tableName, tableName + "Id",
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddIndex (TableName.MACHINE,
                tableName + "Id");
    }
    
  }
}

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
  /// Base migration class in case some machine hierarchy tables must be added
  /// </summary>
  public abstract class MachineHierarchyMigration: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineHierarchyMigration).FullName);
    
    /// <summary>
    /// Generic method to add a machine hierarchy level
    /// </summary>
    /// <param name="tableName"></param>
    protected void AddMachineHierarchyTable (string tableName)
    {
      Database.AddTable (tableName,
                         new Column (tableName + "Id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (tableName + "Version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (tableName + "Name", DbType.String),
                         new Column (tableName + "Code", DbType.String),
                         new Column (tableName + "ExternalCode", DbType.String),
                         new Column (tableName + "DisplayPriority", DbType.Int32));
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
      AddUniqueIndexCondition (tableName,
                               tableName + "externalcode IS NOT NULL",
                               tableName + "externalcode");
      AddIndex (tableName,
                tableName + "displaypriority");
      
      // Add the foreign key to machine
      Database.AddColumn (TableName.MACHINE,
                          tableName + "Id", DbType.Int32);
      Database.GenerateForeignKey (TableName.MACHINE, tableName + "Id",
                                   tableName, tableName + "Id",
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddIndex (TableName.MACHINE,
                tableName + "Id");
    }
    
    /// <summary>
    /// Generic method to remove a machine hierarchy table
    /// </summary>
    /// <param name="tableName"></param>
    protected void RemoveMachineHierarchyTable (string tableName)
    {
      Database.RemoveColumn (TableName.MACHINE,
                             tableName + "Id");
      Database.Delete (TableName.DISPLAY,
                       "displaytable",
                       tableName);
      Database.RemoveTable (tableName);
    }

  }
}

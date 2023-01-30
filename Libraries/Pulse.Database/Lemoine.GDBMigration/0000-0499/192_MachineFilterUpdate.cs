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
  /// Migration 192: Update the machine filter table with dependent tables
  /// </summary>
  [Migration(192)]
  public class MachineFilterUpdate: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineFilterUpdate).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddSequence (SequenceName.MACHINE_FILTER_ITEM_ID);
      AddMachineFilterSubTable (TableName.COMPANY, ColumnName.COMPANY_ID);
      AddMachineFilterSubTable (TableName.DEPARTMENT, ColumnName.DEPARTMENT_ID);
      AddMachineFilterSubTable (TableName.MACHINE_CATEGORY, ColumnName.MACHINE_CATEGORY_ID);
      AddMachineFilterSubTable (TableName.MACHINE_SUB_CATEGORY, ColumnName.MACHINE_SUB_CATEGORY_ID);
      AddMachineFilterSubTable (TableName.CELL, ColumnName.CELL_ID);
      AddMachineFilterSubTable (TableName.MACHINE, ColumnName.MACHINE_ID);
      UpdateMachineFilter ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveMachineFilterSubTable (TableName.COMPANY, ColumnName.COMPANY_ID);
      RemoveMachineFilterSubTable (TableName.DEPARTMENT , ColumnName.DEPARTMENT_ID);
      RemoveMachineFilterSubTable (TableName.MACHINE_CATEGORY , ColumnName.MACHINE_CATEGORY_ID);
      RemoveMachineFilterSubTable (TableName.MACHINE_SUB_CATEGORY , ColumnName.MACHINE_SUB_CATEGORY_ID);
      RemoveMachineFilterSubTable (TableName.CELL, ColumnName.CELL_ID);
      RemoveMachineFilterSubTable (TableName.MACHINE, ColumnName.MACHINE_ID);
      
      Database.RemoveColumn (TableName.MACHINE_FILTER,
                             TableName.MACHINE_FILTER + "InitialSet");
      
      RemoveSequence (SequenceName.MACHINE_FILTER_ITEM_ID);
    }
    
    void UpdateMachineFilter ()
    {
      Database.AddColumn (TableName.MACHINE_FILTER,
                          new Column (TableName.MACHINE_FILTER + "InitialSet", DbType.Int32, ColumnProperty.NotNull, 1));
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {0}InitialSet DROP DEFAULT",
                                               TableName.MACHINE_FILTER));
      Database.RemoveColumn (TableName.MACHINE_FILTER, ColumnName.COMPANY_ID);
      Database.RemoveColumn (TableName.MACHINE_FILTER, ColumnName.DEPARTMENT_ID);
      Database.RemoveColumn (TableName.MACHINE_FILTER, ColumnName.MACHINE_CATEGORY_ID);
      Database.RemoveColumn (TableName.MACHINE_FILTER, ColumnName.MACHINE_SUB_CATEGORY_ID);
      Database.RemoveColumn (TableName.MACHINE_FILTER, ColumnName.CELL_ID);
      Database.RemoveColumn (TableName.MACHINE_FILTER, ColumnName.MACHINE_ID);
    }
    
    void AddMachineFilterSubTable (string tableName, string idColumnName)
    {
      Database.AddTable (TableName.MACHINE_FILTER + tableName,
                         new Column (ColumnName.MACHINE_FILTER_ITEM_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_FILTER_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_FILTER_ITEM_ORDER, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_FILTER_ITEM_RULE, DbType.Int32, ColumnProperty.NotNull),
                         new Column (idColumnName, DbType.Int32, ColumnProperty.NotNull));
      SetSequence (TableName.MACHINE_FILTER + tableName,
                   ColumnName.MACHINE_FILTER_ITEM_ID,
                   SequenceName.MACHINE_FILTER_ITEM_ID);
      Database.GenerateForeignKey (TableName.MACHINE_FILTER + tableName, ColumnName.MACHINE_FILTER_ID,
                                   TableName.MACHINE_FILTER, ColumnName.MACHINE_FILTER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_FILTER + tableName, idColumnName,
                                   tableName, idColumnName,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.MACHINE_FILTER + tableName, ColumnName.MACHINE_FILTER_ID);
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}{1} ({2}, {3}, {4}, {5})
SELECT {2}, 0, 1, {5}
FROM {0}
WHERE {5} IS NOT NULL",
                                               TableName.MACHINE_FILTER, tableName,
                                               ColumnName.MACHINE_FILTER_ID,
                                               ColumnName.MACHINE_FILTER_ITEM_ORDER,
                                               ColumnName.MACHINE_FILTER_ITEM_RULE,
                                               idColumnName));
      // TODO: update the order:  probably not necessary because the machine filters are not really used
      AddUniqueConstraint (TableName.MACHINE_FILTER + tableName,
                           ColumnName.MACHINE_FILTER_ITEM_ID, ColumnName.MACHINE_FILTER_ITEM_ORDER);
    }
    
    void RemoveMachineFilterSubTable (string tableName, string idColumnName)
    {
      Database.AddColumn (TableName.MACHINE_FILTER,
                          new Column (idColumnName, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.MACHINE_FILTER, idColumnName,
                                   tableName, idColumnName,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);

      // Data
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0}
SET {2}=(SELECT {2}
FROM {0}{1}
WHERE {0}{1}.{2}={0}.{2} 
  AND {3}=1 LIMIT 1)",
                                               TableName.MACHINE_FILTER,
                                               tableName, idColumnName,
                                               ColumnName.MACHINE_FILTER_ITEM_RULE));

      Database.RemoveTable (TableName.MACHINE_FILTER + tableName);
    }
  }
}

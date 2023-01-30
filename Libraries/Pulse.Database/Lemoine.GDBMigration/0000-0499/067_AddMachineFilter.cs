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
  /// Migration 067:
  /// <item>Add a new table machinefilter</item>
  /// <item>Add two columns include and exclude filters to machinemodedefaultreason table</item>
  /// </summary>
  [Migration(67)]
  public class AddMachineFilter: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineFilter).FullName);
    
    static readonly string MACHINE_FILTER_VERSION = "machinefilterversion";
    static readonly string MACHINE_FILTER_NAME = "machinefiltername";
    
    static readonly string INCLUDE_COLUMN = "includemachinefilterid";
    static readonly string EXCLUDE_COLUMN = "excludemachinefilterid";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddMachineFilterTable ();
      AddFiltersInMachineModeDefaultReasonTable ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveFiltersInMachineModeDefaultReasonTable ();
      RemoveMachineFilterTable ();
    }

    void AddMachineFilterTable ()
    {
      Database.AddTable (TableName.MACHINE_FILTER,
                         new Column (ColumnName.MACHINE_FILTER_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (MACHINE_FILTER_VERSION, DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (MACHINE_FILTER_NAME, DbType.String, ColumnProperty.NotNull),
                         new Column (ColumnName.COMPANY_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (ColumnName.DEPARTMENT_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (ColumnName.MACHINE_CATEGORY_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (ColumnName.MACHINE_SUB_CATEGORY_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.Null));
      // Unique contraints
      AddUniqueConstraint (TableName.MACHINE_FILTER,
                           MACHINE_FILTER_NAME);
      // Foreign keys
      Database.GenerateForeignKey (TableName.MACHINE_FILTER, ColumnName.COMPANY_ID,
                                   TableName.COMPANY, ColumnName.COMPANY_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_FILTER, ColumnName.DEPARTMENT_ID,
                                   TableName.DEPARTMENT, ColumnName.DEPARTMENT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_FILTER, ColumnName.MACHINE_CATEGORY_ID,
                                   TableName.MACHINE_CATEGORY, ColumnName.MACHINE_CATEGORY_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_FILTER, ColumnName.MACHINE_SUB_CATEGORY_ID,
                                   TableName.MACHINE_SUB_CATEGORY, ColumnName.MACHINE_SUB_CATEGORY_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_FILTER, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      // Indexes
      AddIndex (TableName.MACHINE_FILTER, MACHINE_FILTER_NAME);
    }
    
    void RemoveMachineFilterTable ()
    {
      Database.RemoveTable (TableName.MACHINE_FILTER);
    }
    
    void AddFiltersInMachineModeDefaultReasonTable ()
    {
      Database.AddColumn (TableName.MACHINE_MODE_DEFAULT_REASON,
                          new Column (INCLUDE_COLUMN, DbType.Int32, ColumnProperty.Null));
      Database.AddColumn (TableName.MACHINE_MODE_DEFAULT_REASON,
                          new Column (EXCLUDE_COLUMN, DbType.Int32, ColumnProperty.Null));
      Database.AddForeignKey (string.Format ("fk_{0}_includemachinefilter",
                                             TableName.MACHINE_MODE_DEFAULT_REASON),
                              TableName.MACHINE_MODE_DEFAULT_REASON, INCLUDE_COLUMN,
                              TableName.MACHINE_FILTER, ColumnName.MACHINE_FILTER_ID,
                              Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.AddForeignKey (string.Format ("fk_{0}_excludemachinefilter",
                                             TableName.MACHINE_MODE_DEFAULT_REASON),
                              TableName.MACHINE_MODE_DEFAULT_REASON, EXCLUDE_COLUMN,
                              TableName.MACHINE_FILTER, ColumnName.MACHINE_FILTER_ID,
                              Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    void RemoveFiltersInMachineModeDefaultReasonTable ()
    {
      Database.RemoveColumn (TableName.MACHINE_MODE_DEFAULT_REASON,
                             INCLUDE_COLUMN);
      Database.RemoveColumn (TableName.MACHINE_MODE_DEFAULT_REASON,
                             EXCLUDE_COLUMN);
    }
  }
}

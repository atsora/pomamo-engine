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
  /// Migration 197: Add the nonconformancereport table
  /// </summary>
  [Migration(197)]
  public class AddNonConformanceReportTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddNonConformanceReportTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.NON_CONFORMANCE_REPORT)) {
        AddTable ();
      }      
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.NON_CONFORMANCE_REPORT)) {
        RemoveTable ();
      }
    }
    
    /// <summary>
    /// Add the new nonconformancereport table
    /// </summary>
    private void AddTable() 
    {
      Database.AddTable(TableName.NON_CONFORMANCE_REPORT,
                        new Column(ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                        new Column(ColumnName.DELIVERABLE_PIECE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(ColumnName.INTERMEDIATE_WORK_PIECE_ID, DbType.Int32),
                        new Column(ColumnName.NON_CONFORMANCE_REASON_ID, DbType.Int32),                        
                        new Column(ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),                        
                        new Column("nonconformancefixable", DbType.Boolean),
                        new Column("nonconformanceoperationdatetime", DbType.DateTime));
      
      Database.GenerateForeignKey(TableName.NON_CONFORMANCE_REPORT, ColumnName.MODIFICATION_ID, 
                                  TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      Database.GenerateForeignKey(TableName.NON_CONFORMANCE_REPORT, ColumnName.DELIVERABLE_PIECE_ID, 
                                  TableName.DELIVERABLE_PIECE, ColumnName.DELIVERABLE_PIECE_ID, 
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      Database.GenerateForeignKey(TableName.NON_CONFORMANCE_REPORT, ColumnName.INTERMEDIATE_WORK_PIECE_ID, 
                                  TableName.INTERMEDIATE_WORK_PIECE, ColumnName.INTERMEDIATE_WORK_PIECE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.SetNull);
        
      Database.GenerateForeignKey(TableName.NON_CONFORMANCE_REPORT, ColumnName.NON_CONFORMANCE_REASON_ID, 
                                  TableName.NON_CONFORMANCE_REASON, ColumnName.NON_CONFORMANCE_REASON_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      Database.GenerateForeignKey(TableName.NON_CONFORMANCE_REPORT, ColumnName.MACHINE_ID,
                                  TableName.MACHINE, ColumnName.MACHINE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      SetModificationTable (TableName.NON_CONFORMANCE_REPORT);
    }


    /// <summary>
    /// Add the nonconformancereport table
    /// </summary>
    private void RemoveTable() 
    {
      RemoveModificationTable(TableName.NON_CONFORMANCE_REPORT);
      Database.RemoveTable (TableName.NON_CONFORMANCE_REPORT);      
    }    
    
  }
}

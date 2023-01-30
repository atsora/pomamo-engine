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
  /// Migration 196: Add the nonconformancereason table
  /// </summary>
  [Migration(196)]
  public class AddNonConformanceReasonTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddNonConformanceReasonTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.NON_CONFORMANCE_REASON)) {
        AddTable ();
      }      
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.NON_CONFORMANCE_REASON)) {
        RemoveTable ();
      }
    }
    
    /// <summary>
    /// Add the new nonconformancereason table
    /// </summary>
    private void AddTable() 
    {
      Database.AddTable(TableName.NON_CONFORMANCE_REASON,
                        new Column(ColumnName.NON_CONFORMANCE_REASON_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(TableName.NON_CONFORMANCE_REASON + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(TableName.NON_CONFORMANCE_REASON + "code", DbType.String),
                        new Column(TableName.NON_CONFORMANCE_REASON + "name", DbType.String, ColumnProperty.NotNull),
                        new Column(TableName.NON_CONFORMANCE_REASON + "description", DbType.String) );

      MakeColumnCaseInsensitive(TableName.NON_CONFORMANCE_REASON, TableName.NON_CONFORMANCE_REASON + "code");
      MakeColumnCaseInsensitive(TableName.NON_CONFORMANCE_REASON, TableName.NON_CONFORMANCE_REASON + "name");

      AddUniqueIndex(TableName.NON_CONFORMANCE_REASON, TableName.NON_CONFORMANCE_REASON + "name");

      AddUniqueIndex(TableName.NON_CONFORMANCE_REASON, TableName.NON_CONFORMANCE_REASON + "code");
    }


    /// <summary>
    /// Add the nonconformancereason table
    /// </summary>
    private void RemoveTable() 
    {
      Database.RemoveTable (TableName.NON_CONFORMANCE_REASON);      
    }
    
    
  }
}

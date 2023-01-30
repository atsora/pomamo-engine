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
  /// Migration 121: add the new SequenceSlot table
  /// </summary>
  [Migration(121)]
  public class AddSequenceSlotTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddSequenceSlotTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.SEQUENCE_SLOT,
                         new Column (TableName.SEQUENCE_SLOT + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.SEQUENCE_SLOT + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.SEQUENCE_SLOT + "begin", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (TableName.SEQUENCE_SLOT + "end", DbType.DateTime, ColumnProperty.Null),
                         new Column (ColumnName.SEQUENCE_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (TableName.SEQUENCE_SLOT + "analysisstatusid", DbType.Int32, ColumnProperty.NotNull, 0),
                         new Column (TableName.SEQUENCE_SLOT + "analysisdatetime", DbType.DateTime, ColumnProperty.Null));
      
      Database.GenerateForeignKey (TableName.SEQUENCE_SLOT, ColumnName.MACHINE_MODULE_ID, 
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID, 
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.SEQUENCE_SLOT, ColumnName.SEQUENCE_ID, 
                                   TableName.SEQUENCE, ColumnName.SEQUENCE_ID, 
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.AddCheckConstraint ("sequenceslot_analysisstatusids", TableName.SEQUENCE_SLOT, "sequenceslotanalysisstatusid IN (0, 1, 2)");
      
      AddUniqueConstraint (TableName.SEQUENCE_SLOT, ColumnName.MACHINE_MODULE_ID, TableName.SEQUENCE_SLOT + "begin"); // Also used as an index
      AddUniqueConstraint (TableName.SEQUENCE_SLOT, ColumnName.MACHINE_MODULE_ID, TableName.SEQUENCE_SLOT + "end"); // Also used as an index
      AddIndexCondition (TableName.SEQUENCE_SLOT, "sequenceslotanalysisstatusid IN (0, 1)", TableName.SEQUENCE_SLOT + "analysisstatusid");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.SEQUENCE_SLOT);
    }
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 629: 
  /// </summary>
  [Migration (629)]
  public class AddReasonProposal : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddReasonProposal).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.REASON_PROPOSAL,
        new Column (TableName.REASON_PROPOSAL + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
        new Column (TableName.REASON_PROPOSAL + "version", DbType.Int32, ColumnProperty.NotNull, 1),
        new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
        new Column (ColumnName.MODIFICATION_ID, DbType.Int64, ColumnProperty.NotNull),
        new Column (TableName.REASON_PROPOSAL + "datetimerange", DbType.Int32, ColumnProperty.NotNull),
        new Column (ColumnName.REASON_ID, DbType.Int32, ColumnProperty.NotNull),
        new Column (TableName.REASON_PROPOSAL + "score", DbType.Double, ColumnProperty.NotNull),
        new Column (TableName.REASON_PROPOSAL + "kind", DbType.Int32, ColumnProperty.NotNull),
        new Column (TableName.REASON_PROPOSAL + "details", DbType.String));
      MakeColumnTsRange (TableName.REASON_PROPOSAL, TableName.REASON_PROPOSAL + "datetimerange");

      // constraints / foreign keys
      AddConstraintRangeNotEmpty (TableName.REASON_PROPOSAL, TableName.REASON_PROPOSAL + "datetimerange");
      Database.GenerateForeignKey (TableName.REASON_PROPOSAL, ColumnName.MACHINE_ID,
        TableName.MACHINE, ColumnName.MACHINE_ID,
        Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.REASON_PROPOSAL, ColumnName.MODIFICATION_ID,
        TableName.REASON_MACHINE_ASSOCIATION, ColumnName.MODIFICATION_ID,
        Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.REASON_PROPOSAL, ColumnName.REASON_ID,
        TableName.REASON, ColumnName.REASON_ID,
        Migrator.Framework.ForeignKeyConstraint.Cascade);

      // indexes
      AddGistIndex (TableName.REASON_PROPOSAL,
                    ColumnName.MACHINE_ID,
                    TableName.REASON_PROPOSAL + "datetimerange");
      AddUniqueConstraint (TableName.REASON_PROPOSAL,
        ColumnName.MACHINE_ID,
        ColumnName.MODIFICATION_ID);
      AddNoOverlapConstraintCondition (TableName.REASON_PROPOSAL,
        TableName.REASON_PROPOSAL + "kind = 4", // manual
        TableName.REASON_PROPOSAL + "datetimerange",
        ColumnName.MACHINE_ID);

      // partitioning
      PartitionTable (TableName.REASON_PROPOSAL, TableName.MACHINE);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      UnpartitionTable (TableName.REASON_PROPOSAL);
      Database.RemoveTable (TableName.REASON_PROPOSAL);
    }
  }
}

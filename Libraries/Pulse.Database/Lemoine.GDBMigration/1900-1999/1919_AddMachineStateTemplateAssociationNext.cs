// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration to add the nextmachinestatetemplateid column to the machinestatetemplateassociation table
  /// </summary>
  [Migration (1919)]
  public class AddMachineStateTemplateAssociationNext : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddMachineStateTemplateAssociationNext).FullName);

    static readonly string NEXT_MACHINE_STATE_TEMPLATE_ID = $"next{ColumnName.MACHINE_STATE_TEMPLATE_ID}";

    /// <summary>
    /// Update the database
    /// </summary>
    public override void Up ()
    {
      if (Database.ColumnExists (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION, NEXT_MACHINE_STATE_TEMPLATE_ID)) {
        if (log.IsInfoEnabled) {
          log.Info ($"Up: column {NEXT_MACHINE_STATE_TEMPLATE_ID} already exists in {TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION} => do nothing");
        }
        return;
      }

      Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION,
                          new Column (NEXT_MACHINE_STATE_TEMPLATE_ID, System.Data.DbType.Int32));

      // Nullable foreign key: set it to null if the referenced machine state template is deleted
      Database.GenerateForeignKey (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION, NEXT_MACHINE_STATE_TEMPLATE_ID,
                                   TableName.MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    public override void Down ()
    {
      if (Database.ColumnExists (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION, NEXT_MACHINE_STATE_TEMPLATE_ID)) {
        Database.RemoveColumn (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION, NEXT_MACHINE_STATE_TEMPLATE_ID);
      }
    }
  }
}

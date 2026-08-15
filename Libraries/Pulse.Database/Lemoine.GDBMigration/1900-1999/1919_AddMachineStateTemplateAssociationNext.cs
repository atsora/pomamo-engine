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
    /// Name of the foreign key on the new column
    ///
    /// Note: an explicit name is required here. GenerateForeignKey would name it
    /// FK_machinestatetemplateassociation_machinestatetemplate, which is already the name of the
    /// foreign key on machinestatetemplateid: the constraint would then be silently skipped
    /// </summary>
    static readonly string NEXT_MACHINE_STATE_TEMPLATE_FK =
      $"fk_{TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION}_next{TableName.MACHINE_STATE_TEMPLATE}";

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

      // Note: on a partitioned table, the column is added to the partitions as well,
      // they inherit from the parent table
      Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION,
                          new Column (NEXT_MACHINE_STATE_TEMPLATE_ID, System.Data.DbType.Int32));

      AddNextMachineStateTemplateForeignKey ();
    }

    void AddNextMachineStateTemplateForeignKey ()
    {
      if (IsPartitioned (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION)) {
        // The foreign keys of a partitioned table are managed by pgfkpart: they are removed from
        // the table and restored when the table is unpartitioned. A foreign key that would be added
        // here on the parent table would not be applied to the partitions, where the data is
        if (log.IsInfoEnabled) {
          log.Info ($"AddNextMachineStateTemplateForeignKey: {TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION} is partitioned => no foreign key on {NEXT_MACHINE_STATE_TEMPLATE_ID}, like for the other columns of the table");
        }
      }

      // Nullable foreign key: set it to null if the referenced machine state template is deleted
      Database.AddForeignKey (NEXT_MACHINE_STATE_TEMPLATE_FK,
                              TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION, NEXT_MACHINE_STATE_TEMPLATE_ID,
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

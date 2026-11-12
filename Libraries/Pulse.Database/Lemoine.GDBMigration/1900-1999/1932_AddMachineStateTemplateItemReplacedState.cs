// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;

using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1932: add to machinestatetemplateitem a reference to a replaced machine observation state
  ///
  /// An item that references a sub machine state template may now replace one of the machine observation
  /// states the sub machine state template applies (machinestatetemplateitemreplacedid)
  /// by its own machine observation state (machineobservationstateid)
  /// </summary>
  [Migration (1932)]
  public class AddMachineStateTemplateItemReplacedState : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddMachineStateTemplateItemReplacedState).FullName);

    static readonly string SUB_MACHINE_STATE_TEMPLATE_ID = TableName.MACHINE_STATE_TEMPLATE_ITEM + "subid";
    static readonly string REPLACED_MACHINE_OBSERVATION_STATE_ID = TableName.MACHINE_STATE_TEMPLATE_ITEM + "replacedid";

    /// <summary>
    /// Name of the foreign key on the new replacedid column
    ///
    /// Note: an explicit name is required here. GenerateForeignKey would name it
    /// FK_machinestatetemplateitem_machineobservationstate, which is already the name of the foreign key
    /// on machineobservationstateid: the constraint would then be silently skipped
    /// </summary>
    static readonly string REPLACED_MACHINE_OBSERVATION_STATE_FK =
      $"fk_{TableName.MACHINE_STATE_TEMPLATE_ITEM}_replaced{TableName.MACHINE_OBSERVATION_STATE}";

    /// <summary>
    /// Name of the check constraint on the replaced machine observation state
    /// </summary>
    static readonly string REPLACED_CONSTRAINT = $"{TableName.MACHINE_STATE_TEMPLATE_ITEM}_replaced";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE_ITEM,
                          new Column (REPLACED_MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.Null));
      Database.AddForeignKey (REPLACED_MACHINE_OBSERVATION_STATE_FK,
                              TableName.MACHINE_STATE_TEMPLATE_ITEM, REPLACED_MACHINE_OBSERVATION_STATE_ID,
                              TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                              Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.MACHINE_STATE_TEMPLATE_ITEM, REPLACED_MACHINE_OBSERVATION_STATE_ID);

      // A replaced machine observation state requires both a sub machine state template
      // and a replacing machine observation state
      Database.AddCheckConstraint (REPLACED_CONSTRAINT,
                                   TableName.MACHINE_STATE_TEMPLATE_ITEM,
                                   $"""
                                   ({REPLACED_MACHINE_OBSERVATION_STATE_ID} IS NULL)
                                   OR (({SUB_MACHINE_STATE_TEMPLATE_ID} IS NOT NULL) AND ({ColumnName.MACHINE_OBSERVATION_STATE_ID} IS NOT NULL))
                                   """);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveConstraint (TableName.MACHINE_STATE_TEMPLATE_ITEM, REPLACED_CONSTRAINT);
      RemoveIndex (TableName.MACHINE_STATE_TEMPLATE_ITEM, REPLACED_MACHINE_OBSERVATION_STATE_ID);
      Database.RemoveColumn (TableName.MACHINE_STATE_TEMPLATE_ITEM, REPLACED_MACHINE_OBSERVATION_STATE_ID);
    }
  }
}

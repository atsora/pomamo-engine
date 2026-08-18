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
  /// Migration 1922: add to machinestatetemplateitem
  /// <item>a recursive reference to another machine state template</item>
  /// <item>a specific week (year + week number) with an optional frequency in weeks</item>
  /// <item>a yearly repetition flag (for the public holidays)</item>
  ///
  /// Because an item may now reference a machine state template instead of a machine observation state,
  /// the machineobservationstateid column becomes nullable, with a constraint so that one of the two
  /// columns is set
  /// </summary>
  [Migration (1922)]
  public class AddMachineStateTemplateItemRecursionAndWeek : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddMachineStateTemplateItemRecursionAndWeek).FullName);

    static readonly string SUB_MACHINE_STATE_TEMPLATE_ID = TableName.MACHINE_STATE_TEMPLATE_ITEM + "subid";
    static readonly string WEEK_YEAR = TableName.MACHINE_STATE_TEMPLATE_ITEM + "weekyear";
    static readonly string WEEK_NUMBER = TableName.MACHINE_STATE_TEMPLATE_ITEM + "weeknumber";
    static readonly string WEEK_FREQUENCY = TableName.MACHINE_STATE_TEMPLATE_ITEM + "weekfrequency";
    static readonly string YEARLY_REPEAT = TableName.MACHINE_STATE_TEMPLATE_ITEM + "yearlyrepeat";

    /// <summary>
    /// Name of the foreign key on the new subid column
    ///
    /// Note: an explicit name is required here. GenerateForeignKey would name it
    /// FK_machinestatetemplateitem_machinestatetemplate, which is already the name of the foreign key
    /// on machinestatetemplateid: the constraint would then be silently skipped
    /// </summary>
    static readonly string SUB_MACHINE_STATE_TEMPLATE_FK =
      $"fk_{TableName.MACHINE_STATE_TEMPLATE_ITEM}_sub{TableName.MACHINE_STATE_TEMPLATE}";

    /// <summary>
    /// Name of the check constraint on the week columns
    /// </summary>
    static readonly string WEEK_CONSTRAINT = $"{TableName.MACHINE_STATE_TEMPLATE_ITEM}_week";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // - Recursive reference to another machine state template
      Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE_ITEM,
                          new Column (SUB_MACHINE_STATE_TEMPLATE_ID, DbType.Int32, ColumnProperty.Null));
      Database.AddForeignKey (SUB_MACHINE_STATE_TEMPLATE_FK,
                              TableName.MACHINE_STATE_TEMPLATE_ITEM, SUB_MACHINE_STATE_TEMPLATE_ID,
                              TableName.MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                              Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.MACHINE_STATE_TEMPLATE_ITEM, SUB_MACHINE_STATE_TEMPLATE_ID);

      // An item references either a machine observation state or another machine state template
      DropNotNull (TableName.MACHINE_STATE_TEMPLATE_ITEM, ColumnName.MACHINE_OBSERVATION_STATE_ID);
      AddOneNotNullConstraint (TableName.MACHINE_STATE_TEMPLATE_ITEM,
                               ColumnName.MACHINE_OBSERVATION_STATE_ID, SUB_MACHINE_STATE_TEMPLATE_ID);

      // - Specific week and repetitions
      Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE_ITEM,
                          new Column (WEEK_YEAR, DbType.Int32, ColumnProperty.Null));
      Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE_ITEM,
                          new Column (WEEK_NUMBER, DbType.Int32, ColumnProperty.Null));
      Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE_ITEM,
                          new Column (WEEK_FREQUENCY, DbType.Int32, ColumnProperty.Null));
      Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE_ITEM,
                          new Column (YEARLY_REPEAT, DbType.Boolean, ColumnProperty.NotNull, "FALSE"));

      Database.AddCheckConstraint (WEEK_CONSTRAINT,
                                   TableName.MACHINE_STATE_TEMPLATE_ITEM,
                                   $"""
                                   (({WEEK_NUMBER} IS NULL) OR (({WEEK_NUMBER} BETWEEN 1 AND 53)))
                                   AND (({WEEK_YEAR} IS NULL) OR ({WEEK_NUMBER} IS NOT NULL))
                                   AND (({WEEK_FREQUENCY} IS NULL) OR (({WEEK_FREQUENCY} >= 1) AND ({WEEK_YEAR} IS NOT NULL)))
                                   """);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveConstraint (TableName.MACHINE_STATE_TEMPLATE_ITEM, WEEK_CONSTRAINT);
      Database.RemoveColumn (TableName.MACHINE_STATE_TEMPLATE_ITEM, YEARLY_REPEAT);
      Database.RemoveColumn (TableName.MACHINE_STATE_TEMPLATE_ITEM, WEEK_FREQUENCY);
      Database.RemoveColumn (TableName.MACHINE_STATE_TEMPLATE_ITEM, WEEK_NUMBER);
      Database.RemoveColumn (TableName.MACHINE_STATE_TEMPLATE_ITEM, WEEK_YEAR);

      RemoveConstraint (TableName.MACHINE_STATE_TEMPLATE_ITEM,
                        BuildName (TableName.MACHINE_STATE_TEMPLATE_ITEM, "constraint",
                                   ColumnName.MACHINE_OBSERVATION_STATE_ID, SUB_MACHINE_STATE_TEMPLATE_ID));
      // Note: the items that reference a machine state template must be removed first,
      // else the NOT NULL constraint can't be restored
      Database.ExecuteNonQuery ($"""
        DELETE FROM {TableName.MACHINE_STATE_TEMPLATE_ITEM}
        WHERE {ColumnName.MACHINE_OBSERVATION_STATE_ID} IS NULL
        """);
      SetNotNull (TableName.MACHINE_STATE_TEMPLATE_ITEM, ColumnName.MACHINE_OBSERVATION_STATE_ID);

      RemoveIndex (TableName.MACHINE_STATE_TEMPLATE_ITEM, SUB_MACHINE_STATE_TEMPLATE_ID);
      Database.RemoveColumn (TableName.MACHINE_STATE_TEMPLATE_ITEM, SUB_MACHINE_STATE_TEMPLATE_ID);
    }
  }
}

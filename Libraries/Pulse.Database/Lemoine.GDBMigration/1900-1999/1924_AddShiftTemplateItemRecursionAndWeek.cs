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
  /// Migration 1924: add to shifttemplateitem
  /// <item>a recursive reference to another shift template</item>
  /// <item>a specific week (year + week number) with an optional frequency in weeks</item>
  ///
  /// Because an item may now reference a shift template instead of a shift,
  /// the shiftid column becomes nullable, with a constraint so that one of the two columns is set
  /// </summary>
  [Migration (1924)]
  public class AddShiftTemplateItemRecursionAndWeek : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddShiftTemplateItemRecursionAndWeek).FullName);

    static readonly string SUB_SHIFT_TEMPLATE_ID = TableName.SHIFT_TEMPLATE_ITEM + "subid";
    static readonly string WEEK_YEAR = TableName.SHIFT_TEMPLATE_ITEM + "weekyear";
    static readonly string WEEK_NUMBER = TableName.SHIFT_TEMPLATE_ITEM + "weeknumber";
    static readonly string WEEK_FREQUENCY = TableName.SHIFT_TEMPLATE_ITEM + "weekfrequency";

    /// <summary>
    /// Name of the foreign key on the new subid column
    ///
    /// Note: an explicit name is required here. GenerateForeignKey would name it
    /// FK_shifttemplateitem_shifttemplate, which is already the name of the foreign key
    /// on shifttemplateid: the constraint would then be silently skipped
    /// </summary>
    static readonly string SUB_SHIFT_TEMPLATE_FK =
      $"fk_{TableName.SHIFT_TEMPLATE_ITEM}_sub{TableName.SHIFT_TEMPLATE}";

    /// <summary>
    /// Name of the check constraint on the week columns
    /// </summary>
    static readonly string WEEK_CONSTRAINT = $"{TableName.SHIFT_TEMPLATE_ITEM}_week";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // - Recursive reference to another shift template
      Database.AddColumn (TableName.SHIFT_TEMPLATE_ITEM,
                          new Column (SUB_SHIFT_TEMPLATE_ID, DbType.Int32, ColumnProperty.Null));
      Database.AddForeignKey (SUB_SHIFT_TEMPLATE_FK,
                              TableName.SHIFT_TEMPLATE_ITEM, SUB_SHIFT_TEMPLATE_ID,
                              TableName.SHIFT_TEMPLATE, ColumnName.SHIFT_TEMPLATE_ID,
                              Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.SHIFT_TEMPLATE_ITEM, SUB_SHIFT_TEMPLATE_ID);

      // An item references either a shift or another shift template
      DropNotNull (TableName.SHIFT_TEMPLATE_ITEM, ColumnName.SHIFT_ID);
      AddOneNotNullConstraint (TableName.SHIFT_TEMPLATE_ITEM,
                               ColumnName.SHIFT_ID, SUB_SHIFT_TEMPLATE_ID);

      // - Specific week and repetitions
      Database.AddColumn (TableName.SHIFT_TEMPLATE_ITEM,
                          new Column (WEEK_YEAR, DbType.Int32, ColumnProperty.Null));
      Database.AddColumn (TableName.SHIFT_TEMPLATE_ITEM,
                          new Column (WEEK_NUMBER, DbType.Int32, ColumnProperty.Null));
      Database.AddColumn (TableName.SHIFT_TEMPLATE_ITEM,
                          new Column (WEEK_FREQUENCY, DbType.Int32, ColumnProperty.Null));

      Database.AddCheckConstraint (WEEK_CONSTRAINT,
                                   TableName.SHIFT_TEMPLATE_ITEM,
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
      RemoveConstraint (TableName.SHIFT_TEMPLATE_ITEM, WEEK_CONSTRAINT);
      Database.RemoveColumn (TableName.SHIFT_TEMPLATE_ITEM, WEEK_FREQUENCY);
      Database.RemoveColumn (TableName.SHIFT_TEMPLATE_ITEM, WEEK_NUMBER);
      Database.RemoveColumn (TableName.SHIFT_TEMPLATE_ITEM, WEEK_YEAR);

      RemoveConstraint (TableName.SHIFT_TEMPLATE_ITEM,
                        BuildName (TableName.SHIFT_TEMPLATE_ITEM, "constraint",
                                   ColumnName.SHIFT_ID, SUB_SHIFT_TEMPLATE_ID));
      // Note: the items that reference a shift template must be removed first,
      // else the NOT NULL constraint can't be restored
      Database.ExecuteNonQuery ($"""
        DELETE FROM {TableName.SHIFT_TEMPLATE_ITEM}
        WHERE {ColumnName.SHIFT_ID} IS NULL
        """);
      SetNotNull (TableName.SHIFT_TEMPLATE_ITEM, ColumnName.SHIFT_ID);

      RemoveIndex (TableName.SHIFT_TEMPLATE_ITEM, SUB_SHIFT_TEMPLATE_ID);
      Database.RemoveColumn (TableName.SHIFT_TEMPLATE_ITEM, SUB_SHIFT_TEMPLATE_ID);
    }
  }
}

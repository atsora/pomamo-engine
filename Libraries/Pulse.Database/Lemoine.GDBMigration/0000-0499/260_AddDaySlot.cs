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
  /// Migration 260: add the following tables:
  /// <item>dayslot</item>
  /// <item>daytemplate</item>
  /// <item>daytemplateitem</item>
  /// <item>daytemplateslot</item>
  /// <item>daytemplatechange</item>
  /// </summary>
  [Migration (260)]
  public class AddDaySlot : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddDaySlot).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery ("CREATE EXTENSION IF NOT EXISTS btree_gist");

      AddDayTemplate ();
      AddDayTemplateSlot ();
      AddDayTemplateChange ();
      AddDaySlotTable ();

      MigrateConfiguration ();
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveDaySlot ();
      RemoveDayTemplateChange ();
      RemoveDayTemplateSlot ();
      RemoveDayTemplate ();
    }

    void AddDayTemplate ()
    {
      Database.AddTable (TableName.DAY_TEMPLATE,
                         new Column (ColumnName.DAY_TEMPLATE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.DAY_TEMPLATE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.DAY_TEMPLATE + "timestamp", DbType.DateTime, ColumnProperty.NotNull, "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"),
                         new Column (TableName.DAY_TEMPLATE + "name", DbType.String, ColumnProperty.Null));
      AddTimeStampTrigger (TableName.DAY_TEMPLATE);
      MakeColumnCaseInsensitive (TableName.DAY_TEMPLATE,
                                 TableName.DAY_TEMPLATE + "name");

      Database.AddTable (TableName.DAY_TEMPLATE_ITEM,
                         new Column (TableName.DAY_TEMPLATE_ITEM + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.DAY_TEMPLATE_ITEM + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.DAY_TEMPLATE_ITEM + "timestamp", DbType.DateTime, ColumnProperty.NotNull, "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"),
                         new Column (ColumnName.DAY_TEMPLATE_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (TableName.DAY_TEMPLATE_ITEM + "cutoff", DbType.Time, ColumnProperty.NotNull),
                         new Column (TableName.DAY_TEMPLATE_ITEM + "weekdays", DbType.Int32, ColumnProperty.NotNull, Int32.MaxValue.ToString ()));
      AddTimeStampTrigger (TableName.DAY_TEMPLATE_ITEM);
      Database.GenerateForeignKey (TableName.DAY_TEMPLATE_ITEM, ColumnName.DAY_TEMPLATE_ID,
                                   TableName.DAY_TEMPLATE, ColumnName.DAY_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.DAY_TEMPLATE_ITEM, ColumnName.DAY_TEMPLATE_ID);
    }

    void RemoveDayTemplate ()
    {
      Database.RemoveTable (TableName.DAY_TEMPLATE_ITEM);
      RemoveTimeStampTrigger (TableName.DAY_TEMPLATE_ITEM);

      Database.RemoveTable (TableName.DAY_TEMPLATE);
      RemoveTimeStampTrigger (TableName.DAY_TEMPLATE);
    }

    void AddDayTemplateSlot ()
    {
      CheckPostgresqlVersion ();

      Database.AddTable (TableName.DAY_TEMPLATE_SLOT,
                         new Column (TableName.DAY_TEMPLATE_SLOT + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.DAY_TEMPLATE_SLOT + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.DAY_TEMPLATE_SLOT + "range", DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.DAY_TEMPLATE_ID, DbType.Int32, ColumnProperty.NotNull));
      MakeColumnTsRange (TableName.DAY_TEMPLATE_SLOT, TableName.DAY_TEMPLATE_SLOT + "range");
      Database.GenerateForeignKey (TableName.DAY_TEMPLATE_SLOT, ColumnName.DAY_TEMPLATE_ID,
                                   TableName.DAY_TEMPLATE, ColumnName.DAY_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      AddIndex (TableName.DAY_TEMPLATE_SLOT,
                ColumnName.DAY_TEMPLATE_ID);
      AddGistIndex (TableName.DAY_TEMPLATE_SLOT,
                    TableName.DAY_TEMPLATE_SLOT + "range");
      AddNoOverlapConstraintV1 (TableName.DAY_TEMPLATE_SLOT,
                                TableName.DAY_TEMPLATE_SLOT + "range");
    }

    void RemoveDayTemplateSlot ()
    {
      Database.RemoveTable (TableName.DAY_TEMPLATE_SLOT);
    }

    void AddDayTemplateChange ()
    {
      Database.AddTable (TableName.DAY_TEMPLATE_CHANGE,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.DAY_TEMPLATE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.DAY_TEMPLATE_CHANGE + "begin", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (TableName.DAY_TEMPLATE_CHANGE + "end", DbType.DateTime));
      Database.GenerateForeignKey (TableName.DAY_TEMPLATE_CHANGE, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.DAY_TEMPLATE_CHANGE, ColumnName.DAY_TEMPLATE_ID,
                                   TableName.DAY_TEMPLATE, ColumnName.DAY_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      SetModificationTable (TableName.DAY_TEMPLATE_CHANGE);
    }

    void RemoveDayTemplateChange ()
    {
      RemoveModificationTable (TableName.DAY_TEMPLATE_CHANGE);
    }

    void AddDaySlotTable ()
    {
      CheckPostgresqlVersion ();

      Database.AddTable (TableName.DAY_SLOT,
                         new Column (TableName.DAY_SLOT + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.DAY_SLOT + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.DAY, DbType.Date, ColumnProperty.NotNull),
                         new Column (TableName.DAY_SLOT + "range", DbType.Int32, ColumnProperty.NotNull));
      MakeColumnTsRange (TableName.DAY_SLOT, TableName.DAY_SLOT + "range");
      AddUniqueConstraint (TableName.DAY_SLOT,
                           ColumnName.DAY);
      AddGistIndex (TableName.DAY_SLOT,
                    TableName.DAY_SLOT + "range");
      AddNoOverlapConstraintV1 (TableName.DAY_SLOT,
                                TableName.DAY_SLOT + "range");
    }

    void RemoveDaySlot ()
    {
      Database.RemoveTable (TableName.DAY_SLOT);
    }

    void MigrateConfiguration ()
    {
      Database.ExecuteNonQuery (@"INSERT INTO daytemplate (daytemplatename) VALUES ('migration')");
      Database.ExecuteNonQuery (@"INSERT INTO daytemplateitem (daytemplateid, daytemplateitemcutoff)
SELECT daytemplateid, '0:00'::time
FROM daytemplate
WHERE daytemplatename='migration'
");
      Database.ExecuteNonQuery (@"INSERT INTO daytemplateslot (daytemplateid, daytemplateslotrange)
SELECT daytemplateid, '[,)'::tsrange
FROM daytemplate
WHERE daytemplatename='migration'");
    }
  }
}

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
  /// Migration 512:
  /// </summary>
  [Migration(512)]
  public class CreateToolTables: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CreateToolTables).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up()
    {
      // Tool position table
      Database.AddTable(TableName.TOOL_POSITION,
                        new Column(TableName.TOOL_POSITION + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(TableName.TOOL_POSITION + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.TOOL_POSITION + "magazine", DbType.Int32),
                        new Column(TableName.TOOL_POSITION + "pot", DbType.Int32),
                        new Column(TableName.TOOL_POSITION + "toolnumber", DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.TOOL_POSITION + "stateid", DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.TOOL_POSITION + "toolid", DbType.String, ColumnProperty.NotNull),
                        new Column(TableName.TOOL_POSITION + "leftdatetime", DbType.DateTime),
                        new Column(TableName.TOOL_POSITION + "properties", DbType.String)
                       );
      Database.GenerateForeignKey(TableName.TOOL_POSITION, ColumnName.MACHINE_MODULE_ID,
                                  TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      MakeColumnJson(TableName.TOOL_POSITION, TableName.TOOL_POSITION + "properties");
      AddUniqueConstraint(TableName.TOOL_POSITION, ColumnName.MACHINE_MODULE_ID, TableName.TOOL_POSITION + "toolid");
      Database.ExecuteNonQuery(@"
ALTER TABLE public.toolposition OWNER TO postgres;
GRANT ALL ON TABLE public.toolposition TO postgres;
GRANT SELECT ON TABLE public.toolposition TO reportv2;");
      
      // Tool life table
      Database.AddTable(TableName.TOOL_LIFE,
                        new Column(TableName.TOOL_LIFE + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(TableName.TOOL_LIFE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(ColumnName.TOOL_POSITION_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.TOOL_LIFE + "direction", DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.TOOL_LIFE + "value", DbType.Double, ColumnProperty.NotNull),
                        new Column(TableName.TOOL_LIFE + "warning", DbType.Double),
                        new Column(TableName.TOOL_LIFE + "limit", DbType.Double),
                        new Column(ColumnName.UNIT_ID, DbType.Int32)
                       );
      Database.GenerateForeignKey(TableName.TOOL_LIFE, ColumnName.MACHINE_MODULE_ID,
                                  TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey(TableName.TOOL_LIFE, ColumnName.TOOL_POSITION_ID,
                                  TableName.TOOL_POSITION, ColumnName.TOOL_POSITION_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey(TableName.TOOL_LIFE, ColumnName.UNIT_ID,
                                  TableName.UNIT, ColumnName.UNIT_ID,
                                  Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.ExecuteNonQuery(@"
ALTER TABLE public.toollife OWNER TO postgres;
GRANT ALL ON TABLE public.toollife TO postgres;
GRANT SELECT ON TABLE public.toollife TO reportv2;");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      bool isPartitioned = IsPartitioned(TableName.TOOL_LIFE);
      
      Database.ExecuteNonQuery("DROP TABLE IF EXISTS toollife CASCADE");
      Database.ExecuteNonQuery("DROP TABLE IF EXISTS toolposition CASCADE");
      
      if (isPartitioned) {
        Database.ExecuteNonQuery("DELETE FROM pgfkpart.partition WHERE table_name LIKE 'toollife'");
        Database.ExecuteNonQuery("DELETE FROM pgfkpart.partforeignkey WHERE table_name LIKE 'toollife'");
        Database.ExecuteNonQuery("DELETE FROM pgfkpart.parentindex WHERE table_name LIKE 'toollife'");
        Database.ExecuteNonQuery("DELETE FROM pgfkpart.partition WHERE table_name LIKE 'toolposition'");
        Database.ExecuteNonQuery("DELETE FROM pgfkpart.partforeignkey WHERE table_name LIKE 'toolposition'");
        Database.ExecuteNonQuery("DELETE FROM pgfkpart.parentindex WHERE table_name LIKE 'toolposition'");
      }
    }
  }
}

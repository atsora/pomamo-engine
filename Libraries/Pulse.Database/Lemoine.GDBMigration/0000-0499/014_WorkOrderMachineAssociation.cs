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
  /// Migration 014: New tables to store the work order / machining resource association
  /// <item>Machining Resource</item>
  /// <item>Machining Resource Monitoring Type</item>
  /// <item>Work Order Machining Resource Association</item>
  /// </summary>
  [Migration (14)]
  public class WorkOrderMachineAssociation : Migration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (WorkOrderMachineAssociation).FullName);
    static readonly string TRANSLATION_TABLE = "Translation";
    static readonly string LOCALE = "Locale";
    static readonly string TRANSLATION_KEY = "TranslationKey";
    static readonly string TRANSLATION_VALUE = "TranslationValue";
    static readonly string MACHINE_MONITORING_TYPE_TABLE = "MachineMonitoringType";
    static readonly string MACHINE_MONITORING_TYPE_ID = "MachineMonitoringTypeId";
    static readonly string MACHINE_MONITORING_TYPE_TRANSLATION_KEY = "MachineMonitoringTypeTranslationKey";
    static readonly string MACHINE_ID = "MachineId";
    static readonly string WORK_ORDER_MACHINE_ASSOCIATION_TABLE = "WorkOrderMachineAssociation";
    static readonly string MODIFICATION_TABLE = "Modification";
    static readonly string MODIFICATION_ID = "ModificationId";
    static readonly string WORK_ORDER_TABLE = "WorkOrder";
    static readonly string WORK_ORDER_ID = "WorkOrderId";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // 1. New tables creation
      if (!Database.TableExists (MACHINE_MONITORING_TYPE_TABLE)) {
        Database.AddTable (MACHINE_MONITORING_TYPE_TABLE,
                           new Column (MACHINE_MONITORING_TYPE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                           new Column ("MachineMonitoringTypeName", DbType.String, ColumnProperty.Unique),
                           new Column (MACHINE_MONITORING_TYPE_TRANSLATION_KEY, DbType.String, ColumnProperty.Unique));
        Database.ExecuteNonQuery ("ALTER TABLE machinemonitoringtype " +
                                  "ALTER COLUMN machinemonitoringtypename " +
                                  "SET DATA TYPE CITEXT;");
        Database.AddCheckConstraint ("machinemonitoringtype_name_translationkey",
                                     MACHINE_MONITORING_TYPE_TABLE,
                                     "((machinemonitoringtypename IS NOT NULL) OR (machinemonitoringtypetranslationkey IS NOT NULL))");
        Database.Insert (MACHINE_MONITORING_TYPE_TABLE,
                         new string[] { MACHINE_MONITORING_TYPE_TRANSLATION_KEY },
                         new string[] { "UndefinedValue" }); // id = 1
        Database.Insert (TRANSLATION_TABLE,
                         new string[] { LOCALE, TRANSLATION_KEY, TRANSLATION_VALUE },
                         new string[] { "", "MonitoringTypeMonitored", "Monitored" });
        Database.Insert (MACHINE_MONITORING_TYPE_TABLE,
                         new string[] { MACHINE_MONITORING_TYPE_TRANSLATION_KEY },
                         new string[] { "MonitoringTypeMonitored" }); // id = 2
        Database.Insert (TRANSLATION_TABLE,
                         new string[] { LOCALE, TRANSLATION_KEY, TRANSLATION_VALUE },
                         new string[] { "", "MonitoringTypeNotMonitored", "Not monitored" });
        Database.Insert (MACHINE_MONITORING_TYPE_TABLE,
                         new string[] { MACHINE_MONITORING_TYPE_TRANSLATION_KEY },
                         new string[] { "MonitoringTypeNotMonitored" }); // id = 3
        Database.Insert (TRANSLATION_TABLE,
                         new string[] { LOCALE, TRANSLATION_KEY, TRANSLATION_VALUE },
                         new string[] { "", "MonitoringTypeOutsource", "Outsource" });
        Database.Insert (MACHINE_MONITORING_TYPE_TABLE,
                         new string[] { MACHINE_MONITORING_TYPE_TRANSLATION_KEY },
                         new string[] { "MonitoringTypeOutsource" }); // id = 4
      }
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS machine CASCADE;");
      if (!Database.TableExists (TableName.MACHINE)) {
        Database.AddTable (TableName.MACHINE,
                           new Column (MACHINE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                           new Column ("MachineName", DbType.String, ColumnProperty.Unique),
                           new Column ("MachineCode", DbType.String, ColumnProperty.Unique),
                           new Column ("MachineExternalCode", DbType.String),
                           new Column (MACHINE_MONITORING_TYPE_ID, DbType.Int32, ColumnProperty.NotNull, 1),
                           new Column ("MachineDisplayPriority", DbType.Int32, ColumnProperty.NotNull, 1));
        Database.ExecuteNonQuery ("ALTER TABLE machine " +
                                  "ALTER COLUMN machinename " +
                                  "SET DATA TYPE CITEXT;");
        Database.ExecuteNonQuery ("ALTER TABLE machine " +
                                  "ALTER COLUMN machinecode " +
                                  "SET DATA TYPE CITEXT;");
        Database.AddCheckConstraint ("machine_name_code",
                                     TableName.MACHINE,
                                     "((machinename IS NOT NULL) OR (machinecode IS NOT NULL))");
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX machine_machinename_idx " +
                                  "ON machine (machinename) " +
                                  "WHERE machinename IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX machine_machinecode_idx " +
                                  "ON machine (machinecode) " +
                                  "WHERE machinecode IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX machine_machineexternalcode_idx " +
                                  "ON machine (machineexternalcode) " +
                                  "WHERE machineexternalcode IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE INDEX machine_machinemonitoringtypeid_idx " +
                                  "ON machine (machinemonitoringtypeid);");
        Database.ExecuteNonQuery ("CREATE INDEX machine_machinedisplaypriority_idx " +
                                  "ON machine (machinedisplaypriority);");
        Database.GenerateForeignKey (TableName.MACHINE, MACHINE_MONITORING_TYPE_ID,
                                     MACHINE_MONITORING_TYPE_TABLE, MACHINE_MONITORING_TYPE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.SetDefault);
      }
      if (!Database.TableExists (WORK_ORDER_MACHINE_ASSOCIATION_TABLE)) {
        Database.AddTable (WORK_ORDER_MACHINE_ASSOCIATION_TABLE,
                           new Column (MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column (WORK_ORDER_ID, DbType.Int32),
                           new Column (MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                           new Column ("WorkOrderMachineAssociationBeginDateTime", DbType.DateTime, ColumnProperty.NotNull),
                           new Column ("WorkOrderMachineAssociationEndDateTime", DbType.DateTime));
        Database.GenerateForeignKey (WORK_ORDER_MACHINE_ASSOCIATION_TABLE, MODIFICATION_ID,
                                     MODIFICATION_TABLE, MODIFICATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (WORK_ORDER_MACHINE_ASSOCIATION_TABLE, WORK_ORDER_ID,
                                     WORK_ORDER_TABLE, WORK_ORDER_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (WORK_ORDER_MACHINE_ASSOCIATION_TABLE, MACHINE_ID,
                                     TableName.MACHINE, MACHINE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.ExecuteNonQuery (string.Format (@"CREATE RULE {0}_delete AS
ON DELETE TO {0}
DO ALSO DELETE FROM modification
  WHERE modificationid = OLD.modificationid;",
                                                WORK_ORDER_MACHINE_ASSOCIATION_TABLE));
      }
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // New tables deletion
      if (Database.TableExists (WORK_ORDER_MACHINE_ASSOCIATION_TABLE)) {
        Database.RemoveTable (WORK_ORDER_MACHINE_ASSOCIATION_TABLE);
      }
      if (Database.TableExists (TableName.MACHINE)) {
        Database.RemoveTable (TableName.MACHINE);
      }
      if (Database.TableExists (MACHINE_MONITORING_TYPE_TABLE)) {
        Database.RemoveTable (MACHINE_MONITORING_TYPE_TABLE);
      }

      // Obsolete data
      Database.Delete (TRANSLATION_TABLE,
                       TRANSLATION_KEY,
                       "MonitoringTypeMonitored");
      Database.Delete (TRANSLATION_TABLE,
                       TRANSLATION_KEY,
                       "MonitoringTypeNotMonitored");
      Database.Delete (TRANSLATION_TABLE,
                       TRANSLATION_KEY,
                       "MonitoringTypeOutsource");
    }
  }
}

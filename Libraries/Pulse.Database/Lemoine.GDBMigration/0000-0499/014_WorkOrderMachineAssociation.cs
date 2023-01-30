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

    static readonly string SFKMACH_TABLE = "sfkmach";


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
        // Remove sequence sfkmach_machid_seq
        if (Database.TableExists (TableName.SFK_MACH)) {
          Database.ExecuteNonQuery ("ALTER TABLE sfkmach ALTER COLUMN machid DROP DEFAULT;");
        }
        else {
          CreateSfkMach ();
        }
        Database.GenerateForeignKey (TableName.MACHINE, MACHINE_MONITORING_TYPE_ID,
                                     MACHINE_MONITORING_TYPE_TABLE, MACHINE_MONITORING_TYPE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.SetDefault);
        if (Database.TableExists (SFKMACH_TABLE)) {
          // Temporary trigger for sfkmach
          Database.ExecuteNonQuery ("CREATE OR REPLACE FUNCTION sfkmach_insert_updater() " +
                                    "  RETURNS trigger AS " +
                                    "$BODY$ \n" +
                                    "BEGIN \n" +
                                    "INSERT INTO machine (machineid, machinename, machinemonitoringtypeid, machinedisplaypriority) " +
                                    "VALUES (DEFAULT, NEW.machname, 2, NEW.disp_prio) \n" +
                                    "RETURNING machineid INTO NEW.machid;\n" +
                                    "RETURN NEW;\n" +
                                    "END;\n" +
                                    "$BODY$" +
                                    "  LANGUAGE plpgsql VOLATILE " +
                                    "  COST 100;");
          Database.ExecuteNonQuery ("CREATE TRIGGER sfkmach_updater " +
                                    "  BEFORE INSERT " +
                                    "  ON sfkmach " +
                                    "  FOR EACH ROW " +
                                    "  EXECUTE PROCEDURE sfkmach_insert_updater();");
          Database.ExecuteNonQuery ("ALTER TABLE sfkmach " +
                                    "ALTER COLUMN machid " +
                                    "SET DEFAULT nextval('machine_machineid_seq'::regclass)");
          Database.ExecuteNonQuery ("INSERT INTO machine " +
                                    "(machineid, machinename, machinemonitoringtypeid, machinedisplaypriority) " +
                                    "SELECT machid, machname, 2, disp_prio FROM sfkmach;");
          Database.ExecuteNonQuery ("SELECT SETVAL('machine_machineid_seq', " +
                                    "(SELECT MAX(machineid) FROM machine) + 1);");
        }
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

    void CreateSfkMach ()
    {
      if (!Database.TableExists (TableName.SFK_MACH)) {
        Database.ExecuteNonQuery (@"
CREATE TABLE public.sfkmach
        (
            machid bigint NOT NULL DEFAULT nextval('machine_machineid_seq'::regclass),
    postid bigint NOT NULL,
    machpostid bigint NOT NULL,
    guyid integer NOT NULL,
    machname character varying COLLATE pg_catalog.""default"" NOT NULL,
    machserenc bigint NOT NULL,
    machirpm bigint NOT NULL,
    machmetric integer NOT NULL,
    machobsolete integer NOT NULL,
    machlogpc character varying COLLATE pg_catalog.""default"" NOT NULL,
    sfkspvid integer NOT NULL,
    sfkmode bigint NOT NULL,
    sfkthresh double precision NOT NULL,
    mclassid double precision NOT NULL,
    firstevt timestamp without time zone NOT NULL,
    sfktype integer NOT NULL,
    sfkcost double precision NOT NULL,
    activeflag integer NOT NULL,
    disp_prio integer NOT NULL,
    manualflag integer NOT NULL,
    active_negate integer NOT NULL,
    manual_negate integer NOT NULL,
    first_feed_evt timestamp without time zone NOT NULL,
    rpm_below integer NOT NULL,
    rpm_threshold double precision NOT NULL,
    no_chupchick integer NOT NULL,
    g0g1_thresh double precision NOT NULL,
    opreset_time bigint NOT NULL,
    machtypeid bigint NOT NULL,
    tpfilter bigint NOT NULL,
    montype bigint NOT NULL,
    jitterchannelwidth double precision NOT NULL,
    spindleloadbelow integer NOT NULL,
    spindleloadthreshold double precision NOT NULL,
    cncignorenoconnect integer NOT NULL,
    rotthreshold double precision NOT NULL,
    lightactivitycheckforfull integer NOT NULL,
    spindleloadindex integer NOT NULL,
    CONSTRAINT sfkmach_pkey PRIMARY KEY (machid)
)
          ");
      }
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.SFK_MACH)) {
        // Reininitialize the old sequences
        Database.ExecuteNonQuery ("ALTER TABLE sfkmach ALTER COLUMN machid " +
                                  "SET DEFAULT nextval('sfkmach_machid_seq'::regclass);");
        Database.ExecuteNonQuery ("SELECT SETVAL('sfkmach_machid_seq', " +
                                  "(SELECT MAX(machid) FROM sfkmach) + 1);");
      }

      // New tables deletion
      if (Database.TableExists (WORK_ORDER_MACHINE_ASSOCIATION_TABLE)) {
        Database.RemoveTable (WORK_ORDER_MACHINE_ASSOCIATION_TABLE);
      }
      if (Database.TableExists (TableName.MACHINE)) {
        Database.RemoveTable (TableName.MACHINE);
      }
      Database.ExecuteNonQuery ("DROP TRIGGER IF EXISTS sfkmach_updater ON sfkmach");
      Database.ExecuteNonQuery ("DROP FUNCTION IF EXISTS sfkmach_insert_updater();");
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

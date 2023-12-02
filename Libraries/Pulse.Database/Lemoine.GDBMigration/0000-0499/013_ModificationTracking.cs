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
  /// Migration 013: New tables to track the modification on some tables:
  /// <item>User</item>
  /// <item>Service</item>
  /// <item>Updater</item>
  /// <item>Revision</item>
  /// <item>Modification</item>
  /// </summary>
  [Migration(13)]
  public class ModificationTracking: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ModificationTracking).FullName);
    static readonly string UPDATER_TABLE = "Updater";
    static readonly string UPDATER_ID = "UpdaterId";
    static readonly string USER_ID = "UserId";
    static readonly string COMPUTER_TABLE = "Computer";
    static readonly string COMPUTER_ID = "ComputerId";
    static readonly string SERVICE_TABLE = "Service";
    static readonly string SERVICE_ID = "ServiceId";
    static readonly string REVISION_TABLE = "Revision";
    static readonly string REVISION_ID = "RevisionId";
    static readonly string MODIFICATION_TABLE = "Modification";
    static readonly string MODIFICATION_ID = "ModificationId";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // 0. Add extension citext - done in 12
      //Database.ExecuteNonQuery ("CREATE EXTENSION IF NOT EXISTS citext;");
      
      // 1. New tables creation
      if (!Database.TableExists (UPDATER_TABLE)) {
        Database.AddTable (UPDATER_TABLE,
                           new Column (UPDATER_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                           new Column ("UpdaterTypeId", DbType.Int32, ColumnProperty.NotNull),
                           new Column (USER_ID, DbType.Int32, ColumnProperty.Null),
                           new Column (SERVICE_ID, DbType.Int32, ColumnProperty.Null));
        Database.AddCheckConstraint ("updater_check_ids",
                                     UPDATER_TABLE,
                                     "((updatertypeid = 1) AND (updaterid = userid) AND (serviceid IS NULL)) " +
                                     "OR ((updatertypeid = 2) AND (updaterid = serviceid) AND (userid IS NULL))");
      }
      if (!Database.TableExists (TableName.USER)) {
        Database.AddTable (TableName.USER,
                           new Column (USER_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column ("UserName", DbType.String, ColumnProperty.Unique),
                           new Column ("UserCode", DbType.String, ColumnProperty.Unique),
                           new Column ("UserExternalCode", DbType.String, ColumnProperty.Unique),
                           new Column ("UserLogin", DbType.String, ColumnProperty.NotNull | ColumnProperty.Unique),
                           new Column ("UserPassword", DbType.String, ColumnProperty.NotNull));
        Database.ExecuteNonQuery ($"ALTER TABLE {TableName.USER} " +
                                  "ALTER COLUMN UserLogin " +
                                  "SET DATA TYPE CITEXT;");
        Database.ExecuteNonQuery ($"ALTER TABLE {TableName.USER} " +
                                  "ALTER COLUMN UserCode " +
                                  "SET DATA TYPE CITEXT;");
        Database.ExecuteNonQuery ($"ALTER TABLE {TableName.USER} " +
                                  "ALTER COLUMN userid " +
                                  "SET DEFAULT nextval('updater_updaterid_seq'::regclass)");
        Database.AddCheckConstraint ("usertable_name_code",
                                     TableName.USER,
                                     "((username IS NOT NULL) OR (usercode IS NOT NULL))");
        Database.GenerateForeignKey (UPDATER_TABLE, USER_ID,
                                     TableName.USER, USER_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.ExecuteNonQuery ("CREATE OR REPLACE FUNCTION user_insert_updater() " +
                                  "  RETURNS trigger AS " +
                                  "$BODY$ \n" +
                                  "BEGIN \n" +
                                  "UPDATE updater SET updatertypeid=1, userid=NEW.userid, serviceid=NULL " +
                                  "WHERE updaterid=NEW.userid;\n" +
                                  "INSERT INTO updater SELECT NEW.userid, 1, NEW.userid, NULL " +
                                  "WHERE NOT EXISTS (SELECT 1 FROM updater WHERE updaterid=NEW.userid);\n" +
                                  "RETURN NEW;\n" +
                                  "END;\n" +
                                  "$BODY$" +
                                  "  LANGUAGE plpgsql VOLATILE " +
                                  "  COST 100;");
        Database.ExecuteNonQuery ("CREATE TRIGGER user_updater " +
                                  "  AFTER INSERT " +
                                  "  ON usertable " +
                                  "  FOR EACH ROW " +
                                  "  EXECUTE PROCEDURE user_insert_updater();");
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX user_username_idx " +
                                  $"ON {TableName.USER} (username) " +
                                  "WHERE username IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX user_usercode_idx " +
                                  $"ON {TableName.USER} (usercode) " +
                                  "WHERE usercode IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX user_userexternalcode_idx " +
                                  $"ON {TableName.USER} (userexternalcode) " +
                                  "WHERE userexternalcode IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX user_userlogin_idx " +
                                  $"ON {TableName.USER} (userlogin);");
      }
      if (!Database.TableExists (COMPUTER_TABLE)) {
        Database.AddTable (COMPUTER_TABLE,
                           new Column (COMPUTER_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                           new Column ("ComputerName", DbType.String, ColumnProperty.NotNull | ColumnProperty.Unique),
                           new Column ("ComputerAddress", DbType.String, ColumnProperty.Unique),
                           new Column ("ComputerIsLctr", DbType.Boolean, ColumnProperty.NotNull, false),
                           new Column ("ComputerIsLpst", DbType.Boolean, ColumnProperty.NotNull, false),
                           new Column ("ComputerIsCnc", DbType.Boolean, ColumnProperty.NotNull, false));
        Database.ExecuteNonQuery ("ALTER TABLE computer " +
                                  "ALTER COLUMN computername " +
                                  "SET DATA TYPE CITEXT;");
        Database.ExecuteNonQuery ("CREATE OR REPLACE FUNCTION computer_check_unique_lctr() " +
                                  "  RETURNS trigger AS " +
                                  "$BODY$ \n" +
                                  "BEGIN \n" +
                                  "IF (NEW.ComputerIsLctr=TRUE) " +
                                  "AND EXISTS (SELECT 1 FROM computer WHERE ComputerIsLctr=TRUE) THEN\n" +
                                  "  IF (TG_OP='INSERT')\n" +
                                  "    THEN RAISE EXCEPTION 'unique lctr violation'; \n" +
                                  "  ELSIF (OLD.ComputerIsLctr=FALSE)\n" +
                                  "    THEN RAISE EXCEPTION 'unique lctr violation'; \n" +
                                  "  END IF;\n" +
                                  "END IF; \n" +
                                  "RETURN NEW;\n" +
                                  "END;\n" +
                                  "$BODY$" +
                                  "  LANGUAGE plpgsql VOLATILE " +
                                  "  COST 100;");
        Database.ExecuteNonQuery ("CREATE TRIGGER computer_unique_lctr " +
                                  "  BEFORE UPDATE OR INSERT " +
                                  "  ON computer " +
                                  "  FOR EACH ROW " +
                                  "  EXECUTE PROCEDURE computer_check_unique_lctr();");
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX computer_computername_idx ON computer (computername);");
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX computer_computerislctr_idx " +
                                  "ON computer (computerislctr) " +
                                  "WHERE computerislctr=TRUE;");
        Database.ExecuteNonQuery ("CREATE INDEX computer_computerislpst_idx " +
                                  "ON computer (computerislpst) " +
                                  "WHERE computerislpst=TRUE;");
        Database.ExecuteNonQuery ("CREATE INDEX computer_computeriscnc_idx " +
                                  "ON computer (computeriscnc) " +
                                  "WHERE computeriscnc=TRUE;");
      }
      if (!Database.TableExists (SERVICE_TABLE)) {
        Database.AddTable (SERVICE_TABLE,
                           new Column (SERVICE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column ("ServiceName", DbType.String, ColumnProperty.NotNull),
                           new Column ("ServiceProgram", DbType.String, ColumnProperty.NotNull),
                           new Column ("ServiceLemoine", DbType.Boolean, ColumnProperty.NotNull, true),
                           new Column ("ComputerId", DbType.Int32, ColumnProperty.NotNull));
        Database.ExecuteNonQuery ("ALTER TABLE service " +
                                  "ALTER COLUMN serviceprogram " +
                                  "SET DATA TYPE CITEXT;");
        Database.ExecuteNonQuery ("ALTER TABLE service " +
                                  "ALTER COLUMN serviceid " +
                                  "SET DEFAULT nextval('updater_updaterid_seq'::regclass)");
        Database.GenerateForeignKey (UPDATER_TABLE, SERVICE_ID,
                                     SERVICE_TABLE, SERVICE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (SERVICE_TABLE, COMPUTER_ID,
                                     COMPUTER_TABLE, COMPUTER_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.ExecuteNonQuery ("CREATE OR REPLACE FUNCTION service_insert_updater() " +
                                  "  RETURNS trigger AS " +
                                  "$BODY$ \n" +
                                  "BEGIN \n" +
                                  "UPDATE updater SET updatertypeid=2, userid=NULL, serviceid=NEW.serviceid " +
                                  "WHERE updaterid=NEW.serviceid;\n" +
                                  "INSERT INTO updater SELECT NEW.serviceid, 2, NULL, NEW.serviceid " +
                                  "WHERE NOT EXISTS (SELECT 1 FROM updater WHERE updaterid=NEW.serviceid);\n" +
                                  "RETURN NEW;\n" +
                                  "END;\n" +
                                  "$BODY$" +
                                  "  LANGUAGE plpgsql VOLATILE " +
                                  "  COST 100;");
        Database.ExecuteNonQuery ("CREATE TRIGGER service_updater " +
                                  "  AFTER INSERT " +
                                  "  ON service " +
                                  "  FOR EACH ROW " +
                                  "  EXECUTE PROCEDURE service_insert_updater();");
        Database.ExecuteNonQuery ("CREATE INDEX service_servicename_idx ON service (servicename);");
        Database.ExecuteNonQuery ("CREATE INDEX service_serviceprogram_idx ON service (serviceprogram);");
        #if false // Create default services
        #region LCtr services
        Database.ExecuteNonQuery ("INSERT INTO Service " +
                                  "(ServiceName, ServiceProgram, ServiceLemoine, ComputerId) " +
                                  "SELECT 'postgresql-15.0 - PostgreSQL Server 15.0', 'postgresql-15.0', FALSE, computerid " +
                                  "FROM computer " +
                                  "WHERE ComputerIsLctr=TRUE");
        Database.ExecuteNonQuery ("INSERT INTO Service " +
                                  "(ServiceName, ServiceProgram, ServiceLemoine, ComputerId) " +
                                  "SELECT 'Apache Tomcat', 'Tomcat5', FALSE, computerid " +
                                  "FROM computer " +
                                  "WHERE ComputerIsLctr=TRUE");
        Database.ExecuteNonQuery ("INSERT INTO Service " +
                                  "(ServiceName, ServiceProgram, ServiceLemoine, ComputerId) " +
                                  "SELECT 'TAO_NT_Naming_Service', 'TAO NT Naming Service', FALSE, computerid " +
                                  "FROM computer " +
                                  "WHERE ComputerIsLctr=TRUE");
        Database.ExecuteNonQuery ("INSERT INTO Service " +
                                  "(ServiceName, ServiceProgram, ServiceLemoine, ComputerId) " +
                                  "SELECT 'Lemoine Stamping Service', 'Lem_StampingService', TRUE, computerid " +
                                  "FROM computer " +
                                  "WHERE ComputerIsLctr=TRUE");
        Database.ExecuteNonQuery ("INSERT INTO Service " +
                                  "(ServiceName, ServiceProgram, ServiceLemoine, ComputerId) " +
                                  "SELECT 'Lemoine Job Synchronisation Service', 'JobSynchrService', TRUE, computerid " +
                                  "FROM computer " +
                                  "WHERE ComputerIsLctr=TRUE");
        #endregion
        #region LPst services
        Database.ExecuteNonQuery ("INSERT INTO Service " +
                                  "(ServiceName, ServiceProgram, ServiceLemoine, ComputerId) " +
                                  "SELECT 'Lemoine Monitoring Service', 'L_Srv', TRUE, computerid " +
                                  "FROM computer " +
                                  "WHERE ComputerIsLpst=TRUE");
        Database.ExecuteNonQuery ("INSERT INTO Service " +
                                  "(ServiceName, ServiceProgram, ServiceLemoine, ComputerId) " +
                                  "SELECT 'Lemoine Software Acquisition Service', 'Lem_CNC_Software', TRUE, computerid " +
                                  "FROM computer " +
                                  "WHERE ComputerIsLpst=TRUE");
        Database.ExecuteNonQuery ("INSERT INTO Service " +
                                  "(ServiceName, ServiceProgram, ServiceLemoine, ComputerId) " +
                                  "SELECT 'Lemoine Cnc Service', 'Lem_CncService', TRUE, computerid " +
                                  "FROM computer " +
                                  "WHERE ComputerIsLpst=TRUE");
        #endregion
        #region Cnc services
        Database.ExecuteNonQuery ("INSERT INTO Service " +
                                  "(ServiceName, ServiceProgram, ServiceLemoine, ComputerId) " +
                                  "SELECT 'Lemoine SiemensToCorba Service', 'Lem_SiemensToCorba', TRUE, computerid " +
                                  "FROM computer " +
                                  "WHERE ComputerIsCnc=TRUE;");
        Database.ExecuteNonQuery ("INSERT INTO Service " +
                                  "(ServiceName, ServiceProgram, ServiceLemoine, ComputerId) " +
                                  "SELECT 'Lemoine FanucToCorba Service', 'Lem_FanucToCorba', TRUE, computerid " +
                                  "FROM computer " +
                                  "WHERE ComputerIsCnc=TRUE;");
        Database.ExecuteNonQuery ("INSERT INTO Service " +
                                  "(ServiceName, ServiceProgram, ServiceLemoine, ComputerId) " +
                                  "SELECT 'Lemoine Watching Service', 'Lem_Control', TRUE, computerid " +
                                  "FROM computer " +
                                  "WHERE ComputerIsCnc=TRUE AND ComputerIsLpst=FALSE AND ComputerIsLctr=FALSE");
        #endregion
        #endif // Create default services
      }
      if (!Database.TableExists (REVISION_TABLE)) {
        Database.AddTable (REVISION_TABLE,
                           new Column (REVISION_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                           new Column (UPDATER_ID, DbType.Int32, ColumnProperty.Null),
                           new Column ("RevisionDateTime", DbType.DateTime, ColumnProperty.NotNull),
                           new Column ("RevisionComment", DbType.String));
        Database.ExecuteNonQuery ("ALTER TABLE revision " +
                                  "ALTER COLUMN revisiondatetime " +
                                  "SET DEFAULT now() AT TIME ZONE 'UTC';");
        Database.GenerateForeignKey (REVISION_TABLE, UPDATER_ID,
                                     UPDATER_TABLE, UPDATER_ID,
                                     Migrator.Framework.ForeignKeyConstraint.SetNull);
        Database.ExecuteNonQuery ("CREATE INDEX revision_updaterid_idx ON revision (updaterid);");
      }
      if (!Database.TableExists (MODIFICATION_TABLE)) {
        Database.AddTable (MODIFICATION_TABLE,
                           new Column (MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                           new Column (REVISION_ID, DbType.Int32, ColumnProperty.Null),
                           new Column ("ModificationDateTime", DbType.DateTime, ColumnProperty.NotNull),
                           new Column ("ModificationReferencedTable", DbType.String, ColumnProperty.NotNull),
                           new Column ("AnalysisStatusId", DbType.Int32, ColumnProperty.NotNull, 1),
                           new Column ("AnalysisAppliedDateTime", DbType.DateTime, ColumnProperty.Null));
        Database.ExecuteNonQuery ("ALTER TABLE modification " +
                                  "ALTER COLUMN modificationdatetime " +
                                  "SET DEFAULT now() AT TIME ZONE 'UTC';");
        Database.AddCheckConstraint ("modification_analysisstatusids",
                                     MODIFICATION_TABLE,
                                     "(analysisstatusid = 1) OR (analysisstatusid = 3) OR (analysisstatusid = 4) OR (analysisstatusid = 5) OR (analysisstatusid = 6)"); // 1: Pending, 3: Done, 4: Error, 5: Obsolete, 6: Delete
        Database.GenerateForeignKey (MODIFICATION_TABLE, REVISION_ID,
                                     REVISION_TABLE, REVISION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.SetNull);
        Database.ExecuteNonQuery ("CREATE INDEX modification_revisionid_idx ON modification (revisionid);");
        Database.ExecuteNonQuery ("CREATE INDEX modification_pendinganalysis_idx " +
                                  "ON modification (modificationdatetime) " +
                                  "WHERE analysisstatusid=1;");
      }
      
      // 2. Old tables deletion (deprecated)
      
      // 3. Views and associated rules for compatibility (deprecated)
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // 3. New tables deletion
      if (Database.TableExists (MODIFICATION_TABLE)) {
        Database.RemoveTable (MODIFICATION_TABLE);
      }
      if (Database.TableExists (REVISION_TABLE)) {
        Database.RemoveTable (REVISION_TABLE);
      }
      if (Database.TableExists (SERVICE_TABLE)) {
        Database.RemoveTable (SERVICE_TABLE);
      }
      if (Database.TableExists (TableName.USER)) {
        Database.RemoveTable (TableName.USER);
      }
      if (Database.TableExists (COMPUTER_TABLE)) {
        Database.RemoveTable (COMPUTER_TABLE);
      }
      if (Database.TableExists (UPDATER_TABLE)) {
        Database.RemoveTable (UPDATER_TABLE);
      }
    }
  }
}

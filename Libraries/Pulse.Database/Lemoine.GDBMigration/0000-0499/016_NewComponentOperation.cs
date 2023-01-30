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
  /// Migration 016: Migrate the sfkcomp and sfkprocess tables to some new tables
  /// </summary>
  [Migration (16)]
  public class NewComponentOperation : Migration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (NewComponentOperation).FullName);
    static readonly string MACHINE_TABLE = TableName.MACHINE;
    static readonly string MACHINE_ID = ColumnName.MACHINE_ID;
    static readonly string PROJECT_TABLE = TableName.PROJECT;
    static readonly string PROJECT_ID = ColumnName.PROJECT_ID;

    static readonly string OPERATION_TYPE_TABLE = TableName.OPERATION_TYPE;
    static readonly string OPERATION_TYPE_ID = "OperationTypeId";
    static readonly string OPERATION_TYPE_NAME = "OperationTypeName";
    static readonly string OPERATION_TYPE_TRANSLATION_KEY = "OperationTypeTranslationKey";
    static readonly string OPERATION_TABLE = TableName.OPERATION;
    static readonly string OPERATION_ID = ColumnName.OPERATION_ID;
    static readonly string INTERMEDIATE_WORK_PIECE_TABLE = TableName.INTERMEDIATE_WORK_PIECE;
    static readonly string INTERMEDIATE_WORK_PIECE_ID = ColumnName.INTERMEDIATE_WORK_PIECE_ID;
    static readonly string OPERATION_SOURCE_WORK_PIECE_TABLE = "OperationSourceWorkPiece";
    static readonly string COMPONENT_TYPE_TABLE = TableName.COMPONENT_TYPE;
    static readonly string COMPONENT_TYPE_ID = "ComponentTypeId";
    static readonly string COMPONENT_TYPE_NAME = "ComponentTypeName";
    static readonly string COMPONENT_TYPE_TRANSLATION_KEY = "ComponentTypeTranslationKey";
    static readonly string COMPONENT_TABLE = TableName.COMPONENT;
    static readonly string COMPONENT_ID = ColumnName.COMPONENT_ID;
    static readonly string COMPONENT_INTERMEDIATE_WORK_PIECE_TABLE = "ComponentIntermediateWorkPiece";
    static readonly string MACHINE_OPERATION_TYPE_TABLE = "MachineOperationType";

    static readonly string SFKPROCESSTYPE_TABLE = "sfkprocesstype";
    static readonly string SFKPROCESS_TABLE = "sfkprocess";
    static readonly string SFKCOMPTYPE_TABLE = "sfkcomptype";
    static readonly string SFKCOMP_TABLE = "sfkcomp";
    static readonly string SFKMACHINEPROCESSTYPE_TABLE = "sfkmachineprocesstype";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      #region 1. operationtype / sfkprocesstype
      if (!Database.TableExists (OPERATION_TYPE_TABLE)) {
        Database.AddTable (OPERATION_TYPE_TABLE,
                           new Column (OPERATION_TYPE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                           new Column (OPERATION_TYPE_NAME, DbType.String, ColumnProperty.Unique),
                           new Column (OPERATION_TYPE_TRANSLATION_KEY, DbType.String, ColumnProperty.Unique),
                           new Column ("OperationTypeCode", DbType.String, ColumnProperty.Unique),
                           new Column ("OperationTypePriority", DbType.Int32));
        Database.ExecuteNonQuery ("ALTER TABLE operationtype " +
                                  "ALTER COLUMN operationtypename " +
                                  "SET DATA TYPE CITEXT;");
        Database.ExecuteNonQuery ("ALTER TABLE operationtype " +
                                  "ALTER COLUMN operationtypecode " +
                                  "SET DATA TYPE CITEXT;");
        Database.AddCheckConstraint ("operationtype_name_translationkey",
                                     OPERATION_TYPE_TABLE,
                                     "((operationtypename IS NOT NULL) OR (operationtypetranslationkey IS NOT NULL))");
        Database.Insert (OPERATION_TYPE_TABLE,
                         new string[] { OPERATION_TYPE_TRANSLATION_KEY },
                         new string[] { "UndefinedValue" }); // id = 1
      }

      if (Database.TableExists (SFKPROCESSTYPE_TABLE)) {
        Database.RemoveTable (SFKPROCESSTYPE_TABLE);
      }

      #region sfkprocesstype view and associated rules
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS sfkprocesstype");
      Database.ExecuteNonQuery ("CREATE OR REPLACE VIEW sfkprocesstype AS " +
                                "SELECT operationtypeid-1 AS id, " +
                                "CASE WHEN operationtypename IS NULL " +
                                " THEN translationvalue " +
                                " ELSE operationtypename::varchar END AS name " +
                                "FROM operationtype " +
                                "LEFT OUTER JOIN translation " +
                                "ON (operationtypetranslationkey=translationkey AND locale='');");
      Database.ExecuteNonQuery ("CREATE RULE sfkprocesstype_insert AS " +
                                "ON INSERT TO sfkprocesstype " +
                                "DO INSTEAD " +
                                "INSERT INTO operationtype " +
                                "(operationtypename) " +
                                "VALUES (NEW.name) " +
                                "RETURNING operationtype.operationtypeid-1 AS id, " +
                                "operationtype.operationtypename::varchar AS name;");
      Database.ExecuteNonQuery ("CREATE RULE sfkprocesstype_update AS " +
                                "ON UPDATE TO sfkprocesstype " +
                                "DO INSTEAD " +
                                "UPDATE operationtype " +
                                "SET operationtypename=NEW.name " +
                                "WHERE operationtypeid = OLD.id+1;");
      Database.ExecuteNonQuery ("CREATE RULE sfkprocesstype_delete AS " +
                                "ON DELETE TO sfkprocesstype " +
                                "DO INSTEAD " +
                                "DELETE FROM operationtype " +
                                "WHERE operationtypeid = OLD.id+1;");
      #endregion // sfkprocesstype view and associated rules
      #endregion // 1. operationtype / sfkprocesstype

      #region 2. componenttype / sfkcomptype
      if (!Database.TableExists (COMPONENT_TYPE_TABLE)) {
        Database.AddTable (COMPONENT_TYPE_TABLE,
                           new Column (COMPONENT_TYPE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                           new Column (COMPONENT_TYPE_NAME, DbType.String, ColumnProperty.Unique),
                           new Column (COMPONENT_TYPE_TRANSLATION_KEY, DbType.String, ColumnProperty.Unique),
                           new Column ("ComponentTypeCode", DbType.String, ColumnProperty.Unique));
        Database.ExecuteNonQuery ("ALTER TABLE componenttype " +
                                  "ALTER COLUMN componenttypename " +
                                  "SET DATA TYPE CITEXT;");
        Database.ExecuteNonQuery ("ALTER TABLE componenttype " +
                                  "ALTER COLUMN componenttypecode " +
                                  "SET DATA TYPE CITEXT;");
        Database.AddCheckConstraint ("componenttype_name_translationkey",
                                     COMPONENT_TYPE_TABLE,
                                     "((componenttypename IS NOT NULL) OR (componenttypetranslationkey IS NOT NULL))");
        Database.Insert (COMPONENT_TYPE_TABLE,
                         new string[] { COMPONENT_TYPE_TRANSLATION_KEY },
                         new string[] { "UndefinedValue" }); // id = 1
      }

      if (Database.TableExists (SFKCOMPTYPE_TABLE)) {
        Database.RemoveTable (SFKCOMPTYPE_TABLE);
      }

      #region sfkcomptype view and associated rules
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS sfkcomptype");
      Database.ExecuteNonQuery ("CREATE OR REPLACE VIEW sfkcomptype AS " +
                                "SELECT componenttypeid-1 AS comptypeid, " +
                                "CASE WHEN componenttypename IS NULL " +
                                " THEN translationvalue " +
                                " ELSE componenttypename::varchar END AS comptypename " +
                                "FROM componenttype " +
                                "LEFT OUTER JOIN translation " +
                                "ON (componenttypetranslationkey=translationkey AND locale='');");
      Database.ExecuteNonQuery ("CREATE RULE sfkcomptype_insert AS " +
                                "ON INSERT TO sfkcomptype " +
                                "DO INSTEAD " +
                                "INSERT INTO componenttype " +
                                "(componenttypename) " +
                                "VALUES (NEW.comptypename) " +
                                "RETURNING componenttypeid-1 AS comptypeid, " +
                                "componenttypename::varchar AS comptypename;");
      Database.ExecuteNonQuery ("CREATE RULE sfkcomptype_update AS " +
                                "ON UPDATE TO sfkcomptype " +
                                "DO INSTEAD " +
                                "UPDATE componenttype " +
                                "SET componenttypename=NEW.comptypename " +
                                "WHERE componenttypeid = OLD.comptypeid+1;");
      Database.ExecuteNonQuery ("CREATE RULE sfkcomptype_delete AS " +
                                "ON DELETE TO sfkcomptype " +
                                "DO INSTEAD " +
                                "DELETE FROM componenttype " +
                                "WHERE componenttypeid = OLD.comptypeid+1;");
      #endregion // sfkcomptype view and associated rules
      #endregion // 2. componenttype / sfkcomptype

      #region 3. operation / intermediateworkpiece / component / sfkprocess / sfkcomp
      if (!Database.TableExists (OPERATION_TABLE)) {
        Database.AddTable (OPERATION_TABLE,
                           new Column (OPERATION_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                           new Column ("OperationName", DbType.String),
                           new Column ("OperationCode", DbType.String),
                           new Column ("OperationExternalCode", DbType.String, ColumnProperty.Unique),
                           new Column ("OperationDocumentLink", DbType.String),
                           new Column ("OperationTypeId", DbType.Int32, ColumnProperty.NotNull, 1), // Default to Undefined
                           new Column ("OperationEstimatedMachiningHours", DbType.Double),
                           new Column ("OperationEstimatedSetupHours", DbType.Double),
                           new Column ("OperationEstimatedTearDownHours", DbType.Double));
        Database.ExecuteNonQuery ("ALTER TABLE Operation " +
                                  "ALTER COLUMN OperationName " +
                                  "SET DATA TYPE CITEXT;");
        Database.ExecuteNonQuery ("ALTER TABLE Operation " +
                                  "ALTER COLUMN OperationCode " +
                                  "SET DATA TYPE CITEXT;");
        Database.GenerateForeignKey (OPERATION_TABLE, OPERATION_TYPE_ID,
                                     OPERATION_TYPE_TABLE, OPERATION_TYPE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.SetDefault);
        Database.ExecuteNonQuery ("CREATE INDEX operation_operationname_idx " +
                                  "ON operation (operationname) " +
                                  "WHERE operationname IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE INDEX operation_operationcode_idx " +
                                  "ON operation (operationcode) " +
                                  "WHERE operationcode IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX operation_operationexternalcode_idx " +
                                  "ON operation (operationexternalcode) " +
                                  "WHERE operationexternalcode IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE INDEX operation_operationtypeid_idx " +
                                  "ON operation (operationtypeid)");
      }
      if (!Database.TableExists (INTERMEDIATE_WORK_PIECE_TABLE)) {
        Database.AddTable (INTERMEDIATE_WORK_PIECE_TABLE,
                           new Column (INTERMEDIATE_WORK_PIECE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                           new Column ("IntermediateWorkPieceName", DbType.String),
                           new Column ("IntermediateWorkPieceCode", DbType.String),
                           new Column ("IntermediateWorkPieceExternalCode", DbType.String, ColumnProperty.Unique),
                           new Column ("IntermediateWorkPieceDocumentLink", DbType.String),
                           new Column ("IntermediateWorkPieceWeight", DbType.Double),
                           new Column (OPERATION_ID, DbType.Int32, ColumnProperty.NotNull),
                           new Column ("OperationIntermediateWorkPieceQuantity", DbType.Int32, ColumnProperty.NotNull, 1));
        Database.ExecuteNonQuery ("ALTER TABLE IntermediateWorkPiece " +
                                  "ALTER COLUMN IntermediateWorkPieceName " +
                                  "SET DATA TYPE CITEXT;");
        Database.ExecuteNonQuery ("ALTER TABLE IntermediateWorkPiece " +
                                  "ALTER COLUMN IntermediateWorkPieceCode " +
                                  "SET DATA TYPE CITEXT;");
        Database.GenerateForeignKey (INTERMEDIATE_WORK_PIECE_TABLE, OPERATION_ID,
                                     OPERATION_TABLE, OPERATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.ExecuteNonQuery ("CREATE INDEX intermediateworkpiece_intermediateworkpiecename_idx " +
                                  "ON intermediateworkpiece (intermediateworkpiecename) " +
                                  "WHERE intermediateworkpiecename IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE INDEX intermediateworkpiece_intermediateworkpiececode_idx " +
                                  "ON intermediateworkpiece (intermediateworkpiececode) " +
                                  "WHERE intermediateworkpiececode IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX intermediateworkpiece_intermediateworkpieceexternalcode_idx " +
                                  "ON intermediateworkpiece (intermediateworkpieceexternalcode) " +
                                  "WHERE intermediateworkpieceexternalcode IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE INDEX intermediateworkpiece_operationid_idx " +
                                  "ON intermediateworkpiece (operationid)");
      }
      if (!Database.TableExists (OPERATION_SOURCE_WORK_PIECE_TABLE)) {
        Database.AddTable (OPERATION_SOURCE_WORK_PIECE_TABLE,
                           new Column (OPERATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column (INTERMEDIATE_WORK_PIECE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column ("OperationSourceWorkPieceQuantity", DbType.Int32, ColumnProperty.NotNull, 1));
        Database.GenerateForeignKey (OPERATION_SOURCE_WORK_PIECE_TABLE, OPERATION_ID,
                                     OPERATION_TABLE, OPERATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (OPERATION_SOURCE_WORK_PIECE_TABLE, INTERMEDIATE_WORK_PIECE_ID,
                                     INTERMEDIATE_WORK_PIECE_TABLE, INTERMEDIATE_WORK_PIECE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.ExecuteNonQuery ("CREATE INDEX operationsourceworkpiece_intermediateworkpieceid_idx " +
                                  "ON operationsourceworkpiece (intermediateworkpieceid)");
      }
      if (!Database.TableExists (COMPONENT_TABLE)) {
        Database.AddTable (COMPONENT_TABLE,
                           new Column (COMPONENT_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                           new Column ("ComponentName", DbType.String),
                           new Column ("ComponentCode", DbType.String),
                           new Column ("ComponentExternalCode", DbType.String, ColumnProperty.Unique),
                           new Column ("ComponentDocumentLink", DbType.String),
                           new Column (PROJECT_ID, DbType.Int32, ColumnProperty.NotNull),
                           new Column ("ComponentTypeId", DbType.Int32, ColumnProperty.NotNull, 1), // Default to Undefined
                           new Column ("FinalWorkPieceId", DbType.Int32),
                           new Column ("ComponentEstimatedHours", DbType.Double));
        Database.ExecuteNonQuery ("ALTER TABLE Component " +
                                  "ALTER COLUMN ComponentName " +
                                  "SET DATA TYPE CITEXT;");
        Database.ExecuteNonQuery ("ALTER TABLE Component " +
                                  "ALTER COLUMN ComponentCode " +
                                  "SET DATA TYPE CITEXT;");
        Database.AddUniqueConstraint ("component_projectid_name_key",
                                      COMPONENT_TABLE,
                                      new string[] { PROJECT_ID, "ComponentName" });
        Database.AddUniqueConstraint ("component_projectid_code_key",
                                      COMPONENT_TABLE,
                                      new string[] { PROJECT_ID, "ComponentCode" });
        Database.AddCheckConstraint ("component_name_code",
                                     COMPONENT_TABLE,
                                     "((componentname IS NOT NULL) OR (componentcode IS NOT NULL))");
        Database.GenerateForeignKey (COMPONENT_TABLE, PROJECT_ID,
                                     PROJECT_TABLE, PROJECT_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (COMPONENT_TABLE, COMPONENT_TYPE_ID,
                                     COMPONENT_TYPE_TABLE, COMPONENT_TYPE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.SetDefault);
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX component_projectid_componentname_idx " +
                                  "ON component (projectid, componentname) " +
                                  "WHERE componentname IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX component_projectid_componentcode_idx " +
                                  "ON component (projectid, componentcode) " +
                                  "WHERE componentcode IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX component_componentexternalcode_idx " +
                                  "ON component (componentexternalcode) " +
                                  "WHERE componentexternalcode IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE INDEX component_projectid_idx " +
                                  "ON component (projectid)");
        Database.ExecuteNonQuery ("CREATE INDEX component_componenttypeid_idx " +
                                  "ON component (componenttypeid)");
      }
      if (!Database.TableExists (COMPONENT_INTERMEDIATE_WORK_PIECE_TABLE)) {
        Database.AddTable (COMPONENT_INTERMEDIATE_WORK_PIECE_TABLE,
                           new Column (COMPONENT_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column (INTERMEDIATE_WORK_PIECE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column ("intermediateworkpiececodeforcomponent", DbType.String),
                           new Column ("intermediateworkpieceorderforcomponent", DbType.Int32));
        Database.ExecuteNonQuery ("ALTER TABLE componentintermediateworkpiece " +
                                  "ALTER COLUMN intermediateworkpiececodeforcomponent " +
                                  "SET DATA TYPE CITEXT;");
        Database.GenerateForeignKey (COMPONENT_INTERMEDIATE_WORK_PIECE_TABLE, COMPONENT_ID,
                                     COMPONENT_TABLE, COMPONENT_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (COMPONENT_INTERMEDIATE_WORK_PIECE_TABLE, INTERMEDIATE_WORK_PIECE_ID,
                                     INTERMEDIATE_WORK_PIECE_TABLE, INTERMEDIATE_WORK_PIECE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.ExecuteNonQuery ("CREATE INDEX componentintermediateworkpiece_componentid_idx " +
                                  "ON componentintermediateworkpiece (componentid)");
        Database.ExecuteNonQuery ("CREATE INDEX componentintermediateworkpiece_intermediateworkpieceid_idx " +
                                  "ON componentintermediateworkpiece (intermediateworkpieceid)");
      }

      if (Database.TableExists (SFKPROCESS_TABLE)) {
        Database.RemoveTable (SFKPROCESS_TABLE);
        Database.ExecuteNonQuery ("DROP FUNCTION IF EXISTS processcompid();");
        Database.ExecuteNonQuery ("DROP FUNCTION IF EXISTS processuncompleted();");
      }
      if (Database.TableExists (SFKCOMP_TABLE)) {
        Database.RemoveTable (SFKCOMP_TABLE);
        Database.ExecuteNonQuery ("DROP FUNCTION IF EXISTS componentdone();");
        Database.ExecuteNonQuery ("DROP FUNCTION IF EXISTS componentundone();");
      }

      #region sfkprocess view and associated rules
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS sfkprocess");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW sfkprocess AS 
 SELECT operation.operationid AS id, operation.operationname AS name, operation.operationtypeid - 1 AS processtypeid,
 CASE WHEN componentintermediateworkpiece.componentid IS NULL THEN 0 ELSE componentintermediateworkpiece.componentid END AS componentid,
 1 AS ordernb, operation.operationestimatedmachininghours AS hours, 0 AS completed
   FROM operation
NATURAL JOIN intermediateworkpiece
LEFT OUTER JOIN componentintermediateworkpiece USING (intermediateworkpieceid);");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION sfkprocess_inserter()
  RETURNS trigger AS
$BODY$
DECLARE
varintermediateworkpieceid int8;
BEGIN
  INSERT INTO operation (operationname, operationtypeid,
    operationestimatedmachininghours)
  VALUES (NEW.name, NEW.processtypeid+1, NEW.hours)
  RETURNING operationid AS id INTO STRICT NEW.id;
  INSERT INTO intermediateworkpiece (operationid)
  VALUES (NEW.id)
  RETURNING intermediateworkpieceid
  INTO STRICT varintermediateworkpieceid;
  IF NEW.componentid<>0 AND NEW.componentid IS NOT NULL THEN
    INSERT INTO componentintermediateworkpiece (componentid, intermediateworkpieceid)
    VALUES (NEW.componentid, varintermediateworkpieceid);
  END IF;
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE COST 100;");
      Database.ExecuteNonQuery (@"CREATE TRIGGER sfkprocess_insert INSTEAD OF INSERT
ON sfkprocess
FOR EACH ROW
EXECUTE PROCEDURE sfkprocess_inserter ();");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION sfkprocess_updater()
  RETURNS trigger AS
$BODY$
BEGIN
  UPDATE operation
  SET operationname=NEW.name, operationtypeid=NEW.processtypeid+1,
    operationestimatedmachininghours=NEW.hours
  WHERE operationid = OLD.id;
  UPDATE componentintermediateworkpiece
  SET componentid=NEW.componentid
  WHERE OLD.componentid<>0 AND OLD.componentid IS NOT NULL
    AND componentid=OLD.componentid AND intermediateworkpieceid= (SELECT intermediateworkpieceid FROM intermediateworkpiece WHERE operationid=OLD.id);
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE COST 100;");
      Database.ExecuteNonQuery (@"CREATE TRIGGER sfkprocess_update INSTEAD OF UPDATE
ON sfkprocess
FOR EACH ROW
EXECUTE PROCEDURE sfkprocess_updater ();");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION sfkprocess_deleter()
  RETURNS trigger AS
$BODY$
BEGIN
 DELETE FROM componentintermediateworkpiece
  WHERE componentid=OLD.componentid
   AND intermediateworkpieceid=
    (SELECT intermediateworkpieceid
     FROM intermediateworkpiece WHERE operationid=OLD.id);
 DELETE FROM intermediateworkpiece WHERE operationid=OLD.id;
 DELETE FROM operation
  WHERE operationid = OLD.id;
 RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE COST 100;");
      Database.ExecuteNonQuery (@"CREATE TRIGGER sfkprocess_delete INSTEAD OF DELETE
ON sfkprocess
FOR EACH ROW
EXECUTE PROCEDURE sfkprocess_deleter ();");
      #endregion // sfkprocess view and associated rules
      #region sfkcomp view and associated rules
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS sfkcomp");
      Database.ExecuteNonQuery ("CREATE OR REPLACE VIEW sfkcomp AS " +
                                "SELECT componentid AS compid, " +
                                "componentname AS compname, " +
                                "projectid AS projid, " +
                                "projectcreationdatetime AS starttimeref, " +
                                "projectcreationdatetime AS endtimeref, " +
                                "componentestimatedhours AS comphouref, " +
                                "0 AS sfkdone, " +
                                "componenttypeid-1 AS comptypeid " +
                                "FROM component " +
                                "NATURAL JOIN project;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION sfkcomp_inserter()
  RETURNS trigger AS
$BODY$
DECLARE
BEGIN
  INSERT INTO component (componentname, projectid, componentestimatedhours,
    componenttypeid)
  VALUES (NEW.compname, NEW.projid, NEW.comphouref, NEW.comptypeid+1)
  RETURNING componentid AS compid,
    now() AT TIME ZONE 'UTC' AS starttimeref,
    now() AT TIME ZONE 'UTC' AS endtimeref
  INTO STRICT NEW.compid, NEW.starttimeref, NEW.endtimeref;
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE COST 100;");
      Database.ExecuteNonQuery (@"CREATE TRIGGER sfkcomp_insert INSTEAD OF INSERT
ON sfkcomp
FOR EACH ROW
EXECUTE PROCEDURE sfkcomp_inserter ();");
      Database.ExecuteNonQuery (@"CREATE RULE sfkcomp_update AS
ON UPDATE TO sfkcomp DO INSTEAD
UPDATE component
SET componentname=NEW.compname, projectid=NEW.projid,
  componentestimatedhours=NEW.comphouref, componenttypeid=NEW.comptypeid+1
WHERE componentid = OLD.compid;");
      Database.ExecuteNonQuery (@"CREATE RULE sfkcomp_delete AS
ON DELETE TO sfkcomp DO INSTEAD
DELETE FROM component WHERE componentid = OLD.compid;");
      #endregion // sfkcomp view and associated rules
      #endregion // 3. operation / intermediateworkpiece / component / sfkprocess / sfkcomp

      #region 4. machineoperationtype / sfkprocesstype
      if (!Database.TableExists (MACHINE_OPERATION_TYPE_TABLE)) {
        Database.AddTable (MACHINE_OPERATION_TYPE_TABLE,
                           new Column (MACHINE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column (OPERATION_TYPE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column ("MachineOperationTypePreference", DbType.Int32, ColumnProperty.NotNull, 2));
        Database.GenerateForeignKey (MACHINE_OPERATION_TYPE_TABLE, MACHINE_ID,
                                     MACHINE_TABLE, MACHINE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (MACHINE_OPERATION_TYPE_TABLE, OPERATION_TYPE_ID,
                                     OPERATION_TYPE_TABLE, OPERATION_TYPE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.ExecuteNonQuery ("CREATE INDEX machineoperationtype_machineid_main_idx " +
                                  "ON machineoperationtype (machineid) " +
                                  "WHERE machineoperationtypepreference = 1;");
        Database.ExecuteNonQuery ("CREATE INDEX machineoperationtype_machineid_idx " +
                                  "ON machineoperationtype (machineid);");
      }

      if (Database.TableExists (SFKMACHINEPROCESSTYPE_TABLE)) {
        Database.RemoveTable (SFKMACHINEPROCESSTYPE_TABLE);
      }

      #region sfkmachineprocesstype view and associated rules
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS sfkmachineprocesstype");
      Database.ExecuteNonQuery ("CREATE OR REPLACE VIEW sfkmachineprocesstype AS " +
                                "SELECT machineid AS machineid, " +
                                "operationtypeid-1 AS processtypeid " +
                                "FROM machineoperationtype " +
                                "WHERE machineoperationtypepreference=1;");
      Database.ExecuteNonQuery ("CREATE RULE sfkmachineprocesstype_insert AS " +
                                "ON INSERT TO sfkmachineprocesstype " +
                                "DO INSTEAD " +
                                "INSERT INTO machineoperationtype " +
                                "VALUES (NEW.machineid, NEW.processtypeid+1, 1);");
      Database.ExecuteNonQuery ("CREATE RULE sfkmachineprocesstype_delete AS " +
                                "ON DELETE TO sfkmachineprocesstype " +
                                "DO INSTEAD " +
                                "DELETE FROM machineoperationtype " +
                                "WHERE machineid=OLD.machineid AND operationtypeid=OLD.processtypeid+1;");
      #endregion // sfkmachineprocesstype view and associated rules
      #endregion // 4. machineoperationtype / sfkprocesstype
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // 1. Drop compatibility views
      if (Database.TableExists (SFKPROCESSTYPE_TABLE)) {
        Database.ExecuteNonQuery ("DROP VIEW IF EXISTS sfkprocesstype CASCADE;");
      }
      if (Database.TableExists (SFKPROCESS_TABLE)) {
        Database.ExecuteNonQuery ("DROP VIEW IF EXISTS sfkprocess CASCADE;");
      }
      if (Database.TableExists (SFKCOMPTYPE_TABLE)) {
        Database.ExecuteNonQuery ("DROP VIEW IF EXISTS sfkcomptype CASCADE;");
      }
      if (Database.TableExists (SFKCOMP_TABLE)) {
        Database.ExecuteNonQuery ("DROP VIEW IF EXISTS sfkcomp CASCADE;");
      }
      if (Database.TableExists (SFKMACHINEPROCESSTYPE_TABLE)) {
        Database.ExecuteNonQuery ("DROP VIEW IF EXISTS sfkmachineprocesstype;");
      }
      if (Database.TableExists ("sfkcfgs")) {
        Database.ExecuteNonQuery (@"DELETE FROM sfkcfgs
WHERE config='system' AND sfksection='reporting' AND skey='viewsversion';");
        Database.ExecuteNonQuery (@"DELETE FROM sfkcfgs
WHERE config='system' AND sfksection='reporting' AND skey='viewsdate';");
      }

      // 3. New tables deletion
      if (Database.TableExists (MACHINE_OPERATION_TYPE_TABLE)) {
        Database.RemoveTable (MACHINE_OPERATION_TYPE_TABLE);
      }
      if (Database.TableExists (COMPONENT_INTERMEDIATE_WORK_PIECE_TABLE)) {
        Database.RemoveTable (COMPONENT_INTERMEDIATE_WORK_PIECE_TABLE);
      }
      if (Database.TableExists (COMPONENT_TABLE)) {
        Database.RemoveTable (COMPONENT_TABLE);
      }
      if (Database.TableExists (COMPONENT_TYPE_TABLE)) {
        Database.RemoveTable (COMPONENT_TYPE_TABLE);
      }
      if (Database.TableExists (OPERATION_SOURCE_WORK_PIECE_TABLE)) {
        Database.RemoveTable (OPERATION_SOURCE_WORK_PIECE_TABLE);
      }
      if (Database.TableExists (INTERMEDIATE_WORK_PIECE_TABLE)) {
        Database.RemoveTable (INTERMEDIATE_WORK_PIECE_TABLE);
      }
      if (Database.TableExists (OPERATION_TABLE)) {
        Database.RemoveTable (OPERATION_TABLE);
      }
      if (Database.TableExists (OPERATION_TYPE_TABLE)) {
        Database.RemoveTable (OPERATION_TYPE_TABLE);
      }
    }
  }
}

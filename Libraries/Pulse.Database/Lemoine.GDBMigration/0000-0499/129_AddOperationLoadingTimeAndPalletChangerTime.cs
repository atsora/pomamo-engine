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
  /// Migration 129: add some new properties on the operation:
  /// <item>loading and unloading time</item>
  /// <item>pallet changing time (associated to the machine)</item>
  /// <item>use durations in seconds instead of durations in hours</item>
  /// </summary>
  [Migration(129)]
  public class AddOperationLoadingTimeAndPalletChangerTime: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddOperationLoadingTimeAndPalletChangerTime).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS simpleoperation;");

      ConvertDurationToSeconds ();
      AddLoadingUnloadingDuration ();
      AddPalletChangingDuration ();
      
      RestoreNewSimpleOperation ();
      
      // Restore sfkprocess
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW sfkprocess AS
 SELECT operation.operationid AS id, operation.operationname AS name, operation.operationtypeid - 1 AS processtypeid,
        CASE
            WHEN componentintermediateworkpiece.componentid IS NULL THEN 0
            ELSE componentintermediateworkpiece.componentid
        END AS componentid, 1 AS ordernb, operation.operationmachiningduration / 3600 AS hours, 0 AS completed
   FROM operation
NATURAL JOIN intermediateworkpiece
   LEFT JOIN componentintermediateworkpiece USING (intermediateworkpieceid);");

      // check for existence of schema reportv2
      bool schemaExists = false;
      using (IDataReader reader =
             Database.ExecuteQuery (@"SELECT schema_name FROM information_schema.schemata WHERE schema_name = 'reportv2'")) {
        if (reader.Read()) {
          if (!reader.IsDBNull(0)) {
            schemaExists = true;
          }
        }
      }

      if (schemaExists) {
        // Restore reportv2.operation
        Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW reportv2.operation AS
 SELECT operation.operationid, operation.operationname, operation.operationcode, operation.operationtypeid, operation.operationmachiningduration / 3600.0 AS operationestimatedmachininghours, operation.operationsetupduration / 3600.0 AS operationestimatedsetuphours, operation.operationteardownduration / 3600.0 AS operationestimatedteardownhours
   FROM operation;");
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS simpleoperation;");

      RemovePalletChangingDuration ();
      RemoveLoadingUnloadingDuration ();
      ConvertDurationToHours ();

      RestoreOldSimpleOperation ();

      // Restore sfkprocess
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW sfkprocess AS
 SELECT operation.operationid AS id, operation.operationname AS name, operation.operationtypeid - 1 AS processtypeid,
        CASE
            WHEN componentintermediateworkpiece.componentid IS NULL THEN 0
            ELSE componentintermediateworkpiece.componentid
        END AS componentid, 1 AS ordernb, operation.operationestimatedmachininghours AS hours, 0 AS completed
   FROM operation
NATURAL JOIN intermediateworkpiece
   LEFT JOIN componentintermediateworkpiece USING (intermediateworkpieceid);");
      
      // check for existence of schema reportv2
      bool schemaExists = false;
      using (IDataReader reader =
             Database.ExecuteQuery (@"SELECT schema_name FROM information_schema.schemata WHERE schema_name = 'reportv2'")) {
        if (reader.Read()) {
          if (!reader.IsDBNull(0)) {
            schemaExists = true;
          }
        }
      }

      if (schemaExists) {
        // Restore reportv2.operation
        Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW reportv2.operation AS
 SELECT operation.operationid, operation.operationname, operation.operationcode, operation.operationtypeid, operation.operationestimatedmachininghours, operation.operationestimatedsetuphours, operation.operationestimatedteardownhours
   FROM operation;");
      }
    }
    
    void ConvertDurationToSeconds ()
    {
      Database.RenameColumn (TableName.OPERATION,
                             "operationestimatedmachininghours",
                             "operationmachiningduration");
      Database.RenameColumn (TableName.OPERATION,
                             "operationestimatedsetuphours",
                             "operationsetupduration");
      Database.RenameColumn (TableName.OPERATION,
                             "operationestimatedteardownhours",
                             "operationteardownduration");
      Database.RenameColumn (TableName.OPERATION_INFORMATION,
                             "oldestimatedmachininghours",
                             "oldoperationmachiningduration");
      Database.ExecuteNonQuery (@"UPDATE operation
SET operationmachiningduration = operationmachiningduration * 3600.0");
      Database.ExecuteNonQuery (@"UPDATE operation
SET operationsetupduration = operationsetupduration * 3600.0");
      Database.ExecuteNonQuery (@"UPDATE operation
SET operationteardownduration = operationteardownduration * 3600.0");
      Database.ExecuteNonQuery (@"UPDATE operationinformation
SET oldoperationmachiningduration = oldoperationmachiningduration * 3600.0");
    }
    
    void ConvertDurationToHours ()
    {
      Database.ExecuteNonQuery (@"UPDATE operationinformation
SET oldoperationmachiningduration = oldoperationmachiningduration / 3600.0");
      Database.ExecuteNonQuery (@"UPDATE operation
SET operationmachiningduration = operationmachiningduration / 3600.0");
      Database.ExecuteNonQuery (@"UPDATE operation
SET operationsetupduration = operationsetupduration / 3600.0");
      Database.ExecuteNonQuery (@"UPDATE operation
SET operationteardownduration = operationteardownduration / 3600.0");
      Database.RenameColumn (TableName.OPERATION,
                             "operationmachiningduration",
                             "operationestimatedmachininghours");
      Database.RenameColumn (TableName.OPERATION,
                             "operationsetupduration",
                             "operationestimatedsetuphours");
      Database.RenameColumn (TableName.OPERATION,
                             "operationteardownduration",
                             "operationestimatedteardownhours");
      Database.RenameColumn (TableName.OPERATION_INFORMATION,
                             "oldoperationmachiningduration",
                             "oldestimatedmachininghours");
    }
    
    void AddLoadingUnloadingDuration ()
    {
      Database.AddColumn (TableName.OPERATION,
                          TableName.OPERATION + "loadingduration", DbType.Double, ColumnProperty.Null);
      Database.AddColumn (TableName.OPERATION,
                          TableName.OPERATION + "unloadingduration", DbType.Double, ColumnProperty.Null);
    }
    
    void RemoveLoadingUnloadingDuration ()
    {
      Database.RemoveColumn (TableName.OPERATION,
                             TableName.OPERATION + "loadingduration");
      Database.RemoveColumn (TableName.OPERATION,
                             TableName.OPERATION + "unloadingduration");
    }
    
    void AddPalletChangingDuration ()
    {
      Database.AddColumn (TableName.MONITORED_MACHINE,
                          new Column ("palletchangingduration", DbType.Double, ColumnProperty.Null));
    }
    
    void RemovePalletChangingDuration ()
    {
      Database.RemoveColumn (TableName.MONITORED_MACHINE,
                             "palletchangingduration");
    }
    
    void RestoreNewSimpleOperation ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS simpleoperation;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW simpleoperation AS
 SELECT operation.operationid, intermediateworkpiece.intermediateworkpieceid, operation.operationname, operation.operationcode, operation.operationexternalcode, operation.operationdocumentlink, operation.operationtypeid, operation.operationmachiningduration, operation.operationsetupduration, operation.operationteardownduration, intermediateworkpiece.intermediateworkpieceweight, componentintermediateworkpiece.componentid, componentintermediateworkpiece.intermediateworkpiececodeforcomponent, componentintermediateworkpiece.intermediateworkpieceorderforcomponent,
        operation.operationloadingduration, operation.operationunloadingduration
   FROM operation
NATURAL JOIN intermediateworkpiece
   LEFT JOIN componentintermediateworkpiece USING (intermediateworkpieceid);");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION simpleoperation_deleter()
  RETURNS trigger AS
$BODY$
BEGIN
 DELETE FROM operation
  WHERE operationid=OLD.operationid;
 DELETE FROM intermediateworkpiece
  WHERE intermediateworkpieceid=OLD.intermediateworkpieceid;
 DELETE FROM componentintermediateworkpiece
  WHERE intermediateworkpieceid=OLD.intermediateworkpieceid;
 RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
      Database.ExecuteNonQuery (@"CREATE TRIGGER simpleoperation_delete
  INSTEAD OF DELETE
  ON simpleoperation
  FOR EACH ROW
  EXECUTE PROCEDURE simpleoperation_deleter();");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION simpleoperation_inserter()
  RETURNS trigger AS
$BODY$
BEGIN
  INSERT INTO operation (operationid, operationname, operationcode,
    operationexternalcode, operationdocumentlink, operationtypeid,
    operationmachiningduration, operationsetupduration,
    operationteardownduration, operationloadingduration, operationunloadingduration)
  VALUES (CASE WHEN NEW.operationid IS NOT NULL THEN NEW.operationid
    ELSE nextval('operation_operationid_seq') END, NEW.operationname,
    NEW.operationcode, NEW.operationexternalcode, NEW.operationdocumentlink,
    NEW.operationtypeid, NEW.operationmachiningduration,
    NEW.operationsetupduration, NEW.operationteardownduration,
    NEW.operationloadingduration, NEW.operationunloadingduration)
  RETURNING operationid
  INTO STRICT NEW.operationid;
  INSERT INTO intermediateworkpiece (
   intermediateworkpiecename, intermediateworkpiececode,
   intermediateworkpieceexternalcode, intermediateworkpiecedocumentlink,
   intermediateworkpieceweight, operationid)
  VALUES (NEW.operationname, NEW.operationcode, NEW.operationexternalcode,
   NEW.operationdocumentlink, NEW.intermediateworkpieceweight,
   NEW.operationid)
  RETURNING intermediateworkpieceid
  INTO STRICT NEW.intermediateworkpieceid;
  INSERT INTO componentintermediateworkpiece (intermediateworkpieceid,
    componentid, intermediateworkpiececodeforcomponent,
    intermediateworkpieceorderforcomponent)
  SELECT NEW.intermediateworkpieceid, NEW.componentid,
    NEW.intermediateworkpiececodeforcomponent,
    NEW.intermediateworkpieceorderforcomponent
  WHERE NEW.componentid IS NOT NULL;
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
      Database.ExecuteNonQuery (@"CREATE TRIGGER simpleoperation_insert
  INSTEAD OF INSERT
  ON simpleoperation
  FOR EACH ROW
  EXECUTE PROCEDURE simpleoperation_inserter();");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION simpleoperation_updater()
  RETURNS trigger AS
$BODY$
BEGIN
  UPDATE operation
  SET operationname=NEW.operationname, operationcode=NEW.operationcode,
    operationexternalcode=NEW.operationexternalcode,
    operationdocumentlink=NEW.operationdocumentlink,
    operationtypeid=NEW.operationtypeid,
    operationmachiningduration=NEW.operationmachiningduration,
    operationsetupduration=NEW.operationsetupduration,
    operationteardownduration=NEW.operationteardownduration,
    operationloadingduration=NEW.operationloadingduration,
    operationunloadingduration=NEW.operationunloadingduration
  WHERE operationid=OLD.operationid;
  UPDATE intermediateworkpiece
  SET intermediateworkpiecename=NEW.operationname,
    intermediateworkpiececode=NEW.operationcode,
    intermediateworkpieceexternalcode=NEW.operationexternalcode,
    intermediateworkpiecedocumentlink=NEW.operationdocumentlink,
    intermediateworkpieceweight=NEW.intermediateworkpieceweight
  WHERE intermediateworkpieceid=OLD.intermediateworkpieceid;
  UPDATE componentintermediateworkpiece
  SET componentid=NEW.componentid,
    intermediateworkpiececodeforcomponent=NEW.intermediateworkpiececodeforcomponent,
    intermediateworkpieceorderforcomponent=NEW.intermediateworkpieceorderforcomponent
  WHERE intermediateworkpieceid=OLD.intermediateworkpieceid;
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
      Database.ExecuteNonQuery (@"CREATE TRIGGER simpleoperation_update
  INSTEAD OF UPDATE
  ON simpleoperation
  FOR EACH ROW
  EXECUTE PROCEDURE simpleoperation_updater();");
    }
    
    void RestoreOldSimpleOperation ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS simpleoperation;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW simpleoperation AS
 SELECT operation.operationid, intermediateworkpiece.intermediateworkpieceid, operation.operationname, operation.operationcode, operation.operationexternalcode, operation.operationdocumentlink, operation.operationtypeid, operation.operationestimatedmachininghours, operation.operationestimatedsetuphours, operation.operationestimatedteardownhours, intermediateworkpiece.intermediateworkpieceweight, componentintermediateworkpiece.componentid, componentintermediateworkpiece.intermediateworkpiececodeforcomponent, componentintermediateworkpiece.intermediateworkpieceorderforcomponent
   FROM operation
NATURAL JOIN intermediateworkpiece
   LEFT JOIN componentintermediateworkpiece USING (intermediateworkpieceid);");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION simpleoperation_deleter()
  RETURNS trigger AS
$BODY$
BEGIN
 DELETE FROM operation
  WHERE operationid=OLD.operationid;
 DELETE FROM intermediateworkpiece
  WHERE intermediateworkpieceid=OLD.intermediateworkpieceid;
 DELETE FROM componentintermediateworkpiece
  WHERE intermediateworkpieceid=OLD.intermediateworkpieceid;
 RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
      Database.ExecuteNonQuery (@"CREATE TRIGGER simpleoperation_delete
  INSTEAD OF DELETE
  ON simpleoperation
  FOR EACH ROW
  EXECUTE PROCEDURE simpleoperation_deleter();");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION simpleoperation_inserter()
  RETURNS trigger AS
$BODY$
BEGIN
  INSERT INTO operation (operationid, operationname, operationcode,
    operationexternalcode, operationdocumentlink, operationtypeid,
    operationestimatedmachininghours, operationestimatedsetuphours,
    operationestimatedteardownhours)
  VALUES (CASE WHEN NEW.operationid IS NOT NULL THEN NEW.operationid
    ELSE nextval('operation_operationid_seq') END, NEW.operationname,
    NEW.operationcode, NEW.operationexternalcode, NEW.operationdocumentlink,
    NEW.operationtypeid, NEW.operationestimatedmachininghours,
    NEW.operationestimatedsetuphours, NEW.operationestimatedteardownhours)
  RETURNING operationid
  INTO STRICT NEW.operationid;
  INSERT INTO intermediateworkpiece (
   intermediateworkpiecename, intermediateworkpiececode,
   intermediateworkpieceexternalcode, intermediateworkpiecedocumentlink,
   intermediateworkpieceweight, operationid)
  VALUES (NEW.operationname, NEW.operationcode, NEW.operationexternalcode,
   NEW.operationdocumentlink, NEW.intermediateworkpieceweight,
   NEW.operationid)
  RETURNING intermediateworkpieceid
  INTO STRICT NEW.intermediateworkpieceid;
  INSERT INTO componentintermediateworkpiece (intermediateworkpieceid,
    componentid, intermediateworkpiececodeforcomponent,
    intermediateworkpieceorderforcomponent)
  SELECT NEW.intermediateworkpieceid, NEW.componentid,
    NEW.intermediateworkpiececodeforcomponent,
    NEW.intermediateworkpieceorderforcomponent
  WHERE NEW.componentid IS NOT NULL;
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
      Database.ExecuteNonQuery (@"CREATE TRIGGER simpleoperation_insert
  INSTEAD OF INSERT
  ON simpleoperation
  FOR EACH ROW
  EXECUTE PROCEDURE simpleoperation_inserter();");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION simpleoperation_updater()
  RETURNS trigger AS
$BODY$
BEGIN
  UPDATE operation
  SET operationname=NEW.operationname, operationcode=NEW.operationcode,
    operationexternalcode=NEW.operationexternalcode,
    operationdocumentlink=NEW.operationdocumentlink,
    operationtypeid=NEW.operationtypeid,
    operationestimatedmachininghours=NEW.operationestimatedmachininghours,
    operationestimatedsetuphours=NEW.operationestimatedsetuphours,
    operationestimatedteardownhours=NEW.operationestimatedteardownhours
  WHERE operationid=OLD.operationid;
  UPDATE intermediateworkpiece
  SET intermediateworkpiecename=NEW.operationname,
    intermediateworkpiececode=NEW.operationcode,
    intermediateworkpieceexternalcode=NEW.operationexternalcode,
    intermediateworkpiecedocumentlink=NEW.operationdocumentlink,
    intermediateworkpieceweight=NEW.intermediateworkpieceweight
  WHERE intermediateworkpieceid=OLD.intermediateworkpieceid;
  UPDATE componentintermediateworkpiece
  SET componentid=NEW.componentid,
    intermediateworkpiececodeforcomponent=NEW.intermediateworkpiececodeforcomponent,
    intermediateworkpieceorderforcomponent=NEW.intermediateworkpieceorderforcomponent
  WHERE intermediateworkpieceid=OLD.intermediateworkpieceid;
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
      Database.ExecuteNonQuery (@"CREATE TRIGGER simpleoperation_update
  INSTEAD OF UPDATE
  ON simpleoperation
  FOR EACH ROW
  EXECUTE PROCEDURE simpleoperation_updater();");
    }
  }
}

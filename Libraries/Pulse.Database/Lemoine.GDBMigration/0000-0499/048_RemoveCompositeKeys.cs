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
  /// Migration 048: Remove the composite keys
  /// 
  /// They cause some problems in Lem_MachineAssociationSynchronization
  /// 
  /// The impacted tables are:
  /// <item>WorkOrderProject</item>
  /// <item>ComponentIntermediateWorkPiece</item>
  /// </summary>
  [Migration(48)]
  public class RemoveCompositeKeys: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemoveCompositeKeys).FullName);
    
    static readonly string MIG_SUFFIX = "Mig";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      UpgradeWorkOrderProject ();
      UpgradeComponentIntermediateWorkPiece ();
      
      UpgradeJobView ();
      UpgradeSimpleOperationView ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      DowngradeComponentIntermediateWorkPiece ();
      DowngradeWorkOrderProject ();
    }
    
    void UpgradeWorkOrderProject ()
    {
      string migTable = TableName.WORK_ORDER_PROJECT + MIG_SUFFIX;
      
      // Move the old table
      RemoveIndex (TableName.WORK_ORDER_PROJECT,
                   ColumnName.WORK_ORDER_ID);
      RemoveIndex (TableName.WORK_ORDER_PROJECT,
                   ColumnName.PROJECT_ID);
      Database.RenameTable (TableName.WORK_ORDER_PROJECT, migTable);
      RemoveSequence ("workorderproject_workorderprojectid_seq");
      
      // Create the new table
      Database.AddTable (TableName.WORK_ORDER_PROJECT,
                         new Column (ColumnName.WORK_ORDER_PROJECT_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column ("WorkOrderProjectVersion", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.WORK_ORDER_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.PROJECT_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("WorkOrderProjectQuantity", DbType.Int32, ColumnProperty.NotNull, 1));
      Database.GenerateForeignKey (TableName.WORK_ORDER_PROJECT, ColumnName.WORK_ORDER_ID,
                                   TableName.WORK_ORDER, ColumnName.WORK_ORDER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.WORK_ORDER_PROJECT, ColumnName.PROJECT_ID,
                                   TableName.PROJECT, ColumnName.PROJECT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddNamedUniqueConstraint ("WorkOrderProject_SecondaryKey",
                             TableName.WORK_ORDER_PROJECT,
                             new string [] { ColumnName.WORK_ORDER_ID,
                               ColumnName.PROJECT_ID});
      AddIndex (TableName.WORK_ORDER_PROJECT,
                ColumnName.WORK_ORDER_ID);
      AddIndex (TableName.WORK_ORDER_PROJECT,
                ColumnName.PROJECT_ID);
      if (Database.TableExists (migTable)) {
        Database.ExecuteNonQuery (string.Format (@"
INSERT INTO {0} (WorkOrderId, ProjectId, WorkOrderProjectQuantity)
SELECT WorkOrderId, ProjectId, WorkOrderProjectQuantity
FROM {1}",
                                                 TableName.WORK_ORDER_PROJECT,
                                                 migTable));
        Database.RemoveTable (migTable);
      }
    }
    
    void DowngradeWorkOrderProject ()
    {
    }
    
    void UpgradeComponentIntermediateWorkPiece ()
    {
      string migTable = TableName.COMPONENT_INTERMEDIATE_WORK_PIECE + MIG_SUFFIX;
      
      // Move the old table
      RemoveIndex (TableName.COMPONENT_INTERMEDIATE_WORK_PIECE,
                   ColumnName.COMPONENT_ID);
      RemoveIndex (TableName.COMPONENT_INTERMEDIATE_WORK_PIECE,
                   ColumnName.INTERMEDIATE_WORK_PIECE_ID);
      Database.RenameTable (TableName.COMPONENT_INTERMEDIATE_WORK_PIECE, migTable);
      RemoveSequence ("componentintermediateworkpiec_componentintermediateworkpiec_seq");

      // Create the new table
      Database.AddTable (TableName.COMPONENT_INTERMEDIATE_WORK_PIECE,
                         new Column (ColumnName.COMPONENT_INTERMEDIATE_WORK_PIECE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column ("ComponentIntermediateWorkPieceVersion", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.COMPONENT_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.INTERMEDIATE_WORK_PIECE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("intermediateworkpiececodeforcomponent", DbType.String),
                         new Column ("intermediateworkpieceorderforcomponent", DbType.Int32));
      Database.ExecuteNonQuery ("ALTER TABLE componentintermediateworkpiece " +
                                "ALTER COLUMN intermediateworkpiececodeforcomponent " +
                                "SET DATA TYPE CITEXT;");
      Database.GenerateForeignKey (TableName.COMPONENT_INTERMEDIATE_WORK_PIECE, ColumnName.COMPONENT_ID,
                                   TableName.COMPONENT, ColumnName.COMPONENT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.COMPONENT_INTERMEDIATE_WORK_PIECE, ColumnName.INTERMEDIATE_WORK_PIECE_ID,
                                   TableName.INTERMEDIATE_WORK_PIECE, ColumnName.INTERMEDIATE_WORK_PIECE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddNamedUniqueConstraint ("ComponentIntermediateWorkPiece_SecondaryKey",
                             TableName.COMPONENT_INTERMEDIATE_WORK_PIECE,
                             new string [] { ColumnName.COMPONENT_ID,
                               ColumnName.INTERMEDIATE_WORK_PIECE_ID});
      AddIndex (TableName.COMPONENT_INTERMEDIATE_WORK_PIECE,
                ColumnName.COMPONENT_ID);
      AddIndex (TableName.COMPONENT_INTERMEDIATE_WORK_PIECE,
                ColumnName.INTERMEDIATE_WORK_PIECE_ID);
      if (Database.TableExists (migTable)) {
        Database.ExecuteNonQuery (string.Format (@"
INSERT INTO {0} (componentid, intermediateworkpieceid, intermediateworkpiececodeforcomponent, intermediateworkpieceorderforcomponent)
SELECT componentid, intermediateworkpieceid, intermediateworkpiececodeforcomponent, intermediateworkpieceorderforcomponent
FROM {1}",
                                                 TableName.COMPONENT_INTERMEDIATE_WORK_PIECE,
                                                 migTable));
        Database.RemoveTable (migTable);
      }
    }
    
    void DowngradeComponentIntermediateWorkPiece ()
    {
    }
    
    void UpgradeJobView ()
    {
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION job_deleter()
  RETURNS trigger AS
$BODY$
BEGIN
 DELETE FROM project
  WHERE project.projectid = OLD.projectid;
 DELETE FROM workorder
  WHERE workorder.workorderid = OLD.workorderid;
 RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION job_inserter()
  RETURNS trigger AS
$BODY$
BEGIN
  INSERT INTO project (projectid, projectname, projectcode,
   projectexternalcode, projectdocumentlink,
   projectcreationdatetime, projectreactivationdatetime, projectarchivedatetime)
  VALUES (CASE WHEN NEW.projectid IS NOT NULL THEN NEW.projectid
   ELSE nextval('project_projectid_seq') END, NEW.jobname, NEW.jobcode, NEW.jobexternalcode,
   NEW.jobdocumentlink, CASE WHEN NEW.projectcreationdatetime IS NOT NULL
   THEN NEW.projectcreationdatetime ELSE now() AT TIME ZONE 'UTC' END,
   CASE WHEN NEW.projectreactivationdatetime IS NOT NULL
   THEN NEW.projectreactivationdatetime ELSE now() AT TIME ZONE 'UTC' END,
   NEW.projectarchivedatetime)
  RETURNING projectid, projectcreationdatetime, projectreactivationdatetime
  INTO STRICT NEW.projectid, NEW.projectcreationdatetime, NEW.projectreactivationdatetime;
  INSERT INTO workorder (workordername, workordercode,
   workorderexternalcode, workorderdocumentlink, workorderdeliverydate,
   workorderstatusid)
  VALUES (NEW.jobname, NEW.jobcode, NEW.jobexternalcode,
   NEW.jobdocumentlink, NEW.workorderdeliverydate, NEW.workorderstatusid)
  RETURNING workorderid
  INTO STRICT NEW.workorderid;
  INSERT INTO workorderproject (workorderid, projectid)
  VALUES (NEW.workorderid, NEW.projectid);
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION job_updater()
  RETURNS trigger AS
$BODY$
BEGIN
  UPDATE project
  SET projectid=NEW.projectid,
    projectname=NEW.jobname, projectcode=NEW.jobcode,
    projectexternalcode=NEW.jobexternalcode,
    projectdocumentlink=NEW.jobdocumentlink,
    projectcreationdatetime=NEW.projectcreationdatetime,
    projectreactivationdatetime=NEW.projectreactivationdatetime,
    projectarchivedatetime=NEW.projectarchivedatetime
  WHERE projectid=OLD.projectid;
  UPDATE workorder
  SET workorderid=NEW.workorderid, workordername=NEW.jobname,
    workordercode=NEW.jobcode,
    workorderexternalcode=NEW.jobexternalcode,
    workorderdocumentlink=NEW.jobdocumentlink,
    workorderdeliverydate=NEW.workorderdeliverydate,
    workorderstatusid=NEW.workorderstatusid
  WHERE workorderid=(SELECT workorderid FROM workorderproject
    WHERE projectid=OLD.projectid);
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW job AS
 SELECT workorder.workorderid, workorderproject.projectid, project.projectname AS jobname, project.projectcode AS jobcode, project.projectexternalcode AS jobexternalcode, project.projectdocumentlink AS jobdocumentlink, workorder.workorderdeliverydate, workorder.workorderstatusid, project.projectcreationdatetime, project.projectreactivationdatetime, project.projectarchivedatetime
   FROM workorder
NATURAL JOIN workorderproject
NATURAL JOIN project;

-- Trigger: job_delete on job

-- DROP TRIGGER job_delete ON job;

CREATE TRIGGER job_delete
  INSTEAD OF DELETE
  ON job
  FOR EACH ROW
  EXECUTE PROCEDURE job_deleter();

-- Trigger: job_insert on job

-- DROP TRIGGER job_insert ON job;

CREATE TRIGGER job_insert
  INSTEAD OF INSERT
  ON job
  FOR EACH ROW
  EXECUTE PROCEDURE job_inserter();

-- Trigger: job_update on job

-- DROP TRIGGER job_update ON job;

CREATE TRIGGER job_update
  INSTEAD OF UPDATE
  ON job
  FOR EACH ROW
  EXECUTE PROCEDURE job_updater();
");
    }
    
    void UpgradeSimpleOperationView ()
    {
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
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW simpleoperation AS
 SELECT operation.operationid, intermediateworkpiece.intermediateworkpieceid, operation.operationname, operation.operationcode, operation.operationexternalcode, operation.operationdocumentlink, operation.operationtypeid, operation.operationestimatedmachininghours, operation.operationestimatedsetuphours, operation.operationestimatedteardownhours, intermediateworkpiece.intermediateworkpieceweight, componentintermediateworkpiece.componentid, componentintermediateworkpiece.intermediateworkpiececodeforcomponent, componentintermediateworkpiece.intermediateworkpieceorderforcomponent
   FROM operation
NATURAL JOIN intermediateworkpiece
   LEFT JOIN componentintermediateworkpiece USING (intermediateworkpieceid);

-- Trigger: simpleoperation_delete on simpleoperation

-- DROP TRIGGER simpleoperation_delete ON simpleoperation;

CREATE TRIGGER simpleoperation_delete
  INSTEAD OF DELETE
  ON simpleoperation
  FOR EACH ROW
  EXECUTE PROCEDURE simpleoperation_deleter();

-- Trigger: simpleoperation_insert on simpleoperation

-- DROP TRIGGER simpleoperation_insert ON simpleoperation;

CREATE TRIGGER simpleoperation_insert
  INSTEAD OF INSERT
  ON simpleoperation
  FOR EACH ROW
  EXECUTE PROCEDURE simpleoperation_inserter();

-- Trigger: simpleoperation_update on simpleoperation

-- DROP TRIGGER simpleoperation_update ON simpleoperation;

CREATE TRIGGER simpleoperation_update
  INSTEAD OF UPDATE
  ON simpleoperation
  FOR EACH ROW
  EXECUTE PROCEDURE simpleoperation_updater();
");
    }
  }
}

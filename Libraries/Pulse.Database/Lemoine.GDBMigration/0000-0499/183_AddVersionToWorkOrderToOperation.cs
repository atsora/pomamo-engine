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
  /// Migration 183: Add a version to all the tables between the work order and the operation
  /// </summary>
  [Migration(183)]
  public class AddVersionToWorkOrderToOperation: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddVersionToWorkOrderToOperation).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      UpgradeJob ();
      UpgradePart ();
      UpgradeSimpleOperation ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
    
    void UpgradeJob ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW job CASCADE");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW job AS
 SELECT workorder.workorderid, workorderproject.projectid,
   workorder.workorderversion, project.projectversion,
   project.projectname AS jobname, project.projectcode AS jobcode, project.projectexternalcode AS jobexternalcode, project.projectdocumentlink AS jobdocumentlink, workorder.workorderdeliverydate, workorder.workorderstatusid, project.projectcreationdatetime, project.projectreactivationdatetime, project.projectarchivedatetime
   FROM workorder
NATURAL JOIN workorderproject
NATURAL JOIN project;");
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
      Database.ExecuteNonQuery (@"CREATE TRIGGER job_delete
  INSTEAD OF DELETE
  ON job
  FOR EACH ROW
  EXECUTE PROCEDURE job_deleter();");
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
  RETURNING projectid, projectversion, projectcreationdatetime, projectreactivationdatetime
  INTO STRICT NEW.projectid, NEW.projectversion, NEW.projectcreationdatetime, NEW.projectreactivationdatetime;
  INSERT INTO workorder (workordername, workordercode,
   workorderexternalcode, workorderdocumentlink, workorderdeliverydate,
   workorderstatusid)
  VALUES (NEW.jobname, NEW.jobcode, NEW.jobexternalcode,
   NEW.jobdocumentlink, NEW.workorderdeliverydate, NEW.workorderstatusid)
  RETURNING workorderid, workorderversion
  INTO STRICT NEW.workorderid, NEW.workorderversion;
  INSERT INTO workorderproject (workorderid, projectid)
  VALUES (NEW.workorderid, NEW.projectid);
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
      Database.ExecuteNonQuery (@"CREATE TRIGGER job_insert
  INSTEAD OF INSERT
  ON job
  FOR EACH ROW
  EXECUTE PROCEDURE job_inserter();");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION job_updater()
  RETURNS trigger AS
$BODY$
BEGIN
  SELECT * FROM project
  WHERE projectid=OLD.projectid
    AND projectversion=OLD.projectversion;
  IF NOT FOUND THEN
    RAISE EXCEPTION 'Project with ID % was updated or deleted', OLD.projectid;
  END IF;
  PERFORM * FROM workorder
  WHERE workorderid=OLD.workorderid
    AND workorderversion=OLD.workorderversion;
  IF NOT FOUND THEN
    RAISE EXCEPTION 'WorkOrder with ID % was updated or deleted', OLD.workorderid;
  END IF;
  UPDATE project
  SET projectid=NEW.projectid,
    projectversion=NEW.projectversion+1,
    projectname=NEW.jobname, projectcode=NEW.jobcode,
    projectexternalcode=NEW.jobexternalcode,
    projectdocumentlink=NEW.jobdocumentlink,
    projectcreationdatetime=NEW.projectcreationdatetime,
    projectreactivationdatetime=NEW.projectreactivationdatetime,
    projectarchivedatetime=NEW.projectarchivedatetime
  WHERE projectid=OLD.projectid
    AND projectversion=OLD.projectversion;
  UPDATE workorder
  SET workorderid=NEW.workorderid,
    workorderversion=NEW.workorderversion+1,
    workordername=NEW.jobname,
    workordercode=NEW.jobcode,
    workorderexternalcode=NEW.jobexternalcode,
    workorderdocumentlink=NEW.jobdocumentlink,
    workorderdeliverydate=NEW.workorderdeliverydate,
    workorderstatusid=NEW.workorderstatusid
  WHERE workorderid=OLD.workorderid
    AND workorderversion=OLD.workorderversion;
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
      Database.ExecuteNonQuery (@"CREATE TRIGGER job_update
  INSTEAD OF UPDATE
  ON job
  FOR EACH ROW
  EXECUTE PROCEDURE job_updater();");
    }
    
    void UpgradePart ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW part CASCADE");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW part AS
 SELECT project.projectid, component.componentid,
   project.projectversion, component.componentversion,
   component.componentname AS partname, component.componentcode AS partcode, component.componentexternalcode AS partexternalcode, component.componentdocumentlink AS partdocumentlink, project.projectcreationdatetime, project.projectreactivationdatetime, project.projectarchivedatetime, component.componenttypeid, component.finalworkpieceid, component.componentestimatedhours
   FROM project
NATURAL JOIN component;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE part_delete AS
    ON DELETE TO part DO INSTEAD  DELETE FROM project
  WHERE project.projectid = old.projectid;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION part_inserter()
  RETURNS trigger AS
$BODY$
BEGIN
  INSERT INTO project (projectname, projectcode,
    projectexternalcode, projectdocumentlink, projectcreationdatetime,
    projectreactivationdatetime, projectarchivedatetime)
  VALUES (NEW.partname, NEW.partcode,
    NEW.partexternalcode, NEW.partdocumentlink,
    CASE WHEN NEW.projectcreationdatetime IS NOT NULL
    THEN NEW.projectcreationdatetime ELSE now() AT TIME ZONE 'UTC' END,
    CASE WHEN NEW.projectreactivationdatetime IS NOT NULL
    THEN NEW.projectreactivationdatetime ELSE now() AT TIME ZONE 'UTC' END,
    NEW.projectarchivedatetime)
  RETURNING projectid, projectversion
  INTO STRICT NEW.projectid, NEW.projectversion;
  INSERT INTO component (componentid, componentname, componentcode, componentexternalcode,
    componentdocumentlink, componenttypeid, finalworkpieceid,
    componentestimatedhours, projectid)
  VALUES (CASE WHEN NEW.componentid IS NOT NULL THEN NEW.componentid
    ELSE nextval('component_componentid_seq') END, NEW.partname, NEW.partcode,
    NEW.partexternalcode, NEW.partdocumentlink, NEW.componenttypeid,
    NEW.finalworkpieceid, NEW.componentestimatedhours,
    NEW.projectid)
  RETURNING componentid, componentversion
  INTO STRICT NEW.componentid, NEW.componentversion;
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
      Database.ExecuteNonQuery (@"CREATE TRIGGER part_insert
  INSTEAD OF INSERT
  ON part
  FOR EACH ROW
  EXECUTE PROCEDURE part_inserter();");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION part_updater()
  RETURNS trigger AS
$BODY$
BEGIN
  PERFORM * FROM project
  WHERE projectid=OLD.projectid
    AND projectversion=OLD.projectversion;
  IF NOT FOUND THEN
    RAISE EXCEPTION 'Project with ID % was updated or deleted', OLD.projectid;
  END IF;
  PERFORM * FROM component
  WHERE componentid=OLD.componentid
    AND componentversion=OLD.componentversion;
  IF NOT FOUND THEN
    RAISE EXCEPTION 'Component with ID % was updated or deleted', OLD.componentid;
  END IF;
  UPDATE project
  SET projectversion=NEW.projectversion+1,
    projectname=NEW.partname,
    projectcode=NEW.partcode,
    projectexternalcode=NEW.partexternalcode,
    projectdocumentlink=NEW.partdocumentlink,
    projectcreationdatetime=NEW.projectcreationdatetime,
    projectreactivationdatetime=NEW.projectreactivationdatetime,
    projectarchivedatetime=NEW.projectarchivedatetime
  WHERE projectid=OLD.projectid
    AND projectversion=OLD.projectversion;
  UPDATE component
  SET componentversion=NEW.componentversion+1,
    componentname=NEW.partname, componentcode=NEW.partcode,
    componentexternalcode=NEW.partexternalcode,
    componentdocumentlink=NEW.partdocumentlink,
    projectid=NEW.projectid,
    componenttypeid=NEW.componenttypeid,
    finalworkpieceid=NEW.finalworkpieceid,
    componentestimatedhours=NEW.componentestimatedhours
    WHERE componentid=OLD.componentid
      AND componentversion=OLD.componentversion;
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
      Database.ExecuteNonQuery (@"CREATE TRIGGER part_update
  INSTEAD OF UPDATE
  ON part
  FOR EACH ROW
  EXECUTE PROCEDURE part_updater();");
    }
    
    void UpgradeSimpleOperation ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW simpleoperation CASCADE");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW simpleoperation AS 
 SELECT operation.operationid, intermediateworkpiece.intermediateworkpieceid, operation.operationname, operation.operationcode, operation.operationexternalcode, operation.operationdocumentlink, operation.operationtypeid, operation.operationmachiningduration, operation.operationsetupduration, operation.operationteardownduration, intermediateworkpiece.intermediateworkpieceweight, componentintermediateworkpiece.componentid, componentintermediateworkpiece.intermediateworkpiececodeforcomponent, componentintermediateworkpiece.intermediateworkpieceorderforcomponent, operation.operationloadingduration, operation.operationunloadingduration,
   operation.operationversion, intermediateworkpiece.intermediateworkpieceversion
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
  RETURNING operationid, operationversion
  INTO STRICT NEW.operationid, NEW.operationversion;
  INSERT INTO intermediateworkpiece (
   intermediateworkpiecename, intermediateworkpiececode,
   intermediateworkpieceexternalcode, intermediateworkpiecedocumentlink,
   intermediateworkpieceweight, operationid)
  VALUES (NEW.operationname, NEW.operationcode, NEW.operationexternalcode,
   NEW.operationdocumentlink, NEW.intermediateworkpieceweight,
   NEW.operationid)
  RETURNING intermediateworkpieceid, intermediateworkpieceversion
  INTO STRICT NEW.intermediateworkpieceid, NEW.intermediateworkpieceversion;
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
  PERFORM * FROM operation
  WHERE operationid=OLD.operationid
    AND operationversion=OLD.operationversion;
  IF NOT FOUND THEN
    RAISE EXCEPTION 'Operation with ID % was updated or deleted', OLD.operationid;
  END IF;
  PERFORM * FROM intermediateworkpiece
  WHERE operationid=OLD.intermediateworkpieceid
    AND intermediateworkpieceversion=OLD.intermediateworkpieceversion;
  IF NOT FOUND THEN
    RAISE EXCEPTION 'IntermediateWorkPiece with ID % was updated or deleted', OLD.intermediateworkpieceid;
  END IF;
  UPDATE operation
  SET operationversion=OLD.operationversion+1,
    operationname=NEW.operationname, operationcode=NEW.operationcode,
    operationexternalcode=NEW.operationexternalcode,
    operationdocumentlink=NEW.operationdocumentlink,
    operationtypeid=NEW.operationtypeid,
    operationmachiningduration=NEW.operationmachiningduration,
    operationsetupduration=NEW.operationsetupduration,
    operationteardownduration=NEW.operationteardownduration,
    operationloadingduration=NEW.operationloadingduration,
    operationunloadingduration=NEW.operationunloadingduration
  WHERE operationid=OLD.operationid
    AND operationversion=OLD.operationversion;
  UPDATE intermediateworkpiece
  SET intermediateworkpieceversion=OLD.intermediateworkpieceversion+1,
    intermediateworkpiecename=NEW.operationname,
    intermediateworkpiececode=NEW.operationcode,
    intermediateworkpieceexternalcode=NEW.operationexternalcode,
    intermediateworkpiecedocumentlink=NEW.operationdocumentlink,
    intermediateworkpieceweight=NEW.intermediateworkpieceweight
  WHERE intermediateworkpieceid=OLD.intermediateworkpieceid
    AND intermediateworkpieceversion=OLD.intermediateworkpieceversion;
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

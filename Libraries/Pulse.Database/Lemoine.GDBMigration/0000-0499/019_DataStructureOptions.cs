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
  /// Migration 019: New table to set some data structure options
  /// (to simplify the data structure for some customers)
  /// <item>WorkOrder / Project = Job</item>
  /// <item>Project / Component = Part</item>
  /// <item>Intermediate Work Piece / Operation = SimpleOperation</item>
  /// </summary>
  [Migration(19)]
  public class DataStructureOptions: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DataStructureOptions).FullName);

    static readonly string DATA_STRUCTURE_OPTION_TABLE = "DataStructureOption";
    
    static readonly string WORK_ORDER_PROJECT_IS_JOB = "WorkOrderProjectIsJob";
    static readonly string PROJECT_COMPONENT_IS_PART = "ProjectComponentIsPart";
    static readonly string INTERMEDIATE_WORK_PIECE_OPERATION_IS_SIMPLE_OPERATION = "IntermediateWorkPieceOperationIsSimpleOperation";
    static readonly string UNIQUE_WORK_ORDER_FROM_PROJECT_OR_COMPOENNT = "UniqueWorkOrderFromProjectOrComponent";
    static readonly string UNIQUE_COMPONENT_FROM_OPERATION = "UniqueComponentFromOperation";
    static readonly string COMPONENT_FROM_OPERATION_ONLY = "ComponentFromOperationOnly";
    static readonly string WORK_ORDER_FROM_COMPONENT_ONLY = "WorkOrderFromComponentOnly";

    static readonly string JOB_VIEW = "Job";
    static readonly string PART_VIEW = "Part";
    static readonly string SIMPLE_OPERATION_VIEW = "SimpleOperation";
    
    static readonly string TRANSLATION_TABLE = "Translation";
    static readonly string TRANSLATION_KEY = "TranslationKey";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (JOB_VIEW)) {
        #region job view
        // jobname=projectname=workordername
        // jobcode=projectcode=workordercode
        // jobexternalcode=projectexternalcode=workorderexternalcode
        // jobdocumentlink=projectdocumentlink=workorderdocumentlink
        Database.ExecuteNonQuery ("CREATE OR REPLACE VIEW job AS " +
                                  "SELECT workorderid, projectid, " +
                                  "projectname AS jobname, " +
                                  "projectcode AS jobcode, " +
                                  "projectexternalcode AS jobexternalcode, " +
                                  "projectdocumentlink AS jobdocumentlink, " +
                                  "workorderdeliverydate, " +
                                  "workorderstatusid, " +
                                  "projectcreationdatetime, projectreactivationdatetime, " +
                                  "projectarchivedatetime " +
                                  "FROM workorder " +
                                  "NATURAL JOIN workorderproject " +
                                  "NATURAL JOIN project");
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
  LANGUAGE plpgsql VOLATILE COST 100;");
        Database.ExecuteNonQuery (@"CREATE TRIGGER job_insert INSTEAD OF INSERT
ON job
FOR EACH ROW
EXECUTE PROCEDURE job_inserter ();");
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
  LANGUAGE plpgsql VOLATILE COST 100;");
        Database.ExecuteNonQuery (@"CREATE TRIGGER job_update INSTEAD OF UPDATE
ON job
FOR EACH ROW
EXECUTE PROCEDURE job_updater ();");
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
  LANGUAGE plpgsql VOLATILE COST 100;");
        Database.ExecuteNonQuery (@"CREATE TRIGGER job_delete INSTEAD OF DELETE
ON job
FOR EACH ROW
EXECUTE PROCEDURE job_deleter ();");
        #endregion // job view
      }
      
      if (!Database.TableExists (PART_VIEW)) {
        #region part view
        // partname=projectname=componentname
        // partcode=projectcode=componentcode
        // partexternalcode=projectexternalcode=componentexternalcode
        // partdocumentlink=projectdocumentlink=componentdocumentlink
        Database.ExecuteNonQuery ("CREATE OR REPLACE VIEW part AS " +
                                  "SELECT projectid, componentid, " +
                                  "componentname AS partname, " +
                                  "componentcode AS partcode, " +
                                  "componentexternalcode AS partexternalcode, " +
                                  "componentdocumentlink AS partdocumentlink, " +
                                  "projectcreationdatetime, projectreactivationdatetime, " +
                                  "projectarchivedatetime, " +
                                  "componenttypeid, finalworkpieceid, " +
                                  "componentestimatedhours " +
                                  "FROM project " +
                                  "NATURAL JOIN component");
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
  RETURNING projectid
  INTO STRICT NEW.projectid;
  INSERT INTO component (componentid, componentname, componentcode, componentexternalcode,
    componentdocumentlink, componenttypeid, finalworkpieceid,
    componentestimatedhours, projectid)
  VALUES (CASE WHEN NEW.componentid IS NOT NULL THEN NEW.componentid
    ELSE nextval('component_componentid_seq') END, NEW.partname, NEW.partcode,
    NEW.partexternalcode, NEW.partdocumentlink, NEW.componenttypeid,
    NEW.finalworkpieceid, NEW.componentestimatedhours,
    NEW.projectid)
  RETURNING componentid
  INTO STRICT NEW.componentid;
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE COST 100;");
        Database.ExecuteNonQuery (@"CREATE TRIGGER part_insert INSTEAD OF INSERT
ON part
FOR EACH ROW
EXECUTE PROCEDURE part_inserter ();");
        Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION part_updater()
  RETURNS trigger AS
$BODY$
BEGIN
  UPDATE project 
  SET projectname=NEW.partname, projectcode=NEW.partcode, 
    projectexternalcode=NEW.partexternalcode, 
    projectdocumentlink=NEW.partdocumentlink, 
    projectcreationdatetime=NEW.projectcreationdatetime, 
    projectreactivationdatetime=NEW.projectreactivationdatetime, 
    projectarchivedatetime=NEW.projectarchivedatetime 
  WHERE projectid=OLD.projectid; 
  UPDATE component 
  SET componentname=NEW.partname, componentcode=NEW.partcode, 
    componentexternalcode=NEW.partexternalcode, 
    componentdocumentlink=NEW.partdocumentlink, 
    projectid=NEW.projectid, 
    componenttypeid=NEW.componenttypeid, 
    finalworkpieceid=NEW.finalworkpieceid, 
    componentestimatedhours=NEW.componentestimatedhours 
    WHERE componentid=OLD.componentid;
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE COST 100;");
        Database.ExecuteNonQuery (@"CREATE TRIGGER part_update INSTEAD OF UPDATE
ON part
FOR EACH ROW
EXECUTE PROCEDURE part_updater ();");
        Database.ExecuteNonQuery (@"CREATE RULE part_delete AS
ON DELETE TO part
DO INSTEAD
(
 DELETE FROM project
  WHERE projectid=OLD.projectid;
);");
        #endregion // part view
      }
      
      if (!Database.TableExists (SIMPLE_OPERATION_VIEW)) {
        #region simpleoperation view
        // operationname=intermediateworkpiecename
        // operationcode=intermediateowrkpiececode
        // operationexternalcode=intermediateworkpieceexternalcode
        // operationdocumentlink=intermediateworkpiecedocumentlink
        Database.ExecuteNonQuery ("CREATE OR REPLACE VIEW simpleoperation AS " +
                                  "SELECT operationid, intermediateworkpieceid, " +
                                  "operationname, " +
                                  "operationcode, " +
                                  "operationexternalcode, " +
                                  "operationdocumentlink, " +
                                  "operationtypeid, " +
                                  "operationestimatedmachininghours, " +
                                  "operationestimatedsetuphours, " +
                                  "operationestimatedteardownhours, " +
                                  "intermediateworkpieceweight, " +
                                  "componentid, " +
                                  "intermediateworkpiececodeforcomponent, " +
                                  "intermediateworkpieceorderforcomponent " +
                                  "FROM operation " +
                                  "NATURAL JOIN intermediateworkpiece " +
                                  "LEFT OUTER JOIN componentintermediateworkpiece USING (intermediateworkpieceid)");
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
  LANGUAGE plpgsql VOLATILE COST 100;");
        Database.ExecuteNonQuery (@"CREATE TRIGGER simpleoperation_insert INSTEAD OF INSERT
ON simpleoperation
FOR EACH ROW
EXECUTE PROCEDURE simpleoperation_inserter ();");
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
  LANGUAGE plpgsql VOLATILE COST 100;");
        Database.ExecuteNonQuery (@"CREATE TRIGGER simpleoperation_update INSTEAD OF UPDATE
ON simpleoperation
FOR EACH ROW
EXECUTE PROCEDURE simpleoperation_updater ();");
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
  LANGUAGE plpgsql VOLATILE COST 100;");
        Database.ExecuteNonQuery (@"CREATE TRIGGER simpleoperation_delete INSTEAD OF DELETE
ON simpleoperation
FOR EACH ROW
EXECUTE PROCEDURE simpleoperation_deleter ();");
        #endregion // part view
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // New tables / views deletion
      if (Database.TableExists (DATA_STRUCTURE_OPTION_TABLE)) {
        Database.RemoveTable (DATA_STRUCTURE_OPTION_TABLE);
      }
      if (Database.TableExists (JOB_VIEW)) {
        Database.Delete (TableName.DISPLAY,
                         "displaytable",
                         "Job");
        Database.ExecuteNonQuery ("DROP VIEW IF EXISTS job;");
      }
      if (Database.TableExists (PART_VIEW)) {
        Database.Delete (TableName.DISPLAY,
                         "displaytable",
                         "Part");
        Database.ExecuteNonQuery ("DROP VIEW IF EXISTS part;");
      }
      if (Database.TableExists (SIMPLE_OPERATION_VIEW)) {
        Database.Delete (TableName.DISPLAY,
                         "displaytable",
                         "SimpleOperation");
        Database.ExecuteNonQuery ("DROP VIEW IF EXISTS simpleoperation;");
      }

      // Obsolete data
      Database.Delete (TRANSLATION_TABLE,
                       TRANSLATION_KEY,
                       WORK_ORDER_PROJECT_IS_JOB);
      Database.Delete (TRANSLATION_TABLE,
                       TRANSLATION_KEY,
                       PROJECT_COMPONENT_IS_PART);
      Database.Delete (TRANSLATION_TABLE,
                       TRANSLATION_KEY,
                       INTERMEDIATE_WORK_PIECE_OPERATION_IS_SIMPLE_OPERATION);
      Database.Delete (TRANSLATION_TABLE,
                       TRANSLATION_KEY,
                       UNIQUE_WORK_ORDER_FROM_PROJECT_OR_COMPOENNT);
      Database.Delete (TRANSLATION_TABLE,
                       TRANSLATION_KEY,
                       UNIQUE_COMPONENT_FROM_OPERATION);
      Database.Delete (TRANSLATION_TABLE,
                       TRANSLATION_KEY,
                       COMPONENT_FROM_OPERATION_ONLY);
      Database.Delete (TRANSLATION_TABLE,
                       TRANSLATION_KEY,
                       WORK_ORDER_FROM_COMPONENT_ONLY);
    }
  }
}

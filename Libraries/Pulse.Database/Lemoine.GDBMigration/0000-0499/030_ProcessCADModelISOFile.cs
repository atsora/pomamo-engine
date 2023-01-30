// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 030:
  /// <item>migration of the Tool table</item>
  /// <item>new CAD Model table</item>
  /// <item>new Unit table</item>
  /// <item>new Field table</item>
  /// <item>migration of the ISO File table</item>
  /// <item>migration of the old sfkoperation table into three new tables</item>
  /// </summary>
  [Migration(30)]
  public class ProcessCADModelISOFile: MigrationExt
  {
    static readonly string TOOL_NAME = "ToolName";
    static readonly string TOOL_CODE = "ToolCode";
    static readonly string SFKTOOLS_TABLE = "sfktools";
    
    static readonly string UNIT_NAME = "UnitName";
    static readonly string UNIT_TRANSLATION_KEY = "UnitTranslationKey";
    
    static readonly string FIELD_CODE = "FieldCode";
    static readonly string FIELD_NAME = "FieldName";
    static readonly string FIELD_TRANSLATION_KEY = "FieldTranslationKey";
    static readonly string FIELD_DESCRIPTION = "FieldDescription";
    static readonly string FIELD_TYPE = "FieldType";
    static readonly string STAMPING_DATA_TYPE = "StampingDataType";
    static readonly string CNC_DATA_AGGREGATION_TYPE = "CncDataAggregationType";
    static readonly string FIELD_ASSOCIATED_CLASS = "FieldAssociatedClass";
    static readonly string FIELD_ASSOCIATED_PROPERTY = "FieldAssociatedProperty";
    static readonly string FIELD_CUSTOM = "FieldCustom";
    
    static readonly string ISO_FILE_NAME = "IsoFileName";
    static readonly string ISO_FILE_SOURCE_DIRECTORY = "IsoFileSourceDirectory";
    static readonly string ISO_FILE_STAMPING_DIRECTORY = "IsoFileStampingDirectory";
    static readonly string ISO_FILE_SIZE = "IsoFileSize";
    static readonly string ISO_FILE_STAMPING_DATETIME = "IsoFileStampingDateTime";
    static readonly string SFKISOFILE_TABLE = "sfkisofile";
    
    static readonly string OPERATION_CYCLE_BEGIN = "operationcyclebegin";
    static readonly string OPERATION_CYCLE_END = "operationcycleend";
    
    static readonly string SFKOPERATION_TABLE = "sfkoperation";
    
    static readonly ILog log = LogManager.GetLogger(typeof (ProcessCADModelISOFile).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.TOOL)) {
        AddToolTable ();
      }
      if (!Database.TableExists (TableName.CAD_MODEL)) {
        AddCADModelTable ();
      }
      if (!Database.TableExists (TableName.UNIT)) {
        AddUnitTable ();
      }
      if (!Database.TableExists (TableName.FIELD)) {
        AddFieldTable ();
      }
      if (!Database.TableExists (TableName.ISO_FILE)) {
        AddISOFileTable ();
      }
      UpgradeSfkoperation ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      DowngradeSfkoperation ();
      if (Database.TableExists (TableName.ISO_FILE)) {
        RemoveISOFileTable ();
      }
      if (Database.TableExists (TableName.FIELD)) {
        RemoveFieldTable ();
      }
      if (Database.TableExists (TableName.UNIT)) {
        RemoveUnitTable ();
      }
      if (Database.TableExists (TableName.CAD_MODEL)) {
        RemoveCADModelTable ();
      }
      if (Database.TableExists (TableName.TOOL)) {
        RemoveToolTable ();
      }
    }
    
    void AddToolTable ()
    {
      log.Debug ("AddToolTable /B");
      
      Database.AddTable (TableName.TOOL,
                         new Column (ColumnName.TOOL_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TOOL_NAME, DbType.String),
                         new Column (TOOL_CODE, DbType.String, ColumnProperty.Unique),
                         new Column ("ToolDiameter", DbType.Double),
                         new Column ("ToolRadius", DbType.Double));
      MakeColumnCaseInsensitive (TableName.TOOL, TOOL_NAME);
      MakeColumnCaseInsensitive (TableName.TOOL, TOOL_CODE);
      AddIndex (TableName.TOOL,
                TOOL_NAME);
      AddUniqueIndex (TableName.TOOL,
                      TOOL_CODE);
      AddIndex (TableName.TOOL,
                "ToolDiameter",
                "ToolRadius");
      if (Database.TableExists (SFKTOOLS_TABLE)) {
        Database.RemoveTable (SFKTOOLS_TABLE);
      }
      ResetSequence (TableName.TOOL, ColumnName.TOOL_ID);
      
      // sfktools view
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS sfktools");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW sfktools AS
SELECT toolid-1 AS toolid,
 CASE WHEN toolcode IS NULL
  THEN ''::varchar
  ELSE toolcode::varchar END AS toolcode,
 CASE WHEN toolname IS NULL
  THEN ''::varchar
  ELSE toolname::varchar END AS toolname,
 tooldiameter AS tooldia, toolradius AS toolrad, 0 AS toolmate
FROM tool
UNION
SELECT 0 AS toolid, ''::varchar AS toolcode, '<Undefined>'::varchar AS toolname,
 0 AS tooldia, 0 AS toolrad, 0 AS toolmate;");
      Database.ExecuteNonQuery (@"CREATE RULE sfktools_insert AS
ON INSERT TO sfktools
DO INSTEAD
INSERT INTO tool (toolcode, toolname, tooldiameter, toolradius)
VALUES (NEW.toolcode, NEW.toolname, NEW.tooldia, NEW.toolrad)
RETURNING toolid-1 AS toolid, toolcode::varchar AS toolcode, toolname::varchar AS toolname,
 tooldiameter AS tooldia, toolradius AS toolrad, 0 AS toolmate;");
      Database.ExecuteNonQuery (@"CREATE RULE sfktools_update AS
ON UPDATE TO sfktools
DO INSTEAD
UPDATE tool
SET toolcode=NEW.toolcode, toolname=NEW.toolname, tooldiameter=NEW.tooldia, toolradius=NEW.toolrad
WHERE toolid = OLD.toolid+1 AND OLD.toolid > 0;");
      Database.ExecuteNonQuery (@"CREATE RULE sfktools_delete AS
ON DELETE TO sfktools
DO INSTEAD
DELETE FROM tool
WHERE toolid = OLD.toolid+1 AND OLD.toolid > 0;");
    }
    
    void RemoveToolTable ()
    {
      // sfktools views
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS sfktools");
      
      // display option
      Database.Delete (TableName.DISPLAY,
                       new string [] {"displaytable"},
                       new string [] {"Tool"});
            
      Database.RemoveTable (TableName.TOOL);
    }
    
    void AddCADModelTable ()
    {
      log.Debug ("AddCADModelTable /B");
      
      Database.AddTable (TableName.CAD_MODEL,
                         new Column (ColumnName.CAD_MODEL_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column ("CADModelName", DbType.String, ColumnProperty.NotNull | ColumnProperty.Unique),
                         new Column (ColumnName.COMPONENT_ID, DbType.Int32),
                         new Column (ColumnName.OPERATION_ID, DbType.Int32));
      MakeColumnCaseInsensitive (TableName.CAD_MODEL, "CADModelName");
      Database.GenerateForeignKey (TableName.CAD_MODEL, ColumnName.COMPONENT_ID,
                                   TableName.COMPONENT, ColumnName.COMPONENT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (TableName.CAD_MODEL, ColumnName.OPERATION_ID,
                                   TableName.OPERATION, ColumnName.OPERATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddUniqueIndex (TableName.CAD_MODEL,
                      "CADModelName");
      AddNamedIndexCondition ("cadmodel_null_component",
                              TableName.CAD_MODEL,
                              "componentid IS NULL",
                              ColumnName.COMPONENT_ID);
      AddNamedIndexCondition ("cadmodel_null_operation",
                              TableName.CAD_MODEL,
                              "operationid IS NULL",
                              ColumnName.OPERATION_ID);
    }
    
    void RemoveCADModelTable ()
    {
      Database.RemoveTable (TableName.CAD_MODEL);
    }
    
    void AddUnitTable ()
    {
      log.Debug ("AddUnitTable /B");
      
      Database.AddTable (TableName.UNIT,
                         new Column (ColumnName.UNIT_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (UNIT_NAME, DbType.String),
                         new Column (UNIT_TRANSLATION_KEY, DbType.String),
                         new Column ("UnitDescription", DbType.String));
      MakeColumnCaseInsensitive (TableName.UNIT, UNIT_NAME);
      AddConstraintNameTranslationKey (TableName.UNIT, UNIT_NAME, UNIT_TRANSLATION_KEY);
    }
    
    void RemoveUnitTable ()
    {
      Database.RemoveTable (TableName.UNIT);
    }
    
    void AddFieldTable ()
    {
      log.Debug ("AddFieldTable /B");
      
      Database.AddTable (TableName.FIELD,
                         new Column (ColumnName.FIELD_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (FIELD_CODE, DbType.String, ColumnProperty.NotNull | ColumnProperty.Unique),
                         new Column (FIELD_NAME, DbType.String, ColumnProperty.Unique),
                         new Column (FIELD_TRANSLATION_KEY, DbType.String, ColumnProperty.Unique),
                         new Column (FIELD_DESCRIPTION, DbType.String),
                         new Column (FIELD_TYPE, DbType.String, ColumnProperty.NotNull),
                         new Column (ColumnName.UNIT_ID, DbType.Int32),
                         new Column (STAMPING_DATA_TYPE, DbType.Int32),
                         new Column (CNC_DATA_AGGREGATION_TYPE, DbType.Int32),
                         new Column (FIELD_ASSOCIATED_CLASS, DbType.String),
                         new Column (FIELD_ASSOCIATED_PROPERTY, DbType.String),
                         new Column (FIELD_CUSTOM, DbType.Boolean, ColumnProperty.NotNull, true));
      MakeColumnCaseInsensitive (TableName.FIELD, FIELD_CODE);
      MakeColumnCaseInsensitive (TableName.FIELD, FIELD_NAME);
      AddConstraintNameTranslationKey (TableName.FIELD, FIELD_NAME, FIELD_TRANSLATION_KEY);
      Database.GenerateForeignKey (TableName.FIELD, ColumnName.UNIT_ID,
                                   TableName.UNIT, ColumnName.UNIT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddUniqueIndex (TableName.FIELD,
                      FIELD_CODE);
      #region DBFIELD
      // Only the deprecated one
      // TODO: to remove once the old stamping is removed
      Database.Insert (TableName.TRANSLATION,
                       new string[] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string[] {"", "FieldComponentTypeKey", "Component type key"});
      Database.Insert (TableName.FIELD,
                       new string[] {ColumnName.FIELD_ID, FIELD_CODE, FIELD_TRANSLATION_KEY, FIELD_TYPE, STAMPING_DATA_TYPE, FIELD_CUSTOM},
                       new string[] {"5", "ComponentTypeKey", "FieldComponentTypeKey", "String", "1", "0"});
      Database.Insert (TableName.TRANSLATION,
                       new string[] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string[] {"", "FieldComponentTypeId", "Component type ID"});
      Database.Insert (TableName.FIELD,
                       new string[] {ColumnName.FIELD_ID, FIELD_CODE, FIELD_TRANSLATION_KEY, FIELD_TYPE, STAMPING_DATA_TYPE, FIELD_ASSOCIATED_CLASS, FIELD_ASSOCIATED_PROPERTY, FIELD_CUSTOM},
                       new string[] {"6", "ComponentTypeId", "FieldComponentTypeId", "Int32", "3", "ComponentType", "Id", "0"});
      Database.Insert (TableName.TRANSLATION,
                       new string[] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string[] {"", "FieldOperationTypeKey", "Operation type key"});
      Database.Insert (TableName.FIELD,
                       new string[] {ColumnName.FIELD_ID, FIELD_CODE, FIELD_TRANSLATION_KEY, FIELD_TYPE, STAMPING_DATA_TYPE, FIELD_CUSTOM},
                       new string[] {"9", "OperationTypeKey", "FieldOperationTypeKey", "String", "1", "0"});
      Database.Insert (TableName.TRANSLATION,
                       new string[] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string[] {"", "FieldOperationTypeId", "Operation type ID"});
      Database.Insert (TableName.FIELD,
                       new string[] {ColumnName.FIELD_ID, FIELD_CODE, FIELD_TRANSLATION_KEY, FIELD_TYPE, STAMPING_DATA_TYPE, FIELD_ASSOCIATED_CLASS, FIELD_ASSOCIATED_PROPERTY, FIELD_CUSTOM},
                       new string[] {"10", "OperationTypeId", "FieldOperationTypeId", "Int32", "3", "OperationType", "Id", "0"});
      Database.Insert (TableName.TRANSLATION,
                       new string[] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string[] {"", "FieldToolCode", "Tool code"});
      Database.Insert (TableName.FIELD,
                       new string[] {ColumnName.FIELD_ID, FIELD_CODE, FIELD_TRANSLATION_KEY, FIELD_TYPE, STAMPING_DATA_TYPE, FIELD_ASSOCIATED_CLASS, FIELD_ASSOCIATED_PROPERTY, FIELD_CUSTOM},
                       new string[] {"20", "ToolCode", "FieldToolCode", "String", "3", "Tool", "Code", "0"});
      Database.Insert (TableName.TRANSLATION,
                       new string[] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string[] {"", "FieldToolName", "Tool name"});
      Database.Insert (TableName.FIELD,
                       new string[] {ColumnName.FIELD_ID, FIELD_CODE, FIELD_TRANSLATION_KEY, FIELD_TYPE, STAMPING_DATA_TYPE, FIELD_ASSOCIATED_CLASS, FIELD_ASSOCIATED_PROPERTY, FIELD_CUSTOM},
                       new string[] {"21", "ToolName", "FieldToolName", "String", "3", "Tool", "Name", "0"});
      #endregion // DBFIELD
      SetSequence (TableName.FIELD, ColumnName.FIELD_ID, 200);
    }
    
    void RemoveFieldTable ()
    {
      Database.RemoveTable (TableName.FIELD);
      Database.ExecuteNonQuery (@"DELETE FROM translation
WHERE translationkey LIKE 'Field%'");
    }
    
    void AddISOFileTable ()
    {
      log.Debug ("AddISOFileTable /B");
      
      Database.AddTable (TableName.ISO_FILE,
                         new Column (ColumnName.ISO_FILE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (ISO_FILE_NAME, DbType.String, ColumnProperty.NotNull),
                         new Column (ColumnName.COMPUTER_ID, DbType.Int32),
                         new Column (ISO_FILE_SOURCE_DIRECTORY, DbType.String, ColumnProperty.NotNull),
                         new Column (ISO_FILE_STAMPING_DIRECTORY, DbType.String, ColumnProperty.NotNull),
                         new Column (ISO_FILE_SIZE, DbType.Int32),
                         new Column (ISO_FILE_STAMPING_DATETIME, DbType.DateTime, ColumnProperty.NotNull));
      // Note: the two foreign keys Post-Processor ID and CAM System ID will be added later
      MakeColumnCaseInsensitive (TableName.ISO_FILE, ISO_FILE_NAME);
      MakeColumnCaseInsensitive (TableName.ISO_FILE, ISO_FILE_SOURCE_DIRECTORY);
      MakeColumnCaseInsensitive (TableName.ISO_FILE, ISO_FILE_STAMPING_DIRECTORY);
      Database.GenerateForeignKey (TableName.ISO_FILE, ColumnName.COMPUTER_ID,
                                   TableName.COMPUTER, ColumnName.COMPUTER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddIndex (TableName.ISO_FILE,
                ISO_FILE_NAME);
      AddIndexCondition (TableName.ISO_FILE,
                         "computerid IS NOT NULL",
                         ColumnName.COMPUTER_ID);
      if (Database.TableExists (SFKISOFILE_TABLE)) {
        Database.ExecuteNonQuery (@"INSERT INTO IsoFile
(IsoFileId, IsoFileName, IsoFileSourceDirectory, IsoFileStampingDirectory, IsoFileSize, IsoFileStampingDateTime)
SELECT fileid, substring(filename from '[^\\]*$'),
 substring(filename from '^.*\\'),
 '',
 CASE WHEN filesize=-1 THEN NULL ELSE filesize END,
 timezone('UTC'::text, now())
FROM sfkisofile
GROUP BY fileid, filename, filesize;");
        ResetSequence (TableName.ISO_FILE, ColumnName.ISO_FILE_ID);

        #if false // Really too slow on some databases, that's a pity
        log.Debug ("AddISOFileTable: " +
                   "complete IsoFile with StampingDirectory and StampingDateTime");
        Database.ExecuteNonQuery (@"CREATE INDEX sfkoperation_opfileid ON sfkoperation (opfileid)");
        Database.ExecuteNonQuery (@"UPDATE IsoFile
SET IsoFileStampingDirectory=substring(sfkoperation.opname from '^.*\\'),
    IsoFileStampingDateTime=opstampdate
FROM sfkoperation
WHERE sfkoperation.opfileid=IsoFile.isofileid");
        #endif

        log.Debug ("AddISOFileTable: " +
                   "remove sfkisofile");
        Database.RemoveTable (SFKISOFILE_TABLE);
      }
      
      // sfkisofile view
      log.Debug ("AddISOFileTable: " +
                 "create view sfkisofile");
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS sfkisofile");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW sfkisofile AS
SELECT isofileid AS fileid,
 IsoFileSourceDirectory || IsoFileName AS filename,
 ''::varchar AS postpro,
 CASE WHEN IsoFileSize IS NULL THEN -1 ELSE isofilesize END AS filesize,
 0 AS filecount
FROM isofile;");
      Database.ExecuteNonQuery (@"CREATE RULE sfkisofile_insert AS
ON INSERT TO sfkisofile
DO INSTEAD
INSERT INTO isofile (IsoFileName, IsoFileSourceDirectory, IsoFileStampingDirectory, IsoFileSize, IsoFileStampingDateTime)
VALUES (substring(NEW.filename from '[^\\]*$'), substring(NEW.filename from '^.*\\'), '',
 CASE WHEN NEW.filesize=-1 THEN NULL ELSE NEW.filesize END,
 timezone('UTC'::text, now()))
RETURNING isofileid AS fileid, isofilesourcedirectory || isofilename AS filename, ''::varchar AS postpro,
 CASE WHEN isofilesize IS NULL THEN -1 ELSE isofilesize END AS filesize,
 0 AS filecount");
      Database.ExecuteNonQuery (@"CREATE RULE sfkisofile_update AS
ON UPDATE TO sfkisofile
DO INSTEAD
UPDATE isofile
SET isofilename=substring(NEW.filename from '[^\\]*$'),
    isofilesourcedirectory=substring(NEW.filename from '^.*\\'),
    isofilesize=(CASE WHEN NEW.filesize=-1 THEN NULL ELSE NEW.filesize END)
WHERE isofileid=OLD.fileid");
      Database.ExecuteNonQuery (@"CREATE RULE sfkisofile_delete AS
ON DELETE TO sfkisofile
DO INSTEAD
DELETE FROM isofile
WHERE isofileid = OLD.fileid;");
    }
    
    void RemoveISOFileTable ()
    {
      // sfkisofile view
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS sfkisofile");
      
      // sfkisofile table
      if (!Database.TableExists (SFKISOFILE_TABLE)) {
        Database.ExecuteNonQuery ("DROP SEQUENCE IF EXISTS sfkisofile_fileid_seq;");
        Database.ExecuteNonQuery (@"CREATE TABLE sfkisofile
(
  fileid bigserial NOT NULL,
  filename character varying NOT NULL,
  postpro character varying NOT NULL,
  filesize integer DEFAULT (-1),
  filecount bigint DEFAULT 0,
  CONSTRAINT sfkisofile_pkey PRIMARY KEY (fileid)
)
WITH (
  OIDS=TRUE
);");
        if (Database.TableExists (TableName.ISO_FILE)) {
          Database.ExecuteNonQuery (@"INSERT INTO sfkisofile
SELECT isofileid AS fileid,
 IsoFileSourceDirectory || IsoFileName AS filename,
 ''::varchar AS postpro,
 CASE WHEN isofilesize IS NULL THEN -1 ELSE isofilesize END AS filesize,
 0 AS filecount
FROM isofile");
          ResetSequence (SFKISOFILE_TABLE, "fileid");
        }
      }
      
      Database.RemoveTable (TableName.ISO_FILE);
    }

    void UpgradeSfkoperation ()
    {
      log.Debug ("UpgradeSfkoperation /B");
      
      if (!Database.TableExists (TableName.OLD_SEQUENCE)) {
        AddProcessTable ();
      }
      if (!Database.TableExists (TableName.STAMPING_VALUE)) {
        AddStampingValueTable ();
      }
      if (!Database.TableExists (TableName.STAMP)) {
        AddStampTable ();
      }
      
      if (Database.TableExists (SFKOPERATION_TABLE)) {        
        // Remove the old sfkoperation
        Database.RemoveTable (SFKOPERATION_TABLE);
        
        // sfkoperation view
        log.Debug ("UpgradeSfkoperation: " +
                   "create view sfkoperation");
        Database.ExecuteNonQuery (@"CREATE VIEW sfkoperation AS
SELECT process.processid AS opid,
  CASE WHEN processname IS NOT NULL THEN processname ELSE (CASE WHEN isofilename IS NOT NULL THEN isofilestampingdirectory || isofilename ELSE '' END) END AS opname,
  0 AS optype,
  CASE WHEN processdescription IS NULL THEN ''::varchar ELSE processdescription END AS opdesc,
  0 AS opdone,
  CASE WHEN componentid IS NULL THEN 0 ELSE componentid END AS opcompid,
  CASE WHEN isofileid IS NULL THEN 0 ELSE isofileid END AS opfileid,
  CASE WHEN stampposition IS NULL THEN 0 ELSE stampposition END AS opfilepos,
  CASE WHEN opstrategyid IS NULL THEN 0 ELSE opstrategyid END AS opstratid,
  CASE WHEN toolid IS NULL THEN 0 ELSE toolid-1 END AS optoolid,
  1 AS opmetric,
  CASE WHEN depth.stampingvaluedouble IS NULL THEN 0 ELSE depth.stampingvaluedouble END AS opdepth,
  CASE WHEN width.stampingvaluedouble IS NULL THEN 0 ELSE width.stampingvaluedouble END AS opwidth,
  CASE WHEN tolerance.stampingvaluedouble IS NULL THEN 0 ELSE tolerance.stampingvaluedouble END AS optolerance,
  0 AS opspeedratio,
  CASE WHEN stock.stampingvaluedouble IS NULL THEN 0 ELSE stock.stampingvaluedouble END AS opstock,
  CASE WHEN isofilestampingdatetime IS NULL THEN timezone('UTC'::text, now()) ELSE isofilestampingdatetime END AS opstampdate,
  0 AS opcategoryid,
  operation.operationid AS opprocessid,
  CASE WHEN toolminlength.stampingvaluedouble IS NULL THEN -1 ELSE toolminlength.stampingvaluedouble END AS toolminlength,
  CASE WHEN progfeedrate.stampingvaluedouble IS NULL THEN -1 ELSE progfeedrate.stampingvaluedouble END AS progfeedrate,
  CASE WHEN progspindlespeed.stampingvaluedouble IS NULL THEN -1 ELSE progspindlespeed.stampingvaluedouble END AS progspindlespeed
FROM process
LEFT OUTER JOIN operation USING (operationid)
LEFT OUTER JOIN stamp USING (processid)
LEFT OUTER JOIN isofile USING (isofileid)
LEFT OUTER JOIN stampingvalue strategy ON (strategy.fieldid=40 AND strategy.processid=process.processid)
LEFT OUTER JOIN sfkopstrategy ON (strategy.stampingvaluestring=opstrategyname)
LEFT OUTER JOIN stampingvalue depth ON (depth.fieldid=50 AND depth.processid=process.processid)
LEFT OUTER JOIN stampingvalue width ON (width.fieldid=51 AND width.processid=process.processid)
LEFT OUTER JOIN stampingvalue tolerance ON (tolerance.fieldid=52 AND tolerance.processid=process.processid)
LEFT OUTER JOIN stampingvalue stock ON (stock.fieldid=53 AND stock.processid=process.processid)
LEFT OUTER JOIN stampingvalue toolminlength ON (toolminlength.fieldid=54 AND toolminlength.processid=process.processid)
LEFT OUTER JOIN stampingvalue progfeedrate ON (progfeedrate.fieldid=55 AND progfeedrate.processid=process.processid)
LEFT OUTER JOIN stampingvalue progspindlespeed ON (progspindlespeed.fieldid=56 AND progspindlespeed.processid=process.processid);");
        
        // Note: for the moment, leave sfkopstrategy and sfkoptype
      }
    }
    
    void DowngradeSfkoperation ()
    {
      log.Debug ("DowngradeSfkoperation /B");
      
      // Remove the view sfkoperation
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS sfkoperation");
      
      if (Database.TableExists (TableName.STAMP)) {
        RemoveStampTable ();
      }
      if (Database.TableExists (TableName.STAMPING_VALUE)) {
        RemoveStampingValueTable ();
      }
      if (Database.TableExists (TableName.OLD_SEQUENCE)) {
        RemoveProcessTable ();
      }
    }
    
    void AddProcessTable ()
    {
      log.Debug ("AddProcessTable /B");
      
      Database.AddTable (TableName.OLD_SEQUENCE,
                         new Column (ColumnName.OLD_SEQUENCE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (ColumnName.CAD_MODEL_ID, DbType.Int32),
                         new Column (ColumnName.OPERATION_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("ProcessName", DbType.String),
                         new Column ("ProcessDescription", DbType.String),
                         new Column (ColumnName.TOOL_ID, DbType.Int32));
      MakeColumnCaseInsensitive (TableName.OLD_SEQUENCE, "ProcessName");
      Database.GenerateForeignKey (TableName.OLD_SEQUENCE, ColumnName.CAD_MODEL_ID,
                                   TableName.CAD_MODEL, ColumnName.CAD_MODEL_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (TableName.OLD_SEQUENCE, ColumnName.OPERATION_ID,
                                   TableName.OPERATION, ColumnName.OPERATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.OLD_SEQUENCE, ColumnName.TOOL_ID,
                                   TableName.TOOL, ColumnName.TOOL_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddIndex (TableName.OLD_SEQUENCE,
                ColumnName.CAD_MODEL_ID);
      AddIndex (TableName.OLD_SEQUENCE,
                ColumnName.OPERATION_ID);
    }
    
    void RemoveProcessTable ()
    {
      Database.RemoveTable (TableName.OLD_SEQUENCE);
      
      // display option
      Database.Delete (TableName.DISPLAY,
                       new string [] {"displaytable"},
                       new string [] {"Process"});
    }
    
    void AddStampingValueTable ()
    {
      Database.AddTable (TableName.STAMPING_VALUE,
                         new Column (ColumnName.OLD_SEQUENCE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.FIELD_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column ("StampingValueString", DbType.String),
                         new Column ("StampingValueInt", DbType.Int32),
                         new Column ("StampingValueDouble", DbType.Double));
      Database.GenerateForeignKey (TableName.STAMPING_VALUE, ColumnName.OLD_SEQUENCE_ID,
                                   TableName.OLD_SEQUENCE, ColumnName.OLD_SEQUENCE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.STAMPING_VALUE, ColumnName.FIELD_ID,
                                   TableName.FIELD, ColumnName.FIELD_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.STAMPING_VALUE,
                ColumnName.OLD_SEQUENCE_ID);
    }
    
    void RemoveStampingValueTable ()
    {
      Database.RemoveTable (TableName.STAMPING_VALUE);
    }

    void AddStampTable ()
    {
      log.Debug ("AddStampTable /B");
      
      Database.AddTable (TableName.STAMP,
                         new Column (ColumnName.STAMP_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (ColumnName.ISO_FILE_ID, DbType.Int32),
                         new Column ("StampPosition", DbType.Int32),
                         new Column (ColumnName.OLD_SEQUENCE_ID, DbType.Int32),
                         new Column (ColumnName.OPERATION_ID, DbType.Int32),
                         new Column (ColumnName.COMPONENT_ID, DbType.Int32),
                         new Column (OPERATION_CYCLE_BEGIN, DbType.Boolean, ColumnProperty.NotNull, false),
                         new Column (OPERATION_CYCLE_END, DbType.Boolean, ColumnProperty.NotNull, false));
      Database.GenerateForeignKey (TableName.STAMP, ColumnName.ISO_FILE_ID,
                                   TableName.ISO_FILE, ColumnName.ISO_FILE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (TableName.STAMP, ColumnName.OLD_SEQUENCE_ID,
                                   TableName.OLD_SEQUENCE, ColumnName.OLD_SEQUENCE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (TableName.STAMP, ColumnName.OPERATION_ID,
                                   TableName.OPERATION, ColumnName.OPERATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (TableName.STAMP, ColumnName.COMPONENT_ID,
                                   TableName.COMPONENT, ColumnName.COMPONENT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddIndex (TableName.STAMP,
                ColumnName.ISO_FILE_ID);
      AddIndexCondition (TableName.STAMP,
                         "processid IS NOT NULL",
                         ColumnName.OLD_SEQUENCE_ID);
      AddIndexCondition (TableName.STAMP,
                         "operationid IS NOT NULL",
                         ColumnName.OPERATION_ID);
      AddIndexCondition (TableName.STAMP,
                         "componentid IS NOT NULL",
                         ColumnName.COMPONENT_ID);
      AddIndexCondition (TableName.STAMP,
                         "operationcyclebegin OR operationcycleend",
                         ColumnName.STAMP_ID);
    }
    
    void RemoveStampTable ()
    {
      Database.RemoveTable (TableName.STAMP);
    }
  }
}

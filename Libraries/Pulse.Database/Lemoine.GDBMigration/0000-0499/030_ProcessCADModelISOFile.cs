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
  /// </summary>
  [Migration(30)]
  public class ProcessCADModelISOFile: MigrationExt
  {
    static readonly string TOOL_NAME = "ToolName";
    static readonly string TOOL_CODE = "ToolCode";
    
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
    
    static readonly string OPERATION_CYCLE_BEGIN = "operationcyclebegin";
    static readonly string OPERATION_CYCLE_END = "operationcycleend";
    
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
      CreateOperationTables ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveOperationTables ();
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
      ResetSequence (TableName.TOOL, ColumnName.TOOL_ID);      
    }
    
    void RemoveToolTable ()
    {
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
    }
    
    void RemoveISOFileTable ()
    {
      Database.RemoveTable (TableName.ISO_FILE);
    }

    void CreateOperationTables ()
    {
      log.Debug ("CreateOperationTables /B");
      
      if (!Database.TableExists (TableName.OLD_SEQUENCE)) {
        AddProcessTable ();
      }
      if (!Database.TableExists (TableName.STAMPING_VALUE)) {
        AddStampingValueTable ();
      }
      if (!Database.TableExists (TableName.STAMP)) {
        AddStampTable ();
      }
    }
    
    void RemoveOperationTables ()
    {
      log.Debug ("RemoveOperationTables /B");
      
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

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
  /// Migration 016: create the new operation and component tables
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

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      #region 1. operationtype
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
      #endregion // 1. operationtype 

      #region 2. componenttype 
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
      #endregion // 2. componenttype

      #region 3. operation / intermediateworkpiece / component
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
      #endregion // 3. operation / intermediateworkpiece / component 

      #region 4. machineoperationtype
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
      #endregion // 4. machineoperationtype
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // New tables deletion
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

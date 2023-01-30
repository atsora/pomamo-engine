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
  /// Migration 017: Add the modification tables on data structure
  /// 
  /// They are XxxUpdate tables where Xxx is a table name or a relation name
  /// </summary>
  [Migration(17)]
  public class ModificationTablesOnDataStructure: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ModificationTablesOnDataStructure).FullName);

    static readonly string WORK_ORDER_PROJECT_UPDATE_TABLE = "workorderprojectupdate";
    static readonly string PROJECT_COMPONENT_UPDATE_TABLE = TableName.PROJECT_COMPONENT_UPDATE;
    static readonly string COMPONENT_INTERMEDIATE_WORK_PIECE_UPDATE_TABLE = "componentintermediateworkpieceupdate";
    static readonly string INTERMEDIATE_WORK_PIECE_OPERATION_UPDATE_TABLE = "intermediateworkpieceoperationupdate";
    
    static readonly string MODIFICATION_TABLE = "modification";
    static readonly string MODIFICATION_ID = "modificationid";
    static readonly string WORK_ORDER_TABLE = "workorder";
    static readonly string WORK_ORDER_ID = "workorderid";
    static readonly string PROJECT_TABLE = "project";
    static readonly string PROJECT_ID = "projectid";
    static readonly string COMPONENT_TABLE = "component";
    static readonly string COMPONENT_ID = "componentid";
    static readonly string INTERMEDIATE_WORK_PIECE_TABLE = "intermediateworkpiece";
    static readonly string INTERMEDIATE_WORK_PIECE_ID = "intermediateworkpieceid";
    static readonly string OPERATION_TABLE = "operation";
    static readonly string OPERATION_ID = "operationid";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (WORK_ORDER_PROJECT_UPDATE_TABLE)) {
        Database.AddTable (WORK_ORDER_PROJECT_UPDATE_TABLE,
                           new Column (MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column (WORK_ORDER_ID, DbType.Int32, ColumnProperty.NotNull),
                           new Column (PROJECT_ID, DbType.Int32, ColumnProperty.NotNull),
                           new Column ("oldworkorderprojectquantity", DbType.Int32),
                           new Column ("newworkorderprojectquantity", DbType.Int32),
                           new Column ("modificationtype", DbType.Int32, ColumnProperty.NotNull)); // 1: New, 2: Delete, 3: Modification
        Database.GenerateForeignKey (WORK_ORDER_PROJECT_UPDATE_TABLE, MODIFICATION_ID,
                                     MODIFICATION_TABLE, MODIFICATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (WORK_ORDER_PROJECT_UPDATE_TABLE, WORK_ORDER_ID,
                                     WORK_ORDER_TABLE, WORK_ORDER_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);
        Database.GenerateForeignKey (WORK_ORDER_PROJECT_UPDATE_TABLE, PROJECT_ID,
                                     PROJECT_TABLE, PROJECT_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);
        Database.AddCheckConstraint ("workorderprojectupdate_modificationtype",
                                     WORK_ORDER_PROJECT_UPDATE_TABLE,
                                     "(modificationtype IN (1, 2, 3))");
        Database.ExecuteNonQuery (string.Format (@"CREATE RULE {0}_delete AS
ON DELETE TO {0}
DO ALSO DELETE FROM modification
  WHERE modificationid = OLD.modificationid;",
                                                 WORK_ORDER_PROJECT_UPDATE_TABLE));
      }
      if (!Database.TableExists (PROJECT_COMPONENT_UPDATE_TABLE)) {
        Database.AddTable (PROJECT_COMPONENT_UPDATE_TABLE,
                           new Column (MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column (COMPONENT_ID, DbType.Int32, ColumnProperty.NotNull),
                           new Column ("oldprojectid", DbType.Int32),
                           new Column ("newprojectid", DbType.Int32));
        Database.GenerateForeignKey (PROJECT_COMPONENT_UPDATE_TABLE, MODIFICATION_ID,
                                     MODIFICATION_TABLE, MODIFICATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (PROJECT_COMPONENT_UPDATE_TABLE, COMPONENT_ID,
                                     COMPONENT_TABLE, COMPONENT_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);
        Database.GenerateForeignKey (PROJECT_COMPONENT_UPDATE_TABLE, "oldprojectid",
                                     PROJECT_TABLE, PROJECT_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);
        Database.GenerateForeignKey (PROJECT_COMPONENT_UPDATE_TABLE, "newprojectid",
                                     PROJECT_TABLE, PROJECT_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);
        Database.AddCheckConstraint ("projectcomponentupdate_projectid",
                                     PROJECT_COMPONENT_UPDATE_TABLE,
                                     "((oldprojectid IS NOT NULL) OR (newprojectid IS NOT NULL))");
        Database.ExecuteNonQuery (string.Format (@"CREATE RULE {0}_delete AS
ON DELETE TO {0}
DO ALSO DELETE FROM modification
  WHERE modificationid = OLD.modificationid;",
                                                 PROJECT_COMPONENT_UPDATE_TABLE));
      }
      if (!Database.TableExists (COMPONENT_INTERMEDIATE_WORK_PIECE_UPDATE_TABLE)) {
        Database.AddTable (COMPONENT_INTERMEDIATE_WORK_PIECE_UPDATE_TABLE,
                           new Column (MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column (COMPONENT_ID, DbType.Int32, ColumnProperty.NotNull),
                           new Column (INTERMEDIATE_WORK_PIECE_ID, DbType.Int32, ColumnProperty.NotNull),
                           new Column ("modificationtype", DbType.Int32, ColumnProperty.NotNull)); // 1: New, 2: Delete
        Database.GenerateForeignKey (COMPONENT_INTERMEDIATE_WORK_PIECE_UPDATE_TABLE, MODIFICATION_ID,
                                     MODIFICATION_TABLE, MODIFICATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (COMPONENT_INTERMEDIATE_WORK_PIECE_UPDATE_TABLE, COMPONENT_ID,
                                     COMPONENT_TABLE, COMPONENT_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);
        Database.GenerateForeignKey (COMPONENT_INTERMEDIATE_WORK_PIECE_UPDATE_TABLE, INTERMEDIATE_WORK_PIECE_ID,
                                     INTERMEDIATE_WORK_PIECE_TABLE, INTERMEDIATE_WORK_PIECE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);
        Database.AddCheckConstraint ("componentintermediateworkpieceupdate_modificationtype",
                                     COMPONENT_INTERMEDIATE_WORK_PIECE_UPDATE_TABLE,
                                     "(modificationtype IN (1, 2))");
        Database.ExecuteNonQuery (string.Format (@"CREATE RULE {0}_delete AS
ON DELETE TO {0}
DO ALSO DELETE FROM modification
  WHERE modificationid = OLD.modificationid;",
                                                 COMPONENT_INTERMEDIATE_WORK_PIECE_UPDATE_TABLE));
      }
      if (!Database.TableExists (INTERMEDIATE_WORK_PIECE_OPERATION_UPDATE_TABLE)) {
        Database.AddTable (INTERMEDIATE_WORK_PIECE_OPERATION_UPDATE_TABLE,
                           new Column (MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column (INTERMEDIATE_WORK_PIECE_ID, DbType.Int32, ColumnProperty.NotNull),
                           new Column ("oldoperationid", DbType.Int32),
                           new Column ("newoperationid", DbType.Int32),
                           new Column ("oldoperationintermediateworkpiecequantity", DbType.Int32),
                           new Column ("newoperationintermediateworkpiecequantity", DbType.Int32));
        Database.GenerateForeignKey (INTERMEDIATE_WORK_PIECE_OPERATION_UPDATE_TABLE, MODIFICATION_ID,
                                     MODIFICATION_TABLE, MODIFICATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (INTERMEDIATE_WORK_PIECE_OPERATION_UPDATE_TABLE, INTERMEDIATE_WORK_PIECE_ID,
                                     INTERMEDIATE_WORK_PIECE_TABLE, INTERMEDIATE_WORK_PIECE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);
        Database.GenerateForeignKey (INTERMEDIATE_WORK_PIECE_OPERATION_UPDATE_TABLE, "oldoperationid",
                                     OPERATION_TABLE, OPERATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);
        Database.GenerateForeignKey (INTERMEDIATE_WORK_PIECE_OPERATION_UPDATE_TABLE, "newoperationid",
                                     OPERATION_TABLE, OPERATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Restrict);
        Database.AddCheckConstraint ("intermediateworkpieceoperationupdate_operationid",
                                     INTERMEDIATE_WORK_PIECE_OPERATION_UPDATE_TABLE,
                                     "((oldoperationid IS NOT NULL) OR (newoperationid IS NOT NULL))");
        Database.ExecuteNonQuery (string.Format (@"CREATE RULE {0}_delete AS
ON DELETE TO {0}
DO ALSO DELETE FROM modification
  WHERE modificationid = OLD.modificationid;",
                                                 INTERMEDIATE_WORK_PIECE_OPERATION_UPDATE_TABLE));
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // New tables deletion
      if (Database.TableExists (WORK_ORDER_PROJECT_UPDATE_TABLE)) {
        Database.ExecuteNonQuery (@"DELETE FROM modification 
WHERE modificationreferencedtable='WorkOrderProjectUpdate'");
        Database.RemoveTable (WORK_ORDER_PROJECT_UPDATE_TABLE);
      }
      if (Database.TableExists (PROJECT_COMPONENT_UPDATE_TABLE)) {
        Database.ExecuteNonQuery (@"DELETE FROM modification 
WHERE modificationreferencedtable='ProjectComponentUpdate'");
        Database.RemoveTable (PROJECT_COMPONENT_UPDATE_TABLE);
      }
      if (Database.TableExists (COMPONENT_INTERMEDIATE_WORK_PIECE_UPDATE_TABLE)) {
        Database.ExecuteNonQuery (@"DELETE FROM modification 
WHERE modificationreferencedtable='ComponentIntermediateWorkPieceUpdate'");
        Database.RemoveTable (COMPONENT_INTERMEDIATE_WORK_PIECE_UPDATE_TABLE);
      }
      if (Database.TableExists (INTERMEDIATE_WORK_PIECE_OPERATION_UPDATE_TABLE)) {
        Database.ExecuteNonQuery (@"DELETE FROM modification 
WHERE modificationreferencedtable='IntermediateWorkPieceOperationUpdate'");
        Database.RemoveTable (INTERMEDIATE_WORK_PIECE_OPERATION_UPDATE_TABLE);
      }
    }
  }
}

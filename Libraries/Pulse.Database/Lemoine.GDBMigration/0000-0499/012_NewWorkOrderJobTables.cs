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
  /// Migration 012: new work and project tables
  /// </summary>
  [Migration(12)]
  public class NewWorkOrderProjectTables: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (NewWorkOrderProjectTables).FullName);
    static readonly string TRANSLATION_TABLE = "Translation";
    static readonly string LOCALE = "Locale";
    static readonly string TRANSLATION_KEY = "TranslationKey";
    static readonly string TRANSLATION_VALUE = "TranslationValue";
    static readonly string WORK_ORDER_STATUS_TABLE = "WorkOrderStatus";
    static readonly string WORK_ORDER_STATUS_ID = "WorkOrderStatusId";
    static readonly string WORK_ORDER_STATUS_NAME = "WorkOrderStatusName";
    static readonly string WORK_ORDER_STATUS_TRANSLATION_KEY = "WorkOrderStatusTranslationKey";
    static readonly string WORK_ORDER_TABLE = "WorkOrder";
    static readonly string WORK_ORDER_ID = "WorkOrderId";
    static readonly string PROJECT_TABLE = "Project";
    static readonly string PROJECT_ID = "ProjectId";
    static readonly string WORK_ORDER_PROJECT_TABLE = "WorkOrderProject";
    
    /// <summary>
    /// Update the database
    /// </summary>
    public override void Up ()
    {      
      // 0. Extension citext
      Database.ExecuteNonQuery ("CREATE EXTENSION IF NOT EXISTS citext;");

      // 1. New tables creation
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS translation CASCADE;");
      if (!Database.TableExists (TRANSLATION_TABLE)) {
        Database.AddTable (TRANSLATION_TABLE,
                           new Column (LOCALE, DbType.String, ColumnProperty.PrimaryKey),
                           new Column (TRANSLATION_KEY, DbType.String, ColumnProperty.PrimaryKey),
                           new Column (TRANSLATION_VALUE, DbType.String, ColumnProperty.NotNull));
        Database.ExecuteNonQuery ("CREATE INDEX translation_translationkey_idx " +
                                  "ON translation (translationkey);");
        Database.Insert (TRANSLATION_TABLE,
                         new string [] {LOCALE, TRANSLATION_KEY, TRANSLATION_VALUE},
                         new string [] {"", "UndefinedValue", "Undefined"});
      }
      if (!Database.TableExists (WORK_ORDER_STATUS_TABLE)) {
        Database.AddTable (WORK_ORDER_STATUS_TABLE,
                           new Column (WORK_ORDER_STATUS_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                           new Column (WORK_ORDER_STATUS_NAME, DbType.String, ColumnProperty.Unique),
                           new Column (WORK_ORDER_STATUS_TRANSLATION_KEY, DbType.String, ColumnProperty.Unique));
        Database.ExecuteNonQuery ("ALTER TABLE workorderstatus " +
                                  "ALTER COLUMN workorderstatusname " +
                                  "SET DATA TYPE CITEXT;");
        Database.AddCheckConstraint ("workorderstatus_name_translationkey",
                                     WORK_ORDER_STATUS_TABLE,
                                     "((workorderstatusname IS NOT NULL) OR (workorderstatustranslationkey IS NOT NULL))");
        Database.Insert (WORK_ORDER_STATUS_TABLE,
                         new string [] {WORK_ORDER_STATUS_TRANSLATION_KEY},
                         new string [] {"UndefinedValue"}); // id = 1
        Database.Insert (TRANSLATION_TABLE,
                         new string [] {LOCALE, TRANSLATION_KEY, TRANSLATION_VALUE},
                         new string [] {"", "WorkOrderValid", "Valid"});
        Database.Insert (WORK_ORDER_STATUS_TABLE,
                         new string [] {WORK_ORDER_STATUS_TRANSLATION_KEY},
                         new string [] {"WorkOrderValid"}); // id = 2
        Database.Insert (TRANSLATION_TABLE,
                         new string [] {LOCALE, TRANSLATION_KEY, TRANSLATION_VALUE},
                         new string [] {"", "WorkOrderClosed", "Closed"});
        Database.Insert (WORK_ORDER_STATUS_TABLE,
                         new string [] {WORK_ORDER_STATUS_TRANSLATION_KEY},
                         new string [] {"WorkOrderClosed"}); // id = 3
        Database.Insert (TRANSLATION_TABLE,
                         new string [] {LOCALE, TRANSLATION_KEY, TRANSLATION_VALUE},
                         new string [] {"", "WorkOrderNew", "New"});
        Database.Insert (WORK_ORDER_STATUS_TABLE,
                         new string [] {WORK_ORDER_STATUS_TRANSLATION_KEY},
                         new string [] {"WorkOrderNew"}); // id = 4
      }
      if (!Database.TableExists (WORK_ORDER_TABLE)) {
        Database.AddTable (WORK_ORDER_TABLE,
                           new Column (WORK_ORDER_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                           new Column ("WorkOrderName", DbType.String, ColumnProperty.Unique),
                           new Column ("WorkOrderCode", DbType.String, ColumnProperty.Unique),
                           new Column ("WorkOrderExternalCode", DbType.String, ColumnProperty.Unique),
                           new Column ("WorkOrderDocumentLink", DbType.String),
                           new Column ("WorkOrderDeliveryDate", DbType.Date),
                           new Column (WORK_ORDER_STATUS_ID, DbType.Int32, ColumnProperty.NotNull, 1)); // Default to Undefined
        Database.ExecuteNonQuery ("ALTER TABLE WorkOrder " +
                                  "ALTER COLUMN WorkOrderName " +
                                  "SET DATA TYPE CITEXT;");
        Database.ExecuteNonQuery ("ALTER TABLE WorkOrder " +
                                  "ALTER COLUMN WorkOrderCode " +
                                  "SET DATA TYPE CITEXT;");
        Database.AddCheckConstraint ("workorder_name_code",
                                     WORK_ORDER_TABLE,
                                     "((workordername IS NOT NULL) OR (workordercode IS NOT NULL))");
        Database.GenerateForeignKey (WORK_ORDER_TABLE, WORK_ORDER_STATUS_ID,
                                     WORK_ORDER_STATUS_TABLE, WORK_ORDER_STATUS_ID,
                                     Migrator.Framework.ForeignKeyConstraint.SetDefault);
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX workorder_workordername_idx " +
                                  "ON workorder (workordername) " +
                                  "WHERE workordername IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX workorder_workordercode_idx " +
                                  "ON workorder (workordercode) " +
                                  "WHERE workordercode IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX workorder_workorderexternalcode_idx " +
                                  "ON workorder (workorderexternalcode) " +
                                  "WHERE workorderexternalcode IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE INDEX workorder_workorderstatusid_idx " +
                                  "ON workorder (workorderstatusid)");
      }
      if (!Database.TableExists (PROJECT_TABLE)) {
        Database.AddTable (PROJECT_TABLE,
                           new Column (PROJECT_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                           new Column ("ProjectName", DbType.String, ColumnProperty.Unique),
                           new Column ("ProjectCode", DbType.String, ColumnProperty.Unique),
                           new Column ("ProjectExternalCode", DbType.String, ColumnProperty.Unique),
                           new Column ("ProjectDocumentLink", DbType.String),
                           new Column ("ProjectCreationDateTime", DbType.DateTime, ColumnProperty.NotNull),
                           new Column ("ProjectReactivationDateTime", DbType.DateTime, ColumnProperty.NotNull),
                           new Column ("ProjectArchiveDateTime", DbType.DateTime));
        Database.ExecuteNonQuery ("ALTER TABLE Project " +
                                  "ALTER COLUMN ProjectName " +
                                  "SET DATA TYPE CITEXT;");
        Database.ExecuteNonQuery ("ALTER TABLE Project " +
                                  "ALTER COLUMN ProjectCode " +
                                  "SET DATA TYPE CITEXT;");
        Database.AddCheckConstraint ("project_name_code",
                                     PROJECT_TABLE,
                                     "((projectname IS NOT NULL) OR (projectcode IS NOT NULL))");
        Database.ExecuteNonQuery ("ALTER TABLE Project " +
                                  "ALTER COLUMN ProjectCreationDateTime " +
                                  "SET DEFAULT now() AT TIME ZONE 'UTC';");
        Database.ExecuteNonQuery ("ALTER TABLE Project " +
                                  "ALTER COLUMN ProjectReactivationDateTime " +
                                  "SET DEFAULT now() AT TIME ZONE 'UTC';");
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX project_projectname_idx " +
                                  "ON project (projectname) " +
                                  "WHERE projectname IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX project_projectcode_idx " +
                                  "ON project (projectcode) " +
                                  "WHERE projectcode IS NOT NULL;");
        Database.ExecuteNonQuery ("CREATE UNIQUE INDEX project_projectexternalcode_idx " +
                                  "ON project (projectexternalcode) " +
                                  "WHERE projectexternalcode IS NOT NULL;");
      }
      if (!Database.TableExists (WORK_ORDER_PROJECT_TABLE)) {
        Database.AddTable (WORK_ORDER_PROJECT_TABLE,
                           new Column (WORK_ORDER_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column (PROJECT_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column ("WorkOrderProjectQuantity", DbType.Int32, ColumnProperty.NotNull, 1));
        Database.GenerateForeignKey (WORK_ORDER_PROJECT_TABLE, WORK_ORDER_ID,
                                     WORK_ORDER_TABLE, WORK_ORDER_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (WORK_ORDER_PROJECT_TABLE, PROJECT_ID,
                                     PROJECT_TABLE, PROJECT_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.ExecuteNonQuery ("CREATE INDEX workorderproject_workorderid_idx " +
                                  "ON workorderproject (workorderid);");
        Database.ExecuteNonQuery ("CREATE INDEX workorderproject_projectid_idx " +
                                  "ON workorderproject (projectid);");
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    public override void Down ()
    {
      // New tables deletion
      if (Database.TableExists (WORK_ORDER_PROJECT_TABLE)) {
        Database.RemoveTable (WORK_ORDER_PROJECT_TABLE);
      }
      if (Database.TableExists (PROJECT_TABLE)) {
        Database.RemoveTable (PROJECT_TABLE);
      }
      if (Database.TableExists (WORK_ORDER_TABLE)) {
        Database.RemoveTable (WORK_ORDER_TABLE);
      }
      if (Database.TableExists (WORK_ORDER_STATUS_TABLE)) {
        Database.RemoveTable (WORK_ORDER_STATUS_TABLE);
      }
      if (Database.TableExists (TRANSLATION_TABLE)) {
        Database.RemoveTable (TRANSLATION_TABLE);
      }
    }
  }
}

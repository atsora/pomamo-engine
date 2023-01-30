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
  /// Migration 531: 
  /// </summary>
  [Migration (531)]
  public class FixConstraintsWithNull : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FixConstraintsWithNull).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // 012
      FixNameTranslationKeyConstraint (TableName.WORK_ORDER_STATUS);
      FixNameCodeConstraint (TableName.WORK_ORDER);
      FixNameCodeConstraint (TableName.PROJECT);

      // 013
      {
        Database.RemoveConstraint (TableName.UPDATER, "updater_check_ids");
        Database.AddCheckConstraint ("updater_check_ids",
          TableName.UPDATER,
          @"((updatertypeid = 1) AND (updaterid = userid) AND (serviceid IS NULL)) 
OR ((updatertypeid = 2) AND (updaterid = serviceid) AND (userid IS NULL))");
      }
      {
        Database.RemoveConstraint (TableName.USER, "usertable_name_code");
        Database.AddCheckConstraint ("usertable_name_code",
                                     TableName.USER,
                                     "((username IS NOT NULL) OR (usercode IS NOT NULL))");
      }

      // 014
      FixNameTranslationKeyConstraint (TableName.MACHINE_MONITORING_TYPE);
      FixNameCodeConstraint (TableName.MACHINE);

      // 016
      FixNameTranslationKeyConstraint (TableName.OPERATION_TYPE);
      FixNameTranslationKeyConstraint (TableName.COMPONENT_TYPE);

      // 022
      FixNameCodeConstraint (TableName.MACHINE_MODULE);
      FixNameTranslationKeyConstraint (TableName.MACHINE_MODE);

      // 028
      FixNameTranslationKeyConstraint (TableName.MACHINE_OBSERVATION_STATE);

      // 029
      FixNameTranslationKeyConstraint (TableName.REASON_GROUP);
      FixNameTranslationKeyConstraint (TableName.REASON);

      // 030
      FixNameTranslationKeyConstraint (TableName.UNIT);
      FixNameTranslationKeyConstraint (TableName.FIELD);

      // 036
      FixNameTranslationKeyConstraint (TableName.EVENT_LEVEL);

      // 064
      FixNameCodeConstraint (TableName.COMPANY);
      FixNameCodeConstraint (TableName.DEPARTMENT);
      FixNameCodeConstraint (TableName.MACHINE_CATEGORY);
      FixNameCodeConstraint (TableName.MACHINE_SUB_CATEGORY);
      FixNameCodeConstraint (TableName.SHOP_FLOOR_DISPLAY);

      // 095
      FixNameCodeConstraint (TableName.SHIFT);

      // 125
      FixNameCodeConstraint (TableName.CELL);

      // 191
      FixNameTranslationKeyConstraint (TableName.ROLE);

      // 204
      FixNameTranslationKeyConstraint (TableName.MACHINE_STATE_TEMPLATE);

      // 267
      FixNameTranslationKeyConstraint (TableName.GOALTYPE);

      // 277
      FixNameTranslationKeyConstraint (TableName.TASK_STATUS);
    }


    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }

    void FixNameTranslationKeyConstraint (string tableName)
    {
      RemoveConstraint (tableName, tableName + "_name_translationkey");
      AddConstraintNameTranslationKey (tableName, tableName + "name", tableName + "translationkey");
    }

    void FixNameCodeConstraint (string tableName)
    {
      string constraintName = tableName + "_name_code";
      string nameColumn = tableName + "name";
      string codeColumn = tableName + "code";

      RemoveConstraint (tableName, constraintName);
      Database.ExecuteNonQuery (string.Format(@"
UPDATE {0}
SET {0}name='{0}-' || {0}id
WHERE {0}name IS NULL and {0}code IS NULL
",
        tableName));
      Database.AddCheckConstraint (constraintName, tableName,
        string.Format ("(({0} IS NOT NULL) OR ({1} IS NOT NULL))", nameColumn, codeColumn));
    }


  }
}

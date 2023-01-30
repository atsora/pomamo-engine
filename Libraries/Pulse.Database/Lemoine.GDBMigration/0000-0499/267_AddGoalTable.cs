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
  /// Migration 267: add a goal table
  /// </summary>
  [Migration(267)]
  public class AddGoalTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddGoalTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddGoalType ();
      AddGoal ();
      MigrateData ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveGoal ();
      RemoveGoalType ();
    }
    
    void AddGoalType ()
    {
      Database.AddTable (TableName.GOALTYPE,
                         new Column (TableName.GOALTYPE + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.GOALTYPE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.GOALTYPE + "name", DbType.String, ColumnProperty.Unique),
                         new Column (TableName.GOALTYPE + "translationkey", DbType.String, ColumnProperty.Unique),
                         new Column (ColumnName.UNIT_ID, DbType.Int32));
      Database.GenerateForeignKey (TableName.GOALTYPE, ColumnName.UNIT_ID,
                                   TableName.UNIT, ColumnName.UNIT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddConstraintNameTranslationKey (TableName.GOALTYPE,
                                       TableName.GOALTYPE + "name",
                                       TableName.GOALTYPE + "translationkey");
    }
    
    void RemoveGoalType ()
    {
      Database.RemoveTable (TableName.GOALTYPE);
    }
    
    void AddGoal ()
    {
      Database.AddTable (TableName.GOAL,
                         new Column (TableName.GOAL + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.GOAL + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.GOALTYPE + "id", DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.GOAL + "value", DbType.Double, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32),
                         new Column (ColumnName.COMPANY_ID, DbType.Int32),
                         new Column (ColumnName.DEPARTMENT_ID, DbType.Int32),
                         new Column (ColumnName.MACHINE_CATEGORY_ID, DbType.Int32),
                         new Column (ColumnName.MACHINE_SUB_CATEGORY_ID, DbType.Int32),
                         new Column (ColumnName.CELL_ID, DbType.Int32),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32));
      // Next constraint check returns an error. Is it too long ?
/*
      Database.AddCheckConstraint (TableName.GOAL + "reference",
                                   TableName.GOAL,
                                   string.Format (@"(
({0} IS NULL AND {1} IS NULL AND {2} IS NULL AND {3} IS NULL AND {4} IS NULL AND {5} IS NULL)
OR ({0} IS NOT NULL AND {1} IS NULL AND {2} IS NULL AND {3} IS NULL AND {4} IS NULL AND {5} IS NULL)
OR ({0} IS NULL AND {1} IS NOT NULL AND {2} IS NULL AND {3} IS NULL AND {4} IS NULL AND {5} IS NULL)
OR ({0} IS NULL AND {1} IS NULL AND {2} IS NOT NULL AND {3} IS NULL AND {4} IS NULL AND {5} IS NULL)
OR ({0} IS NULL AND {1} IS NULL AND {2} IS NULL AND {3} IS NOT NULL AND {4} IS NULL AND {5} IS NULL)
OR ({0} IS NULL AND {1} IS NULL AND {2} IS NULL AND {3} IS NULL AND {4} IS NOT NULL AND {5} IS NULL)
OR ({0} IS NULL AND {1} IS NULL AND {2} IS NULL AND {3} IS NULL AND {4} IS NULL AND {5} NOT IS NULL)
)",
                                                  ColumnName.COMPANY_ID,
                                                  ColumnName.DEPARTMENT_ID,
                                                  ColumnName.MACHINE_CATEGORY_ID,
                                                  ColumnName.MACHINE_SUB_CATEGORY_ID,
                                                  ColumnName.CELL_ID,
                                                  ColumnName.MACHINE_ID)); // Only one not null
 */
      Database.GenerateForeignKey (TableName.GOAL, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.GOAL, ColumnName.COMPANY_ID,
                                   TableName.COMPANY, ColumnName.COMPANY_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (TableName.GOAL, ColumnName.DEPARTMENT_ID,
                                   TableName.DEPARTMENT, ColumnName.DEPARTMENT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (TableName.GOAL, ColumnName.MACHINE_CATEGORY_ID,
                                   TableName.MACHINE_CATEGORY, ColumnName.MACHINE_CATEGORY_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (TableName.GOAL, ColumnName.MACHINE_SUB_CATEGORY_ID,
                                   TableName.MACHINE_SUB_CATEGORY, ColumnName.MACHINE_SUB_CATEGORY_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (TableName.GOAL, ColumnName.CELL_ID,
                                   TableName.CELL, ColumnName.CELL_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (TableName.GOAL, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void RemoveGoal ()
    {
      Database.RemoveTable (TableName.GOAL);
    }
    
    void MigrateData ()
    {
      if (Database.TableExists ("sfkcfgs")) { // Clean sfkcfgs
        Database.ExecuteNonQuery (@"DELETE FROM sfkcfgs 
WHERE config='UseGoal'
  AND skey IN ('24x7_goal', 'AllShift_goal', 'OutOfShifts_goal')
  AND NOT EXISTS (SELECT 1 FROM machine WHERE machineid=sfkcfgs.sfksection::integer);");
      }
    }
  }
}

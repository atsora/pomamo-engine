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
  /// Migration 256: add the concept of modification step:
  /// <item>add the analysisstatusid 11</item>
  /// <item>add the table analysisstatus</item>
  /// <item>add the column analysisstepspan</item>
  /// <item>add the column submodifications</item>
  /// </summary>
  [Migration(256)]
  public class AddModificationStep: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddModificationStep).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddAnalysisStatusTable ();
      AddAnalysisStepSpan ();
      AddAnalysisSubModifications ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveAnalysisSubModifications ();
      RemoveAnalysisStepSpan ();
      RemoveAnalysisStatusTable ();
    }
    
    void AddAnalysisStatusTable ()
    {
      Database.RemoveConstraint (TableName.MODIFICATION_STATUS,
                                 "modificationstatus_analysisstatusids");
      Database.AddTable (TableName.ANALYSIS_STATUS,
                         new Column (TableName.ANALYSIS_STATUS + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.ANALYSIS_STATUS + "name", DbType.String, ColumnProperty.NotNull));
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] {TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name"},
                       new string[] {"0", "New"});
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] {TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name"},
                       new string[] {"1", "Pending"});
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] {TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name"},
                       new string[] {"3", "Done"});
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] {TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name"},
                       new string[] {"4", "Error"});
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] {TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name"},
                       new string[] {"5", "Obsolete"});
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] {TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name"},
                       new string[] {"6", "Delete"});
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] {TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name"},
                       new string[] {"7", "Timeout"});
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] {TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name"},
                       new string[] {"8", "ConstraintIntegrityViolation"});
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] {TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name"},
                       new string[] {"9", "InProgress"});
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] {TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name"},
                       new string[] {"10", "PendingSubModifications"});
      Database.Insert (TableName.ANALYSIS_STATUS,
                       new string[] {TableName.ANALYSIS_STATUS + "id", TableName.ANALYSIS_STATUS + "name"},
                       new string[] {"11", "StepTimeout"});
      Database.GenerateForeignKey (TableName.MODIFICATION_STATUS, TableName.ANALYSIS_STATUS + "id",
                                   TableName.ANALYSIS_STATUS, TableName.ANALYSIS_STATUS + "id",
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
    }
    
    void RemoveAnalysisStatusTable ()
    {
      Database.RemoveTable (TableName.ANALYSIS_STATUS);
      Database.AddCheckConstraint ("modificationstatus_analysisstatusids",
                                   TableName.MODIFICATION_STATUS,
                                   "analysisstatusid IN (0, 1, 3, 4, 5, 6, 7, 8, 9, 10)"); // 0: New, 1: Pending, 3: Done, 4: Error, 5: Obsolete, 6: Delete, 7: Timeout, 8: Integrity Violation
    }
    
    void AddAnalysisStepSpan ()
    {
      Database.AddColumn (TableName.MODIFICATION_STATUS,
                          new Column ("analysisstepspan", DbType.Int32));
    }
    
    void RemoveAnalysisStepSpan ()
    {
      Database.RemoveColumn (TableName.MODIFICATION_STATUS,
                             "analysisstepspan");
    }
    
    void AddAnalysisSubModifications ()
    {
      Database.AddColumn (TableName.MODIFICATION_STATUS,
                          new Column ("analysissubmodifications", DbType.Boolean, ColumnProperty.NotNull, "FALSE"));
    }
    
    void RemoveAnalysisSubModifications ()
    {
      Database.RemoveColumn (TableName.MODIFICATION_STATUS,
                             "analysissubmodifications");
    }
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 304:
  /// </summary>
  [Migration(304)]
  public class MachineModificationConver: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineModificationConver).FullName);
    static readonly IList<string> GLOBAL_MODIFICATIONS = new List<string> {"daytemplatechange", "workorderlineassociation", "operationinformation",
      "shifttemplateassociation", "componentintermediateworkpieceupdate",
      "intermediateworkpieceoperationupdate", "projectcomponentupdate", "workorderprojectupdate",
      "usermachineassociation", "usershiftassociation", "userattendance", "shiftchange"};
    static readonly IList<string> MACHINE_MODIFICATIONS = new List<string> {
      "linkoperation", "activitymanual", "componentmachineassociation", "machineobservationstateassociation",
      "machinestatetemplateassociation", "operationmachineassociation", "reasonmachineassociation",
      "workordermachineassociation", "serialnumbermachinestamp", "serialnumbermodification",
      "workordermachinestamp", "nonconformancereport", "operationcycleinformation",
      "productioninformationshift", "shiftmachineassociation",
      "machinecellupdate", "machinecompanyupdate", "machinedepartmentupdate"};
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Convert ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      foreach (var tableName in GLOBAL_MODIFICATIONS) {
        RevertConvert ("global", tableName);
      }
      foreach (var tableName in MACHINE_MODIFICATIONS) {
        RevertConvert ("machine", tableName);
      }
    }
    
    void Convert ()
    {
      foreach (var tableName in GLOBAL_MODIFICATIONS) {
        ConvertToModificationStep1 ("global", tableName);
      }
      foreach (var tableName in MACHINE_MODIFICATIONS) {
        ConvertToModificationStep1 ("machine", tableName);
      }
      foreach (var tableName in GLOBAL_MODIFICATIONS) {
        ConvertToGlobalModificationStep2 (tableName);
      }
      foreach (var tableName in MACHINE_MODIFICATIONS) {
        ConvertToMachineModificationStep2 (tableName);
      }
      for (int i = 0; i < 20; ++i) {
        foreach (var tableName in GLOBAL_MODIFICATIONS) {
          ConvertToGlobalModificationStep3 (tableName);
        }
        foreach (var tableName in MACHINE_MODIFICATIONS) {
          ConvertToMachineModificationStep3 (tableName);
        }
      }
      foreach (var tableName in GLOBAL_MODIFICATIONS) {
        MigrateGlobalModificationStatus (tableName);
      }
      foreach (var tableName in MACHINE_MODIFICATIONS) {
        MigrateMachineModificationStatus (tableName);
      }
      UpdateSubModifications ("global");
      UpdateSubModifications ("machine");
    }
    
    void ConvertToModificationStep1 (string prefix, string tableName)
    {
      Database.RemoveForeignKey (tableName, "fk_" + tableName + "_modification");
    }
    
    void ConvertToGlobalModificationStep2 (string tableName)
    {
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}modification
SELECT modificationid, revisionid, modificationdatetime, modificationreferencedtable, modificationpriority, NULL, NULL, FALSE
FROM oldmodification NATURAL JOIN {1}
WHERE parentmodificationid IS NULL
",
                                               "global", tableName));
    }
    
    void MigrateGlobalModificationStatus (string tableName)
    {
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}modificationstatus
SELECT modificationid, analysisstatusid, analysisapplieddatetime, analysisbegin, analysisend, analysisiterations,
  analysistotalduration, analysislastduration, analysiscompletionorder, analysisstepspan, FALSE, FALSE
FROM oldmodificationstatus
WHERE modificationid IN (SELECT modificationid FROM {1})",
                                               "global", tableName));
    }
    
    void ConvertToMachineModificationStep2 (string tableName)
    {
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}modification
SELECT modificationid, revisionid, modificationdatetime, modificationreferencedtable, modificationpriority, NULL, NULL, FALSE, machineid
FROM oldmodification NATURAL JOIN {1}
WHERE parentmodificationid IS NULL
",
                                               "machine", tableName));
    }
    
    void MigrateMachineModificationStatus (string tableName)
    {
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}modificationstatus
SELECT modificationid, analysisstatusid, analysisapplieddatetime, analysisbegin, analysisend, analysisiterations,
  analysistotalduration, analysislastduration, analysiscompletionorder, analysisstepspan, FALSE, FALSE, machineid
FROM oldmodificationstatus NATURAL JOIN {1}",
                                               "machine", tableName));
    }
    
    void ConvertToGlobalModificationStep3 (string tableName)
    {
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}modification
SELECT modificationid, revisionid, modificationdatetime, modificationreferencedtable, modificationpriority, parentmodificationid, NULL, FALSE
FROM oldmodification NATURAL JOIN {1}
WHERE parentmodificationid IN (SELECT modificationid FROM globalmodification)
  AND modificationid NOT IN (SELECT modificationid FROM {0}modification)
",
                                               "global", tableName));
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}modification
SELECT modificationid, revisionid, modificationdatetime, modificationreferencedtable, modificationpriority, NULL, parentmodificationid, FALSE
FROM oldmodification NATURAL JOIN {1}
WHERE parentmodificationid IN (SELECT modificationid FROM machinemodification)
  AND modificationid NOT IN (SELECT modificationid FROM {0}modification)
",
                                               "global", tableName));
    }
    
    void ConvertToMachineModificationStep3 (string tableName)
    {
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}modification
SELECT modificationid, revisionid, modificationdatetime, modificationreferencedtable, modificationpriority, parentmodificationid, NULL, FALSE, machineid
FROM oldmodification NATURAL JOIN {1}
WHERE parentmodificationid IN (SELECT modificationid FROM globalmodification)
  AND modificationid NOT IN (SELECT modificationid FROM {0}modification)
",
                                               "machine", tableName));
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}modification
SELECT modificationid, revisionid, modificationdatetime, modificationreferencedtable, modificationpriority, NULL, parentmodificationid, FALSE, machineid
FROM oldmodification NATURAL JOIN {1}
WHERE parentmodificationid IN (SELECT modificationid FROM machinemodification)
  AND modificationid NOT IN (SELECT modificationid FROM {0}modification)
",
                                               "machine", tableName));
      
    }
    
    void UpdateSubModifications (string prefix)
    {
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0}modificationstatus
SET analysissubglobalmodifications=TRUE
WHERE EXISTS (SELECT 1 FROM globalmodification
              WHERE {0}modificationstatus.modificationid=globalmodification.parent{0}modificationid)
",
                                               prefix));
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0}modificationstatus
SET analysissubmachinemodifications=TRUE
WHERE EXISTS (SELECT 1 FROM machinemodification
              WHERE {0}modificationstatus.modificationid=machinemodification.parent{0}modificationid)
",
                                               prefix));
    }

    void RevertConvert (string prefix, string tableName)
    {
      // Cannot go further: oldmodification is empty due to missing "down" in 310
      Database.GenerateForeignKey (tableName, ColumnName.MODIFICATION_ID,
                                   "old" + TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
  }
}

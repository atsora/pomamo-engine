// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 305:
  /// </summary>
  [Migration(305)]
  public class MachineModificationRestoreForeignKey: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineModificationRestoreForeignKey).FullName);
    static readonly IList<string> GLOBAL_MODIFICATIONS = new List<string> {"daytemplatechange", "workorderlineassociation", "operationinformation",
      "shifttemplateassociation", "componentintermediateworkpieceupdate",
      "intermediateworkpieceoperationupdate", "projectcomponentupdate", "workorderprojectupdate",
      "usermachineassociation", "usershiftassociation", "userattendance"};
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
      foreach (var tableName in GLOBAL_MODIFICATIONS) {
        ConvertToModificationStep4 ("global", tableName);
      }
      foreach (var tableName in MACHINE_MODIFICATIONS) {
        ConvertToModificationStep4 ("machine", tableName);
      }
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
    
    void ConvertToModificationStep4 (string prefix, string tableName)
    {
      Database.GenerateForeignKey (tableName, ColumnName.MODIFICATION_ID,
                                   prefix + TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.ExecuteNonQuery (string.Format(@"CREATE OR REPLACE RULE {0}_delete AS
ON DELETE TO {0} DO DELETE FROM {1}modification
WHERE {1}modification.modificationid=old.modificationid",
                                              tableName, prefix));
    }
    
    void RevertConvert (string prefix, string tableName)
    {
      Database.RemoveForeignKey (tableName, "fk_" + tableName + "_" + prefix + "modification");
      Database.ExecuteNonQuery (string.Format(@"CREATE OR REPLACE RULE {0}_delete AS
ON DELETE TO {0} DO DELETE FROM oldmodification
WHERE oldmodification.modificationid=old.modificationid",
                                              tableName));
    }
    
  }
}

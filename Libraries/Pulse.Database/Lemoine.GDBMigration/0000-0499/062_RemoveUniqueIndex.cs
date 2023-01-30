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
  /// Migration 062: Remove the unique index in some tables
  /// </summary>
  [Migration(62)]
  public class RemoveUniqueIndex: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemoveUniqueIndex).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // workorder_workordername_idx      
      RemoveIndex(TableName.WORK_ORDER, "workordername");
      AddIndexCondition (TableName.WORK_ORDER,
                         "workordername IS NOT NULL",
                         "workordername");
      // workorder_workordercode_idx      
      RemoveIndex(TableName.WORK_ORDER, "workordercode");
      AddIndexCondition (TableName.WORK_ORDER,
                         "workordercode IS NOT NULL",
                         "workordercode");
      // workorder_workorderexternalcode_idx      
      RemoveIndex(TableName.WORK_ORDER, "workorderexternalcode");
      AddIndexCondition (TableName.WORK_ORDER,
                         "workorderexternalcode IS NOT NULL",
                         "workorderexternalcode");
      
      // project_projectname_idx      
      RemoveIndex(TableName.PROJECT,"projectname");
      AddIndexCondition (TableName.PROJECT,
                         "projectname IS NOT NULL",
                         "projectname");
      // project_projectcode_idx      
      RemoveIndex(TableName.PROJECT,"projectcode");
      AddIndexCondition (TableName.PROJECT,
                         "projectcode IS NOT NULL",
                         "projectcode");
      // project_projectexternalcode_idx      
      RemoveIndex(TableName.PROJECT,"projectexternalcode");
      AddIndexCondition (TableName.PROJECT,
                         "projectexternalcode IS NOT NULL",
                         "projectexternalcode");
      
      // user_username_idx      
      RemoveIndex(TableName.USER, "username");
      AddIndexCondition (TableName.USER,
                         "username IS NOT NULL",
                         "username");
      // user_usercode_idx      
      RemoveIndex(TableName.USER, "usercode");
      AddIndexCondition (TableName.USER,
                         "usercode IS NOT NULL",
                         "usercode");
      // user_userexternalcode_idx      
      RemoveIndex(TableName.USER, "userexternalcode");
      AddIndexCondition (TableName.USER,
                         "userexternalcode IS NOT NULL",
                         "userexternalcode");
      // user_userlogin_idx
      RemoveIndex(TableName.USER, "userlogin");
      AddIndex (TableName.USER,
                "userlogin");
      
      // computer_computername_idx
      RemoveIndex(TableName.COMPUTER, "computername");
      AddIndexCondition (TableName.COMPUTER,
                         "computername IS NOT NULL",
                         "computername");
      
      // machine_machinename_idx
      RemoveIndex(TableName.MACHINE, "machinename");
      AddIndexCondition (TableName.MACHINE,
                         "machinename IS NOT NULL",
                         "machinename");
      // machine_machinecode_idx
      RemoveIndex(TableName.MACHINE, "machinecode");
      AddIndexCondition (TableName.MACHINE,
                         "machinecode IS NOT NULL",
                         "machinecode");
      // machine_machineexternalcode_idx
      RemoveIndex(TableName.MACHINE, "machineexternalcode");
      AddIndexCondition (TableName.MACHINE,
                         "machineexternalcode IS NOT NULL",
                         "machineexternalcode");
      
      // operation_operationexternalcode_idx
      RemoveIndex(TableName.OPERATION, "operationexternalcode");
      AddIndexCondition (TableName.OPERATION,
                         "operationexternalcode IS NOT NULL",
                         "operationexternalcode");
      
      // intermediateworkpiece_intermediateworkpieceexternalcode_idx
      RemoveIndex(TableName.INTERMEDIATE_WORK_PIECE, "intermediateworkpieceexternalcode");
      AddIndexCondition (TableName.INTERMEDIATE_WORK_PIECE,
                         "intermediateworkpieceexternalcode IS NOT NULL",
                         "intermediateworkpieceexternalcode");
      
      // component_projectid_componentname_idx
      RemoveIndex (TableName.COMPONENT, ColumnName.PROJECT_ID, "componentname");      
      Database.ExecuteNonQuery ("CREATE INDEX component_projectid_componentname_idx " +
                                "ON component (projectid, componentname) " +
                                "WHERE componentname IS NOT NULL;");
      // component_projectid_componetncode_idx
      RemoveIndex (TableName.COMPONENT, ColumnName.PROJECT_ID, "componentcode");
      Database.ExecuteNonQuery ("CREATE INDEX component_projectid_componentcode_idx " +
                                "ON component (projectid, componentcode) " +
                                "WHERE componentcode IS NOT NULL;");
      // component_componentexternalcode_idx      
      RemoveIndex(TableName.COMPONENT, "componentexternalcode");
      AddIndexCondition (TableName.COMPONENT,
                         "componentexternalcode IS NOT NULL",
                         "componentexternalcode");
      
      // monitoredmachine_mainmachinemoduleid_idx
      RemoveIndex (TableName.MONITORED_MACHINE, "mainmachinemoduleid");
      Database.ExecuteNonQuery ("CREATE INDEX monitoredmachine_mainmachinemoduleid_idx " +
                                "ON monitoredmachine (mainmachinemoduleid) " +
                                "WHERE mainmachinemoduleid IS NOT NULL;");
      // machinemodule_machineid_name_idx
      RemoveIndex (TableName.MACHINE_MODULE, ColumnName.MACHINE_ID, "name");
      Database.ExecuteNonQuery ("CREATE INDEX machinemodule_machineid_name_idx " +
                                "ON machinemodule (machineid, machinemodulename) " +
                                "WHERE machinemodulename IS NOT NULL;");
      // machinemodule_machineid_code_idx
      RemoveIndex (TableName.MACHINE_MODULE, ColumnName.MACHINE_ID, "code");
      Database.ExecuteNonQuery ("CREATE INDEX machinemodule_machineid_code_idx " +
                                "ON machinemodule (machineid, machinemodulecode) " +
                                "WHERE machinemodulecode IS NOT NULL;");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}

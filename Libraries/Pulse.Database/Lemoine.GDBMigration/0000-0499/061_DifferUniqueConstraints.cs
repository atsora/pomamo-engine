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
  /// Migration 061: Differ most unique constraints
  /// </summary>
  [Migration(61)]
  public class DifferUniqueConstraints: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DifferUniqueConstraints).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // component_projectid_name_key
      Database.RemoveConstraint (TableName.COMPONENT,
                                 "component_projectid_name_key");
      AddUniqueConstraint (TableName.COMPONENT,
                           ColumnName.PROJECT_ID,
                           "componentname");

      // component_projectid_code_key
      Database.RemoveConstraint (TableName.COMPONENT,
                                 "component_projectid_code_key");
      AddUniqueConstraint (TableName.COMPONENT,
                           ColumnName.PROJECT_ID,
                           "componentcode");
      
      // component_componentexternalcode_key
      Database.RemoveConstraint (TableName.COMPONENT,
                                 "component_componentexternalcode_key");
      AddUniqueConstraint (TableName.COMPONENT,
                           "componentexternalcode");
      
      // machinemodule_machineid_name_key
      Database.RemoveConstraint (TableName.MACHINE_MODULE,
                                 "machinemodule_machineid_name_key");
      AddUniqueConstraint (TableName.MACHINE_MODULE,
                           ColumnName.MACHINE_ID,
                           "machinemodulename");

      // machinemodule_machineid_code_key
      Database.RemoveConstraint (TableName.MACHINE_MODULE,
                                 "machinemodule_machineid_code_key");
      AddUniqueConstraint (TableName.MACHINE_MODULE,
                           ColumnName.MACHINE_ID,
                           "machinemodulecode");
      
      // MachineModeDefaultReason_SecondaryKey
      Database.RemoveConstraint (TableName.MACHINE_MODE_DEFAULT_REASON,
                                 "machinemodedefaultreason_secondarykey");
      AddUniqueConstraint (TableName.MACHINE_MODE_DEFAULT_REASON,
                           ColumnName.MACHINE_MODE_ID,
                           ColumnName.MACHINE_OBSERVATION_STATE_ID,
                           "defaultreasonmaximumduration");
      
      // ReasonSelection_SecondaryKey
      Database.RemoveConstraint (TableName.REASON_SELECTION,
                                 "reasonselection_secondarykey");
      AddUniqueConstraint (TableName.REASON_SELECTION,
                           ColumnName.MACHINE_MODE_ID,
                           ColumnName.MACHINE_OBSERVATION_STATE_ID,
                           ColumnName.REASON_ID);
      
      // eventlongperiodconfig_unique
      Database.RemoveConstraint (TableName.EVENT_LONG_PERIOD_CONFIG,
                                 "eventlongperiodconfig_unique");
      AddUniqueConstraint (TableName.EVENT_LONG_PERIOD_CONFIG,
                           ColumnName.MACHINE_ID,
                           ColumnName.MACHINE_MODE_ID,
                           ColumnName.MACHINE_OBSERVATION_STATE_ID,
                           "eventtriggerduration");
      
      // workorderproject_secondarykey
      // Do not differ this one for the moment
      
      // componentintermediateworkpiece_secondarykey
      // Do not differ this one for the moment
      
      // On work order
      Database.RemoveConstraint (TableName.WORK_ORDER,
                                 "workorder_workordercode_key");
      AddUniqueConstraint (TableName.WORK_ORDER,
                           "workordercode");
      Database.RemoveConstraint (TableName.WORK_ORDER,
                                 "workorder_workordername_key");
      AddUniqueConstraint (TableName.WORK_ORDER,
                           "workordername");
      Database.RemoveConstraint (TableName.WORK_ORDER,
                                 "workorder_workorderexternalcode_key");
      AddUniqueConstraint (TableName.WORK_ORDER,
                           "workorderexternalcode");
      
      // On project
      Database.RemoveConstraint (TableName.PROJECT,
                                 "project_projectcode_key");
      AddUniqueConstraint (TableName.PROJECT,
                           "projectcode");
      Database.RemoveConstraint (TableName.PROJECT,
                                 "project_projectname_key");
      AddUniqueConstraint (TableName.PROJECT,
                           "projectname");
      Database.RemoveConstraint (TableName.PROJECT,
                                 "project_projectexternalcode_key");
      AddUniqueConstraint (TableName.PROJECT,
                           "projectexternalcode");
      
      // On User
      Database.RemoveConstraint (TableName.USER,
                                 "usertable_usercode_key");
      AddUniqueConstraint (TableName.USER,
                           "usercode");
      Database.RemoveConstraint (TableName.USER,
                                 "usertable_username_key");
      AddUniqueConstraint (TableName.USER,
                           "username");
      Database.RemoveConstraint (TableName.USER,
                                 "usertable_userexternalcode_key");
      AddUniqueConstraint (TableName.USER,
                           "userexternalcode");
      Database.RemoveConstraint (TableName.USER,
                                 "usertable_userlogin_key");
      AddUniqueConstraint (TableName.USER,
                           "userlogin");
      
      // On Machine
      Database.RemoveConstraint (TableName.MACHINE,
                                 "machine_machinecode_key");
      AddUniqueConstraint (TableName.MACHINE,
                           "machinecode");
      Database.RemoveConstraint (TableName.MACHINE,
                                 "machine_machinename_key");
      AddUniqueConstraint (TableName.MACHINE,
                           "machinename");
      
      // On Operation
      Database.RemoveConstraint (TableName.OPERATION,
                                 "operation_operationexternalcode_key");
      AddUniqueConstraint (TableName.OPERATION,
                           "operationexternalcode");
      
      // On IntermediateWorkPiece
      Database.RemoveConstraint (TableName.INTERMEDIATE_WORK_PIECE,
                                 "intermediateworkpiece_intermediateworkpieceexternalcode_key");
      AddUniqueConstraint (TableName.INTERMEDIATE_WORK_PIECE,
                           "intermediateworkpieceexternalcode");      
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}

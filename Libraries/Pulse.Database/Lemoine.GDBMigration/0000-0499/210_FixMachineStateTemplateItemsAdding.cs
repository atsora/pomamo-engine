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
  /// Migration 210:
  /// </summary>
  [Migration(210)]
  public class FixMachineStateTemplateItemsAdding: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixMachineStateTemplateItemsAdding).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if(Database.TableExists(TableName.MACHINE_STATE_TEMPLATE_ITEM)){
        if(Database.ColumnExists(TableName.MACHINE_STATE_TEMPLATE_ITEM,TableName.MACHINE_STATE_TEMPLATE+"id")){
          Database.ExecuteNonQuery(@"ALTER TABLE machinestatetemplateitem
                                     ALTER COLUMN machinestatetemplateid DROP NOT NULL;");
        }
        if(Database.ColumnExists(TableName.MACHINE_STATE_TEMPLATE_ITEM,TableName.MACHINE_STATE_TEMPLATE_ITEM+"order")){
          Database.ExecuteNonQuery(@"ALTER TABLE machinestatetemplateitem
                                     ALTER COLUMN machinestatetemplateitemorder DROP NOT NULL;");
        }
      }
      if(Database.TableExists(TableName.MACHINE_STATE_TEMPLATE_STOP)){
        if(Database.ColumnExists(TableName.MACHINE_STATE_TEMPLATE_STOP,TableName.MACHINE_STATE_TEMPLATE+"id")){
          Database.ExecuteNonQuery(@"ALTER TABLE machinestatetemplatestop
                                     ALTER COLUMN machinestatetemplateid DROP NOT NULL;");
        }
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if(Database.TableExists(TableName.MACHINE_STATE_TEMPLATE_ITEM)){
        if(Database.ColumnExists(TableName.MACHINE_STATE_TEMPLATE_ITEM,TableName.MACHINE_STATE_TEMPLATE+"id")){
          Database.ExecuteNonQuery(@"ALTER TABLE machinestatetemplateitem 
                                     ALTER COLUMN machinestatetemplateid SET NOT NULL;");
        }
        if(Database.ColumnExists(TableName.MACHINE_STATE_TEMPLATE_ITEM,TableName.MACHINE_STATE_TEMPLATE_ITEM+"order")){
          Database.ExecuteNonQuery(@"ALTER TABLE machinestatetemplateitem 
                                     ALTER COLUMN machinestatetemplateitemorder SET NOT NULL;");
        }
      }
      if(Database.TableExists(TableName.MACHINE_STATE_TEMPLATE_STOP)){
        if(Database.ColumnExists(TableName.MACHINE_STATE_TEMPLATE_STOP,TableName.MACHINE_STATE_TEMPLATE+"id")){
          Database.ExecuteNonQuery(@"ALTER TABLE machinestatetemplatestop 
                                     ALTER COLUMN machinestatetemplateid SET NOT NULL;");
        }
      }
    }
  }
}

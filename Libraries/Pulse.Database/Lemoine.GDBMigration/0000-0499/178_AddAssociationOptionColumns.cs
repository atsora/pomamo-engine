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
  /// Migration 178: add associationoption to table OperationMachineAssociation,
  /// ComponentMachineAssociation and WorkOrderMachineAssociation.
  /// </summary>
  [Migration(178)]
  public class AddAssociationOptionColumns: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddAssociationOptionColumns).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.OPERATION_MACHINE_ASSOCIATION,
                          new Column (TableName.OPERATION_MACHINE_ASSOCIATION + "option", DbType.Int32));
      Database.AddColumn (TableName.COMPONENT_MACHINE_ASSOCIATION,
                          new Column (TableName.COMPONENT_MACHINE_ASSOCIATION + "option", DbType.Int32));
      Database.AddColumn (TableName.WORKORDER_MACHINE_ASSOCIATION,
                          new Column (TableName.WORKORDER_MACHINE_ASSOCIATION + "option", DbType.Int32));
      Database.AddColumn (TableName.REASON_MACHINE_ASSOCIATION,
                          new Column (TableName.REASON_MACHINE_ASSOCIATION + "option", DbType.Int32));
      
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.OPERATION_MACHINE_ASSOCIATION,
                             TableName.OPERATION_MACHINE_ASSOCIATION + "option");
      Database.RemoveColumn (TableName.COMPONENT_MACHINE_ASSOCIATION,
                             TableName.COMPONENT_MACHINE_ASSOCIATION + "option");
      Database.RemoveColumn (TableName.WORKORDER_MACHINE_ASSOCIATION,
                             TableName.WORKORDER_MACHINE_ASSOCIATION + "option");
      Database.RemoveColumn (TableName.REASON_MACHINE_ASSOCIATION,
                             TableName.REASON_MACHINE_ASSOCIATION + "option");
      
    }
  }
}

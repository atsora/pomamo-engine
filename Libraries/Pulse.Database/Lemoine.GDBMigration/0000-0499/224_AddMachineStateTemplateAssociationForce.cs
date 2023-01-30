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
  /// Migration 224: add a column to machinestatetemplateassociation to force the re-build of observationstateslot
  /// </summary>
  [Migration(224)]
  public class AddMachineStateTemplateAssociationForce: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineStateTemplateAssociationForce).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.ColumnExists (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION,
                                 TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION + "force")) {
        Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION,
                            new Column (TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION + "force", DbType.Boolean, ColumnProperty.NotNull, "FALSE"));
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}

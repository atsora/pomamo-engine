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
  /// Migration 223: Remove composite key from : UserMachineSlotMachine and UserMachineAssociationMachine
  /// and restore a basic primary key
  /// </summary>
  [Migration(223)]
  public class AddAddPkToUMSMandUMAM: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddAddPkToUMSMandUMAM).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      //UserMachineSlotMachine
      if (!Database.ColumnExists (TableName.USER_MACHINE_SLOT_MACHINE,
                                  TableName.USER_MACHINE_SLOT_MACHINE + "id")) {
        Database.RemoveConstraint(TableName.USER_MACHINE_SLOT_MACHINE, "usermachineslotmachine_pkey");
        Database.AddColumn(TableName.USER_MACHINE_SLOT_MACHINE,
                           new Column (TableName.USER_MACHINE_SLOT_MACHINE + "id",
                                       DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity));
        AddUniqueConstraint (TableName.USER_MACHINE_SLOT_MACHINE,
                             TableName.USER_MACHINE_SLOT + "id",
                             ColumnName.MACHINE_ID);
      }
      else { // Re-build the primary key
        Database.RemoveConstraint(TableName.USER_MACHINE_SLOT_MACHINE, "usermachineslotmachine_pkey");
        Database.AddPrimaryKey ("usermachineslotmachine_pkey",
                                TableName.USER_MACHINE_SLOT_MACHINE,
                                TableName.USER_MACHINE_SLOT_MACHINE + "id");
      }
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1} DROP NOT NULL",
                                               TableName.USER_MACHINE_SLOT_MACHINE,
                                               TableName.USER_MACHINE_SLOT + "id"));
      
      //UserMachineAssociationMachine
      if (!Database.ColumnExists (TableName.USER_MACHINE_ASSOCIATION_MACHINE,
                                  TableName.USER_MACHINE_ASSOCIATION_MACHINE + "id")) {
        Database.RemoveConstraint(TableName.USER_MACHINE_ASSOCIATION_MACHINE, "usermachineassociationmachine_pkey");
        Database.AddColumn(TableName.USER_MACHINE_ASSOCIATION_MACHINE,
                           new Column (TableName.USER_MACHINE_ASSOCIATION_MACHINE + "id",
                                       DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity));
        AddUniqueConstraint (TableName.USER_MACHINE_ASSOCIATION_MACHINE,
                             ColumnName.MODIFICATION_ID,
                             ColumnName.MACHINE_ID);
      }
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1} DROP NOT NULL",
                                               TableName.USER_MACHINE_ASSOCIATION_MACHINE,
                                               ColumnName.MODIFICATION_ID));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}

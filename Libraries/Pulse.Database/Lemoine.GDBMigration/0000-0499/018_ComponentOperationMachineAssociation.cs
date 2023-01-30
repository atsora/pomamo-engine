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
  /// Migration 018: New tables to store the component / machine, operation / machine and user / machine associations
  /// <item>ComponentMachineAssociation</item>
  /// <item>OperationMachineAssociation</item>
  /// <item>UserMachineAssociation</item>
  /// </summary>
  [Migration(18)]
  public class ComponentOperationMachineAssociation: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ComponentOperationMachineAssociation).FullName);
    
    static readonly string MACHINE_TABLE = "Machine";
    static readonly string MACHINE_ID = "MachineId";
    static readonly string MODIFICATION_TABLE = "Modification";
    static readonly string MODIFICATION_ID = "ModificationId";
    static readonly string COMPONENT_TABLE = "Component";
    static readonly string COMPONENT_ID = "ComponentId";
    static readonly string OPERATION_TABLE = "Operation";
    static readonly string OPERATION_ID = "OperationId";
    static readonly string USER_ID = "UserId";

    static readonly string COMPONENT_MACHINE_ASSOCIATION_TABLE = "ComponentMachineAssociation";
    static readonly string OPERATION_MACHINE_ASSOCIATION_TABLE = "OperationMachineAssociation";
    static readonly string USER_MACHINE_ASSOCIATION_TABLE = "UserMachineAssociation";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (COMPONENT_MACHINE_ASSOCIATION_TABLE)) {
        Database.AddTable (COMPONENT_MACHINE_ASSOCIATION_TABLE,
                           new Column (MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column (COMPONENT_ID, DbType.Int32),
                           new Column (MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                           new Column ("ComponentMachineAssociationBeginDateTime", DbType.DateTime, ColumnProperty.NotNull),
                           new Column ("ComponentMachineAssociationEndDateTime", DbType.DateTime));
        Database.GenerateForeignKey (COMPONENT_MACHINE_ASSOCIATION_TABLE, MODIFICATION_ID,
                                     MODIFICATION_TABLE, MODIFICATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (COMPONENT_MACHINE_ASSOCIATION_TABLE, COMPONENT_ID,
                                     COMPONENT_TABLE, COMPONENT_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (COMPONENT_MACHINE_ASSOCIATION_TABLE, MACHINE_ID,
                                     MACHINE_TABLE, MACHINE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.ExecuteNonQuery (string.Format (@"CREATE RULE {0}_delete AS
ON DELETE TO {0}
DO ALSO DELETE FROM modification
  WHERE modificationid = OLD.modificationid;",
                                                 COMPONENT_MACHINE_ASSOCIATION_TABLE));
      }
      if (!Database.TableExists (OPERATION_MACHINE_ASSOCIATION_TABLE)) {
        Database.AddTable (OPERATION_MACHINE_ASSOCIATION_TABLE,
                           new Column (MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column (OPERATION_ID, DbType.Int32),
                           new Column (MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                           new Column ("OperationMachineAssociationBeginDateTime", DbType.DateTime, ColumnProperty.NotNull),
                           new Column ("OperationMachineAssociationEndDateTime", DbType.DateTime));
        Database.GenerateForeignKey (OPERATION_MACHINE_ASSOCIATION_TABLE, MODIFICATION_ID,
                                     MODIFICATION_TABLE, MODIFICATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (OPERATION_MACHINE_ASSOCIATION_TABLE, OPERATION_ID,
                                     OPERATION_TABLE, OPERATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (OPERATION_MACHINE_ASSOCIATION_TABLE, MACHINE_ID,
                                     MACHINE_TABLE, MACHINE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.ExecuteNonQuery (string.Format (@"CREATE RULE {0}_delete AS
ON DELETE TO {0}
DO ALSO DELETE FROM modification
  WHERE modificationid = OLD.modificationid;",
                                                 OPERATION_MACHINE_ASSOCIATION_TABLE));
      }
      if (!Database.TableExists (USER_MACHINE_ASSOCIATION_TABLE)) {
        Database.AddTable (USER_MACHINE_ASSOCIATION_TABLE,
                           new Column (MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column (USER_ID, DbType.Int32),
                           new Column (MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                           new Column ("UserMachineAssociationBeginDateTime", DbType.DateTime, ColumnProperty.NotNull),
                           new Column ("UserMachineAssociationEndDateTime", DbType.DateTime));
        Database.GenerateForeignKey (USER_MACHINE_ASSOCIATION_TABLE, MODIFICATION_ID,
                                     MODIFICATION_TABLE, MODIFICATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (USER_MACHINE_ASSOCIATION_TABLE, USER_ID,
                                     TableName.USER, USER_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (USER_MACHINE_ASSOCIATION_TABLE, MACHINE_ID,
                                     MACHINE_TABLE, MACHINE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.ExecuteNonQuery (string.Format (@"CREATE RULE {0}_delete AS
ON DELETE TO {0}
DO ALSO DELETE FROM modification
  WHERE modificationid = OLD.modificationid;",
                                                 USER_MACHINE_ASSOCIATION_TABLE));
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // New tables deletion
      if (Database.TableExists (COMPONENT_MACHINE_ASSOCIATION_TABLE)) {
        Database.ExecuteNonQuery (@"DELETE FROM modification 
WHERE modificationreferencedtable='ComponentMachineAssociation'");
        Database.RemoveTable (COMPONENT_MACHINE_ASSOCIATION_TABLE);
      }
      if (Database.TableExists (OPERATION_MACHINE_ASSOCIATION_TABLE)) {
        Database.ExecuteNonQuery (@"DELETE FROM modification 
WHERE modificationreferencedtable='OperationMachineAssociation'");
        Database.RemoveTable (OPERATION_MACHINE_ASSOCIATION_TABLE);
      }
      if (Database.TableExists (USER_MACHINE_ASSOCIATION_TABLE)) {
        Database.ExecuteNonQuery (@"DELETE FROM modification 
WHERE modificationreferencedtable='UserMachineAssociation'");
        Database.RemoveTable (USER_MACHINE_ASSOCIATION_TABLE);
      }
    }
  }
}

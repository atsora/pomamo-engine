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
  /// Migration 185: add a few user slots and associated modifications
  /// </summary>
  [Migration(185)]
  public class AddUserSlots: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddUserSlots).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddUserShiftAssociation ();
      AddUserMachineAssociation ();
      AddUserShiftSlot ();
      AddUserMachineSlot ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveUserMachineSlot ();
      RemoveUserShiftSlot ();
      RemoveUserMachineAssociation ();
      RemoveUserShiftAssociation ();
    }
    
    void AddUserShiftAssociation ()
    {
      Database.AddTable (TableName.USER_SHIFT_ASSOCIATION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.USER_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.SHIFT_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (TableName.USER_SHIFT_ASSOCIATION + "begin", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (TableName.USER_SHIFT_ASSOCIATION + "end", DbType.DateTime, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.USER_SHIFT_ASSOCIATION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.USER_SHIFT_ASSOCIATION, ColumnName.USER_ID,
                                   TableName.USER, ColumnName.USER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.USER_SHIFT_ASSOCIATION, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      SetModificationTable (TableName.USER_SHIFT_ASSOCIATION);
    }
    
    void RemoveUserShiftAssociation ()
    {
      Database.ExecuteNonQuery (string.Format (@"DELETE FROM modification
WHERE modificationreferencedtable='{0}'",
                                               TableName.USER_SHIFT_ASSOCIATION));
      Database.RemoveTable (TableName.USER_SHIFT_ASSOCIATION);
    }

    void AddUserMachineAssociation ()
    {
      // - UserMachineAssociation
      Database.AddTable (TableName.USER_MACHINE_ASSOCIATION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.USER_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.USER_MACHINE_ASSOCIATION + "begin", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (TableName.USER_MACHINE_ASSOCIATION + "end", DbType.DateTime, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.USER_MACHINE_ASSOCIATION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.USER_MACHINE_ASSOCIATION, ColumnName.USER_ID,
                                   TableName.USER, ColumnName.USER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      SetModificationTable (TableName.USER_MACHINE_ASSOCIATION);

      // - UserMachineAssociationMachine
      Database.AddTable (TableName.USER_MACHINE_ASSOCIATION_MACHINE,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.USER_MACHINE_ASSOCIATION_MACHINE, ColumnName.MODIFICATION_ID,
                                   TableName.USER_MACHINE_ASSOCIATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.USER_MACHINE_ASSOCIATION_MACHINE, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.USER_MACHINE_ASSOCIATION_MACHINE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void RemoveUserMachineAssociation ()
    {
      Database.RemoveTable (TableName.USER_MACHINE_ASSOCIATION_MACHINE);
      Database.ExecuteNonQuery (string.Format (@"DELETE FROM modification
WHERE modificationreferencedtable='{0}'",
                                               TableName.USER_MACHINE_ASSOCIATION));
      Database.RemoveTable (TableName.USER_MACHINE_ASSOCIATION);
    }
    
    void AddUserShiftSlot ()
    {
      Database.AddTable (TableName.USER_SHIFT_SLOT,
                         new Column (TableName.USER_SHIFT_SLOT + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.USER_SHIFT_SLOT + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.USER_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.USER_SHIFT_SLOT + "begin", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (TableName.USER_SHIFT_SLOT + "end", DbType.DateTime, ColumnProperty.Null),
                         new Column (ColumnName.SHIFT_ID, DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.USER_SHIFT_SLOT, ColumnName.USER_ID,
                                   TableName.USER, ColumnName.USER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.USER_SHIFT_SLOT, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      // Constraints
      Database.AddCheckConstraint (string.Format ("{0}_beginend", TableName.USER_SHIFT_SLOT),
                                   TableName.USER_SHIFT_SLOT,
                                   string.Format ("({1} IS NULL) OR ({0} < {1})",
                                                  TableName.USER_SHIFT_SLOT + "begin",
                                                  TableName.USER_SHIFT_SLOT + "end"));
      // Index
      AddUniqueConstraint (TableName.USER_SHIFT_SLOT,
                           ColumnName.USER_ID,
                           TableName.USER_SHIFT_SLOT + "begin"); // Makes an index too
      AddIndex (TableName.USER_SHIFT_SLOT,
                ColumnName.USER_ID,
                TableName.USER_SHIFT_SLOT + "end",
                TableName.USER_SHIFT_SLOT + "begin");
      AddUniqueConstraint (TableName.USER_SHIFT_SLOT,
                           ColumnName.USER_ID,
                           TableName.USER_SHIFT_SLOT + "end");
    }
    
    void RemoveUserShiftSlot ()
    {
      Database.RemoveTable (TableName.USER_SHIFT_SLOT);
    }
    
    void AddUserMachineSlot ()
    {
      // - usermachineslot
      Database.AddTable (TableName.USER_MACHINE_SLOT,
                         new Column (TableName.USER_MACHINE_SLOT + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.USER_MACHINE_SLOT + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.USER_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.USER_MACHINE_SLOT + "begin", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (TableName.USER_MACHINE_SLOT + "end", DbType.DateTime, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.USER_MACHINE_SLOT, ColumnName.USER_ID,
                                   TableName.USER, ColumnName.USER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      // Constraints
      Database.AddCheckConstraint (string.Format ("{0}_beginend", TableName.USER_MACHINE_SLOT),
                                   TableName.USER_MACHINE_SLOT,
                                   string.Format ("({1} IS NULL) OR ({0} < {1})",
                                                  TableName.USER_MACHINE_SLOT + "begin",
                                                  TableName.USER_MACHINE_SLOT + "end"));
      // Index
      AddUniqueConstraint (TableName.USER_MACHINE_SLOT,
                           ColumnName.USER_ID,
                           TableName.USER_MACHINE_SLOT + "begin"); // Makes an index too
      AddIndex (TableName.USER_MACHINE_SLOT,
                ColumnName.USER_ID,
                TableName.USER_MACHINE_SLOT + "end",
                TableName.USER_MACHINE_SLOT + "begin");
      AddUniqueConstraint (TableName.USER_MACHINE_SLOT,
                           ColumnName.USER_ID,
                           TableName.USER_MACHINE_SLOT + "end");
      
      // - usermachineslotmachine
      Database.AddTable (TableName.USER_MACHINE_SLOT_MACHINE,
                         new Column (TableName.USER_MACHINE_SLOT_MACHINE + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.USER_MACHINE_SLOT + "id", DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.USER_MACHINE_SLOT_MACHINE, TableName.USER_MACHINE_SLOT + "id",
                                   TableName.USER_MACHINE_SLOT, TableName.USER_MACHINE_SLOT + "id",
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.USER_MACHINE_SLOT_MACHINE, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.USER_MACHINE_SLOT_MACHINE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddUniqueConstraint (TableName.USER_MACHINE_SLOT_MACHINE,
                           TableName.USER_MACHINE_SLOT + "id",
                           ColumnName.MACHINE_ID);
    }
    
    void RemoveUserMachineSlot ()
    {
      Database.RemoveTable (TableName.USER_MACHINE_SLOT_MACHINE);
      Database.RemoveTable (TableName.USER_MACHINE_SLOT);
    }
  }
}

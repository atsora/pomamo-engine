// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 294: fix constraint names
  /// </summary>
  [Migration(294)]
  public class RenameConstraints: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RenameConstraints).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveConstraints();
      AddGoodConstraints();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      RemoveConstraints();
      AddBadConstraints();
    }
    
    void RemoveConstraints()
    {
      // From migration 286
      Database.RemoveConstraint (TableName.EVENT_TOOL_LIFE_CONFIG, "uniquekey");
      
      // From migration 288
      try {
        Database.RemoveForeignKey(TableName.EVENT_TOOL_LIFE, "fk");
      } catch {}
      try {
        Database.RemoveForeignKey(TableName.EVENT_TOOL_LIFE, "fk_eventtoollife_machineobservationstate");
      } catch {}
    }
    
    void AddGoodConstraints()
    {

      // From migration 286
      AddUniqueConstraint(TableName.EVENT_TOOL_LIFE_CONFIG,
                          ColumnName.MACHINE_FILTER_ID,
                          ColumnName.MACHINE_OBSERVATION_STATE_ID,
                          TableName.EVENT_TOOL_LIFE_CONFIG + "typeid");
      
      // From migration 288
      Database.GenerateForeignKey(TableName.EVENT_TOOL_LIFE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                  TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    void AddBadConstraints()
    {
      // From migration 286
      AddNamedUniqueConstraint("uniquekey", TableName.EVENT_TOOL_LIFE_CONFIG,
                            ColumnName.MACHINE_FILTER_ID,
                            ColumnName.MACHINE_OBSERVATION_STATE_ID,
                            TableName.EVENT_TOOL_LIFE_CONFIG + "typeid");
      
      // From migration 288
      Database.AddForeignKey("fk", TableName.EVENT_TOOL_LIFE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                             TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                             Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
  }
}

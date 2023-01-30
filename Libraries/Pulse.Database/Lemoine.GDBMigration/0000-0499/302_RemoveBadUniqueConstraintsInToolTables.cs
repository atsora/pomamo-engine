// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 302: remove some bad unique constraints in some tool tables that prevent from partitioning them
  /// 
  /// There was a bug in migration 294 that explained why there were still here
  /// </summary>
  [Migration(302)]
  public class RemoveBadUniqueConstraintsInToolTables: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemoveBadUniqueConstraintsInToolTables).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      try {
        Database.RemoveConstraint (TableName.TOOL_POSITION, "uniqueposition");
        Database.RemoveConstraint (TableName.TOOL_LIFE, "uniquelifevaluetype");
        Database.RemoveConstraint (TableName.EVENT_TOOL_LIFE_CONFIG, "uniquekey");
      } catch (Exception e) {
        log.ErrorFormat("Couldn't remove unique constraint: {0}", e);
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

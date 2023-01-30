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
  /// Migration 309:
  /// </summary>
  [Migration(309)]
  public class SetMachineStateTemplateAssociationBeginNullable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (SetMachineStateTemplateAssociationBeginNullable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} DROP NOT NULL",
                                               TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION,
                                               "begin"));
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} DROP NOT NULL",
                                               TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION,
                                               "begin"));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.MACHINE_STATE_TEMPLATE_ASSOCIATION,
                                               "begin"));
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION,
                                               "begin"));
    }
  }
}

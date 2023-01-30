// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration ReasonProposalDetailsAsText: 
  /// </summary>
  [Migration (635)]
  public class ReasonProposalDetailsAsText : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ReasonProposalDetailsAsText).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      MakeColumnText (TableName.REASON_PROPOSAL, TableName.REASON_PROPOSAL + "details");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1}
SET DATA TYPE CHARACTER VARYING(255)",
                                               TableName.REASON_PROPOSAL, TableName.REASON_PROPOSAL + "details"));
    }
  }
}

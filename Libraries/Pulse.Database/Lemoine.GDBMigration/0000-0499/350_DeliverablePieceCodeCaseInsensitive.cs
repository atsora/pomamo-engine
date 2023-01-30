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
  /// Migration 350:
  /// </summary>
  [Migration(350)]
  public class DeliverablePieceCodeCaseInsensitive: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DeliverablePieceCodeCaseInsensitive).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      MakeColumnCaseInsensitive (TableName.DELIVERABLE_PIECE, TableName.DELIVERABLE_PIECE + "code");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1}
SET DATA TYPE VARCHAR(255)",
                                               TableName.DELIVERABLE_PIECE, TableName.DELIVERABLE_PIECE + "code"));
    }
  }
}

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
  /// Migration 515:
  /// </summary>
  [Migration (515)]
  public class SequenceToolNumber: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (SequenceToolNumber).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"
DELETE FROM schemainfo
WHERE version=614"); // Old migration number

      // This is ok to add a same column twice, so there is no conflict with the old 614
      Database.AddColumn (TableName.SEQUENCE,
                          TableName.SEQUENCE + "toolnumber", DbType.String);
      MakeColumnCaseInsensitive (TableName.SEQUENCE, TableName.SEQUENCE + "toolnumber");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.SEQUENCE,
                            TableName.SEQUENCE + "toolnumber");
    }
  }
}

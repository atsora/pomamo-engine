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
  /// Migration 1300: Add a sequence detail column
  /// </summary>
  [Migration (1300)]
  public class AddSequenceDetail : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddSequenceDetail).FullName);

    static readonly string SEQUENCE_DETAIL = TableName.SEQUENCE + "detail";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.SEQUENCE,
        new Column (SEQUENCE_DETAIL, DbType.String));
      MakeColumnJson (TableName.SEQUENCE, SEQUENCE_DETAIL);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.SEQUENCE, SEQUENCE_DETAIL);
    }
  }
}

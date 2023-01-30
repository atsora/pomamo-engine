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
  /// Migration 083: Add row for Sequence in display table
  /// </summary>
  [Migration(83)]
  public class AddRowForSequenceInDisplayTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddRowForSequenceInDisplayTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Managed by DefaultValues now
    }

    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.DISPLAY)) {
        Database.Delete (TableName.DISPLAY,
                         "displaytable",
                         "Sequence");
      }
    }
  }
}

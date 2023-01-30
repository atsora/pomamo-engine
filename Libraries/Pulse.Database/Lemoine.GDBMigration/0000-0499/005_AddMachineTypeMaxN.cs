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
  /// Migration 005: Add the column maxn in table sfkmachtype
  /// </summary>
  [Migration(05)]
  public class AddMachineTypeMaxN: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineTypeMaxN).FullName);
    
    static readonly int DEFAULT_SIZE = 8;
    
    static readonly string TABLE_NAME = "sfkmachtype";
    static readonly string FILEEND_COLUMN = "fileend";
    static readonly string MAXN_COLUMN = "maxn";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.TableExists (TABLE_NAME)) {
        // Check the latest columns have already been set
        if (!Database.ColumnExists (TABLE_NAME,
                                    FILEEND_COLUMN)) {
          log.ErrorFormat ("Up: column {0} does not exist " +
                           "in table {1}, abort",
                           FILEEND_COLUMN,
                           TABLE_NAME);
          throw new Exception (String.Format ("Column {0} does not exist " +
                                              "in table {1}",
                                              FILEEND_COLUMN,
                                              TABLE_NAME));
        }

        if (!Database.ColumnExists (TABLE_NAME,
                                    MAXN_COLUMN)) {
          Database.AddColumn (TABLE_NAME,
                              MAXN_COLUMN, DbType.Int32,
                              DEFAULT_SIZE, ColumnProperty.NotNull, 0);
        }
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TABLE_NAME)) {
        Database.RemoveColumn (TABLE_NAME,
                             MAXN_COLUMN);
      }
    }
  }
}

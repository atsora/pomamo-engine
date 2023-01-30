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
  /// Migration 159: add a column nextbegin to the table sequenceslot
  /// </summary>
  [Migration(159)]
  public class AddSequenceSlotNextBegin: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddSequenceSlotNextBegin).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.SEQUENCE_SLOT,
                          TableName.SEQUENCE_SLOT + "nextbegin", DbType.DateTime, ColumnProperty.Null);
      Database.AddCheckConstraint (TableName.SEQUENCE_SLOT + "nextbeginafter",
                                   TableName.SEQUENCE_SLOT,
                                   string.Format (@"{0}nextbegin IS NULL OR ({0}end IS NOT NULL AND {0}end <= {0}nextbegin)",
                                                  TableName.SEQUENCE_SLOT));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveConstraint (TableName.SEQUENCE_SLOT,
                                 TableName.SEQUENCE_SLOT + "nextbeginafter");
      Database.RemoveColumn (TableName.SEQUENCE_SLOT,
                             TableName.SEQUENCE_SLOT + "nextbegin");
    }
  }
}

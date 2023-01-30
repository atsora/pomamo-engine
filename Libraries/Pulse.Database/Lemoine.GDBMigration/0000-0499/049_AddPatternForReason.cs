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
  /// Migration 049: Add a default display pattern for Reason and ReasonGroup
  /// </summary>
  [Migration(49)]
  public class AddPatternForReason: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddPatternForReason).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.Insert (TableName.DISPLAY,
                       new string [] {"displaytable", "displaypattern"},
                       new string [] {"ReasonGroup", "<%NameOrTranslation%>"});
      Database.Insert (TableName.DISPLAY,
                       new string [] {"displaytable", "displaypattern"},
                       new string [] {"Reason", "<%NameOrTranslation%>"});
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}

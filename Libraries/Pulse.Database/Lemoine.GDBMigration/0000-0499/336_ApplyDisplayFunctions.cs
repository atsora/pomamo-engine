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
  /// Migration 336:
  /// </summary>
  [Migration(336)]
  public class ApplyDisplayFunctions: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ApplyDisplayFunctions).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"SELECT displayfromnametranslationkeyupdate ();");
      Database.ExecuteNonQuery (@"SELECT displayupdate ();");
      
      Database.ExecuteNonQuery (@"
UPDATE display
SET displaypattern=regexp_replace (displaypattern, '<\^(\w+)\.Display\^>', '<%\1.Display%>', 'g');
");
      
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"
UPDATE display
SET displaypattern=regexp_replace (displaypattern, '<%(\w+)\.Display%>', '<^\1.Display^>', 'g');
");
    }
  }
}

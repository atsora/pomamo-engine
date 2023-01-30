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
  /// Migration 060: change the type of the configvalue column
  /// from bytea to xml
  /// </summary>
  [Migration(60)]
  public class ConfigXmlColumn: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ConfigXmlColumn).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"DELETE FROM config");
      Database.ExecuteNonQuery (@"
ALTER TABLE config
ALTER COLUMN configvalue 
SET DATA TYPE CHARACTER VARYING
USING '<migration />';");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DELETE FROM config");
      Database.ExecuteNonQuery (@"
ALTER TABLE config
ALTER COLUMN configvalue 
SET DATA TYPE bytea
USING ''::bytea;");
    }
  }
}

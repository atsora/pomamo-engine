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
  /// Migration 1205: remove the datastructureoption view 
  /// </summary>
  [Migration (1205)]
  public class RemoveDataStructureOption : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (RemoveDataStructureOption).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS datastructureoption");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW datastructureoption AS
SELECT right(config.configkey::text, - char_length('DataStructure.'::text))::citext AS datastructureoptionkey,
CASE WHEN config.configvalue::text = '<boolean>true</boolean>'::text
THEN true
ELSE false
END AS datastructureoptionvalue
FROM config
WHERE config.configkey::text LIKE 'DataStructure.%'::text;");
    }
  }
}

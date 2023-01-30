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
  /// Migration 161: Most sequences refer to id columns with type integer,
  /// set the max value to 2147483647
  /// </summary>
  [Migration(161)]
  public class LimitMaxValueInSequences: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (LimitMaxValueInSequences).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"create or replace function exec(text) returns void as $$
begin
 execute $1;
end;
$$ language plpgsql;");
      Database.ExecuteNonQuery (@"select exec ('alter sequence ' || sequence_name || ' maxvalue 2147483647')
from information_schema.sequences
where sequence_schema='public'
  and sequence_name not like 'sfk%';");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"select exec ('alter sequence ' || sequence_name || ' no maxvalue')
from information_schema.sequences
where sequence_schema='public'
  and sequence_name not like 'sfk%';");
      Database.ExecuteNonQuery (@"drop function if exists exec(text)");
    }
  }
}

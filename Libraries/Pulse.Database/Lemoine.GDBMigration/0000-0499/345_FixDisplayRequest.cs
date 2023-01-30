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
  /// Migration 345:
  /// </summary>
  [Migration(345)]
  public class FixDisplayRequest: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixDisplayRequest).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION displayrequest (tablename NAME, variant TEXT DEFAULT NULL) RETURNS text AS
$BODY$
  SELECT E'SELECT E\'SELECT \\\'' --'
    || regexp_replace (displaypattern, '<%([\w.]+)%>', E'\' || E\'\\\' || \' || displayextract(\'' || tablename || E'\',\'\\1\') || E\' || \\\'\' || \'', 'g')
    || E'\' || E\'\\\'::text\'' --'
  FROM display
  WHERE display.displaytable=tablename::citext
    AND variant===displayvariant;
$BODY$ LANGUAGE SQL STABLE;
");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}

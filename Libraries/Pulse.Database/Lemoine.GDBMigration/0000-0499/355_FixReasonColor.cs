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
  /// Migration 355:
  /// </summary>
  [Migration(355)]
  public class FixReasonColor: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixReasonColor).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Fix ("color");
      Fix ("reportcolor");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
    
    void Fix (string suffix)
    {
      Database.ExecuteNonQuery (string.Format (@"
CREATE OR REPLACE FUNCTION public.{0}{1}({0})
  RETURNS VARCHAR(7) AS
' SELECT CASE WHEN {0}.{0}custom{1} IS NOT NULL THEN {0}.{0}custom{1} ELSE {0}group.{0}group{1} END AS {0}{1} FROM public.{0} NATURAL JOIN public.{0}group WHERE {0}id=$1.{0}id'
  LANGUAGE sql IMMUTABLE
  COST 100;
",
                                               TableName.REASON,
                                               suffix));
    }
  }
}

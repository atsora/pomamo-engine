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
  /// Migration 817
  /// </summary>
  [Migration (817)]
  public class RestoreSfkcfgs : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (RestoreSfkcfgs).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists ("sfkcfgs")) { // View or table
        Database.ExecuteNonQuery (@"
CREATE TABLE public.sfkcfgs
(
    config character varying,
    sfksection character varying,
    skey character varying,
    sfkvalue character varying
)
      ");
      }
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}

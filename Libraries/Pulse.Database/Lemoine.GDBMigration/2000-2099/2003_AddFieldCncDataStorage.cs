// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 2003: storage of the cnc data of a field
  /// <item>fieldcncdatastorage: tables where the cnc data are stored (flags 1: cncvalue, 2: cncdataperiod),
  ///       null for the config Cnc.DefaultDataStorage</item>
  /// <item>fieldperiodmaxduration: maximum duration of a period in cncdataperiod, in seconds, null for no limit</item>
  /// </summary>
  [Migration (2003)]
  public class AddFieldCncDataStorage : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddFieldCncDataStorage).FullName);

    static readonly string CNC_DATA_STORAGE = TableName.FIELD + "cncdatastorage";
    static readonly string PERIOD_MAX_DURATION = TableName.FIELD + "periodmaxduration";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // No upper limit on the flags, for future storages
      Database.ExecuteNonQuery ($@"
ALTER TABLE public.{TableName.FIELD}
  ADD COLUMN IF NOT EXISTS {CNC_DATA_STORAGE} integer CONSTRAINT {CNC_DATA_STORAGE}_positive CHECK ({CNC_DATA_STORAGE} >= 0),
  ADD COLUMN IF NOT EXISTS {PERIOD_MAX_DURATION} double precision CONSTRAINT {PERIOD_MAX_DURATION}_strictlypositive CHECK ({PERIOD_MAX_DURATION} > 0)");
      Database.ExecuteNonQuery ($@"
COMMENT ON COLUMN public.{TableName.FIELD}.{CNC_DATA_STORAGE} IS
'Tables where the cnc data are stored (flags 1: cncvalue, 2: cncdataperiod). NULL: config Cnc.DefaultDataStorage'");
      Database.ExecuteNonQuery ($@"
COMMENT ON COLUMN public.{TableName.FIELD}.{PERIOD_MAX_DURATION} IS
'Maximum duration of a period in cncdataperiod in seconds. NULL: no limit (a period never crosses a week bound)'");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery ($@"
ALTER TABLE public.{TableName.FIELD}
  DROP COLUMN IF EXISTS {PERIOD_MAX_DURATION},
  DROP COLUMN IF EXISTS {CNC_DATA_STORAGE}");
    }
  }
}

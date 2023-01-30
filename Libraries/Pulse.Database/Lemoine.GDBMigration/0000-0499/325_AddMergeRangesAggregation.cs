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
  /// Migration 325:
  /// </summary>
  [Migration(325)]
  public class AddMergeRangesAggregation: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMergeRangesAggregation).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION merge_ranges_sfunc (acc tsrange[], a tsrange) RETURNS tsrange[] AS $$
DECLARE
  to_merge_into tsrange;
  merged tsrange ;
  newval tsrange[];
BEGIN
  IF isempty(a) THEN
    RETURN acc;
  END IF;
  -- First, try to see if the new value should be merged in an existing one.
  SELECT v INTO to_merge_into FROM unnest(acc) as v WHERE v && a OR v -|- a;
  -- If there isn't, just append the new value to the list
  IF to_merge_into IS NULL THEN
    RETURN acc || a;
  END IF;
  -- If there is one, merge it:
  merged := to_merge_into + a;
  SELECT array_agg(v) INTO newval FROM unnest(acc) as v WHERE NOT merged @> v;
  -- Recursively call ourself to simplify existing ranges
  RETURN merge_ranges_sfunc(newval, merged);
END
$$ language plpgsql;");
      Database.ExecuteNonQuery (@"
CREATE AGGREGATE merge_ranges (BASETYPE=tsrange, SFUNC=merge_ranges_sfunc, STYPE=tsrange[])");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DROP AGGREGATE public.merge_ranges(tsrange);");
      Database.ExecuteNonQuery (@"DROP FUNCTION public.merge_ranges_sfunc(tsrange[], tsrange);");
    }
  }
}

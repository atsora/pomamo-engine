// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 2006: parameters of the cnc data of a field (field.fieldcncdataparameters, a json object),
  /// for example for the aggregations, the type of the json values (JsonType) or the maximum gap (MaxGap).
  /// A new parameter does not need any new migration
  /// </summary>
  [Migration (2006)]
  public class AddFieldCncDataParameters : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddFieldCncDataParameters).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"
ALTER TABLE public.field ADD COLUMN IF NOT EXISTS fieldcncdataparameters jsonb
  CONSTRAINT field_cncdataparameters_object CHECK (jsonb_typeof (fieldcncdataparameters) = 'object')");
      Database.ExecuteNonQuery (@"
COMMENT ON COLUMN public.field.fieldcncdataparameters IS
'Parameters of the cnc data of the field (json object), for example JsonType (.NET type of the json values)
or MaxGap (maximum duration without any value before a new period, hh:mm:ss)'");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery ("ALTER TABLE public.field DROP COLUMN IF EXISTS fieldcncdataparameters");
    }
  }
}

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
  /// Migration 626: add an index on reason slots that are in processing state 
  /// </summary>
  [Migration (626)]
  public class AddProcessingReasonSlotsIndex : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddProcessingReasonSlotsIndex).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      { // Stop the migration if the reason id = 8 is not valid
        var badReasonId = (long)Database.ExecuteScalar (@"
SELECT count (*) FROM reason WHERE reasonid=8 and reasontranslationkey IS NULL;
");
        if (0 < badReasonId) {
          log.Fatal ($"Invalid reason with id=8 in database, please re-associate ids in table reason");
          throw new Exception ("Invalid reason with id=8 in database, please ask LAT to re-associate ids in table reason");
        }
      }

      AddNamedIndexCondition ("reasonslot_processing", TableName.REASON_SLOT,
        "reasonid=" + (int)Lemoine.Model.ReasonId.Processing,
        ColumnName.MACHINE_ID, "lower(reasonslotdatetimerange)");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveNamedIndex ("reasonslot_processing", TableName.REASON_SLOT);
    }
  }
}

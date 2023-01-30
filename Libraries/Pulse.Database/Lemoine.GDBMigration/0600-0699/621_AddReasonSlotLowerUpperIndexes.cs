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
  /// Migration 621: 
  /// </summary>
  [Migration (621)]
  public class AddReasonSlotLowerUpperIndexes : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddReasonSlotLowerUpperIndexes).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddNamedIndex ("reasonslot_lower", TableName.REASON_SLOT, ColumnName.MACHINE_ID, "lower(reasonslotdatetimerange)");
      AddNamedIndex ("reasonslot_upper", TableName.REASON_SLOT, ColumnName.MACHINE_ID, "upper(reasonslotdatetimerange)");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveNamedIndex ("reasonslot_lower", TableName.REASON_SLOT);
      RemoveNamedIndex ("reasonslot_upper", TableName.REASON_SLOT);
    }
  }
}

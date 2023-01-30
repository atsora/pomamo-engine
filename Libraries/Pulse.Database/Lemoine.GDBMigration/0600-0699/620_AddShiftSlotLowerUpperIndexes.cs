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
  /// Migration 620: 
  /// </summary>
  [Migration (620)]
  public class AddShiftSlotLowerUpperIndexes : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddShiftSlotLowerUpperIndexes).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddNamedIndex ("shiftslot_lower", TableName.SHIFT_SLOT, "lower(shiftslotrange)");
      AddNamedIndex ("shiftslot_upper", TableName.SHIFT_SLOT, "upper(shiftslotrange)");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveNamedIndex ("shiftslot_lower", TableName.SHIFT_SLOT);
      RemoveNamedIndex ("shiftslot_upper", TableName.SHIFT_SLOT);
    }
  }
}

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
  /// Migration 623: 
  /// </summary>
  [Migration (623)]
  public class AddOperationSlotLowerUpperIndexes : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddOperationSlotLowerUpperIndexes).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddNamedIndex ("operationslot_lower", TableName.OPERATION_SLOT, ColumnName.MACHINE_ID, "lower(operationslotdatetimerange)");
      AddNamedIndex ("operationslot_upper", TableName.OPERATION_SLOT, ColumnName.MACHINE_ID, "upper(operationslotdatetimerange)");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveNamedIndex ("operationslot_lower", TableName.OPERATION_SLOT);
      RemoveNamedIndex ("operationslot_upper", TableName.OPERATION_SLOT);
    }
  }
}

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
  /// Migration 1204: make deliverablepiececode nullable 
  /// </summary>
  [Migration (1204)]
  public class DeliverablePieceCodeNullable : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DeliverablePieceCodeNullable).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      DropNotNull (TableName.DELIVERABLE_PIECE, $"{TableName.DELIVERABLE_PIECE}code");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      SetNotNull (TableName.DELIVERABLE_PIECE, $"{TableName.DELIVERABLE_PIECE}code");
    }
  }
}

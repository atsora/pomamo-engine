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
  /// Migration 239: the column workorderlineid must be nullable for NHibernate in workorderlinequantity
  /// </summary>
  [Migration(239)]
  public class WorkOrderLineQuantityForeignKeyNotNull: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (WorkOrderLineQuantityForeignKeyNotNull).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"ALTER TABLE workorderlinequantity ALTER COLUMN workorderlineid DROP NOT NULL;");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"ALTER TABLE workorderlinequantity ALTER COLUMN workorderlineid SET NOT NULL;");
    }
  }
}

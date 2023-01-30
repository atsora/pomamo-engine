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
  /// Migration 535: add the columns for auto-reason in reason slot
  /// 
  /// without default values
  /// </summary>
  [Migration (535)]
  public class AutoReasonReasonSlotAddColumns : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonReasonSlotAddColumns).FullName);

    static readonly string REASON_SLOT_REASON_SCORE = "reasonslotreasonscore";
    static readonly string REASON_SLOT_REASON_SOURCE = "reasonslotreasonsource";
    static readonly string REASON_SLOT_AUTO_REASON_NUMBER = "reasonslotautoreasonnumber";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddReasonSlotReasonScore ();
      AddReasonSlotReasonSource ();
      AddReasonSlotAutoReasonNumber ();
    }

    void AddReasonSlotReasonScore ()
    {
      if (!Database.ColumnExists (TableName.REASON_SLOT, REASON_SLOT_REASON_SCORE)) {
        Database.AddColumn (TableName.REASON_SLOT, new Column (REASON_SLOT_REASON_SCORE, DbType.Double));
      }
    }

    void AddReasonSlotReasonSource ()
    {
      if (!Database.ColumnExists (TableName.REASON_SLOT, REASON_SLOT_REASON_SOURCE)) {
        Database.AddColumn (TableName.REASON_SLOT, new Column (REASON_SLOT_REASON_SOURCE, DbType.Int32));
      }
    }

    void AddReasonSlotAutoReasonNumber ()
    {
      if (!Database.ColumnExists (TableName.REASON_SLOT, REASON_SLOT_AUTO_REASON_NUMBER)) {
        Database.AddColumn (TableName.REASON_SLOT, new Column (REASON_SLOT_AUTO_REASON_NUMBER, DbType.Int32));
      }
    }


    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveReasonSlotReasonSource ();
      RemoveReasonSlotReasonScore ();
      RemoveReasonSlotAutoReasonNumber ();
    }

    void RemoveReasonSlotAutoReasonNumber ()
    {
      Database.RemoveColumn (TableName.REASON_SLOT, REASON_SLOT_AUTO_REASON_NUMBER);
    }

    void RemoveReasonSlotReasonScore ()
    {
      Database.RemoveColumn (TableName.REASON_SLOT, REASON_SLOT_REASON_SCORE);
    }

    void RemoveReasonSlotReasonSource ()
    {
      Database.RemoveColumn (TableName.REASON_SLOT, REASON_SLOT_REASON_SOURCE);
    }
  }
}

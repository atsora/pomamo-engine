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
  /// Migration 537: set the new columns for auto-reason as not null in reason slot
  /// 
  /// without default values
  /// </summary>
  [Migration (537)]
  public class AutoReasonReasonSlotNotNull : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonReasonSlotNotNull).FullName);

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
      SetNotNull (TableName.REASON_SLOT, REASON_SLOT_REASON_SCORE);
    }

    void AddReasonSlotReasonSource ()
    {
      SetNotNull (TableName.REASON_SLOT, REASON_SLOT_REASON_SOURCE);
    }

    void AddReasonSlotAutoReasonNumber ()
    {
      SetNotNull (TableName.REASON_SLOT, REASON_SLOT_AUTO_REASON_NUMBER);
    }


    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}

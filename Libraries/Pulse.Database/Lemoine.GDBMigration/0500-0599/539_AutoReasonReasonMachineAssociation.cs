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
  /// Migration 539: add the columns for auto-reason in table reasonmachineassociation
  /// </summary>
  [Migration (539)]
  public class AutoReasonReasonMachineAssociation : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonReasonMachineAssociation).FullName);

    static readonly string REASON_MACHINE_ASSOCIATION_REASON_SCORE = "reasonmachineassociationreasonscore";
    static readonly string REASON_MACHINE_ASSOCIATION_KIND = "reasonmachineassociationkind";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.ColumnExists (TableName.REASON_MACHINE_ASSOCIATION, REASON_MACHINE_ASSOCIATION_REASON_SCORE)) {
        AddReasonMachineAssociationReasonScore ();
      }
      if (!Database.ColumnExists (TableName.REASON_MACHINE_ASSOCIATION, REASON_MACHINE_ASSOCIATION_KIND)) {
        AddReasonMachineAssociationKind ();
      }
    }

    void AddReasonMachineAssociationReasonScore ()
    {
      Database.AddColumn (TableName.REASON_MACHINE_ASSOCIATION, new Column (REASON_MACHINE_ASSOCIATION_REASON_SCORE, DbType.Double));
      Database.ExecuteNonQuery (@"
UPDATE reasonmachineassociation
SET reasonmachineassociationreasonscore=100
WHERE reasonid IS NOT NULL");
      Database.AddCheckConstraint ("reasonmachineassociation_reasonscore", TableName.REASON_MACHINE_ASSOCIATION,
        @"reasonmachineassociationreasonscore IS NOT NULL OR reasonid IS NULL");
    }

    void AddReasonMachineAssociationKind ()
    {
      Database.AddColumn (TableName.REASON_MACHINE_ASSOCIATION, new Column (REASON_MACHINE_ASSOCIATION_KIND, DbType.Int32));
      Database.ExecuteNonQuery (@"
UPDATE reasonmachineassociation
SET reasonmachineassociationkind=4");
      SetNotNull (TableName.REASON_MACHINE_ASSOCIATION, REASON_MACHINE_ASSOCIATION_KIND);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveReasonMachineAssociationKind ();
      RemoveReasonMachineAssociationReasonScore ();
    }

    void RemoveReasonMachineAssociationKind ()
    {
      Database.RemoveColumn (TableName.REASON_MACHINE_ASSOCIATION, REASON_MACHINE_ASSOCIATION_KIND);
    }

    void RemoveReasonMachineAssociationReasonScore ()
    {
      Database.RemoveColumn (TableName.REASON_MACHINE_ASSOCIATION, REASON_MACHINE_ASSOCIATION_REASON_SCORE);
    }
  }
}

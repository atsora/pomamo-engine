// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Migrator.Framework;
using Lemoine.Core.Log;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 544: set the option 128 for manual reason associations without an end
  /// </summary>
  [Migration (544)]
  public class CompleteManualReasonMachineAssociation : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CompleteManualReasonMachineAssociation).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"
UPDATE reasonmachineassociation
SET reasonmachineassociationoption=128
WHERE reasonmachineassociationend is null
and reasonmachineassociationkind=4
and reasonmachineassociationoption is null
");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}

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
  /// Migration 352:
  /// </summary>
  [Migration(352)]
  public class AddReasonbor: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddReasonbor).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW reasonbor AS
SELECT reasonid, reasonname, reasontranslationkey, reasoncode AS code, reasondescription AS description,
  reasondescriptiontranslationkey AS descriptiontranslationkey, reason.reasoncolor AS color,
  reasonlinkoperationdirection, reasongroupid, reasonversion
FROM reason;
");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"
DROP VIEW IF EXISTS reasonbor;
");
    }
  }
}

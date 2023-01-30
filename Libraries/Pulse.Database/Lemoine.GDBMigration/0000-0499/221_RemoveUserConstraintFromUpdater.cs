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
  /// Migration 221: Remove User constraint from Updater table.
  /// When deleting a User, NHibernate was removing auto. the linked parent (updater)
  /// but PgSQL was removing the child too because of a CASCADE on Updater.
  /// Causing a blocking transaction.  
  /// </summary>
  [Migration(221)]
  public class RemUserConstraintFromUpdater: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemUserConstraintFromUpdater).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.RemoveConstraint(TableName.UPDATER, "fk_updater_usertable");
      Database.ExecuteNonQuery(@"ALTER TABLE updater
      ADD CONSTRAINT fk_updater_usertable FOREIGN KEY (userid)
      REFERENCES usertable (userid) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE SET NULL;");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveConstraint(TableName.UPDATER, "fk_updater_usertable");
      Database.ExecuteNonQuery(@"ALTER TABLE updater
      ADD CONSTRAINT fk_updater_usertable FOREIGN KEY (userid)
      REFERENCES usertable (userid) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE;");
    }
  }
}

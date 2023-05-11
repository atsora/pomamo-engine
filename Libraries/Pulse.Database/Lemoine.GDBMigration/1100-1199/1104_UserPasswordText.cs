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
  /// Migration 1104: userpassword to text type
  /// </summary>
  [Migration (1104)]
  public class UserPasswordText : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (UserPasswordText).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      MakeColumnText (TableName.USER, "userpassword");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}

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
  /// Migration 1106: make user password nullable 
  /// </summary>
  [Migration (1106)]
  public class UserPasswordNullable : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (UserPasswordNullable).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      DropNotNull (TableName.USER, "userpassword");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      SetNotNull (TableName.USER, "userpassword");
    }
  }
}

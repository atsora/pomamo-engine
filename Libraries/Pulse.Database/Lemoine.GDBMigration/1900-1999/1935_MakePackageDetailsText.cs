// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;

using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1935: convert package.packagedetails, the description of a package, from varchar(255) to text,
  /// so that a package with a description longer than 255 characters can be installed
  /// </summary>
  [Migration (1935)]
  public class MakePackageDetailsText : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MakePackageDetailsText).FullName);

    static readonly string PACKAGE_DETAILS = TableName.PACKAGE + "details";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      MakeColumnText (TableName.PACKAGE, PACKAGE_DETAILS);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Nothing to do: a text column is compatible with the previous versions,
      // and a conversion back to varchar(255) would fail with the longer descriptions
    }
  }
}

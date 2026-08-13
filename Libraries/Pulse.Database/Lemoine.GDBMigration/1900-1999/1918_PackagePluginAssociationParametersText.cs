// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration to set the type of the parameters column of the packagepluginassociation table to text
  ///
  /// The plugin parameters may be long, for example when a json configuration is stored in them,
  /// and the 255 characters of a varchar column may not be sufficient
  /// </summary>
  [Migration (1918)]
  public class PackagePluginAssociationParametersText : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PackagePluginAssociationParametersText).FullName);

    static readonly string PARAMETERS = $"{TableName.PACKAGE_PLUGIN_ASSOCIATION}parameters";

    /// <summary>
    /// Update the database
    /// </summary>
    public override void Up ()
    {
      if (log.IsInfoEnabled) {
        log.Info ($"Up: set the type of {PARAMETERS} to text");
      }
      // Note: on PostgreSQL, converting a varchar column into text does not require any table rewrite,
      // and running it on a column that is already text is a no-op
      Database.ExecuteNonQuery ($"""
ALTER TABLE {TableName.PACKAGE_PLUGIN_ASSOCIATION}
ALTER COLUMN {PARAMETERS}
SET DATA TYPE TEXT;
""");
    }

    /// <summary>
    /// Downgrade the database
    ///
    /// Nothing to do here: restoring a limited length would truncate the existing parameters
    /// </summary>
    public override void Down ()
    {
    }
  }
}

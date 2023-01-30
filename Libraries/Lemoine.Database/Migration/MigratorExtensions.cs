// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Database.Migration
{
  /// <summary>
  /// Extensions to <see cref="Migrator"/>
  /// </summary>
  public static class MigratorExtensions
  {
    /// <summary>
    /// Get the last migration number
    /// </summary>
    /// <param name="migrator"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static long GetLastMigrationNumber (this Migrator.Migrator migrator, ILog log)
    {
      var migrationNumbers = migrator.MigrationsTypes
        .Select (m => Migrator.MigrationLoader.GetMigrationVersion (m));
      if (migrationNumbers.Any ()) {
        return migrationNumbers.Max ();
      }
      else {
        log.Error ($"GetLastMigratioNumber: no migration type was registered");
        return -1;
      }
    }

    /// <summary>
    /// Get the latest applied migration
    /// </summary>
    /// <param name="migrator"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static long GetLatestAppliedMigration (this Migrator.Migrator migrator, ILog log)
    {
      var appliedMigrationNumbers = migrator.AppliedMigrations;
      if (appliedMigrationNumbers.Any ()) {
        return appliedMigrationNumbers.Max ();
      }
      else {
        log.Error ($"GetLatestAppliedMigration: no applied migration");
        return -1;
      }
    }
  }
}

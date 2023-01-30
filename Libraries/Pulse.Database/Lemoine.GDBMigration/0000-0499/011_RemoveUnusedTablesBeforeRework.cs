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
  /// Migration 011: remove the following unused tables before the database rework:
  /// <item>sfkprojtype (Job type)</item>
  /// <item>sfkcost (Customer)</item>
  /// <item>sfkcategory (Category)</item>
  /// <item>sfkmate (Material)</item>
  /// <item>sfkstock</item>
  /// </summary>
  [Migration(11)]
  public class RemoveUnusedTablesBeforeRework: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemoveUnusedTablesBeforeRework).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.TableExists ("sfkprojtype")) {
        Database.RemoveTable ("sfkprojtype");
      }
      if (Database.TableExists ("sfkcost")) {
        Database.RemoveTable ("sfkcost");
      }
      if (Database.TableExists ("sfkcategory")) {
        Database.RemoveTable ("sfkcategory");
      }
      if (Database.TableExists ("sfkmate")) {
        Database.RemoveTable ("sfkmate");
      }
      if (Database.TableExists ("sfkstock")) {
        Database.RemoveTable ("sfkstock");
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // No rollback is possible
    }
  }
}

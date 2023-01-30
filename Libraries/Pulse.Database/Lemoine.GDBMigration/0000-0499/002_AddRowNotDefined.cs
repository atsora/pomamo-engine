// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.I18N;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 002: add in some external data tables
  ///                a row for ID=0 (for Undefined)
  /// This eases the code and
  /// allows in the future the use of foreign keys
  /// </summary>
  [Migration(2)]
  public class AddRowNotDefined: Migration
  {
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Note: ProjectType, ComponentType, Strategy, OperationType
      //       already have a row <Undefined>
      
      string program = PulseCatalog.GetString ("Program");
      string undefined = PulseCatalog.GetString ("UndefinedExternalData");
      // ProcessType
      if (Database.TableExists (TableName.SFK_PROCESS_TYPE)) {
        Database.Delete (TableName.SFK_PROCESS_TYPE,
                         "id", "0");
        Database.ExecuteNonQuery (String.Format ("INSERT INTO sfkprocesstype (id, name) " +
                                                 "VALUES (0, '{0}');",
                                                 PulseCatalog.GetString ("UndefinedExternalData")));
      }
      // Tool
      if (Database.TableExists ("sfktools")) {
        Database.ExecuteNonQuery (String.Format ("INSERT INTO sfktools (toolid, toolcode, toolname, tooldia, toolrad, toolmate)" +
                                                 "VALUES (0, '{0}', '{0}', 0, 0, 0)",
                                                 PulseCatalog.GetString ("UndefinedExternalData")));
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // ProcessType
      if (Database.TableExists (TableName.SFK_PROCESS_TYPE)) {
        Database.ExecuteNonQuery ("DELETE FROM sfkprocesstype WHERE id=0;");
      }
      // Tool
      if (Database.TableExists ("sfktools")) {
        Database.ExecuteNonQuery ("DELETE FROM sfktools WHERE toolid=0;");
      }
    }
  }
}

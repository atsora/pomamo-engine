// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Migrator.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 510: Fix the taskexternalcode column
  /// </summary>
  [Migration (510)]
  public class FixTaskExternalCode: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FixTaskExternalCode).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.TableExists (TableName.MANUFACTURING_ORDER_IMPLEMENTATION)) {
        Database.ExecuteNonQuery ($"DROP VIEW IF EXISTS {TableName.MANUFACTURING_ORDER} CASCADE");
        MakeColumnCaseInsensitive (TableName.MANUFACTURING_ORDER_IMPLEMENTATION, $"{TableName.MANUFACTURING_ORDER}externalcode");
        Database.ExecuteNonQuery ($"""
          INSERT INTO display(displaytable, displaypattern)
          SELECT '{TableName.MANUFACTURING_ORDER_IMPLEMENTATION}', '<%WorkOrder.Display%>/<%Component.Display%>/<%Operation.Display%> on <%Machine.Display%>'
          WHERE NOT EXISTS (SELECT 1 FROM display WHERE displaytable='{TableName.MANUFACTURING_ORDER_IMPLEMENTATION}' AND displayvariant IS NULL)
          """);
        Database.ExecuteNonQuery ($"""
          CREATE OR REPLACE VIEW {TableName.MANUFACTURING_ORDER} AS
          SELECT *, {TableName.MANUFACTURING_ORDER_IMPLEMENTATION}.{TableName.MANUFACTURING_ORDER_IMPLEMENTATION}display AS {TableName.MANUFACTURING_ORDER}display
          FROM {TableName.MANUFACTURING_ORDER_IMPLEMENTATION}
          ;
          """);
      }

      // Keep the next lines until all customers were upgraded to version >= 19.0.0
      // And after version >= 19.0.0 was installed at new customers
      else if (Database.TableExists (TableName.TASK_FULL)) {
        Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS task CASCADE");
        MakeColumnCaseInsensitive (TableName.TASK_FULL, "taskexternalcode");
        Database.ExecuteNonQuery (@"
INSERT INTO display(displaytable, displaypattern)
SELECT 'TaskFull', '<%WorkOrder.Display%>/<%Component.Display%>/<%Operation.Display%> on <%Machine.Display%>'
WHERE NOT EXISTS (SELECT 1 FROM display WHERE displaytable='TaskFull' AND displayvariant IS NULL)
");
        Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW task AS
SELECT *, taskfull.taskfulldisplay AS taskdisplay
FROM taskfull");
      }
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}

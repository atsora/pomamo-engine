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
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS task CASCADE");
      MakeColumnCaseInsensitive (TableName.TASK_FULL, "taskexternalcode");
      AddTaskView ();
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }

    void AddTaskView ()
    {
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
}

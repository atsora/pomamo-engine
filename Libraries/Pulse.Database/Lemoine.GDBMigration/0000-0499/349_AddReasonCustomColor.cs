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
  /// Migration 349:
  /// </summary>
  [Migration(349)]
  public class AddReasonCustomColor: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddReasonCustomColor).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Up ("color");
      Up ("reportcolor");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Down ("color");
      Down ("reportcolor");
    }

    void Up (string suffix)
    {
      // Rename the column color to customcolor
      RemoveConstraintColor (TableName.REASON, TableName.REASON + suffix);
      Database.RenameColumn (TableName.REASON,
                             TableName.REASON + suffix,
                             TableName.REASON + "custom" + suffix);
      Database.ExecuteNonQuery (string.Format (@"
ALTER TABLE {0}
ALTER COLUMN {0}custom{1} DROP NOT NULL;
",
                                               TableName.REASON,
                                               suffix));
      Database.ExecuteNonQuery (string.Format (@"
ALTER TABLE {0}
ALTER COLUMN {0}custom{1} DROP DEFAULT;
",
                                               TableName.REASON,
                                               suffix));
      AddConstraintColor (TableName.REASON, TableName.REASON + "custom" + suffix);
      
      // Reset to null the renamed column if the color is the same as in reasongroup
      Database.ExecuteNonQuery (string.Format (@"
UPDATE {0}
SET {0}custom{1}=NULL
FROM {0}group
WHERE {0}.{0}groupid={0}group.{0}groupid
  AND {0}custom{1}={0}group.{0}group{1}
",
                                               TableName.REASON,
                                               suffix));
      
      // Create the dynamic column reasoncolor with the column value from the reason group or from the reason
      Database.ExecuteNonQuery (string.Format (@"
CREATE OR REPLACE FUNCTION public.{0}{1}({0})
  RETURNS VARCHAR(7) AS
' SELECT CASE WHEN {0}.{0}custom{1} IS NOT NULL THEN {0}.{0}custom{1} ELSE {0}group.{0}group{1} END AS {0}{1} FROM public.{0} NATURAL JOIN public.{0}group WHERE {0}id=$1.{0}id'
  LANGUAGE sql IMMUTABLE
  COST 100;
",
                                               TableName.REASON,
                                               suffix));
    }
    
    void Down (string suffix)
    {
      // Remove the virtual column
      Database.ExecuteNonQuery (string.Format (@"
DROP FUNCTION public.{0}{1}({0});
",
                                               TableName.REASON,
                                               suffix));
      
      // Rename the column color to customcolor
      RemoveConstraintColor (TableName.REASON, TableName.REASON + "custom" + suffix);
      Database.RenameColumn (TableName.REASON,
                             TableName.REASON + "custom" + suffix,
                             TableName.REASON + suffix);
      Database.ExecuteNonQuery (string.Format (@"
UPDATE {0}
SET {0}{1}={0}group.{0}group{1}
FROM {0}group
WHERE {0}.{0}groupid={0}group.{0}groupid
  AND {0}{1} IS NULL;
",
                                               TableName.REASON,
                                               suffix));
      Database.ExecuteNonQuery (string.Format (@"
ALTER TABLE {0}
ALTER COLUMN {0}{1} SET NOT NULL;
",
                                               TableName.REASON,
                                               suffix));
      Database.ExecuteNonQuery (string.Format (@"
ALTER TABLE {0}
ALTER COLUMN {0}{1} SET DEFAULT '#FFFF00';
",
                                               TableName.REASON,
                                               suffix));
      AddConstraintColor (TableName.REASON, TableName.REASON + suffix);
    }
  }
}

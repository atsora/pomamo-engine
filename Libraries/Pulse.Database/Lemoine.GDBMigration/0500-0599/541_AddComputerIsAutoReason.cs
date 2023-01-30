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
  /// Migration 541: add new server properties in the computer table
  /// </summary>
  [Migration (541)]
  public class AddComputerIsAutoReason : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddComputerIsAutoReason).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddIsXxx ("AutoReason");
      AddIsXxx ("Alert");
      AddIsXxx ("Synchronization");

      SetLctrIsXxx ("Alert");
      SetLctrIsXxx ("Synchronization");
    }

    void AddIsXxx (string xxx) {
      Database.AddColumn (TableName.COMPUTER,
        new Column (string.Format ("ComputerIs{0}", xxx), DbType.Boolean, ColumnProperty.NotNull, false));
      Database.ExecuteNonQuery (string.Format (@"
CREATE OR REPLACE FUNCTION computer_check_unique_{0}() 
RETURNS trigger AS 
$BODY$
BEGIN 
IF (NEW.ComputerIs{0}=TRUE) AND EXISTS (SELECT 1 FROM computer WHERE ComputerIs{0}=TRUE) THEN
  IF (TG_OP='INSERT')
  THEN RAISE EXCEPTION 'unique {0} violation';
  ELSIF (OLD.ComputerIs{0}=FALSE)
  THEN RAISE EXCEPTION 'unique {0} violation';
  END IF;
END IF;
RETURN NEW;
END;
$BODY$
LANGUAGE plpgsql VOLATILE 
COST 100;
", xxx));
      Database.ExecuteNonQuery (string.Format (@"
CREATE TRIGGER computer_unique_{0} 
 BEFORE UPDATE OR INSERT
 ON computer 
 FOR EACH ROW 
 EXECUTE PROCEDURE computer_check_unique_{0}();
", xxx));
      Database.ExecuteNonQuery (string.Format (@"
CREATE UNIQUE INDEX computer_computeris{0}_idx 
ON computer (computeris{0}) 
WHERE computeris{0}=TRUE;
", xxx));
    }

    void SetLctrIsXxx (string xxx) {
      Database.ExecuteNonQuery (string.Format (@"
UPDATE computer
SET computeris{0}=TRUE
WHERE computerislctr=TRUE
", xxx));
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIsXxx ("Synchronization");
      RemoveIsXxx ("Alert");
      RemoveIsXxx ("AutoReason");
    }

    void RemoveIsXxx (string xxx)
    {
      Database.ExecuteNonQuery (string.Format (@"
DROP TRIGGER computer_unique_{0} ON public.computer;
", xxx));
      Database.ExecuteNonQuery (string.Format (@"
DROP FUNCTION public.computer_check_unique_{0}();
", xxx));
      Database.RemoveColumn (TableName.COMPUTER, TableName.COMPUTER + "is" + xxx);
    }
  }
}

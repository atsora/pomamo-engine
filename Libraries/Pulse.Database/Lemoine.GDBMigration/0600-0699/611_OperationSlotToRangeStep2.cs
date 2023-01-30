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
  /// Migration 611:
  /// </summary>
  [Migration(611)]
  public class OperationSlotToRangeStep2: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (OperationSlotToRangeStep2).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {

      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}datetimerange=tsrange({0}begindatetime,{0}enddatetime)",
                                               TableName.OPERATION_SLOT));
      
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}dayrange=daterange({0}beginday::date,{0}endday::date,'[]')",
                                               TableName.OPERATION_SLOT));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}begindatetime=lower({0}datetimerange)
WHERE NOT lower_inf({0}datetimerange)",
                                               TableName.OPERATION_SLOT));
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}enddatetime=upper({0}datetimerange)
WHERE NOT upper_inf({0}datetimerange)",
                                               TableName.OPERATION_SLOT));
      
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}beginday=lower({0}dayrange)
WHERE NOT lower_inf({0}dayrange)",
                                               TableName.OPERATION_SLOT));
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}endday=upper({0}dayrange)
WHERE NOT upper_inf({0}dayrange)",
                                               TableName.OPERATION_SLOT));
    }
  }
}

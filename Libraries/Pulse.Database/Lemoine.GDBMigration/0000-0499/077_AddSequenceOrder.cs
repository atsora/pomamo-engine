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
  /// Migration 077: create order and estimatedtime columns in sequence table
  /// </summary>
  [Migration(77)]
  public class SequenceOrder: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (SequenceOrder).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery ("ALTER TABLE sequence " +
                                "ADD COLUMN sequenceorder integer;");

      Database.ExecuteNonQuery ("ALTER TABLE sequence " +
                                "ADD COLUMN sequenceestimatedtime double precision;");

      Database.ExecuteNonQuery(
        "UPDATE sequence " +
        "SET sequenceorder = " + 
        "(SELECT seqorder FROM " +
        "(SELECT sequenceid AS seqid, (sequenceid - rrr.minId) AS seqorder " +
        "FROM sequence AS uuu, " +
        "(SELECT operationid, MIN(sequenceid) as minId FROM sequence GROUP BY operationid) rrr " +
        "WHERE uuu.operationid = rrr.operationid AND uuu.sequenceid = sequenceid) vvv " +
        "WHERE vvv.seqid = sequenceid);"
       );
      
      Database.ExecuteNonQuery(
        "ALTER TABLE sequence " +
        "ALTER COLUMN sequenceorder SET NOT NULL;"
       );
      
      Database.ExecuteNonQuery(
        "ALTER TABLE sequence " +
        "ADD CONSTRAINT sequence_operationid_sequenceorder_key UNIQUE (operationid, sequenceorder);"
       );
      
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery ("ALTER TABLE sequence " +
                                "DROP CONSTRAINT sequence_operationid_sequenceorder_key");
      
      Database.ExecuteNonQuery ("ALTER TABLE sequence " +
                                "DROP COLUMN sequenceorder;");

      Database.ExecuteNonQuery ("ALTER TABLE sequence " +
                                "DROP COLUMN sequenceestimatedtime;");
    }
  }
}

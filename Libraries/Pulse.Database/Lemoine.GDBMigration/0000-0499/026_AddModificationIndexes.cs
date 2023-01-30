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
  /// Migration 026: add some indexes on the following modification tables
  /// <item>activitydetection</item>
  /// <item>operationcycleinformation</item>
  /// </summary>
  [Migration(26)]
  public class AddModificationIndexes: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddModificationIndexes).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.TableExists (TableName.ACTIVITY_DETECTION)) {
        Database.ExecuteNonQuery (@"CREATE INDEX  
activitydetection_machinemoduleid_idx
ON activitydetection 
USING btree (machinemoduleid)");
      }
      
      if (Database.TableExists (TableName.OLD_SEQUENCE_DETECTION)) {
        Database.ExecuteNonQuery (@"CREATE INDEX  
processdetection_machinemoduleid_idx
ON processdetection 
USING btree (machinemoduleid)");
      }
      
      if (Database.TableExists (TableName.OPERATION_CYCLE_INFORMATION)) {
        Database.ExecuteNonQuery (@"CREATE INDEX  
operationcycleinformation_machineid_idx 
ON operationcycleinformation 
USING btree (machineid)");
        Database.ExecuteNonQuery (@"CREATE INDEX  
operationcycleinformation_machineid_operationid_idx 
ON operationcycleinformation 
USING btree (machineid, operationid)");
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DROP INDEX IF EXISTS 
operationcycleinformation_machineid_operationid_idx");
      Database.ExecuteNonQuery (@"DROP INDEX IF EXISTS 
operationcycleinformation_machineid_idx");

      Database.ExecuteNonQuery (@"DROP INDEX IF EXISTS 
processdetection_machinemoduleid_idx");      
      
      Database.ExecuteNonQuery (@"DROP INDEX IF EXISTS 
activitydetection_machinemoduleid_idx");      
    }
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 521: Fanuc alarm have multiple cncinfo (Fanuc - 30, Fanuc - 15i, 20, ...)
  /// Now this is always "Fanuc" and the SubCncInfo contains the version (or the machine alarm file path)
  /// </summary>
  [Migration(521)]
  public class FanucAlarmUpdate: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FanucAlarmUpdate).FullName);
    static readonly string [] cncKinds = {"0", "0i", "15", "15i", "16", "16i", "18", "18i", "21", "21i", "30", "30i",
      "31", "31i", "32", "32i", "35", "35i", "PD", "PDi", "PH", "PHi", "PM", "PMi"};
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Foreach possible version of a Fanuc CNC
      string in1 = "'" + string.Join("', '", cncKinds) + "'";
      string in2 = "'Fanuc - " + string.Join("', 'Fanuc - ", cncKinds) + "'";
      
      // Update for operator messages and cnc alarms
      Database.ExecuteNonQuery(
        "UPDATE cncalarm " +
        "SET cncalarmcncsubinfo=cncalarmcncinfo, cncalarmcncinfo='Fanuc' " +
        "WHERE cncalarmcncinfo IN (" + in1 + ")");
      
      Database.ExecuteNonQuery(
        "UPDATE cncalarm " +
        "SET cncalarmcncsubinfo=SUBSTRING(cncalarmcncinfo, 9, 8000), cncalarmcncinfo='Fanuc' " +
        "WHERE cncalarmcncinfo IN (" + in2 + ")");
      
      // Update machine alarms (cannot use the index "cncinfo-type" here)
      Database.ExecuteNonQuery("UPDATE cncalarm " +
                               "SET cncalarmcncsubinfo=cncalarmcncinfo, cncalarmcncinfo='Fanuc' " +
                               "WHERE cncalarmtype='machine alarm' AND (cncalarmcncsubinfo = '') IS NOT FALSE");
      
      // Fix types
      Database.ExecuteNonQuery("UPDATE cncalarm " +
                               "SET cncalarmtype='Operator message' " +
                               "WHERE cncalarmcncinfo='Fanuc' AND cncalarmtype='operator message'");
      Database.ExecuteNonQuery("UPDATE cncalarm " +
                               "SET cncalarmtype='PMC error' " +
                               "WHERE cncalarmcncinfo='Fanuc' AND cncalarmtype='PCM error'");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down() {}
  }
}

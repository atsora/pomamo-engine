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
  /// Migration 111: add the new following columns to the modificationstatus to keep a track of the analysis performance
  /// <item>Begin of the first analysis</item>
  /// <item>End of the last analysis</item>
  /// <item>Number of analysis iterations</item>
  /// <item>Total duration of the analysis</item>
  /// <item>Duration of the last analysis</item>
  /// </summary>
  [Migration(111)]
  public class AddModificationAnalysisProperties: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddModificationAnalysisProperties).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MODIFICATION_STATUS,
                          new Column ("analysisbegin", DbType.DateTime));
      Database.AddColumn (TableName.MODIFICATION_STATUS,
                          new Column ("analysisend", DbType.DateTime));
      Database.AddColumn (TableName.MODIFICATION_STATUS,
                          new Column ("analysisiterations", DbType.Int32));
      Database.AddColumn (TableName.MODIFICATION_STATUS,
                          new Column ("analysistotalduration", DbType.Double));
      Database.AddColumn (TableName.MODIFICATION_STATUS,
                          new Column ("analysislastduration", DbType.Double));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MODIFICATION_STATUS,
                            "analysisbegin");
      Database.RemoveColumn (TableName.MODIFICATION_STATUS,
                            "analysisend");
      Database.RemoveColumn (TableName.MODIFICATION_STATUS,
                            "analysisiterations");
      Database.RemoveColumn (TableName.MODIFICATION_STATUS,
                            "analysistotalduration");
      Database.RemoveColumn (TableName.MODIFICATION_STATUS,
                            "analysislastduration");
    }
  }
}

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
  /// Migration 322: add a displayvariant column in table display
  /// </summary>
  [Migration(322)]
  public class DisplayVariant: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DisplayVariant).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      try {
        RemoveUniqueConstraint (TableName.DISPLAY, TableName.DISPLAY + "table");
      } catch (Exception e) {
        log.WarnFormat("Migration 322: couldn't remove the old unique constraint: {0}", e);
      }
      
      Database.AddColumn (TableName.DISPLAY,
                          new Column (TableName.DISPLAY + "variant", DbType.String, ColumnProperty.Null));
      this.MakeColumnCaseInsensitive (TableName.DISPLAY, TableName.DISPLAY + "variant");
      Database.AddColumn (TableName.DISPLAY,
                          new Column (TableName.DISPLAY + "description", DbType.String, ColumnProperty.Null));
      AddUniqueConstraint (TableName.DISPLAY, TableName.DISPLAY + "table", TableName.DISPLAY + "variant");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveUniqueConstraint (TableName.DISPLAY, TableName.DISPLAY + "table", TableName.DISPLAY + "variant");
      Database.RemoveColumn (TableName.DISPLAY,
                             TableName.DISPLAY + "description");
      Database.RemoveColumn (TableName.DISPLAY,
                             TableName.DISPLAY + "variant");
      
      // Triggers a bug
//      try {
//        AddUniqueConstraint (TableName.DISPLAY, TableName.DISPLAY + "table");
//      } catch (Exception e) {
//        log.WarnFormat("Migration 322: couldn't add the old unique constraint");
//      }
    }
  }
}

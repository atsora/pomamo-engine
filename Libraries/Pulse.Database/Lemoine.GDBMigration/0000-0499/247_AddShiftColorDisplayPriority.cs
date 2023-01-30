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
  /// Migration 247:
  /// </summary>
  [Migration(247)]
  public class AddShiftColorDisplayPriority: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddShiftColorDisplayPriority).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddColor ();
      AddDisplayPriority ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveDisplayPriority ();
      RemoveColor ();
    }
    
    void AddColor ()
    {
      Database.AddColumn (TableName.SHIFT,
                          new Column (TableName.SHIFT + "color", DbType.String, 7, ColumnProperty.NotNull, "'#808080'"));
      AddConstraintColor (TableName.SHIFT, TableName.SHIFT + "color");
    }
    
    void RemoveColor ()
    {
      Database.RemoveColumn (TableName.SHIFT, TableName.SHIFT + "color");
    }
    
    void AddDisplayPriority ()
    {
      Database.AddColumn (TableName.SHIFT,
                          new Column (TableName.SHIFT + "displaypriority", DbType.Int32));
      AddIndex (TableName.SHIFT,
                TableName.SHIFT + "displaypriority");
    }
    
    void RemoveDisplayPriority ()
    {
      Database.RemoveColumn (TableName.SHIFT, TableName.SHIFT + "displaypriority");
    }
  }
}

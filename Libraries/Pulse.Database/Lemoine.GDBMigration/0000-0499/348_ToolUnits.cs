// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.SharedData;
using Lemoine.Model;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 348: remove the columns toollifetypeid and eventtoollifelifetype
  /// put all information in unitid
  /// add the column geometryunitid in tool position
  /// </summary>
  [Migration(348)]
  public class ToolUnits: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ToolUnits).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Complete the units
      AddUnit(UnitId.Unknown, "Unknown", "UnitUnknown");
      AddUnit(UnitId.ToolNumberOfTimes, "Number of times", "UnitToolNumberOfTimes");
      AddUnit(UnitId.ToolWear, "ToolWear", "UnitToolWear");
      
      // Update the column unitid
      UpdateToolLifeUnit(ToolUnit.Unknown, UnitId.Unknown);
      UpdateToolLifeUnit(ToolUnit.TimeSeconds, UnitId.DurationSeconds);
      UpdateToolLifeUnit(ToolUnit.Parts, UnitId.NumberOfParts);
      UpdateToolLifeUnit(ToolUnit.NumberOfTimes, UnitId.ToolNumberOfTimes);
      UpdateToolLifeUnit(ToolUnit.Wear, UnitId.ToolWear);
      UpdateToolLifeUnit(ToolUnit.DistanceMillimeters, UnitId.DistanceMillimeter);
      UpdateToolLifeUnit(ToolUnit.DistanceInch, UnitId.DistanceInch);
      
      // Delete the columns "toollifetypeid" and "eventtoollifelifetype"
      Database.RemoveColumn(TableName.TOOL_LIFE, TableName.TOOL_LIFE + "typeid");
      Database.RemoveColumn(TableName.EVENT_TOOL_LIFE, TableName.EVENT_TOOL_LIFE + "lifetype");
      
      // Add the column geometryunitid
      Database.AddColumn(TableName.TOOL_POSITION, TableName.TOOL_POSITION + "geometryunitid", DbType.Int32);
      Database.GenerateForeignKey(TableName.TOOL_POSITION, TableName.TOOL_POSITION + "geometryunitid",
                                  TableName.UNIT, ColumnName.UNIT_ID,
                                  Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Restore the column "toollifetypeid" and "eventtoollifelifetype", which can now be null
      Database.AddColumn(TableName.TOOL_LIFE, TableName.TOOL_LIFE + "typeid", DbType.Int32);
      Database.AddColumn(TableName.EVENT_TOOL_LIFE, TableName.EVENT_TOOL_LIFE + "lifetype", DbType.Int32);
      
      // Update the column "toollifetypeid"
      UpdateToolLifeType(ToolUnit.Unknown, UnitId.Unknown);
      UpdateToolLifeType(ToolUnit.TimeSeconds, UnitId.DurationSeconds);
      UpdateToolLifeType(ToolUnit.Parts, UnitId.NumberOfParts);
      UpdateToolLifeType(ToolUnit.NumberOfTimes, UnitId.ToolNumberOfTimes);
      UpdateToolLifeType(ToolUnit.Wear, UnitId.ToolWear);
      UpdateToolLifeType(ToolUnit.DistanceMillimeters, UnitId.DistanceMillimeter);
      UpdateToolLifeType(ToolUnit.DistanceInch, UnitId.DistanceInch);
      
      // Remove the column geometryunitid
      Database.RemoveColumn(TableName.TOOL_POSITION, TableName.TOOL_POSITION + "geometryunitid");
    }
    
    void UpdateToolLifeUnit(ToolUnit lifeType, UnitId unitId)
    {
      Database.ExecuteNonQuery(string.Format(
        "UPDATE toollife SET unitid={0} WHERE toollifetypeid={1};",
        (int)unitId, (int)lifeType));
      Database.ExecuteNonQuery(string.Format(
        "UPDATE eventtoollife SET unitid={0} WHERE eventtoollifelifetype={1};",
        (int)unitId, (int)lifeType));
    }
    
    void UpdateToolLifeType(ToolUnit lifeType, UnitId unitId)
    {
      Database.ExecuteNonQuery(string.Format(
        "UPDATE toollife SET toollifetypeid={0} WHERE unitid={1};",
        (int)lifeType, (int)unitId));
      Database.ExecuteNonQuery(string.Format(
        "UPDATE eventtoollife SET eventtoollifelifetype={0} WHERE unitid={1};",
        (int)lifeType, (int)unitId));
    }
    
    void AddUnit(UnitId unitId, string translationKey, string description)
    {
      Database.ExecuteNonQuery(String.Format(
        "INSERT INTO {0} ({1}, {0}translationkey, {0}description) " +
        "SELECT {2}, '{3}', '{4}' " +
        "WHERE NOT EXISTS (SELECT {1} FROM {0} WHERE {1}={2})",
        TableName.UNIT, ColumnName.UNIT_ID, (int)unitId, translationKey, description));
    }
  }
}

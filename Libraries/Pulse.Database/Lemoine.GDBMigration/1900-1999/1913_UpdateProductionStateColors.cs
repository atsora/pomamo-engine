// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Model;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration to update the colors of specific production states
  /// </summary>
  [Migration(1913)]
  public class UpdateProductionStateColors: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (UpdateProductionStateColors).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    public override void Up ()
    {
      // Update production state id=1 to #008000 (Green) if translation key is TableName.PRODUCTION_STATEProduction
      Database.ExecuteNonQuery ($"""
UPDATE {TableName.PRODUCTION_STATE}         
SET {TableName.PRODUCTION_STATE}color='#008000' 
WHERE { TableName.PRODUCTION_STATE}id={(int)ProductionStateId.Production}
  AND {TableName.PRODUCTION_STATE}translationkey='ProductionStateProduction';
""");
      
      // Update production state id=2 to #FFFF00 (Yellow) if translation key is ProductionStateNoProduction
      Database.ExecuteNonQuery ($"""
UPDATE {TableName.PRODUCTION_STATE}       
SET {TableName.PRODUCTION_STATE}color='#FFFF00' 
WHERE { TableName.PRODUCTION_STATE}id ={(int)ProductionStateId.NoProduction} 
  AND {TableName.PRODUCTION_STATE}translationkey='ProductionStateNoProduction';
""");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    public override void Down ()
    {
      // No downgrade: we don't restore old colors since we don't know what they were
      log.Info ("Down: no downgrade action for color updates");
    }
  }
}
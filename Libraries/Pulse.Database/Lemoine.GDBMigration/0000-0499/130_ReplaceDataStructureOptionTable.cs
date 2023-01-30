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
  /// Migration 130: replace DataStructureOption table by a view
  /// </summary>
  [Migration(130)]
  public class ReplaceDataStructureOptionTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ReplaceDataStructureOptionTable).FullName);
    
    static readonly string DATA_STRUCTURE_OPTION_TABLE = "DataStructureOption";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.TableExists (DATA_STRUCTURE_OPTION_TABLE)) {
        // update already existing datastructure options in configuration
        Database.ExecuteNonQuery (
          @"CREATE OR REPLACE FUNCTION update_config_datastructureoptions() RETURNS void AS $$
DECLARE configrecord RECORD;
DECLARE datastructurerecord RECORD;
DECLARE translationrecord RECORD;
DECLARE key citext;
DECLARE translation citext;
BEGIN
  FOR datastructurerecord in
      (SELECT * FROM datastructureoption)
  LOOP
      SELECT * INTO translationrecord FROM translation WHERE translationkey = datastructurerecord.datastructureoptionkey ;

      UPDATE config SET configvalue = '<boolean>' || (CASE WHEN datastructurerecord.datastructureoptionvalue THEN 'true' ELSE 'false' END) || '</boolean>',
                        configdescription = translationrecord.translationvalue,
                        configtimestamp = current_timestamp at time zone 'utc'
      WHERE config.configkey = 'DataStructure.' || datastructurerecord.datastructureoptionkey;
       
  END LOOP;
END;

$$ LANGUAGE plpgsql;

SELECT update_config_datastructureoptions();");

        // other datastructure options are copied into config table
        Database.ExecuteNonQuery (@"INSERT INTO config (configkey, configvalue, configdescription, configtimestamp)
SELECT 'DataStructure.' || datastructureoptionkey,
       '<boolean>' || (CASE WHEN datastructureoptionvalue THEN 'true' ELSE 'false' END) || '</boolean>',
       translationvalue,
       current_timestamp at time zone 'utc'
FROM datastructureoption left outer join translation on (datastructureoption.datastructureoptionkey = translation.translationkey)
WHERE 'DataStructure.' || datastructureoptionkey NOT IN (SELECT configkey from config);");

        // get rid of datastructureoption table
        Database.ExecuteNonQuery (@"DROP TABLE datastructureoption;");
      }
      
      // replace it by view based on config table
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW datastructureoption AS
SELECT right(config.configkey::text, - char_length('DataStructure.'::text))::citext AS datastructureoptionkey,
CASE WHEN config.configvalue::text = '<boolean>true</boolean>'::text
THEN true
ELSE false
END AS datastructureoptionvalue
FROM config
WHERE config.configkey::text LIKE 'DataStructure.%'::text;");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // drop view
      Database.ExecuteNonQuery(String.Format(@"DROP VIEW IF EXISTS {0};", DATA_STRUCTURE_OPTION_TABLE));
    }
  }
}

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
  /// Migration 042: add a version columns in most tables
  /// </summary>
  [Migration(42)]
  public class AddVersionInTables: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddVersionInTables).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddVersionColumns ();
      
      UpgradeBooleanViews ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      DowngradeBooleanViews ();
    }
    
    void AddVersionColumns ()
    {
      foreach (string tableName in new string [] {TableName.CAD_MODEL,
                 TableName.COMPONENT, TableName.COMPONENT_TYPE, TableName.COMPUTER,
                 TableName.FIELD, TableName.INTERMEDIATE_WORK_PIECE, TableName.ISO_FILE,
                 TableName.MACHINE, TableName.MACHINE_MODE, TableName.MACHINE_MONITORING_TYPE,
                 TableName.MACHINE_MODE_DEFAULT_REASON, TableName.MACHINE_MODULE,
                 TableName.MACHINE_OBSERVATION_STATE, TableName.OPERATION,
                 TableName.OPERATION_TYPE, TableName.OLD_SEQUENCE, TableName.PROJECT, TableName.REASON,
                 TableName.REASON_GROUP, TableName.REASON_SELECTION, TableName.STAMP, TableName.TOOL,
                 TableName.UNIT, TableName.UPDATER, TableName.WORK_ORDER, TableName.WORK_ORDER_STATUS}) {
        Database.AddColumn (tableName,
                            new Column (string.Format ("{0}version", tableName),
                                        DbType.Int32, ColumnProperty.NotNull, 1));
      }
    }
    
    void UpgradeBooleanViews ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS reasonselectionboolean CASCADE");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW reasonselectionboolean AS
 SELECT reasonselection.reasonselectionid, reasonselection.machinemodeid, reasonselection.machineobservationstateid, reasonselection.reasonid,
        CASE
            WHEN reasonselection.reasonselectionselectable = false THEN 0
            ELSE 1
        END AS reasonselectionselectable,
        CASE
            WHEN reasonselection.reasonselectiondetailsrequired = true THEN 1
            ELSE 0
        END AS reasonselectiondetailsrequired,
        reasonselection.reasonselectionversion
   FROM reasonselection;");
      
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS stampboolean CASCADE");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW stampboolean AS 
 SELECT stamp.stampid, stamp.isofileid, stamp.stampposition, stamp.processid, stamp.operationid, stamp.componentid, 
        CASE
            WHEN stamp.operationcyclebegin THEN 1
            ELSE 0
        END AS operationcyclebegin, 
        CASE
            WHEN stamp.operationcycleend THEN 1
            ELSE 0
        END AS operationcycleend, 
        CASE
            WHEN stamp.stampisofileend THEN 1
            ELSE 0
        END AS stampisofileend,
        stamp.stampversion
   FROM stamp;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE stampboolean_insert AS
    ON INSERT TO stampboolean DO INSTEAD  INSERT INTO stamp (isofileid, stampposition, processid, operationid, componentid, operationcyclebegin, operationcycleend, stampisofileend) 
  VALUES (new.isofileid, new.stampposition, new.processid, new.operationid, new.componentid, new.operationcyclebegin = 1, new.operationcycleend = 1, new.stampisofileend = 1)
  RETURNING stamp.stampid, stamp.isofileid, stamp.stampposition, stamp.processid, stamp.operationid, stamp.componentid, 
        CASE
            WHEN stamp.operationcyclebegin THEN 1
            ELSE 0
        END AS operationcyclebegin, 
        CASE
            WHEN stamp.operationcycleend THEN 1
            ELSE 0
        END AS operationcycleend, 
        CASE
            WHEN stamp.stampisofileend THEN 1
            ELSE 0
        END AS stampisofileend,
        stamp.stampversion;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE stampboolean_update AS
    ON UPDATE TO stampboolean DO INSTEAD  UPDATE stamp SET stampposition = new.stampposition, processid = new.processid, operationid = new.operationid, componentid = new.componentid, operationcyclebegin = new.operationcyclebegin = 1, operationcycleend = new.operationcycleend = 1, stampisofileend = new.stampisofileend = 1, stampversion = new.stampversion
  WHERE stamp.stampid = old.stampid;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE stampboolean_delete AS
    ON DELETE TO stampboolean DO INSTEAD  DELETE FROM stamp
  WHERE stamp.stampid = old.stampid;");
    }
    
    void DowngradeBooleanViews ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS reasonselectionboolean CASCADE");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW reasonselectionboolean AS
 SELECT reasonselection.reasonselectionid, reasonselection.machinemodeid, reasonselection.machineobservationstateid, reasonselection.reasonid,
        CASE
            WHEN reasonselection.reasonselectionselectable = false THEN 0
            ELSE 1
        END AS reasonselectionselectable,
        CASE
            WHEN reasonselection.reasonselectiondetailsrequired = true THEN 1
            ELSE 0
        END AS reasonselectiondetailsrequired
   FROM reasonselection;");
      
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS stampboolean CASCADE");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW stampboolean AS 
 SELECT stamp.stampid, stamp.isofileid, stamp.stampposition, stamp.processid, stamp.operationid, stamp.componentid, 
        CASE
            WHEN stamp.operationcyclebegin THEN 1
            ELSE 0
        END AS operationcyclebegin, 
        CASE
            WHEN stamp.operationcycleend THEN 1
            ELSE 0
        END AS operationcycleend, 
        CASE
            WHEN stamp.stampisofileend THEN 1
            ELSE 0
        END AS stampisofileend
   FROM stamp;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE stampboolean_insert AS
    ON INSERT TO stampboolean DO INSTEAD  INSERT INTO stamp (isofileid, stampposition, processid, operationid, componentid, operationcyclebegin, operationcycleend, stampisofileend) 
  VALUES (new.isofileid, new.stampposition, new.processid, new.operationid, new.componentid, new.operationcyclebegin = 1, new.operationcycleend = 1, new.stampisofileend = 1)
  RETURNING stamp.stampid, stamp.isofileid, stamp.stampposition, stamp.processid, stamp.operationid, stamp.componentid, 
        CASE
            WHEN stamp.operationcyclebegin THEN 1
            ELSE 0
        END AS operationcyclebegin, 
        CASE
            WHEN stamp.operationcycleend THEN 1
            ELSE 0
        END AS operationcycleend, 
        CASE
            WHEN stamp.stampisofileend THEN 1
            ELSE 0
        END AS stampisofileend;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE stampboolean_update AS
    ON UPDATE TO stampboolean DO INSTEAD  UPDATE stamp SET stampposition = new.stampposition, processid = new.processid, operationid = new.operationid, componentid = new.componentid, operationcyclebegin = new.operationcyclebegin = 1, operationcycleend = new.operationcycleend = 1, stampisofileend = new.stampisofileend = 1
  WHERE stamp.stampid = old.stampid;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE stampboolean_delete AS
    ON DELETE TO stampboolean DO INSTEAD  DELETE FROM stamp
  WHERE stamp.stampid = old.stampid;");
    }    
  }
}

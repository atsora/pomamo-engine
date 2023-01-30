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
  /// Migration 087:
  /// </summary>
  [Migration(87)]
  public class RepairViews: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RepairViews).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      #region sfkprocesstype view and associated rules
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS sfkprocesstype");
      Database.ExecuteNonQuery ("CREATE OR REPLACE VIEW sfkprocesstype AS " +
                                "SELECT operationtypeid-1 AS id, " +
                                "CASE WHEN operationtypename IS NULL " +
                                " THEN translationvalue " +
                                " ELSE operationtypename::varchar END AS name " +
                                "FROM operationtype " +
                                "LEFT OUTER JOIN translation " +
                                "ON (operationtypetranslationkey=translationkey AND locale='');");
      Database.ExecuteNonQuery ("CREATE RULE sfkprocesstype_insert AS " +
                                "ON INSERT TO sfkprocesstype " +
                                "DO INSTEAD " +
                                "INSERT INTO operationtype " +
                                "(operationtypename) " +
                                "VALUES (NEW.name) " +
                                "RETURNING operationtype.operationtypeid-1 AS id, " +
                                "operationtype.operationtypename::varchar AS name;");
      Database.ExecuteNonQuery ("CREATE RULE sfkprocesstype_update AS " +
                                "ON UPDATE TO sfkprocesstype " +
                                "DO INSTEAD " +
                                "UPDATE operationtype " +
                                "SET operationtypename=NEW.name " +
                                "WHERE operationtypeid = OLD.id+1;");
      Database.ExecuteNonQuery ("CREATE RULE sfkprocesstype_delete AS " +
                                "ON DELETE TO sfkprocesstype " +
                                "DO INSTEAD " +
                                "DELETE FROM operationtype " +
                                "WHERE operationtypeid = OLD.id+1;");
      #endregion // sfkprocesstype view and associated rules
      
      #region sfkcomptype view and associated rules
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS sfkcomptype");
      Database.ExecuteNonQuery ("CREATE OR REPLACE VIEW sfkcomptype AS " +
                                "SELECT componenttypeid-1 AS comptypeid, " +
                                "CASE WHEN componenttypename IS NULL " +
                                " THEN translationvalue " +
                                " ELSE componenttypename::varchar END AS comptypename " +
                                "FROM componenttype " +
                                "LEFT OUTER JOIN translation " +
                                "ON (componenttypetranslationkey=translationkey AND locale='');");
      Database.ExecuteNonQuery ("CREATE RULE sfkcomptype_insert AS " +
                                "ON INSERT TO sfkcomptype " +
                                "DO INSTEAD " +
                                "INSERT INTO componenttype " +
                                "(componenttypename) " +
                                "VALUES (NEW.comptypename) " +
                                "RETURNING componenttypeid-1 AS comptypeid, " +
                                "componenttypename::varchar AS comptypename;");
      Database.ExecuteNonQuery ("CREATE RULE sfkcomptype_update AS " +
                                "ON UPDATE TO sfkcomptype " +
                                "DO INSTEAD " +
                                "UPDATE componenttype " +
                                "SET componenttypename=NEW.comptypename " +
                                "WHERE componenttypeid = OLD.comptypeid+1;");
      Database.ExecuteNonQuery ("CREATE RULE sfkcomptype_delete AS " +
                                "ON DELETE TO sfkcomptype " +
                                "DO INSTEAD " +
                                "DELETE FROM componenttype " +
                                "WHERE componenttypeid = OLD.comptypeid+1;");
      #endregion // sfkcomptype view and associated rules

      #region sfkyreas view and associated rules
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS sfkyreas");
      Database.ExecuteNonQuery (@"CREATE VIEW sfkyreas AS
SELECT reasonid-20 AS rid,
  CASE
    WHEN reasonname IS NULL THEN tname.translationvalue
    ELSE reasonname::varchar
  END AS reason,
  CASE
    WHEN reasoncode IS NULL THEN ''::varchar
    ELSE reasoncode::varchar
  END AS code,
  CASE
    WHEN reasondescription IS NULL AND tdesc.translationvalue IS NULL THEN ''
    WHEN reasondescription IS NULL THEN tdesc.translationvalue
    ELSE reasondescription
  END AS descr,
  reasongroupid-20 AS rgrpid,
  reasonlinkoperationdirection AS linkwith
FROM reason
LEFT JOIN translation tname ON reasontranslationkey = tname.translationkey
                               AND tname.locale = ''
LEFT JOIN translation tdesc ON reasondescriptiontranslationkey = tdesc.translationkey
                               AND tdesc.locale= ''
WHERE reasonid >= 15");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE sfkyreas_delete AS
ON DELETE TO sfkyreas DO INSTEAD
DELETE FROM reason
WHERE reason.reasonid = (old.rid + 20)");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE sfkyreas_insert AS
ON INSERT TO sfkyreas DO INSTEAD
INSERT INTO reason (reasonname, reasoncode, reasondescription, reasongroupid, reasonlinkoperationdirection)
VALUES (new.reason::citext,
 CASE WHEN new.code='' THEN NULL ELSE new.code::citext END,
 CASE WHEN new.descr='' THEN NULL ELSE new.descr END,
 new.rgrpid+20, new.linkwith)
RETURNING reason.reasonid - 20 AS rid,
 reason.reasonname::varchar AS reason,
 CASE
  WHEN reason.reasoncode IS NULL THEN ''::varchar
  ELSE reason.reasoncode::varchar
 END AS code,
 CASE
  WHEN reason.reasondescription IS NULL THEN ''::varchar
  ELSE reason.reasondescription
 END AS descr,
 reason.reasongroupid - 20 AS rgrpid,
 reason.reasonlinkoperationdirection AS linkwith");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE sfkyreas_update AS
ON UPDATE TO sfkyreas DO INSTEAD
UPDATE reason
 SET reasonname = new.reason,
     reasoncode = CASE WHEN new.code='' THEN NULL ELSE new.code END,
     reasondescription = CASE WHEN new.descr='' THEN NULL ELSE new.descr END,
     reasongroupid = new.rgrpid+20,
     reasonlinkoperationdirection = new.linkwith
WHERE reason.reasonid = (old.rid + 20)");

      #endregion //  sfkyreas view and associated rules
      
      #region skfyrgrp view and associated rules
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS sfkyrgrp");
      Database.ExecuteNonQuery (@"CREATE VIEW sfkyrgrp AS
SELECT reasongroupid-20 AS rgrpid,
  CASE
    WHEN reasongroupname IS NULL THEN tname.translationvalue
    ELSE reasongroupname
  END AS grpname,
  CASE
    WHEN reasongroupdescription IS NULL AND tdesc.translationvalue IS NULL THEN ''
    WHEN reasongroupdescription IS NULL THEN tdesc.translationvalue
    ELSE reasongroupdescription
  END AS descr
FROM reasongroup
LEFT JOIN translation tname ON reasongrouptranslationkey = tname.translationkey
                               AND tname.locale = ''
LEFT JOIN translation tdesc ON reasongroupdescriptiontranslationkey = tdesc.translationkey
                               AND tdesc.locale= ''");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE sfkyrgrp_delete AS
ON DELETE TO sfkyrgrp DO INSTEAD
DELETE FROM reasongroup
WHERE reasongroup.reasongroupid = (old.rgrpid + 20)");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE sfkyrgrp_insert AS
ON INSERT TO sfkyrgrp DO INSTEAD
INSERT INTO reasongroup (reasongroupname, reasongroupdescription, reasongroupcolor)
VALUES (new.grpname::citext,
 CASE WHEN new.descr = '' THEN NULL ELSE new.descr END,
 '#FFFF00')
RETURNING reasongroup.reasongroupid - 20 AS rgrpid,
 reasongroup.reasongroupname::varchar AS rgrpname,
 CASE
  WHEN reasongroup.reasongroupdescription IS NULL THEN ''
  ELSE reasongroup.reasongroupdescription
 END AS descr");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE sfkyrgrp_update AS
ON UPDATE TO sfkyrgrp DO INSTEAD
UPDATE reasongroup
SET reasongroupname = new.grpname,
 reasongroupdescription =
  CASE WHEN new.descr = '' THEN NULL ELSE new.descr END
WHERE reasongroup.reasongroupid = (old.rgrpid + 20)");
      #endregion
      
      
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // none performed (view are built or rebuilt in Up(), and are supposed to exist at
      // previous migration anyway)
    }
  }
}

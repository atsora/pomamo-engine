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
  /// Migration 146: fix the sfkprocess view after some columns update in operation table
  /// </summary>
  [Migration(146)]
  public class FixSfkProcessView: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixSfkProcessView).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION sfkprocess_inserter()
  RETURNS trigger AS
$BODY$
DECLARE
varintermediateworkpieceid int8;
BEGIN
  INSERT INTO operation (operationname, operationtypeid,
    operationmachiningduration)
  VALUES (NEW.name, NEW.processtypeid+1, NEW.hours*3600.0)
  RETURNING operationid AS id INTO STRICT NEW.id;
  INSERT INTO intermediateworkpiece (operationid)
  VALUES (NEW.id)
  RETURNING intermediateworkpieceid
  INTO STRICT varintermediateworkpieceid;
  IF NEW.componentid<>0 AND NEW.componentid IS NOT NULL THEN
    INSERT INTO componentintermediateworkpiece (componentid, intermediateworkpieceid)
    VALUES (NEW.componentid, varintermediateworkpieceid);
  END IF;
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION sfkprocess_updater()
  RETURNS trigger AS
$BODY$
BEGIN
  UPDATE operation
  SET operationname=NEW.name, operationtypeid=NEW.processtypeid+1,
    operationmachiningduration=NEW.hours*3600.0
  WHERE operationid = OLD.id;
  UPDATE componentintermediateworkpiece
  SET componentid=NEW.componentid
  WHERE OLD.componentid<>0 AND OLD.componentid IS NOT NULL
    AND componentid=OLD.componentid AND intermediateworkpieceid= (SELECT intermediateworkpieceid FROM intermediateworkpiece WHERE operationid=OLD.id);
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION sfkprocess_inserter()
  RETURNS trigger AS
$BODY$
DECLARE
varintermediateworkpieceid int8;
BEGIN
  INSERT INTO operation (operationname, operationtypeid,
    operationestimatedmachininghours)
  VALUES (NEW.name, NEW.processtypeid+1, NEW.hours)
  RETURNING operationid AS id INTO STRICT NEW.id;
  INSERT INTO intermediateworkpiece (operationid)
  VALUES (NEW.id)
  RETURNING intermediateworkpieceid
  INTO STRICT varintermediateworkpieceid;
  IF NEW.componentid<>0 AND NEW.componentid IS NOT NULL THEN
    INSERT INTO componentintermediateworkpiece (componentid, intermediateworkpieceid)
    VALUES (NEW.componentid, varintermediateworkpieceid);
  END IF;
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION sfkprocess_updater()
  RETURNS trigger AS
$BODY$
BEGIN
  UPDATE operation
  SET operationname=NEW.name, operationtypeid=NEW.processtypeid+1,
    operationestimatedmachininghours=NEW.hours
  WHERE operationid = OLD.id;
  UPDATE componentintermediateworkpiece
  SET componentid=NEW.componentid
  WHERE OLD.componentid<>0 AND OLD.componentid IS NOT NULL
    AND componentid=OLD.componentid AND intermediateworkpieceid= (SELECT intermediateworkpieceid FROM intermediateworkpiece WHERE operationid=OLD.id);
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;
");
    }
  }
}

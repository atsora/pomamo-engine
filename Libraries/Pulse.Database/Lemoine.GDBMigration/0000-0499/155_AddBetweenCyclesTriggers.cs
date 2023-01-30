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
  /// Migration 155: Add a few triggers that are associated to the betweencycles table
  /// </summary>
  [Migration(155)]
  public class AddBetweenCyclesTriggers: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddBetweenCyclesTriggers).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // This is too complex to debug triggers. Give up !
      
      /*
      Database.ExecuteNonQuery (@"BEGIN;

CREATE OR REPLACE FUNCTION betweencycles_offsetduration (machineidarg INTEGER, betweencyclesbegin TIMESTAMP WITHOUT TIME ZONE, betweencyclesend TIMESTAMP WITHOUT TIME ZONE, previouscycleid INTEGER, nextcycleid INTEGER)
  RETURNS double precision AS
$BODY$
DECLARE
  monitoredmachinevar monitoredmachine%ROWTYPE;
  previousoperation operation%ROWTYPE;
  nextoperation operation%ROWTYPE;
  standardduration double precision;
BEGIN
  RAISE LOG 'betweencycles_offsetduration previouscycleid=% nextcycleid=%', previouscycleid, nextcycleid;
  standardduration := 0;
  IF previouscycleid IS NOT NULL AND nextcycleid IS NOT NULL THEN
    SELECT * INTO monitoredmachinevar FROM monitoredmachine WHERE machineid=machineidarg;
    IF monitoredmachinevar.palletchangingduration IS NOT NULL THEN
      -- pallet changing duration
      standardduration := monitoredmachinevar.palletchangingduration;
    ELSE
      -- previous cycle
      SELECT operation.* INTO previousoperation FROM operationcycle NATURAL JOIN operationslot NATURAL JOIN operation
        WHERE operationcycleid=previouscycleid;
      IF FOUND AND previousoperation.operationunloadingduration IS NOT NULL THEN
        RAISE LOG 'unloadingduration for % is %', previousoperation, previousoperation.operationunloadingduration;
        standardduration := previousoperation.operationunloadingduration;
      ELSE
        RAISE LOG 'operation for previous cycle % not found', previouscycleid;
      END IF;
      -- next cycle
      SELECT operation.* INTO nextoperation FROM operationcycle NATURAL JOIN operationslot NATURAL JOIN operation
        WHERE operationcycleid=nextcycleid;
      IF FOUND AND nextoperation.operationloadingduration IS NOT NULL THEN
        RAISE LOG 'unloadingduration for % is %', nextoperation, nextoperation.operationloadingduration;
        standardduration := standardduration + nextoperation.operationloadingduration;
      ELSE
        RAISE LOG 'operation for next cycle % not found', nextcycleid;
      END IF;
    END IF;
    IF 0 < standardduration THEN
      RAISE LOG 'betweencycles_offsetduration standardduration=% duration=%', standardduration, betweencyclesend-betweencyclesbegin;
      RETURN EXTRACT (SECOND FROM betweencyclesend - betweencyclesbegin) * 100.0 / standardduration;
    END IF;
  END IF;
  RETURN NULL;
END;
$BODY$
  LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION betweencycles_inserter ()
  RETURNS trigger AS
$BODY$
BEGIN
  NEW.betweencyclesoffsetduration := betweencycles_offsetduration (NEW.machineid, NEW.betweencyclesbegin, NEW.betweencyclesend, NEW.previouscycleid, NEW.nextcycleid);
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION betweencycles_cycle_updater ()
  RETURNS trigger AS
$BODY$
BEGIN
  IF NEW.previouscycleid<>OLD.previouscycleid OR NEW.nextcycleid<>OLD.nextcycleid THEN
    NEW.betweencyclesoffsetduration := betweencycles_offsetduration (NEW.machineid, NEW.betweencyclesbegin, NEW.betweencyclesend, NEW.previouscycleid, NEW.nextcycleid);
  END IF;
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION operationslot_operation_updater ()
  RETURNS trigger AS
$BODY$
DECLARE
  monitoredmachinevar monitoredmachine%ROWTYPE;
  betweencyclesvar betweencycles%ROWTYPE;
BEGIN
  SELECT * INTO monitoredmachinevar FROM monitoredmachine WHERE machineid=NEW.machineid;
  IF monitoredmachinevar.palletchangingduration IS NULL THEN
    FOR betweencyclesvar IN SELECT betweencycles.* FROM betweencycles
      INNER JOIN operationcycle previouscycle ON (previouscycleid=previouscycle.operationcycleid)
      INNER JOIN operationcycle nextcycle ON (nextcycleid=nextcycle.operationcycleid)
      WHERE nextcycle.operationslotid=NEW.operationslotid OR previouscycle.operationslotid=NEW.operationslotid
      LOOP
      UPDATE betweencycles SET betweencyclesoffsetduration=betweencycles_offsetduration (machineid, betweencyclesbegin, betweencyclesend, previouscycleid, nextcycleid)
      WHERE betweencyclesid=betweencyclesvar.betweencyclesid;
    END LOOP;
  END IF;
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION operationcycle_operationslot_updater ()
  RETURNS trigger AS
$BODY$
DECLARE
  monitoredmachinevar monitoredmachine%ROWTYPE;
BEGIN
  SELECT * INTO monitoredmachinevar FROM monitoredmachine WHERE machineid=NEW.machineid;
  IF monitoredmachinevar.palletchangingduration IS NULL THEN
    -- previous
    UPDATE betweencycles SET betweencyclesoffsetduration=betweencycles_offsetduration (machineid, betweencyclesbegin, betweencyclesend, previouscycleid, nextcycleid)
    WHERE nextcycleid=NEW.operationcycleid;
    -- next
    UPDATE betweencycles SET betweencyclesoffsetduration=betweencycles_offsetduration (machineid, betweencyclesbegin, betweencyclesend, previouscycleid, nextcycleid)
    WHERE previouscycleid=NEW.operationcycleid;
  END IF;
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql;


CREATE TRIGGER betweencycles_insert
BEFORE INSERT ON betweencycles
FOR EACH ROW
EXECUTE PROCEDURE betweencycles_inserter();

CREATE TRIGGER betweencycles_cycle_update
BEFORE UPDATE ON betweencycles
FOR EACH ROW
WHEN (OLD.previouscycleid<>NEW.previouscycleid OR OLD.nextcycleid<>NEW.nextcycleid)
EXECUTE PROCEDURE betweencycles_cycle_updater();

CREATE TRIGGER operationslot_operation_update
AFTER UPDATE ON operationslot
FOR EACH ROW
WHEN (OLD.operationid IS DISTINCT FROM NEW.operationid)
EXECUTE PROCEDURE operationslot_operation_updater();

CREATE TRIGGER operationcycle_operationslot_update
AFTER UPDATE ON operationcycle
FOR EACH ROW
WHEN (OLD.operationslotid IS DISTINCT FROM NEW.operationslotid)
EXECUTE PROCEDURE operationcycle_operationslot_updater();

COMMIT;");*/
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"BEGIN;
      
DROP TRIGGER IF EXISTS betweencycles_insert ON betweencycles CASCADE;
DROP TRIGGER IF EXISTS betweencycles_cycle_update ON betweencycles CASCADE;

DROP TRIGGER IF EXISTS operationslot_operation_update ON operationslot CASCADE;

DROP TRIGGER IF EXISTS operationcycle_operationslot_update ON operationcycle CASCADE;

DROP FUNCTION IF EXISTS betweencycles_offsetduration(integer, integer) CASCADE;
DROP FUNCTION IF EXISTS betweencycles_offsetduration(integer, integer, integer) CASCADE;
DROP FUNCTION IF EXISTS betweencycles_offsetduration(integer, timestamp without time zone, timestamp without time zone, integer, integer) CASCADE;

COMMIT;");
    }
  }
}

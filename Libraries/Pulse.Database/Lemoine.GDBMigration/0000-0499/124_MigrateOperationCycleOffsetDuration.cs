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
  /// Migration 124: update operationcyclestatus (which may also add estimated values in
  /// operationcyclebegin or operationcycleend).
  /// Then recompute operationcycleoffsetduration (if operation estimated machining time
  /// available).
  /// </summary>
  [Migration(124)]
  public class MigrateOperationCycleOffsetDuration: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MigrateOperationCycleOffsetDuration).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery(
        @"CREATE OR REPLACE FUNCTION migrate_operationcycleestimated() RETURNS void AS $$
DECLARE operationcyclerecord RECORD;
DECLARE hasEstimatedBegin BOOLEAN;
DECLARE hasEstimatedEnd BOOLEAN;
DECLARE partialSetSize INT;
DECLARE estimatedBegin TIMESTAMP WITHOUT TIME ZONE;
DECLARE lastCycleEnd TIMESTAMP WITHOUT TIME ZONE;
DECLARE nextCycleBegin TIMESTAMP WITHOUT TIME ZONE;
DECLARE estimatedEnd TIMESTAMP WITHOUT TIME ZONE;
DECLARE currentSlotBegin TIMESTAMP WITHOUT TIME ZONE;
DECLARE currentSlotEnd TIMESTAMP WITHOUT TIME ZONE;
BEGIN

  FOR operationcyclerecord in SELECT * from operationcycle WHERE operationcyclebegin is NULL or operationcycleend is NULL LOOP

    hasEstimatedBegin := false;
    hasEstimatedEnd := false;
    
    IF operationcyclerecord.operationcyclebegin is NULL THEN
	    lastCycleEnd := (SELECT max(operationcycleend) from operationcycle where operationcycleend < operationcyclerecord.operationcycleend
	                     AND machineid = operationcyclerecord.machineid);
	    	    
	    currentSlotBegin := (SELECT operationslotbegindatetime FROM operationslot WHERE operationslotid = operationcyclerecord.operationslotid);
	    
	    If (currentSlotBegin is not null) and (currentSlotBegin > operationcyclerecord.operationcycleend) THEN
	      currentSlotBegin := NULL;
	    END IF;

	    If (lastCycleEnd is not null) and (lastCycleEnd > operationcyclerecord.operationcycleend) THEN
	      lastCycleEnd := NULL;
	    END IF;

      IF (currentSlotBegin is null) THEN
        estimatedBegin := lastCycleEnd;
      ELSIF (lastCycleEnd is null) THEN
        estimatedBegin := currentSlotBegin;
      ELSIF (lastCycleEnd > currentSlotBegin) THEN
        estimatedBegin := lastCycleEnd;
      ELSE
        estimatedBegin := currentSlotBegin;
      END IF;
      
      IF (estimatedBegin is NULL) and (operationcyclerecord.operationslotid is NOT NULL) THEN
        estimatedBegin := (SELECT operationslotbegindatetime FROM operationslot where operationslotid = operationcyclerecord.operationslotid);
      END IF;
    
	    IF (estimatedBegin is not null) and (estimatedBegin <= operationcyclerecord.operationcycleend) THEN
	      -- this avoids glueing together cycle with no end followed by cycle with no begin
	      -- which would create two cycles with same begin/end date
   	    partialSetSize := (SELECT COUNT(*) from operationcycle where operationcyclebegin >= estimatedBegin
   	                      AND operationcyclebegin < operationcyclerecord.operationcycleend
   	                      AND machineid = operationcyclerecord.machineid
	                      AND operationcycleend is null);
        IF (partialSetSize = 0) THEN
              hasEstimatedBegin := true;
        END IF;
      END IF;
    END IF;

    IF operationcyclerecord.operationcycleend is NULL THEN
	    nextCycleBegin := (SELECT min(operationcyclebegin) from operationcycle where operationcyclebegin > operationcyclerecord.operationcyclebegin
	                      AND machineid = operationcyclerecord.machineid);
	               
	    currentSlotEnd := (SELECT operationslotenddatetime FROM operationslot where operationslotid = operationcyclerecord.operationslotid);

	    If (currentSlotEnd is not null) and (currentSlotEnd < operationcyclerecord.operationcyclebegin) THEN
	      currentSlotEnd := NULL;
	    END IF;

	    If (nextCycleBegin is not null) and (nextCycleBegin < operationcyclerecord.operationcyclebegin) THEN
	      nextCycleBegin := NULL;
	    END IF;
	               
      IF (currentSlotEnd is null) THEN
        estimatedEnd := nextCycleBegin;
      ELSIF (nextCycleBegin is null) THEN
        estimatedEnd := currentSlotEnd;
      ELSIF (nextCycleBegin < currentSlotEnd) THEN
        estimatedEnd := nextCycleBegin;
      ELSE
        estimatedEnd := currentSlotEnd;
      END IF;

      IF (estimatedEnd is not null) and (estimatedEnd >= operationcyclerecord.operationcyclebegin) THEN
  	    -- this avoids glueing together cycle with no end followed by cycle with no begin
  	    -- which would create two cycles with same begin/end date
  	    partialSetSize := (SELECT COUNT(*) from operationcycle where operationcycleend <= estimatedEnd
	                      AND operationcycleend > operationcyclerecord.operationcyclebegin
	                      AND machineid = operationcyclerecord.machineid
	                      AND operationcyclebegin is null);
        IF (partialSetSize = 0) THEN
          hasEstimatedEnd := true;
        END IF;
      END IF;
    END IF;

    -- db constraint: an operationcycle has either a begin or an end
    -- thus no need to consider case hasEstimatedBegin and hasEstimatedEnd
    IF hasEstimatedEnd AND operationcyclerecord.operationcyclebegin is not null and (estimatedEnd < operationcyclerecord.operationcyclebegin) THEN
      raise NOTICE '%', operationcyclerecord.operationcycleid;
    END IF;  
    
    IF hasEstimatedBegin  THEN
      UPDATE operationcycle
	SET operationcyclebegin = estimatedBegin,
	    operationcyclestatus = 1 -- code for begin estimated
	WHERE operationcycleid = operationcyclerecord.operationcycleid;
    ELSE
      IF hasEstimatedEnd THEN
        UPDATE operationcycle
	  SET operationcycleend = estimatedEnd,
	      operationcyclestatus = 2 -- code for end estimated
	  WHERE operationcycleid = operationcyclerecord.operationcycleid;
      END IF;
    END IF;
    
  END LOOP;

END;

$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION migrate_operationcycleoffsetduration() RETURNS void AS $$
DECLARE estimatedtimeinseconds DOUBLE PRECISION;
DECLARE operationrecord RECORD;
DECLARE operationcyclerecord RECORD;
BEGIN

  FOR operationrecord in SELECT * from operation where (operationestimatedmachininghours is not null) and (operationestimatedmachininghours > 0) LOOP
	estimatedtimeinseconds := operationrecord.operationestimatedmachininghours * 3600;
	FOR operationcyclerecord in SELECT * FROM operationcycle WHERE operationcyclebegin is not null and operationcycleend is not null and
	operationcycle.operationslotid in (SELECT operationslotid FROM operationslot WHERE operationid = operationrecord.operationid) LOOP

	UPDATE operationcycle
	SET operationcycleoffsetduration = (100 * (select extract (epoch from (operationcycleend - operationcyclebegin)) - estimatedtimeinseconds)) / estimatedtimeinseconds
	WHERE operationcycleid = operationcyclerecord.operationcycleid;
	END LOOP;
  END LOOP;
  
END;

$$ LANGUAGE plpgsql;

-- should take around 12.5 sec for 150k cycles
SELECT migrate_operationcycleestimated();
SELECT migrate_operationcycleoffsetduration();");

    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery(
        @"CREATE OR REPLACE FUNCTION unmigrate_offsetduration() RETURNS void AS $$
DECLARE operationcyclerecord RECORD;
BEGIN

  UPDATE operationcycle
  SET operationcycleoffsetduration = null;
  
  UPDATE operationcycle
  SET operationcyclebegin = null,
      operationcyclestatus = null
  WHERE operationcyclestatus = 1;

  UPDATE operationcycle
  SET operationcycleend = null,
      operationcyclestatus = null
  WHERE operationcyclestatus = 2;

  -- cas operationcyclestatus = 3 should not exit
    
END;

$$ LANGUAGE plpgsql;

-- around 9 seconds for 150k cycles
SELECT unmigrate_offsetduration();");
    }
  }
}

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
  /// Migration 046: Rename Process into Sequence in:
  /// <item>MachineMode table</item>
  /// <item>Stamp table</item>
  /// <item>StampingValue table</item>
  /// <item>Process table</item>
  /// <item>AutoProcess table</item>
  /// <item>ProcessDetection table</item>
  /// <item>stampboolean view</item>
  /// <item>sfkoperation view</item>
  /// 
  /// Note: the foreign key constraints are not renamed
  /// </summary>
  [Migration(46)]
  public class RenameProcessToSequence: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RenameProcessToSequence).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      UpgradeMachineMode ();
      UpgradeColumnSequenceId (TableName.STAMP);
      UpgradeColumnSequenceId (TableName.STAMPING_VALUE);
      UpgradeProcessTable ();
      UpgradeAutoProcessTable ();
      UpgradeProcessDetectionTable ();
      
      UpgradeStampBoolean ();
      UpgradeSfkoperation ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      DowngradeProcessDetectionTable ();
      DowngradeAutoProcessTable ();
      DowngradeProcessTable ();
      DowngradeColumnSequenceId (TableName.STAMPING_VALUE);
      DowngradeColumnSequenceId (TableName.STAMP);
      DowngradeMachineMode ();
      
      DowngradeStampBoolean ();
      DowngradeSfkoperation ();
    }
    
    void UpgradeMachineMode ()
    {
      Database.RenameColumn (TableName.MACHINE_MODE,
                             "machinemodeautoprocess",
                             "machinemodeautosequence");
    }
    
    void DowngradeMachineMode ()
    {
      Database.RenameColumn (TableName.MACHINE_MODE,
                             "machinemodeautosequence",
                             "machinemodeautoprocess");
    }

    void UpgradeColumnSequenceId (string table)
    {
      Database.RenameColumn (table,
                             ColumnName.OLD_SEQUENCE_ID,
                             ColumnName.SEQUENCE_ID);
    }
    
    void DowngradeColumnSequenceId (string table)
    {
      Database.RenameColumn (table,
                             ColumnName.SEQUENCE_ID,
                             ColumnName.OLD_SEQUENCE_ID);
    }
    
    void UpgradeProcessTable ()
    {
      Database.RenameTable (TableName.OLD_SEQUENCE,
                            TableName.SEQUENCE);
      UpgradeColumnSequenceId (TableName.SEQUENCE);
      Database.RenameColumn (TableName.SEQUENCE,
                             "processname",
                             "sequencename");
      Database.RenameColumn (TableName.SEQUENCE,
                             "processdescription",
                             "sequencedescription");
      Database.RenameColumn (TableName.SEQUENCE,
                             "processversion",
                             "sequenceversion");
   
      Database.ExecuteNonQuery (@"ALTER SEQUENCE process_processid_seq 
RENAME TO sequence_sequenceid_seq");
    }
    
    void DowngradeProcessTable ()
    {
      Database.RenameColumn (TableName.SEQUENCE,
                             "sequencename",
                             "processname");
      Database.RenameColumn (TableName.SEQUENCE,
                             "sequencedescription",
                             "processdescription");
      Database.RenameColumn (TableName.SEQUENCE,
                             "sequenceversion",
                             "processversion");
      DowngradeColumnSequenceId (TableName.SEQUENCE);
      Database.RenameTable (TableName.SEQUENCE,
                            TableName.OLD_SEQUENCE);
      
      Database.ExecuteNonQuery (@"ALTER SEQUENCE sequence_sequenceid_seq 
RENAME TO process_processid_seq");
    }
    
    void UpgradeAutoProcessTable ()
    {
      Database.RenameTable (TableName.OLD_AUTO_SEQUENCE,
                            TableName.AUTO_SEQUENCE);
      UpgradeColumnSequenceId (TableName.AUTO_SEQUENCE);
      Database.RenameColumn (TableName.AUTO_SEQUENCE,
                             ColumnName.OLD_AUTO_SEQUENCE_ID,
                             ColumnName.AUTO_SEQUENCE_ID);
      Database.RenameColumn (TableName.AUTO_SEQUENCE,
                             "autoprocessversion",
                             "autosequenceversion");
      Database.RenameColumn (TableName.AUTO_SEQUENCE,
                             "autoprocessbegin",
                             "autosequencebegin");
      Database.RenameColumn (TableName.AUTO_SEQUENCE,
                             "autoprocessend",
                             "autosequenceend");
      Database.RenameColumn (TableName.AUTO_SEQUENCE,
                             "autoprocessactivitybegin",
                             "autosequenceactivitybegin");
      Database.RenameColumn (TableName.AUTO_SEQUENCE,
                             "autoprocessactivityend",
                             "autosequenceactivityend");
      Database.RenameColumn (TableName.AUTO_SEQUENCE,
                             "autoprocessanalysis",
                             "autosequenceanalysis");
      
      Database.ExecuteNonQuery (@"ALTER SEQUENCE autoprocess_autoprocessid_seq 
RENAME TO autosequence_autosequenceid_seq");
    }

    void DowngradeAutoProcessTable ()
    {
      Database.RenameColumn (TableName.AUTO_SEQUENCE,
                             ColumnName.AUTO_SEQUENCE_ID,
                             ColumnName.OLD_AUTO_SEQUENCE_ID);
      Database.RenameColumn (TableName.AUTO_SEQUENCE,
                             "autosequenceversion",
                             "autoprocessversion");
      Database.RenameColumn (TableName.AUTO_SEQUENCE,
                             "autosequencebegin",
                             "autoprocessbegin");
      Database.RenameColumn (TableName.AUTO_SEQUENCE,
                             "autosequenceend",
                             "autoprocessend");
      Database.RenameColumn (TableName.AUTO_SEQUENCE,
                             "autosequenceactivitybegin",
                             "autoprocessactivitybegin");
      Database.RenameColumn (TableName.AUTO_SEQUENCE,
                             "autosequenceactivityend",
                             "autoprocessactivityend");
      Database.RenameColumn (TableName.AUTO_SEQUENCE,
                             "autosequenceanalysis",
                             "autoprocessanalysis");
      DowngradeColumnSequenceId (TableName.AUTO_SEQUENCE);
      Database.RenameTable (TableName.AUTO_SEQUENCE,
                            TableName.OLD_AUTO_SEQUENCE);
      
      Database.ExecuteNonQuery (@"ALTER SEQUENCE autosequence_autosequenceid_seq 
RENAME TO autoprocess_autoprocessid_seq");
    }
    
    void UpgradeProcessDetectionTable ()
    {
      Database.RenameTable (TableName.OLD_SEQUENCE_DETECTION,
                            TableName.SEQUENCE_DETECTION);
      UpgradeColumnSequenceId (TableName.SEQUENCE_DETECTION);
      Database.ExecuteNonQuery (@"UPDATE modification 
SET modificationreferencedtable='SequenceDetection' 
WHERE modificationreferencedtable='ProcessDetection'");
    }
    
    void DowngradeProcessDetectionTable ()
    {
      Database.ExecuteNonQuery (@"UPDATE modification 
SET modificationreferencedtable='SequenceDetection' 
WHERE modificationreferencedtable='ProcessDetection'");
      DowngradeColumnSequenceId (TableName.SEQUENCE_DETECTION);
      Database.RenameTable (TableName.SEQUENCE_DETECTION,
                            TableName.OLD_SEQUENCE_DETECTION);
    }
    
    void UpgradeStampBoolean ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW stampboolean;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW stampboolean AS 
 SELECT stamp.stampid, stamp.isofileid, stamp.stampposition, stamp.sequenceid, stamp.operationid, stamp.componentid, 
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
        END AS stampisofileend, stamp.stampversion
   FROM stamp;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE stampboolean_delete AS
    ON DELETE TO stampboolean DO INSTEAD  DELETE FROM stamp
  WHERE stamp.stampid = old.stampid;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE stampboolean_insert AS
    ON INSERT TO stampboolean DO INSTEAD  INSERT INTO stamp (isofileid, stampposition, sequenceid, operationid, componentid, operationcyclebegin, operationcycleend, stampisofileend) 
  VALUES (new.isofileid, new.stampposition, new.sequenceid, new.operationid, new.componentid, new.operationcyclebegin = 1, new.operationcycleend = 1, new.stampisofileend = 1)
  RETURNING stamp.stampid, stamp.isofileid, stamp.stampposition, stamp.sequenceid, stamp.operationid, stamp.componentid, 
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
        END AS stampisofileend, stamp.stampversion;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE stampboolean_update AS
    ON UPDATE TO stampboolean DO INSTEAD  UPDATE stamp SET stampposition = new.stampposition, sequenceid = new.sequenceid, operationid = new.operationid, componentid = new.componentid, operationcyclebegin = new.operationcyclebegin = 1, operationcycleend = new.operationcycleend = 1, stampisofileend = new.stampisofileend = 1, stampversion = new.stampversion
  WHERE stamp.stampid = old.stampid;");
    }
    
    void DowngradeStampBoolean ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW stampboolean;");
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
        END AS stampisofileend, stamp.stampversion
   FROM stamp;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE stampboolean_delete AS
    ON DELETE TO stampboolean DO INSTEAD  DELETE FROM stamp
  WHERE stamp.stampid = old.stampid;");
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
        END AS stampisofileend, stamp.stampversion;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE stampboolean_update AS
    ON UPDATE TO stampboolean DO INSTEAD  UPDATE stamp SET stampposition = new.stampposition, processid = new.processid, operationid = new.operationid, componentid = new.componentid, operationcyclebegin = new.operationcyclebegin = 1, operationcycleend = new.operationcycleend = 1, stampisofileend = new.stampisofileend = 1, stampversion = new.stampversion
  WHERE stamp.stampid = old.stampid;");
    }
    
    void UpgradeSfkoperation ()
    {
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW sfkoperation AS 
 SELECT sequence.sequenceid AS opid, 
        CASE
            WHEN sequence.sequencename IS NOT NULL THEN sequence.sequencename::text
            ELSE 
            CASE
                WHEN isofile.isofilename IS NOT NULL THEN isofile.isofilestampingdirectory::text || isofile.isofilename::text
                ELSE ''::text
            END
        END AS opname, 0 AS optype, 
        CASE
            WHEN sequence.sequencedescription IS NULL THEN ''::character varying
            ELSE sequence.sequencedescription
        END AS opdesc, 0 AS opdone, 
        CASE
            WHEN stamp.componentid IS NULL THEN 0
            ELSE stamp.componentid
        END AS opcompid, 
        CASE
            WHEN stamp.isofileid IS NULL THEN 0
            ELSE stamp.isofileid
        END AS opfileid, 
        CASE
            WHEN stamp.stampposition IS NULL THEN 0
            ELSE stamp.stampposition
        END AS opfilepos, 
        CASE
            WHEN sfkopstrategy.opstrategyid IS NULL THEN 0::bigint
            ELSE sfkopstrategy.opstrategyid
        END AS opstratid, 
        CASE
            WHEN sequence.toolid IS NULL THEN 0
            ELSE sequence.toolid - 1
        END AS optoolid, 1 AS opmetric, 
        CASE
            WHEN depth.stampingvaluedouble IS NULL THEN 0::double precision
            ELSE depth.stampingvaluedouble
        END AS opdepth, 
        CASE
            WHEN width.stampingvaluedouble IS NULL THEN 0::double precision
            ELSE width.stampingvaluedouble
        END AS opwidth, 
        CASE
            WHEN tolerance.stampingvaluedouble IS NULL THEN 0::double precision
            ELSE tolerance.stampingvaluedouble
        END AS optolerance, 0 AS opspeedratio, 
        CASE
            WHEN stock.stampingvaluedouble IS NULL THEN 0::double precision
            ELSE stock.stampingvaluedouble
        END AS opstock, 
        CASE
            WHEN isofile.isofilestampingdatetime IS NULL THEN timezone('UTC'::text, now())
            ELSE isofile.isofilestampingdatetime
        END AS opstampdate, 0 AS opcategoryid, operation.operationid AS opprocessid, 
        CASE
            WHEN toolminlength.stampingvaluedouble IS NULL THEN (-1)::double precision
            ELSE toolminlength.stampingvaluedouble
        END AS toolminlength, 
        CASE
            WHEN progfeedrate.stampingvaluedouble IS NULL THEN (-1)::double precision
            ELSE progfeedrate.stampingvaluedouble
        END AS progfeedrate, 
        CASE
            WHEN progspindlespeed.stampingvaluedouble IS NULL THEN (-1)::double precision
            ELSE progspindlespeed.stampingvaluedouble
        END AS progspindlespeed
   FROM sequence
   LEFT JOIN operation USING (operationid)
   LEFT JOIN stamp USING (sequenceid)
   LEFT JOIN isofile USING (isofileid)
   LEFT JOIN stampingvalue strategy ON strategy.fieldid = 40 AND strategy.sequenceid = sequence.sequenceid
   LEFT JOIN sfkopstrategy ON strategy.stampingvaluestring::text = sfkopstrategy.opstrategyname::text
   LEFT JOIN stampingvalue depth ON depth.fieldid = 50 AND depth.sequenceid = sequence.sequenceid
   LEFT JOIN stampingvalue width ON width.fieldid = 51 AND width.sequenceid = sequence.sequenceid
   LEFT JOIN stampingvalue tolerance ON tolerance.fieldid = 52 AND tolerance.sequenceid = sequence.sequenceid
   LEFT JOIN stampingvalue stock ON stock.fieldid = 53 AND stock.sequenceid = sequence.sequenceid
   LEFT JOIN stampingvalue toolminlength ON toolminlength.fieldid = 54 AND toolminlength.sequenceid = sequence.sequenceid
   LEFT JOIN stampingvalue progfeedrate ON progfeedrate.fieldid = 55 AND progfeedrate.sequenceid = sequence.sequenceid
   LEFT JOIN stampingvalue progspindlespeed ON progspindlespeed.fieldid = 56 AND progspindlespeed.sequenceid = sequence.sequenceid;");
    }
    
    void DowngradeSfkoperation ()
    {
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW sfkoperation AS 
 SELECT process.processid AS opid, 
        CASE
            WHEN process.processname IS NOT NULL THEN process.processname::text
            ELSE 
            CASE
                WHEN isofile.isofilename IS NOT NULL THEN isofile.isofilestampingdirectory::text || isofile.isofilename::text
                ELSE ''::text
            END
        END AS opname, 0 AS optype, 
        CASE
            WHEN process.processdescription IS NULL THEN ''::character varying
            ELSE process.processdescription
        END AS opdesc, 0 AS opdone, 
        CASE
            WHEN stamp.componentid IS NULL THEN 0
            ELSE stamp.componentid
        END AS opcompid, 
        CASE
            WHEN stamp.isofileid IS NULL THEN 0
            ELSE stamp.isofileid
        END AS opfileid, 
        CASE
            WHEN stamp.stampposition IS NULL THEN 0
            ELSE stamp.stampposition
        END AS opfilepos, 
        CASE
            WHEN sfkopstrategy.opstrategyid IS NULL THEN 0::bigint
            ELSE sfkopstrategy.opstrategyid
        END AS opstratid, 
        CASE
            WHEN process.toolid IS NULL THEN 0
            ELSE process.toolid - 1
        END AS optoolid, 1 AS opmetric, 
        CASE
            WHEN depth.stampingvaluedouble IS NULL THEN 0::double precision
            ELSE depth.stampingvaluedouble
        END AS opdepth, 
        CASE
            WHEN width.stampingvaluedouble IS NULL THEN 0::double precision
            ELSE width.stampingvaluedouble
        END AS opwidth, 
        CASE
            WHEN tolerance.stampingvaluedouble IS NULL THEN 0::double precision
            ELSE tolerance.stampingvaluedouble
        END AS optolerance, 0 AS opspeedratio, 
        CASE
            WHEN stock.stampingvaluedouble IS NULL THEN 0::double precision
            ELSE stock.stampingvaluedouble
        END AS opstock, 
        CASE
            WHEN isofile.isofilestampingdatetime IS NULL THEN timezone('UTC'::text, now())
            ELSE isofile.isofilestampingdatetime
        END AS opstampdate, 0 AS opcategoryid, operation.operationid AS opprocessid, 
        CASE
            WHEN toolminlength.stampingvaluedouble IS NULL THEN (-1)::double precision
            ELSE toolminlength.stampingvaluedouble
        END AS toolminlength, 
        CASE
            WHEN progfeedrate.stampingvaluedouble IS NULL THEN (-1)::double precision
            ELSE progfeedrate.stampingvaluedouble
        END AS progfeedrate, 
        CASE
            WHEN progspindlespeed.stampingvaluedouble IS NULL THEN (-1)::double precision
            ELSE progspindlespeed.stampingvaluedouble
        END AS progspindlespeed
   FROM process
   LEFT JOIN operation USING (operationid)
   LEFT JOIN stamp USING (processid)
   LEFT JOIN isofile USING (isofileid)
   LEFT JOIN stampingvalue strategy ON strategy.fieldid = 40 AND strategy.processid = process.processid
   LEFT JOIN sfkopstrategy ON strategy.stampingvaluestring::text = sfkopstrategy.opstrategyname::text
   LEFT JOIN stampingvalue depth ON depth.fieldid = 50 AND depth.processid = process.processid
   LEFT JOIN stampingvalue width ON width.fieldid = 51 AND width.processid = process.processid
   LEFT JOIN stampingvalue tolerance ON tolerance.fieldid = 52 AND tolerance.processid = process.processid
   LEFT JOIN stampingvalue stock ON stock.fieldid = 53 AND stock.processid = process.processid
   LEFT JOIN stampingvalue toolminlength ON toolminlength.fieldid = 54 AND toolminlength.processid = process.processid
   LEFT JOIN stampingvalue progfeedrate ON progfeedrate.fieldid = 55 AND progfeedrate.processid = process.processid
   LEFT JOIN stampingvalue progspindlespeed ON progspindlespeed.fieldid = 56 AND progspindlespeed.processid = process.processid;");
    }
  }
}

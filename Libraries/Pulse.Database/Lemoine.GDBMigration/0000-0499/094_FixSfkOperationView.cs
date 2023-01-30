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
  /// Migration 094: fix the sfkoperation view
  /// </summary>
  [Migration(94)]
  public class FixSfkOperationView: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixSfkOperationView).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
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
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}

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
  /// Migration 038: new views to get rid of the limitation of ODBC to understand the Boolean type
  /// <item>reasonselectionboolean</item>
  /// <item>machinemodeboolean</item>
  /// 
  /// Fix also in the same time the sfkoperation view
  /// </summary>
  [Migration(38)]
  public class BooleanViews2: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (BooleanViews).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      FixSfkOperationView ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
    
    void FixSfkOperationView ()
    {
      if (!Database.TableExists ("sfkopstrategy")) {
        Database.AddTable ("sfkopstrategy",
          new Column ("opstrategyid", DbType.Int64, ColumnProperty.NotNull),
          new Column ("opstrategycode", DbType.String, ColumnProperty.NotNull),
          new Column ("opstrategyname", DbType.String, ColumnProperty.NotNull),
          new Column ("opstrategygroup", DbType.Int64, ColumnProperty.NotNull));
        Database.ExecuteNonQuery (@"CREATE SEQUENCE sfkopstrategy_opstrategyid_seq
  INCREMENT 1
  MINVALUE 1
  MAXVALUE 9223372036854775807
  START 1
  CACHE 1;
        ");
        Database.ExecuteNonQuery (@"ALTER TABLE public.sfkopstrategy ALTER COLUMN opstrategyid SET DEFAULT nextval('sfkopstrategy_opstrategyid_seq'::regclass);");
        Database.ExecuteNonQuery (@"ALTER TABLE public.sfkopstrategy
  ADD CONSTRAINT sfkopstrategy_pkey PRIMARY KEY(opstrategyid);");
      }

      if (!Database.ColumnExists ("sfkoperation",
                                  "optoolid")) {
        Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS sfkoperation CASCADE");
        Database.ExecuteNonQuery (@"CREATE VIEW sfkoperation AS
SELECT process.processid AS opid,
  CASE WHEN processname IS NOT NULL THEN processname ELSE (CASE WHEN isofilename IS NOT NULL THEN isofilestampingdirectory || isofilename ELSE '' END) END AS opname,
  0 AS optype,
  CASE WHEN processdescription IS NULL THEN ''::varchar ELSE processdescription END AS opdesc,
  0 AS opdone,
  CASE WHEN componentid IS NULL THEN 0 ELSE componentid END AS opcompid,
  CASE WHEN isofileid IS NULL THEN 0 ELSE isofileid END AS opfileid,
  CASE WHEN stampposition IS NULL THEN 0 ELSE stampposition END AS opfilepos,
  CASE WHEN opstrategyid IS NULL THEN 0 ELSE opstrategyid END AS opstratid,
  CASE WHEN toolid IS NULL THEN 0 ELSE toolid-1 END AS optoolid,
  1 AS opmetric,
  CASE WHEN depth.stampingvaluedouble IS NULL THEN 0 ELSE depth.stampingvaluedouble END AS opdepth,
  CASE WHEN width.stampingvaluedouble IS NULL THEN 0 ELSE width.stampingvaluedouble END AS opwidth,
  CASE WHEN tolerance.stampingvaluedouble IS NULL THEN 0 ELSE tolerance.stampingvaluedouble END AS optolerance,
  0 AS opspeedratio,
  CASE WHEN stock.stampingvaluedouble IS NULL THEN 0 ELSE stock.stampingvaluedouble END AS opstock,
  CASE WHEN isofilestampingdatetime IS NULL THEN timezone('UTC'::text, now()) ELSE isofilestampingdatetime END AS opstampdate,
  0 AS opcategoryid,
  operation.operationid AS opprocessid,
  CASE WHEN toolminlength.stampingvaluedouble IS NULL THEN -1 ELSE toolminlength.stampingvaluedouble END AS toolminlength,
  CASE WHEN progfeedrate.stampingvaluedouble IS NULL THEN -1 ELSE progfeedrate.stampingvaluedouble END AS progfeedrate,
  CASE WHEN progspindlespeed.stampingvaluedouble IS NULL THEN -1 ELSE progspindlespeed.stampingvaluedouble END AS progspindlespeed
FROM process
LEFT OUTER JOIN operation USING (operationid)
LEFT OUTER JOIN stamp USING (processid)
LEFT OUTER JOIN isofile USING (isofileid)
LEFT OUTER JOIN stampingvalue strategy ON (strategy.fieldid=40 AND strategy.processid=process.processid)
LEFT OUTER JOIN sfkopstrategy ON (strategy.stampingvaluestring=opstrategyname)
LEFT OUTER JOIN stampingvalue depth ON (depth.fieldid=50 AND depth.processid=process.processid)
LEFT OUTER JOIN stampingvalue width ON (width.fieldid=51 AND width.processid=process.processid)
LEFT OUTER JOIN stampingvalue tolerance ON (tolerance.fieldid=52 AND tolerance.processid=process.processid)
LEFT OUTER JOIN stampingvalue stock ON (stock.fieldid=53 AND stock.processid=process.processid)
LEFT OUTER JOIN stampingvalue toolminlength ON (toolminlength.fieldid=54 AND toolminlength.processid=process.processid)
LEFT OUTER JOIN stampingvalue progfeedrate ON (progfeedrate.fieldid=55 AND progfeedrate.processid=process.processid)
LEFT OUTER JOIN stampingvalue progspindlespeed ON (progspindlespeed.fieldid=56 AND progspindlespeed.processid=process.processid);");
      }
      // Reset reportviews
      if (Database.TableExists ("sfkcfgs")) {
        Database.ExecuteNonQuery (@"DELETE FROM sfkcfgs
WHERE config='system' AND sfksection='reporting' AND skey='viewsversion';");
      }
    }
  }
}

// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Migrator.Framework;
using Lemoine.Core.Log;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 546: complete the applied date/time in manual reason associations
  /// to ease a faster process of the data in the future
  /// </summary>
  [Migration (546)]
  public class CompleteAnalysisAppliedDateTimeInManualReasonAssociations : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CompleteAnalysisAppliedDateTimeInManualReasonAssociations).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.ColumnExists (TableName.REASON_SLOT, "reasonslotbegindatetime")) {
        Database.ExecuteNonQuery (@"
with m as
(
  select * from reasonmachineassociation
  natural join machinemodificationstatus
  where reasonmachineassociationend is null
  and machineid=machinemodificationstatusmachineid
  and analysisapplieddatetime is null
  and analysisstatusid=3
  and reasonmachineassociationkind=4
)
, r as
(
  select * from reasonslot, m
  where reasonslot.machineid=m.machineid
  and reasonslotbegindatetime <= m.reasonmachineassociationbegin
  and reasonslotenddatetime > m.reasonmachineassociationbegin  
)
update machinemodificationstatus s
set analysisapplieddatetime=r.reasonslotenddatetime
from r
where s.analysisapplieddatetime is null
  and s.machinemodificationstatusmachineid=r.machinemodificationstatusmachineid
  and s.modificationid=r.modificationid
");
      }
      else {
        Database.ExecuteNonQuery (@"
with m as
(
  select * from reasonmachineassociation
  natural join machinemodificationstatus
  where reasonmachineassociationend is null
  and machineid=machinemodificationstatusmachineid
  and analysisapplieddatetime is null
  and analysisstatusid=3
  and reasonmachineassociationkind=4
)
, r as
(
  select * from reasonslot, m
  where reasonslot.machineid=m.machineid
  and lower (reasonslotdatetimerange) <= m.reasonmachineassociationbegin
  and upper (reasonslotdatetimerange) > m.reasonmachineassociationbegin  
)
update machinemodificationstatus s
set analysisapplieddatetime=upper (r.reasonslotdatetimerange)
from r
where s.analysisapplieddatetime is null
  and s.machinemodificationstatusmachineid=r.machinemodificationstatusmachineid
  and s.modificationid=r.modificationid
");
      }
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
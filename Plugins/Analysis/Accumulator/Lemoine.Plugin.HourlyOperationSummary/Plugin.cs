// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Extensions;
using Lemoine.Core.Log;
using Lemoine.Extensions.Plugin;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.GDBMigration;
using static Lemoine.Plugin.HourlyOperationSummary.TableName;
using Lemoine.Extensions.Interfaces;
using Pulse.Extensions.Plugin;

namespace Lemoine.Plugin.HourlyOperationSummary
{
  /// <summary>
  /// Support of a hourlyoperationsummary table
  /// </summary>
  public class Plugin : PluginWithAutoCustomAction<CustomAction>, IPluginDll, IFlaggedPlugin
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    TransformationProviderExt m_database = null;

    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name => "HourlyOperationSummary";

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description => "Support of a hourlyoperationsummary table";

    public PluginFlag Flags => PluginFlag.Config | PluginFlag.NHibernateExtension | PluginFlag.Analysis | PluginFlag.OperationExplorer | PluginFlag.Web;

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version => 2;

    TransformationProviderExt Database
    {
      get {
        if (null == m_database) {
          m_database = new TransformationProviderExt ();
        }
        return m_database;
      }
    }

    /// <summary>
    /// Install from a specific version
    /// (create or update tables if necessary, ...)
    /// This method is called within a transaction
    /// </summary>
    /// <param name="version"></param>
    protected override void InstallVersion (int version)
    {
      switch (version) {
        case 1: // First installation
          Install1 ();
          break;
        case 2: // Rename task into manuforder
          Install2 ();
          break;
        default:
          throw new InvalidOperationException ();
      }
    }

    void Install1 ()
    {
      if (Database.TableExists (TableName.HOURLY_OPERATION_SUMMARY)) {
        log.Warn ($"Inistall1: Table {TableName.HOURLY_OPERATION_SUMMARY} already exists which is unexpected");
        return;
      }

      Database.ExecuteSetOfQueries ($@"
CREATE TABLE public.hourlyoperationsummary
(
  hourlyoperationsummaryid serial NOT NULL,
  hourlyoperationsummaryversion integer NOT NULL DEFAULT 1,
  operationid integer,
  componentid integer,
  workorderid integer,
  machineid integer NOT NULL,
  hourlyoperationsummarytotalcycles integer NOT NULL DEFAULT 0,
  hourlyoperationsummaryadjustedcycles integer NOT NULL DEFAULT 0,
  hourlyoperationsummaryadjustedquantity integer NOT NULL DEFAULT 0,
  lineid integer,
  manuforderid integer,
  hourlyoperationsummaryday date,
  shiftid integer,
  localdatehour timestamp without time zone,
  hourlyoperationsummaryduration integer,
  CONSTRAINT hourlyoperationsummary_pkey PRIMARY KEY (hourlyoperationsummaryid),
  CONSTRAINT fk_hourlyoperationsummary_component FOREIGN KEY (componentid)
      REFERENCES public.component (componentid) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT fk_hourlyoperationsummary_operation FOREIGN KEY (operationid)
      REFERENCES public.operation (operationid) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_hourlyoperationsummary_line FOREIGN KEY (lineid)
      REFERENCES public.linetable (lineid) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT fk_hourlyoperationsummary_machine FOREIGN KEY (machineid)
      REFERENCES public.monitoredmachine (machineid) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_hourlyoperationsummary_shift FOREIGN KEY (shiftid)
      REFERENCES public.shift (shiftid) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT fk_hourlyoperationsummary_workorder FOREIGN KEY (workorderid)
      REFERENCES public.workorder (workorderid) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public.hourlyoperationsummary
  OWNER TO ""{GDBConnectionParameters.DatabaseUser}"";
GRANT ALL ON TABLE public.hourlyoperationsummary TO ""{GDBConnectionParameters.DatabaseUser}"";
GRANT SELECT ON TABLE public.hourlyoperationsummary TO PUBLIC;
");
      if (!Database.PartitionTable (HOURLY_OPERATION_SUMMARY, "monitoredmachine")) {
        log.Error ($"PartitionTable of {HOURLY_OPERATION_SUMMARY} failed");
      }
      Database.AddIndex (HOURLY_OPERATION_SUMMARY, new string[] { "machineid", "localdatehour" });
    }

    void Install2 ()
    {
      if (Database.ColumnExists (HOURLY_OPERATION_SUMMARY, ColumnName.TASK_ID)) {
        Database.RenameColumn (HOURLY_OPERATION_SUMMARY, ColumnName.TASK_ID, ColumnName.MANUFACTURING_ORDER_ID);
      }
    }

    /// <summary>
    /// Uninstall the plugin
    /// (delete tables if necessary, ...)
    /// This method is called within a transaction
    /// </summary>
    public override void Uninstall ()
    {
      Database.RemoveTable (HOURLY_OPERATION_SUMMARY);
    }
  }
}

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
using Lemoine.Model;
using Lemoine.GDBMigration;
using Lemoine.Extensions.Interfaces;
using Pulse.Extensions.Plugin;
using Lemoine.Info;

namespace Lemoine.Plugin.CycleCountSummary
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : PluginNoConfig, IPluginDll, IFlaggedPlugin
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    static readonly string CYCLE_COUNT_SUMMARY = "cyclecountsummary";

    TransformationProviderExt m_database = null;

    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name => "CycleCountSummary";

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description => "Support of a cyclecountsummary table. Used by MachineCycleCount and MachinePartCount reports";

    public PluginFlag Flags => PluginFlag.Config | PluginFlag.NHibernateExtension | PluginFlag.Analysis | PluginFlag.OperationExplorer | PluginFlag.Web; // Web: for a later use, to support the merge of components / operations

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

    #region Methods
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
        case 2: // Rename Task into Manufacturing Order
          Install2 ();
          break;
        default:
          throw new InvalidOperationException ();
      }
    }

    void Install1 ()
    {
      if (Database.TableExists (CYCLE_COUNT_SUMMARY)) {
        return;
      }

      Database.ExecuteSetOfQueries ($@"
CREATE TABLE public.cyclecountsummary
(
  cyclecountsummaryid serial NOT NULL,
  cyclecountsummaryversion integer NOT NULL DEFAULT 1,
  machineid integer NOT NULL,
  cyclecountsummaryday date NOT NULL,
  shiftid integer,
  workorderid integer,
  componentid integer,
  operationid integer,
  cyclecountsummaryfull integer NOT NULL DEFAULT 0,
  cyclecountsummarypartial integer NOT NULL DEFAULT 0,
  lineid integer,
  manuforderid integer,
  CONSTRAINT cyclecountsummary_pkey PRIMARY KEY (cyclecountsummaryid),
  CONSTRAINT fk_cyclecountsummary_component FOREIGN KEY (componentid)
      REFERENCES public.component (componentid) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT fk_cyclecountsummary_line FOREIGN KEY (lineid)
      REFERENCES public.linetable (lineid) MATCH SIMPLE
      ON UPDATE SET NULL ON DELETE SET NULL,
  CONSTRAINT fk_cyclecountsummary_monitoredmachine FOREIGN KEY (machineid)
      REFERENCES public.monitoredmachine (machineid) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_cyclecountsummary_operation FOREIGN KEY (operationid)
      REFERENCES public.operation (operationid) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT fk_cyclecountsummary_shift FOREIGN KEY (shiftid)
      REFERENCES public.shift (shiftid) MATCH SIMPLE
      ON UPDATE SET NULL ON DELETE SET NULL,
  CONSTRAINT fk_cyclecountsummary_workorder FOREIGN KEY (workorderid)
      REFERENCES public.workorder (workorderid) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT cyclecountsummary_unique UNIQUE (machineid, cyclecountsummaryday, shiftid, operationid, componentid, workorderid, lineid, manuforderid),
  CONSTRAINT cyclecountsummaryfull_positive CHECK (cyclecountsummaryfull >= 0),
  CONSTRAINT cyclecountsummarypartial_positive CHECK (cyclecountsummarypartial >= 0)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public.cyclecountsummary
  OWNER TO ""{GDBConnectionParameters.DatabaseUser}"";
GRANT ALL ON TABLE public.cyclecountsummary TO ""{GDBConnectionParameters.DatabaseUser}"";
GRANT SELECT ON TABLE public.cyclecountsummary TO PUBLIC;
");
      Database.PartitionTable (CYCLE_COUNT_SUMMARY, "monitoredmachine");
    }

    void Install2 ()
    {
      if (Database.ColumnExists (CYCLE_COUNT_SUMMARY, ColumnName.TASK_ID)) {
        Database.RenameColumn (CYCLE_COUNT_SUMMARY, ColumnName.TASK_ID, ColumnName.MANUFACTURING_ORDER_ID);
      }
      if (Database.ColumnExists (CYCLE_COUNT_SUMMARY, "taskfullid")) {
        Database.RemoveColumnCascade (CYCLE_COUNT_SUMMARY, "taskfullid");
      }
    }

    /// <summary>
    /// Uninstall the plugin
    /// (delete tables if necessary, ...)
    /// This method is called within a transaction
    /// </summary>
    public override void Uninstall ()
    {
      Database.RemoveTable (CYCLE_COUNT_SUMMARY);
    }
    #endregion // Methods
  }
}

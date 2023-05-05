// Copyright (C) 2009-2023 Lemoine Automation Technologies
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

namespace Lemoine.Plugin.CycleDurationSummary
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : PluginNoConfig, IPluginDll, IFlaggedPlugin
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    static readonly string CYCLE_DURATION_SUMMARY = "cycledurationsummary";

    #region Members
    TransformationProviderExt m_database = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name { get { return "CycleCountSummary"; } }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description
    {
      get
      {
        return "Support of a cycledurationsummary table. Use by the CyclesProgressReport and MachiningCyclesReport reports";
      }
    }

    public PluginFlag Flags
    {
      get
      {
        return PluginFlag.NHibernateExtension | PluginFlag.Analysis | PluginFlag.OperationExplorer | PluginFlag.Web; // Web: for a later use, to support the merge of components / operations
      }
    }

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version { get { return 1; } }

    TransformationProviderExt Database
    {
      get
      {
        if (null == m_database) {
          m_database = new TransformationProviderExt ();
        }
        return m_database;
      }
    }
    #endregion // Getters / Setters

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
      default:
        throw new InvalidOperationException ();
      }
    }

    void Install1 ()
    {
      if (Database.TableExists (CYCLE_DURATION_SUMMARY)) {
        return;
      }

      Database.ExecuteSetOfQueries ($@"
CREATE TABLE public.cycledurationsummary
(
  cycledurationsummaryid serial NOT NULL,
  cycledurationsummaryversion integer NOT NULL DEFAULT 1,
  machineid integer NOT NULL,
  cycledurationsummaryday date NOT NULL,
  workorderid integer,
  componentid integer,
  operationid integer,
  cycledurationsummaryoffset integer NOT NULL,
  cycledurationsummarynumber integer NOT NULL,
  shiftid integer,
  cycledurationsummarypartial integer NOT NULL DEFAULT 0,
  lineid integer,
  taskfullid integer,
  taskid integer,
  CONSTRAINT cycledurationsummary_pkey PRIMARY KEY (cycledurationsummaryid),
  CONSTRAINT fk_cycledurationsummary_component FOREIGN KEY (componentid)
      REFERENCES public.component (componentid) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT fk_cycledurationsummary_line FOREIGN KEY (lineid)
      REFERENCES public.linetable (lineid) MATCH SIMPLE
      ON UPDATE SET NULL ON DELETE SET NULL,
  CONSTRAINT fk_cycledurationsummary_monitoredmachine FOREIGN KEY (machineid)
      REFERENCES public.monitoredmachine (machineid) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_cycledurationsummary_operation FOREIGN KEY (operationid)
      REFERENCES public.operation (operationid) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT fk_cycledurationsummary_shift FOREIGN KEY (shiftid)
      REFERENCES public.shift (shiftid) MATCH SIMPLE
      ON UPDATE SET NULL ON DELETE SET NULL,
  CONSTRAINT fk_cycledurationsummary_workorder FOREIGN KEY (workorderid)
      REFERENCES public.workorder (workorderid) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT cycledurationsummary_unique UNIQUE (machineid, cycledurationsummaryday, shiftid, operationid, componentid, workorderid, lineid, taskid, cycledurationsummaryoffset),
  CONSTRAINT cycledurationsummarynumber_positive CHECK (cycledurationsummarynumber >= 0),
  CONSTRAINT cycledurationsummarypartial_positive CHECK (cycledurationsummarypartial >= 0)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public.cycledurationsummary
  OWNER TO ""{GDBConnectionParameters.DatabaseUser}"";
GRANT ALL ON TABLE public.cycledurationsummary TO ""{GDBConnectionParameters.DatabaseUser}"";
    GRANT SELECT ON TABLE public.cycledurationsummary TO PUBLIC;
");
      Database.PartitionTable (CYCLE_DURATION_SUMMARY, "monitoredmachine");
    }

    /// <summary>
    /// Uninstall the plugin
    /// (delete tables if necessary, ...)
    /// This method is called within a transaction
    /// </summary>
    public override void Uninstall ()
    {
      Database.RemoveTable (CYCLE_DURATION_SUMMARY);
    }
    #endregion // Methods
  }
}

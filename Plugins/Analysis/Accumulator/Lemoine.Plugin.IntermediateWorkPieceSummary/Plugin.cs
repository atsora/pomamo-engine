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
using static Lemoine.Plugin.IntermediateWorkPieceSummary.TableName;
using Lemoine.Extensions.Interfaces;
using Pulse.Extensions.Plugin;

namespace Lemoine.Plugin.IntermediateWorkPieceSummary
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : PluginNoConfig, IPluginDll, IFlaggedPlugin
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    #region Members
    TransformationProviderExt m_database = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name { get { return "IntermediateWorkPieceSummary"; } }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description
    {
      get {
        return "Support of a iwpbymachinesummary table and intermediateworkpiecesummary view. Used by the LineProduction and LineProductionByMachine reports";
      }
    }

    public PluginFlag Flags
    {
      get {
        return PluginFlag.Config | PluginFlag.NHibernateExtension | PluginFlag.Analysis | PluginFlag.OperationExplorer | PluginFlag.Web; // Web: for a later use, to support the merge of components / operations
      }
    }

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version => 1;

    TransformationProviderExt Database
    {
      get {
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
      if (!Database.TableExists (INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY)) {
        Database.ExecuteSetOfQueries (@"
CREATE TABLE public.iwpbymachinesummary
(
  iwpbymachinesummaryid serial NOT NULL,
  iwpbymachinesummaryversion integer NOT NULL DEFAULT 1,
  intermediateworkpieceid integer NOT NULL,
  componentid integer,
  workorderid integer,
  machineid integer NOT NULL,
  iwpbymachinesummarycounted integer NOT NULL DEFAULT 0,
  iwpbymachinesummarycorrected integer NOT NULL DEFAULT 0,
  iwpbymachinesummarychecked integer NOT NULL DEFAULT 0,
  iwpbymachinesummaryscrapped integer NOT NULL DEFAULT 0,
  iwpbymachinesummarytargeted integer,
  lineid integer,
  iwpbymachinesummaryday date,
  shiftid integer,
  taskid integer,
  CONSTRAINT iwpbymachinesummary_pkey PRIMARY KEY (iwpbymachinesummaryid),
  CONSTRAINT fk_iwpbymachinesummary_component FOREIGN KEY (componentid)
      REFERENCES public.component (componentid) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT fk_iwpbymachinesummary_intermediateworkpiece FOREIGN KEY (intermediateworkpieceid)
      REFERENCES public.intermediateworkpiece (intermediateworkpieceid) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_iwpbymachinesummary_line FOREIGN KEY (lineid)
      REFERENCES public.linetable (lineid) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT fk_iwpbymachinesummary_machine FOREIGN KEY (machineid)
      REFERENCES public.machine (machineid) MATCH SIMPLE
      ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_iwpbymachinesummary_shift FOREIGN KEY (shiftid)
      REFERENCES public.shift (shiftid) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT,
  CONSTRAINT fk_iwpbymachinesummary_workorder FOREIGN KEY (workorderid)
      REFERENCES public.workorder (workorderid) MATCH SIMPLE
      ON UPDATE RESTRICT ON DELETE RESTRICT
)
WITH (
  OIDS=FALSE
);
ALTER TABLE public.iwpbymachinesummary
  OWNER TO ""DatabaseUser"";
GRANT ALL ON TABLE public.iwpbymachinesummary TO ""DatabaseUser"";
GRANT SELECT ON TABLE public.iwpbymachinesummary TO PUBLIC;
");
      }
      if (!Database.IsPartitioned (INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY)) {
        Database.PartitionTable (INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY, "machine");
      }

      if (!Database.TableExists (INTERMEDIATE_WORK_PIECE_SUMMARY)) {
        Database.ExecuteSetOfQueries (@"
CREATE OR REPLACE VIEW public.intermediateworkpiecesummary AS 
 SELECT min(iwpbymachinesummary.iwpbymachinesummaryid) AS intermediateworkpiecesummaryid,
    iwpbymachinesummary.intermediateworkpieceid,
    iwpbymachinesummary.componentid,
    iwpbymachinesummary.workorderid,
    sum(iwpbymachinesummary.iwpbymachinesummarycounted) AS intermediateworkpiecesummarycounted,
    sum(iwpbymachinesummary.iwpbymachinesummarycorrected) AS intermediateworkpiecesummarycorrected,
    sum(iwpbymachinesummary.iwpbymachinesummarychecked) AS intermediateworkpiecesummarychecked,
    sum(iwpbymachinesummary.iwpbymachinesummaryscrapped) AS intermediateworkpiecesummaryscrapped,
    iwpbymachinesummary.lineid,
    iwpbymachinesummary.iwpbymachinesummaryday AS intermediateworkpiecesummaryday,
    iwpbymachinesummary.shiftid
   FROM iwpbymachinesummary
  GROUP BY iwpbymachinesummary.intermediateworkpieceid, iwpbymachinesummary.componentid, iwpbymachinesummary.workorderid, iwpbymachinesummary.lineid, iwpbymachinesummary.iwpbymachinesummaryday, iwpbymachinesummary.shiftid;

ALTER TABLE public.intermediateworkpiecesummary
  OWNER TO ""DatabaseUser"";
GRANT ALL ON TABLE public.intermediateworkpiecesummary TO ""DatabaseUser"";
GRANT SELECT ON TABLE public.intermediateworkpiecesummary TO PUBLIC;
");
      }
    }

    /// <summary>
    /// Uninstall the plugin
    /// (delete tables if necessary, ...)
    /// This method is called within a transaction
    /// </summary>
    public override void Uninstall ()
    {
      Database.RemoveTable (INTERMEDIATE_WORK_PIECE_SUMMARY);
      Database.RemoveTable (INTERMEDIATE_WORK_PIECE_BY_MACHINE_SUMMARY);
    }
    #endregion // Methods
  }
}

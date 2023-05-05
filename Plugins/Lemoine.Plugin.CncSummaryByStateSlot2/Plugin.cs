// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.GDBMigration;
using Lemoine.Extensions.Plugin;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.Extensions.Interfaces;
using Pulse.Extensions.Plugin;

namespace Lemoine.Plugin.CncSummaryByStateSlot2
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public class Plugin : PluginWithAutoConfigCustomAction<Configuration, CustomAction>, IPluginDll, IFlaggedPlugin
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Plugin).FullName);

    #region Members
    TransformationProviderExt m_database = null;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Plugin () : base (new ConfigurationLoader ())
    {
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name { get { return "Cnc value per production period - Variant 2"; } }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description
    {
      get
      {
        return "This plugin computes, for each machine module and for each production period, the " +
          "average of cnc values. The kind of cnc values can be chosen (for example feedrate override).";
      }
    }

    /// <summary>
    /// Plugin flags
    /// </summary>
    public PluginFlag Flags
    {
      get
      {
        return PluginFlag.Analysis | PluginFlag.CncData;
      }
    }

    public override bool MultipleConfigurations => false;

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
      Database.ExecuteSetOfQueries ($@"CREATE TABLE IF NOT EXISTS plugins.cncsummarybystateslot2_values 
(
  id bigserial NOT NULL,
  machinemoduleid integer NOT NULL,
  machineid integer NOT NULL,
  startday date,
  startdatetime timestamp without time zone NOT NULL,
  endday date,
  enddatetime timestamp without time zone NOT NULL,
  machineobservationstateid integer,
  shiftid integer,
  fieldid integer NOT NULL,
  average double precision NOT NULL DEFAULT '0'::double precision,
  computedseconds double precision NOT NULL DEFAULT '0'::double precision,
  CONSTRAINT cncsummarybystateslot2_values_pkey PRIMARY KEY (id),
  CONSTRAINT cncsummarybystateslot2_values_machinemoduleid_fkey FOREIGN KEY (machinemoduleid)
      REFERENCES public.machinemodule (machinemoduleid) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE CASCADE,
  CONSTRAINT cncsummarybystateslot2_values_machineid_machinemoduleid_fie_key UNIQUE (machineid, machinemoduleid, fieldid, startdatetime)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE plugins.cncsummarybystateslot2_values
  OWNER TO ""{GDBConnectionParameters.DatabaseUser}"";
GRANT ALL ON TABLE plugins.cncsummarybystateslot2_values TO ""{GDBConnectionParameters.DatabaseUser}"";
GRANT SELECT ON TABLE plugins.cncsummarybystateslot2_values TO PUBLIC;");
      Database.PartitionTable ("plugins", "cncsummarybystateslot2_values", "machinemodule");
    }

    /// <summary>
    /// Uninstall the plugin
    /// (delete tables if necessary, ...)
    /// This method is called within a transaction
    /// </summary>
    public override void Uninstall ()
    {
      Database.RemoveTable ("plugins.cncsummarybystateslot2_values");
    }
    #endregion // Methods
  }
}

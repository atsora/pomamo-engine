// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions;
using Lemoine.Info;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.GDBMigration;
using Lemoine.Extensions.Plugin;
using Lemoine.Model;
using Lemoine.Extensions.Interfaces;
using Pulse.Extensions.Plugin;

namespace Lemoine.Plugin.IdleTimePerPeriod2
{
  /// <summary>
  /// Deprecated plugin (just clean the tables...)
  /// </summary>
  public class Plugin: PluginNoConfig, IPluginDll, IFlaggedPlugin
  {
    #region Members
    TransformationProviderExt m_database = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Name of the plugin, displayed to the user
    /// </summary>
    public override string Name { get { return "Idle durations per production period - Variant 2"; } }
    
    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description {
      get {
        return "For each production period and for each machine, the first idle duration, " +
          "the last idle duration and the sum of the other idle durations are stored.";
      }
    }

    /// <summary>
    /// Plugin flags
    /// </summary>
    public PluginFlag Flags
    {
      get
      {
        return PluginFlag.None;
      }
    }

    /// <summary>
    /// Version of the plugin
    /// </summary>
    public override int Version { get { return 3; } }
    
    TransformationProviderExt Database {
      get
      {
        if (null == m_database) {
          m_database = new TransformationProviderExt ();
        }
        return m_database;
      }
    }
    #endregion // Getters / Setters

    static readonly ILog log = LogManager.GetLogger(typeof (Plugin).FullName);

    #region Methods
    /// <summary>
    /// Install from a specific version
    /// (create or update tables if necessary, ...)
    /// This method is called within a transaction
    /// </summary>
    /// <param name="version"></param>
    protected override void InstallVersion(int version)
    {
      switch (version) {
        case 1: // First installation
          Install1 ();
          break;
        case 2: // Second installation
          UpgradeTo2 ();
          break;
        case 3: // Third installation: auto-destruction
          UpgradeTo3 ();
          break;
        default:
          throw new InvalidOperationException ();
      }
    }
    
    void Install1 ()
    {
      Database.ExecuteSetOfQueries ($@"CREATE TABLE IF NOT EXISTS plugins.idletimeperperiod2_values 
(
  id bigserial NOT NULL,
  machineid integer,
  startday date,
  startdatetime timestamp without time zone NOT NULL,
  endday date,
  enddatetime timestamp without time zone NOT NULL,
  machineobservationstateid integer,
  shiftid integer,
  firstidleseconds double precision,
  lastidleseconds double precision,
  otheridleseconds double precision,
  CONSTRAINT idletimeperperiod2_values_pkey PRIMARY KEY (id),
  CONSTRAINT idletimeperperiod2_values_machineid_fkey FOREIGN KEY (machineid)
      REFERENCES public.monitoredmachine (machineid) MATCH SIMPLE
      ON UPDATE NO ACTION ON DELETE CASCADE,
  CONSTRAINT idletimeperperiod2_values_machineid_startdatetime_key UNIQUE (machineid, startdatetime)
)
WITH (
  OIDS=FALSE
);
ALTER TABLE plugins.idletimeperperiod2_values
  OWNER TO ""{GDBConnectionParameters.DatabaseUser}"";
GRANT ALL ON TABLE plugins.idletimeperperiod2_values TO ""{GDBConnectionParameters.DatabaseUser}"";
GRANT SELECT ON TABLE plugins.idletimeperperiod2_values TO PUBLIC;");
      Database.PartitionTable ("plugins", "idletimeperperiod2_values", "monitoredmachine");
    }
    
    /// <summary>
    /// Remove the not null constraints and the default values
    /// </summary>
    void UpgradeTo2 ()
    {
      Database.ExecuteSetOfQueries (@"
ALTER TABLE plugins.idletimeperperiod2_values
ALTER COLUMN firstidleseconds DROP NOT NULL;
ALTER TABLE plugins.idletimeperperiod2_values
ALTER COLUMN lastidleseconds DROP NOT NULL;
ALTER TABLE plugins.idletimeperperiod2_values
ALTER COLUMN otheridleseconds DROP NOT NULL;
ALTER TABLE plugins.idletimeperperiod2_values
ALTER COLUMN firstidleseconds DROP DEFAULT;
ALTER TABLE plugins.idletimeperperiod2_values
ALTER COLUMN lastidleseconds DROP DEFAULT;
ALTER TABLE plugins.idletimeperperiod2_values
ALTER COLUMN otheridleseconds DROP DEFAULT;
      ");
    }

    void UpgradeTo3 ()
    {
      Uninstall ();
    }

    /// <summary>
    /// Uninstall the plugin
    /// (delete tables if necessary, ...)
    /// This method is called within a transaction
    /// </summary>
    public override void Uninstall()
    {
      // Table "values"
      Database.RemoveTable("plugins.idletimeperperiod2_values");
    }
    #endregion // Methods
  }
}

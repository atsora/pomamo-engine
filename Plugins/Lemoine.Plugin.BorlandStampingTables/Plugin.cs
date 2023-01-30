// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions;
using Lemoine.Extensions.Interfaces;
using Lemoine.Extensions.Plugin;
using Lemoine.GDBMigration;
using Lemoine.Model;
using Pulse.Extensions.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lemoine.Plugin.BorlandStampingTables
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
    public override string Name { get { return "BorlandStampingTables"; } }

    /// <summary>
    /// Description of the plugin
    /// </summary>
    public override string Description
    {
      get {
        return "Create the database tables that are used by the old Borland stamping program if they don't exist";
      }
    }

    public PluginFlag Flags
    {
      get {
        return PluginFlag.Config;
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

    #region Migration
    void Install1 ()
    {
      try {
        AddSfkCamSystem ();
        AddSfkMachType ();
      }
      catch (Exception ex) {
        log.Error ($"Install1: exception", ex);
        throw;
      }
    }

    void AddSfkCamSystem ()
    {
      Database.ExecuteNonQuery (@"
CREATE TABLE IF NOT EXISTS public.sfkcamsystem
(
    camid bigserial NOT NULL,
    camdesc character varying NOT NULL,
    camisocode character varying NOT NULL,
    camcadkeyword character varying NOT NULL,
    camwidthkeyword character varying NOT NULL,
    camjobkeyword character varying NOT NULL,
    camtoolidkeyword character varying NOT NULL,
    camcustomerkeyword character varying NOT NULL,
    camstrategykeyword character varying NOT NULL,
    camtolerancekeyword character varying NOT NULL,
    camunitskeyword character varying NOT NULL,
    camtooldiakeyword character varying NOT NULL,
    camdepthkeyword character varying NOT NULL,
    camcompkeyword character varying NOT NULL,
    camjobtypekeyword character varying NOT NULL,
    camcomptypekeyword character varying NOT NULL,
    camstockkeyword character varying NOT NULL,
    camoptypekeyword character varying NOT NULL,
    camspindlespkeyword character varying NOT NULL,
    camhourskeyword character varying NOT NULL,
    camtoolradkeyword character varying NOT NULL,
    camfeedratekeyword character varying NOT NULL,
    camcategorykeyword character varying NOT NULL,
    camprocessnamekeyword character varying NOT NULL,
    camprocesstypekeyword character varying NOT NULL,
    camprocessorderkeyword character varying NOT NULL,
    processhourskeyword character varying NOT NULL,
    opdescriptionkeyword character varying NOT NULL,
    toolnamekeyword character varying NOT NULL,
    toolminlengthkeyword character varying NOT NULL,
    CONSTRAINT sfkcamsystem_pkey PRIMARY KEY (camid),
    CONSTRAINT sfkcamsystem_camisocode_key UNIQUE (camisocode)
);
");
    }

    void AddSfkMachType ()
    {
      Database.ExecuteNonQuery (@"
CREATE TABLE IF NOT EXISTS public.sfkmachtype
(
    machtypeid bigserial NOT NULL,
    pprname character varying NOT NULL,
    usen bigint NOT NULL,
    coden character varying,
    nspace bigint,
    commentstart character varying,
    commentend character varying,
    linestart character varying,
    lineend character varying,
    posformat bigint NOT NULL,
    processingtype bigint NOT NULL,
    dataon character varying,
    dataoff character varying,
    clockoff character varying,
    opidvar character varying,
    rmpulsecomments bigint,
    varsuffix character varying,
    varmultiplier bigint,
    startendcptvarprefix character varying,
    startendcptvarsuffix character varying,
    startendcptvarmultiplier bigint,
    fileend character varying,
    maxn bigint NOT NULL,
    CONSTRAINT sfkmachtype_pkey PRIMARY KEY (machtypeid),
    CONSTRAINT sfkmachtype_pprname_key UNIQUE (pprname)
);
");
    }

    /// <summary>
    /// Uninstall the plugin
    /// (delete tables if necessary, ...)
    /// This method is called within a transaction
    /// </summary>
    public override void Uninstall ()
    {
      Database.RemoveTable (TableName.SFK_CAMSYSTEM);
      Database.RemoveTable (TableName.SFK_MACHTYPE);
    }
    #endregion // Migration

    #endregion // Methods
  }
}

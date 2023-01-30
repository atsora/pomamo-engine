// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Config;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.CreateExternRoleSchema
{
  public class InstallationExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IInstallationExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (InstallationExtension).FullName);

    public double Priority => 0.0;

    public bool CheckConfig ()
    {

      try {
        var database = new Lemoine.GDBMigration.TransformationProviderExt ();
        try {
          database.ExecuteSetOfQueries (@"
CREATE ROLE extern LOGIN
  ENCRYPTED PASSWORD 'md5805ea2c810958885998ce5aed1266138'
  NOSUPERUSER INHERIT NOCREATEDB NOCREATEROLE NOREPLICATION;
", sqlErrorOk: true);
        }
        catch (Exception ex) {
          log.Info ($"CheckConfig: create role failed, probably because it already exists", ex);
        }

        try {
          database.ExecuteSetOfQueries (@"
CREATE SCHEMA extern
  AUTHORIZATION extern;
", sqlErrorOk: true);
        }
        catch (Exception ex) {
          log.Info ($"CheckConfig: create role failed, probably because it already exists", ex);
        }

        return true;
      }
      catch (Exception ex) {
        log.Error ($"CheckConfig: exception", ex);
        return false;
      }
    }

    public bool RemoveConfig ()
    {
      // Do nothing
      return true;
    }
  }
}

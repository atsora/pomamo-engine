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

namespace Lemoine.Plugin.GrantDatabaseRightsToReport
{
  public class InstallationExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IInstallationExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (InstallationExtension).FullName);

    public double Priority => 100.0;

    public bool CheckConfig ()
    {

      try {
        var database = new Lemoine.GDBMigration.TransformationProviderExt ();
        database.ExecuteSetOfQueries (@"
GRANT SELECT
ON ALL TABLES IN SCHEMA reportV2, public
TO reportV2;

GRANT EXECUTE
ON ALL FUNCTIONS IN SCHEMA reportV2, public
TO reportV2;

GRANT USAGE
ON SCHEMA reportV2, public
TO reportV2;

REVOKE ALL
ON TABLE public.applicationstate
FROM reportV2;
");
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

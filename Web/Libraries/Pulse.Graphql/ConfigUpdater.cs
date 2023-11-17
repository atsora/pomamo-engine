// Copyright (c) 2023 Nicolas Relange

using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Pulse.Graphql
{
  /// <summary>
  /// Utility class to update the config date/times
  /// </summary>
  internal static class ConfigUpdater
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ConfigUpdater).FullName);

    /// <summary>
    /// Notify a config update (may restart the asp service)
    /// </summary>
    internal static void Notify ()
    {
      if (!Lemoine.WebClient.Request.NotifyConfigUpdate ()) {
        log.Warn ($"Notify: NotifyConfigUpdate failed => try a direct update in database");
        Update ();
      }
    }

    /// <summary>
    /// Update the database that the config was updated
    /// </summary>
    internal static void Update ()
    {
      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (var transaction = session.BeginTransaction ("Package.PackageFile.configupdate")) {
          ModelDAOHelper.DAOFactory.ApplicationStateDAO.Update (ApplicationStateKey.ConfigUpdate.ToKey (), DateTime.UtcNow);
          transaction.Commit ();
        }
      }
      catch (Exception ex) {
        log.Error ($"Update: configupdate in database failed", ex);
      }
    }

  }
}

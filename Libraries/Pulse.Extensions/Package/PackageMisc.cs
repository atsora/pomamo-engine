// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Linq;

namespace Lemoine.Extensions.Package
{
  /// <summary>
  /// Description of Package.
  /// </summary>
  public static class PackageMisc
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PackageMisc).FullName);

    /// <summary>
    /// Activate a package
    /// </summary>
    /// <param name="package"></param>
    public static void Activate (IPackage package)
    {
      ChangeActivation (package, true);
    }

    /// <summary>
    /// Activate a package
    /// </summary>
    /// <param name="packageIdentifier"></param>
    public static void Activate (string packageIdentifier)
    {
      ChangeActivation (packageIdentifier, true);
    }

    /// <summary>
    /// De-activate a package
    /// </summary>
    /// <param name="package"></param>
    public static void Deactivate (IPackage package)
    {
      ChangeActivation (package, false);
    }

    /// <summary>
    /// De-activate a package
    /// </summary>
    /// <param name="packageIdentifier"></param>
    public static void Deactivate (string packageIdentifier)
    {
      ChangeActivation (packageIdentifier, false);
    }

    /// <summary>
    /// Change the activation status of a package
    /// </summary>
    /// <param name="packageIdentifier"></param>
    /// <param name="active"></param>
    public static void ChangeActivation (string packageIdentifier, bool active)
    {
      IPackage package = GetPackage (packageIdentifier);
      ChangeActivation (package, active);
    }

    /// <summary>
    /// Change the activation status of a package
    /// </summary>
    /// <param name="package"></param>
    /// <param name="active"></param>
    public static void ChangeActivation (IPackage package, bool active)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("Extensions.Package.ChangeActivation")) {
          ModelDAOHelper.DAOFactory.PackageDAO.Lock (package);
          package.Activated = active;
          transaction.Commit ();
        }
      }

      NotifyConfigUpdate ();
    }

    static IPackage GetPackage (string packageIdentifier)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Extensions.Package.GetPackage")) {
          var package = ModelDAOHelper.DAOFactory.PackageDAO.FindByIdentifyingName (packageIdentifier);
          if (log.IsDebugEnabled) {
            log.Debug ($"GetPackage: identifier={packageIdentifier} => name={package.Name}");
          }
          return package;
        }
      }
    }

    /// <summary>
    /// Remove a package, given its identifier
    /// </summary>
    /// <param name="packageIdentifier"></param>
    public static void Remove (string packageIdentifier)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("PackageManager.Remove")) {
          var package = ModelDAOHelper.DAOFactory.PackageDAO.FindByIdentifyingName (packageIdentifier);
          if (package is null) {
            log.Warn ($"Remove: package {packageIdentifier} does not exist in database");
          }
          else {
            ModelDAOHelper.DAOFactory.PackageDAO.MakeTransient (package);
          }
          transaction.Commit ();
        }
      }

      NotifyConfigUpdate ();
    }

    static void NotifyConfigUpdate ()
    {
      if (!Lemoine.WebClient.Request.NotifyConfigUpdate ()) {
        log.Warn ($"NotifyConfigUpdate: NotifyConfigUpdate failed => try a direct update in database");
        try {
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          using (IDAOTransaction transaction = session.BeginTransaction ("Package.PackageMisc.configupdate")) {
            ModelDAOHelper.DAOFactory.ApplicationStateDAO.Update (ApplicationStateKey.ConfigUpdate.ToKey (), DateTime.UtcNow);
            transaction.Commit ();
          }
        }
        catch (Exception ex) {
          log.Error ($"NotifyConfigUpdate: configupdate in database failed", ex);
        }
      }
    }

    /// <summary>
    /// Run CheckConfig on <see cref="IInstallationExtension"/> extensions
    /// </summary>
    public static void CheckConfigs ()
    {
      var extensions = Lemoine.Extensions.ExtensionManager
        .GetExtensions<Lemoine.Extensions.Business.Config.IInstallationExtension> ()
        .OrderBy (x => x.Priority);
      foreach (var extension in extensions) {
        try {
          extension.CheckConfig ();
        }
        catch (Exception ex) {
          log.Error ($"CheckConfigs: CheckConfig of {extension} ended in exception", ex);
        }
      }
    }
  }
}

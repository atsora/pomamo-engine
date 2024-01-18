// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;

namespace Lem_PackageManager.Console
{
  /// <summary>
  /// ConsoleRunner
  /// </summary>
  public class ConsoleRunner: IConsoleRunner<Options>
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConsoleRunner).FullName);

    Options m_options;
    readonly IApplicationInitializer m_applicationInitializer;
    readonly IExtensionsLoader m_extensionsLoader;

    /// <summary>
    /// Constructor
    /// </summary>
    public ConsoleRunner (IApplicationInitializer applicationInitializer, IExtensionsLoader extensionsLoader)
    {
      Debug.Assert (null != applicationInitializer);
      Debug.Assert (null != extensionsLoader);

      m_applicationInitializer = applicationInitializer;
      m_extensionsLoader = extensionsLoader;
    }

    public async Task RunConsoleAsync (CancellationToken cancellationToken = default)
    {
      try {
        await m_applicationInitializer.InitializeApplicationAsync (cancellationToken);

        cancellationToken.ThrowIfCancellationRequested ();

        if (!string.IsNullOrEmpty (m_options.InstallOrUpgradePackage)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"RunConsoleAsync: install or upgrade package {m_options.InstallOrUpgradePackage} with the overwrite parameters option {m_options.OverwriteParameters}");
          }
          Install (m_options.InstallOrUpgradePackage, m_options.OverwriteParameters);
        }

        if (!string.IsNullOrEmpty (m_options.RemovePackage)) {
          await RemoveAsync (m_options.RemovePackage);
        }

        if (!string.IsNullOrEmpty (m_options.DeactivatePackage)) {
          await DeactivateAsync (m_options.DeactivatePackage);
        }

        if (m_options.Check) {
          if (log.IsDebugEnabled) {
            log.Debug ($"RunConsoleAsync: check the configs");
          }
          await m_extensionsLoader.LoadExtensionsAsync ();
          CheckConfigs ();
        }
      }
      catch (Exception ex) {
        log.Error ($"RunConsoleAsync: exception", ex);
        throw;
      }
    }

    public void SetOptions (Options options)
    {
      m_options = options;
    }

    void Install (string jsonPath, bool overwriteParameters)
    {
      // - Install it
      try {
        Lemoine.Extensions.Package.PackageFile.InstallOrUpgrade (jsonPath, overwriteParameters);
      }
      catch (Exception ex) {
        log.Error ($"Install: error while install {jsonPath}", ex);
        string message = $"Error: the json installation file {jsonPath} could not be installed";
        System.Console.Out.WriteLine (message);
      }
    }

    async Task DeactivateAsync (string packageIdentifier)
    {
      await m_extensionsLoader.LoadExtensionsAsync ();
      RemoveConfig (packageIdentifier);

      try {
        Lemoine.Extensions.Package.PackageMisc.Deactivate (packageIdentifier);
      }
      catch (Exception ex) {
        log.Error ($"Deactivate: error while de-activating package {packageIdentifier}", ex);
        string message = $"Error: the package {packageIdentifier} could not be de-activated";
        System.Console.Out.WriteLine (message);
      }
    }

    async Task RemoveAsync (string packageIdentifier)
    {
      await m_extensionsLoader.LoadExtensionsAsync ();
      RemoveConfig (packageIdentifier);

      try {
        Lemoine.Extensions.Package.PackageMisc.Remove (packageIdentifier);
      }
      catch (Exception ex) {
        log.Error ($"Remove: error while removing package {packageIdentifier}", ex);
        string message = $"Error: the package {packageIdentifier} could not be removed";
        System.Console.Out.WriteLine (message);
      }
    }

    void CheckConfigs ()
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

    void RemoveConfig (string packageIdentifier)
    {
      var extensions = Lemoine.Extensions.ExtensionManager
        .GetExtensions<Lemoine.Extensions.Business.Config.IInstallationExtension> (packageIdentifier: packageIdentifier)
        .OrderByDescending (x => x.Priority);
      foreach (var extension in extensions) {
        try {
          extension.RemoveConfig ();
        }
        catch (Exception ex) {
          log.Error ($"RemoveConfig: RemoveConfig of {extension} with package identifier {packageIdentifier} ended in exception", ex);
        }
      }
    }
  }
}

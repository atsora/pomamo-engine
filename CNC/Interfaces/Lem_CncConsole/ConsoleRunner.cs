// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Cnc.Engine;
using Lemoine.CncEngine;
using Lemoine.Core.Hosting;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using Lemoine.Extensions.Interfaces;
using Lemoine.FileRepository;
using Lemoine.Info;
using Pulse.Hosting;

namespace Lem_CncConsole
{
  /// <summary>
  /// ConsoleRunner
  /// </summary>
  public class ConsoleRunner : IConsoleRunner<Options>
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConsoleRunner).FullName);

    static readonly TimeSpan DEFAULT_EVERY = TimeSpan.FromSeconds (2);

    readonly ICncEngineConfig m_cncEngineConfig;
    readonly IApplicationInitializer m_applicationInitializer;
    readonly IAssemblyLoader m_assemblyLoader;
    readonly IFileRepoClientFactory m_fileRepoClientFactory;
    readonly IExtensionsLoader m_extensionsLoader;
    TimeSpan m_every = DEFAULT_EVERY;
    bool m_staThread = false;
    int m_cncAcquisitionId = int.MinValue;
    string m_configurationFilePath = "";
    bool m_stamp = false;
    int m_parentProcessId = 0;

    public ConsoleRunner (ICncEngineConfig cncEngineConfig, IApplicationInitializer applicationInitializer, IAssemblyLoader assemblyLoader, IFileRepoClientFactory fileRepoClientFactory, IExtensionsLoader extensionsLoader)
    {
      Debug.Assert (null != cncEngineConfig);

      m_cncEngineConfig = cncEngineConfig;
      m_applicationInitializer = applicationInitializer;
      m_assemblyLoader = assemblyLoader;
      m_fileRepoClientFactory = fileRepoClientFactory;
      m_extensionsLoader = extensionsLoader;
    }

    public void SetOptions (Options options)
    {
      m_cncAcquisitionId = options.CncAcquisitionId;
      m_configurationFilePath = options.File;
      m_stamp = options.Stamp;
      m_parentProcessId = options.ParentProcessId;
      try {
        m_every = TimeSpan.FromMilliseconds (options.Every);
      }
      catch (Exception ex) {
        log.Error ($"SetOptions: parsing of options.Every failed => use default value {m_every} instead", ex);
      }
      m_staThread = options.StaThread;
    }

    public async Task RunConsoleAsync (CancellationToken cancellationToken = default)
    {
      await m_applicationInitializer.InitializeApplicationAsync (cancellationToken);

      if (cancellationToken.IsCancellationRequested) {
        return;
      }

      var copyDistantResources = false;
      if (copyDistantResources) { // May be for later
        var resourceDir = new DirectoryInfo (Path.Combine (PulseInfo.LocalConfigurationDirectory, "cnc_resources"));
        if (!resourceDir.Exists) {
          resourceDir.Create ();
        }
        if (Lemoine.FileRepository.FileRepoClient.ForceSynchronize ("cnc_resources", resourceDir.FullName, cancellationToken: cancellationToken) ==
            Lemoine.FileRepository.SynchronizationStatus.SYNCHRONIZATION_FAILED) {
          log.Error ("Main: error in the synchronization of the cnc resources, but continue");
        }
      }

      if (cancellationToken.IsCancellationRequested) {
        return;
      }

      // - Run !
      Acquisition acquisition;
      if (!string.IsNullOrEmpty (m_configurationFilePath)) {
        acquisition = new Acquisition (m_cncEngineConfig, m_configurationFilePath, m_assemblyLoader, m_fileRepoClientFactory);
      }
      else if (int.MinValue != m_cncAcquisitionId) {
        acquisition = new Acquisition (m_cncEngineConfig, m_extensionsLoader, m_cncAcquisitionId, m_every, m_assemblyLoader, m_fileRepoClientFactory, m_staThread);
      }
      else {
        log.Error ($"Main: no configuration file path {m_configurationFilePath} and not a valid cnc acquisition id {m_cncAcquisitionId} => nothing to do");
        return;
      }
      acquisition.UseStampFile = m_stamp;
      acquisition.ParentProcessId = m_parentProcessId;
      acquisition.RunDirectly (System.Threading.CancellationToken.None);
    }
  }
}

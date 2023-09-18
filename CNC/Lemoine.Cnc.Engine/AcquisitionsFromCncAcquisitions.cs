// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETSTANDARD || NET48 || NETCOREAPP

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Cnc.Engine;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using Lemoine.Extensions.Interfaces;
using Lemoine.FileRepository;

namespace Lemoine.CncEngine
{
  /// <summary>
  /// AcquisitionsFromCncAcquisitions
  /// 
  /// Thread safe <see cref="IAcquisitionSet"/> implementation getting the acquisitions that are configured
  /// for the current lpost computer
  /// </summary>
  public class AcquisitionsFromCncAcquisitions
    : IAcquisitionSet
  {
    readonly ILog log = LogManager.GetLogger (typeof (AcquisitionsFromCncAcquisitions).FullName);

    readonly ICncEngineConfig m_cncEngineConfig;
    readonly IExtensionsLoader m_extensionsLoader;
    readonly ICncAcquisitionInitializer m_cncAcquisitionInitializer;
    readonly IAssemblyLoader m_assemblyLoader;
    readonly IFileRepoClientFactory m_fileRepoClientFactory;
    readonly SemaphoreSlim m_semaphore = new SemaphoreSlim (1);
    volatile IList<Acquisition> m_acquisitions = null;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cncEngineConfig">not null</param>
    /// <param name="extensionsLoader">not null</param>
    /// <param name="cncAcquisitionInitializer"></param>
    /// <param name="assemblyLoader">nullable</param>
    /// <param name="fileRepoClientFactory">nullable</param>
    public AcquisitionsFromCncAcquisitions (ICncEngineConfig cncEngineConfig, IExtensionsLoader extensionsLoader, ICncAcquisitionInitializer cncAcquisitionInitializer, IAssemblyLoader assemblyLoader, IFileRepoClientFactory fileRepoClientFactory)
    {
      Debug.Assert (null != cncEngineConfig);
      Debug.Assert (null != extensionsLoader);

      m_cncEngineConfig = cncEngineConfig;
      m_extensionsLoader = extensionsLoader;
      m_cncAcquisitionInitializer = cncAcquisitionInitializer;
      m_assemblyLoader = assemblyLoader;
      m_fileRepoClientFactory = fileRepoClientFactory;
    }

    /// <summary>
    /// <see cref="IAcquisitionSet"/>
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Acquisition> GetAcquisitions (CancellationToken cancellationToken)
    {
      if (null != m_acquisitions) {
        return m_acquisitions;
      }

      using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (m_semaphore, cancellationToken)) {
        if (null != m_acquisitions) {
          return m_acquisitions;
        }

        if (cancellationToken.IsCancellationRequested) {
          return new List<Acquisition> ();
        }

        var cncAcquisitions = m_cncAcquisitionInitializer
         .GetRegisteredCncAcquisitions (cancellationToken);

        m_acquisitions = new List<Acquisition> ();
        foreach (var cncAcquisition in cncAcquisitions) {
          try {
            var acquisition = new Acquisition (m_cncEngineConfig, m_extensionsLoader, cncAcquisition, m_assemblyLoader, m_fileRepoClientFactory);
            m_acquisitions.Add (acquisition);
          }
          catch (Exception ex) {
            log.Error ($"GetAcquisitions.{cncAcquisition.Id}: exception creating Acquisition", ex);
          }
        }
        return m_acquisitions;
      }
    }
  }
}

#endif // NETSTANDARD || NET48 || NETCOREAPP

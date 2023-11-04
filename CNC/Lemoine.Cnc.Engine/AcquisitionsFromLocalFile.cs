// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Lemoine.CncEngine;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using Lemoine.FileRepository;

namespace Lemoine.CncEngine
{
  /// <summary>
  /// AcquisitionsFromLocalFile
  /// 
  /// Thread safe <see cref="IAcquisitionSet"/> implementation getting one acquisition from a local configuration file
  /// </summary>
  public class AcquisitionsFromLocalFile: IAcquisitionSet
  {
    readonly ILog log = LogManager.GetLogger (typeof (AcquisitionsFromLocalFile).FullName);

    readonly ICncEngineConfig m_cncEngineConfig;
    readonly string m_localFilePath;
    readonly IAssemblyLoader m_assemblyLoader;
    readonly IFileRepoClientFactory m_fileRepoClientFactory;
    readonly SemaphoreSlim m_semaphore = new SemaphoreSlim (1);
    volatile Acquisition m_acquisition = null;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cncEngineConfig">not null</param>
    /// <param name="localFilePath">Absolute or relative to the program</param>
    /// <param name="assemblyLoader">not null</param>
    /// <param name="fileRepoClientFactory">not null</param>
    public AcquisitionsFromLocalFile (ICncEngineConfig cncEngineConfig, string localFilePath, IAssemblyLoader assemblyLoader, IFileRepoClientFactory fileRepoClientFactory)
    {
      Debug.Assert (null != cncEngineConfig);

      m_cncEngineConfig = cncEngineConfig;
      if (Path.IsPathRooted (localFilePath)) {
        m_localFilePath = localFilePath;
      }
      else { // Relative to the program
        m_localFilePath = Path.Combine (Lemoine.Info.ProgramInfo.AbsoluteDirectory, localFilePath);
      }
      m_assemblyLoader = assemblyLoader;
      m_fileRepoClientFactory = fileRepoClientFactory;
    }

    /// <summary>
    /// <see cref="IAcquisitionSet"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IEnumerable<Acquisition> GetAcquisitions (CancellationToken cancellationToken)
    {
      if (null == m_acquisition) {
        using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (m_semaphore, cancellationToken)) {
          if (null == m_acquisition) {
            if (cancellationToken.IsCancellationRequested) {
              return new List<Acquisition> ();
            }

            m_acquisition = new Acquisition (m_cncEngineConfig, m_localFilePath, m_assemblyLoader, m_fileRepoClientFactory);
          }
        }
      }

      return new List<Acquisition> { m_acquisition };
    }

  }
}

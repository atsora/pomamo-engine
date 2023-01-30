// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.CncEngine;
using Lemoine.Core.Log;
using Lemoine.Core.Plugin;
using Lemoine.FileRepository;
using Lemoine.Hosting.AsyncInitialization;
using Lemoine.I18N;
using Lemoine.Info;
using Lemoine.Info.ConfigReader;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lem_CncCoreService
{
  public class EarlyAssemblyLoader : IAsyncInitializer
  {
    readonly ILog log = LogManager.GetLogger<EarlyAssemblyLoader> ();

    readonly IAssemblyLoader m_assemblyLoader;

    /// <summary>
    /// Constructor
    /// </summary>
    public EarlyAssemblyLoader (IAssemblyLoader assemblyLoader)
    {
      m_assemblyLoader = assemblyLoader;
    }

    public async Task InitializeAsync ()
    {
      await Task.Run (Initialize);
    }

    void Initialize ()
    {
      Debug.Assert (null != m_assemblyLoader);

      // TODO: make it flexible
      // - configurable ?
      // - dynamic depending on the used modules ?
      var typeLoader = new TypeLoader (m_assemblyLoader);
      typeLoader.Load<Lemoine.Cnc.SQLiteQueue.SQLiteCncDataQueue> ();
    }
  }
}
